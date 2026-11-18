using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using WebApolice.Auditoria.Contracts;
using WebApolice.Auditoria.Infrastructure;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Queries;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas.EncerrarProposta;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas.ObterProposta;
using WebApolice.Modulos.Seguro.Infrastructure.Persistence;
using WebApolice.Modulos.Seguro.Infrastructure.Persistence.Repositories;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Integration.Tests.Modulos.Seguro;

public partial class PropostasPostgresTests
{
    private AlterarCancelamentoVigenciaHandler AlteradorCancelamento(string codigo, TimeProvider relogio, IRegistradorAuditoria? auditor = null) => new(
        new PropostasRepository(fixture.DbContext, new ClientesPropostas(fixture.Cadastro)), Operacao(auditor ?? new RegistradorAuditoria(fixture.Auditoria)), ConfigConfirmacao(codigo), relogio);
    private RetirarCancelamentoVigenciaHandler RetiradorCancelamento(string codigo, TimeProvider relogio, IRegistradorAuditoria? auditor = null) => new(
        new PropostasRepository(fixture.DbContext, new ClientesPropostas(fixture.Cadastro)), Operacao(auditor ?? new RegistradorAuditoria(fixture.Auditoria)), ConfigConfirmacao(codigo), relogio);
    private async Task<(Guid Id, string Codigo)> PedidoCancelamentoAsync(RelogioPropostas relogio)
    {
        var r = await VigenteParaEncerrarAsync(relogio);
        await SolicitadorCancelamento(r.Codigo, relogio).Handle(new(r.Id, new(2, new(2026, 11, 13), "Pedido Original")), default);
        fixture.DbContext.ChangeTracker.Clear(); return r;
    }

    [Theory]
    [InlineData(10)] [InlineData(12)] [InlineData(14)]
    public async Task AlteraPedidoComDataRetroativaHojeOuFuturaPreservandoInstanteOriginal(int dia)
    {
        var relogio = new RelogioPropostas(); var (id, codigo) = await PedidoCancelamentoAsync(relogio);
        var solicitadoEm = relogio.Agora.UtcDateTime; relogio.Agora = relogio.Agora.AddHours(1);
        var resultado = await AlteradorCancelamento(codigo, relogio).Handle(new(id, new(3, new(2026, 11, dia), "ALTERAÇÃO DA DATA")), default);
        Assert.Equal(dia > 12, resultado.Agendada); Assert.Equal(dia <= 12, resultado.Cancelada); Assert.Equal(4, resultado.RevisaoPreenchimento);
        fixture.DbContext.ChangeTracker.Clear();
        var dto = await new ObterPropostaHandler(new PropostasRepository(fixture.DbContext, new ClientesPropostas(fixture.Cadastro)), ConfigConfirmacao(codigo), relogio).Handle(new(id), default);
        Assert.NotNull(dto); Assert.Equal(solicitadoEm, dto.CancelamentoSolicitadoEm); Assert.Equal(new DateOnly(2026, 11, dia), dto.DataCancelamentoVigencia);
        Assert.Equal(dia > 12, dto.PodeAlterarCancelamento); Assert.Equal(dia > 12, dto.PodeRetirarCancelamento);
        Assert.Equal(dia > 12, dto.Vigente); Assert.Equal(dia <= 12 ? new DateOnly(2026, 11, dia) : null, dto.DataFimVigencia);
        Assert.Contains(dto.Historico, h => h.Observacao != null && h.Observacao.Contains("13/11/2026") && h.Observacao.Contains($"{dia:00}/11/2026") && h.Observacao.Contains("Alteração da Data"));
        Assert.Contains(dto.Historico, h => h.Observacao != null && h.Observacao.Contains("Pedido Original"));
        Assert.Equal(15.12m, dto.Coberturas.Single().PremioTitular); Assert.Equal(100m, dto.Beneficiarios.Single().PercentualParticipacao);
        await using var leitura = LeituraNovaConexao(); var p = await leitura.Proposta.SingleAsync(p => p.PublicId == id);
        Assert.Null(p.SubstituidaEm); Assert.False(await leitura.PropostaMovimentos.AnyAsync(m => m.PropostaId == p.Id));
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task GerePedidoVencidoComPendenciaAteAEfetivacao(bool retirar)
    {
        var relogio = new RelogioPropostas(); var (id, codigo) = await PedidoCancelamentoAsync(relogio);
        relogio.Agora = new(2026, 11, 14, 12, 0, 0, TimeSpan.Zero);
        var config = ConfigConfirmacao(codigo); config.Value.StatusCanceladaCodigo = "";
        await new EfetivarCancelamentoVigenciaHandler(new PropostasRepository(fixture.DbContext, new ClientesPropostas(fixture.Cadastro)), Operacao(new RegistradorAuditoria(fixture.Auditoria)), config, relogio).Handle(new(id, 3), default);
        fixture.DbContext.ChangeTracker.Clear();
        var dto = await new ObterPropostaHandler(new PropostasRepository(fixture.DbContext, new ClientesPropostas(fixture.Cadastro)), ConfigConfirmacao(codigo), relogio).Handle(new(id), default);
        Assert.NotNull(dto); Assert.Equal("CANCELAMENTO_PENDENTE", dto.SituacaoVigenciaCodigo); Assert.True(dto.PodeAlterarCancelamento); Assert.True(dto.PodeRetirarCancelamento);
        if (retirar) await RetiradorCancelamento(codigo, relogio).Handle(new(id, new(4, "MANTER A VIGÊNCIA")), default);
        else await AlteradorCancelamento(codigo, relogio).Handle(new(id, new(4, new(2026, 11, 15), "ADIAR O ENCERRAMENTO")), default);
        await using var leitura = LeituraNovaConexao(); var p = await leitura.Proposta.SingleAsync(p => p.PublicId == id);
        Assert.True(p.Vigente); Assert.Null(p.CanceladaEm); Assert.Null(p.DataFimVigencia); Assert.Null(p.MotivoPendenciaVigencia); Assert.Null(p.PendenciaVigenciaEm);
        Assert.Equal(5, p.RevisaoPreenchimento); Assert.Equal(retirar ? null : new DateOnly(2026, 11, 15), p.DataCancelamentoVigencia);
        Assert.Equal(retirar, p.CancelamentoSolicitadoEm == null);
    }

    [Fact]
    public async Task RetiradaPreservaVidaCondicoesEHistoricoELiberaNovaSolicitacao()
    {
        var relogio = new RelogioPropostas(); var (id, codigo) = await PedidoCancelamentoAsync(relogio);
        var antes = await fixture.DbContext.Database.SqlQuery<string>($"SELECT md5((to_jsonb(p)-'data_cancelamento_vigencia'-'cancelamento_solicitado_em'-'motivo_pendencia_vigencia'-'pendencia_vigencia_em'-'revisao_preenchimento'-'updated_at')::text) AS \"Value\" FROM seguro.proposta p WHERE public_id={id}").SingleAsync();
        var vida = await fixture.DbContext.Database.SqlQuery<string>($"SELECT md5(to_jsonb(v)::text) AS \"Value\" FROM seguro.apolice_vida v JOIN seguro.proposta p ON p.apolice_vida_id=v.id WHERE p.public_id={id}").SingleAsync();
        await RetiradorCancelamento(codigo, relogio).Handle(new(id, new(3, "MANUTENÇÃO DA VIGÊNCIA")), default); fixture.DbContext.ChangeTracker.Clear();
        Assert.Equal(antes, await fixture.DbContext.Database.SqlQuery<string>($"SELECT md5((to_jsonb(p)-'data_cancelamento_vigencia'-'cancelamento_solicitado_em'-'motivo_pendencia_vigencia'-'pendencia_vigencia_em'-'revisao_preenchimento'-'updated_at')::text) AS \"Value\" FROM seguro.proposta p WHERE public_id={id}").SingleAsync());
        Assert.Equal(vida, await fixture.DbContext.Database.SqlQuery<string>($"SELECT md5(to_jsonb(v)::text) AS \"Value\" FROM seguro.apolice_vida v JOIN seguro.proposta p ON p.apolice_vida_id=v.id WHERE p.public_id={id}").SingleAsync());
        var repository = new PropostasRepository(fixture.DbContext, new ClientesPropostas(fixture.Cadastro));
        Assert.DoesNotContain(await repository.ListarCancelamentosAgendadosAsync(new(2026, 11, 20), "CONFIRMADA", 0, default), p => p.PublicId == id);
        var dto = await new ObterPropostaHandler(repository, ConfigConfirmacao(codigo), relogio).Handle(new(id), default);
        Assert.NotNull(dto); Assert.Equal("VIGENTE", dto.SituacaoVigenciaCodigo); Assert.True(dto.PodeCancelarVigencia); Assert.False(dto.PodeAlterarCancelamento); Assert.False(dto.PodeRetirarCancelamento);
        Assert.Contains(dto.Historico, h => h.Observacao != null && h.Observacao.Contains("Manutenção da Vigência") && h.Observacao.Contains("13/11/2026"));
        Assert.True((await SolicitadorCancelamento(codigo, relogio).Handle(new(id, new(4, new(2026, 11, 15), "Novo Pedido")), default)).Agendada);
    }

    [Fact]
    public async Task RetiraSemContextoAtivoOuProcessadorECatalogoDeCancelamento()
    {
        var relogio = new RelogioPropostas(); var (id, codigo) = await PedidoCancelamentoAsync(relogio);
        var p = await fixture.DbContext.Proposta.Include(p => p.Apolice).Include(p => p.ApoliceVida).SingleAsync(p => p.PublicId == id);
        p.Apolice!.Ativo = false; p.ApoliceVida!.Ativo = false; await fixture.DbContext.SaveChangesAsync(); fixture.DbContext.ChangeTracker.Clear();
        var config = ConfigConfirmacao(codigo, false); config.Value.StatusCanceladaCodigo = "";
        await new RetirarCancelamentoVigenciaHandler(new PropostasRepository(fixture.DbContext, new ClientesPropostas(fixture.Cadastro)), Operacao(new RegistradorAuditoria(fixture.Auditoria)), config, relogio).Handle(new(id, new(3, "Manter")), default);
        await using var leitura = LeituraNovaConexao(); Assert.True((await leitura.Proposta.SingleAsync(p => p.PublicId == id)).Vigente);
    }

    [Theory]
    [InlineData("sem_data")] [InlineData("igual")] [InlineData("antes_inicio")] [InlineData("fora_apolice")] [InlineData("fora_vida")]
    [InlineData("revisao")] [InlineData("sem_revisao")] [InlineData("sem_justificativa")] [InlineData("texto_longo")]
    [InlineData("sem_pedido")] [InlineData("legado")] [InlineData("oculta")] [InlineData("excluida")] [InlineData("efetivada")]
    public async Task RecusaGestaoInvalidaSemEscritaOuAuditoria(string tipo)
    {
        var relogio = new RelogioPropostas(); var (id, codigo) = await PedidoCancelamentoAsync(relogio);
        if (tipo == "efetivada") { relogio.Agora = relogio.Agora.AddDays(1); await EfetivadorCancelamento(codigo, relogio).Handle(new(id, 3), default); fixture.DbContext.ChangeTracker.Clear(); }
        var p = await fixture.DbContext.Proposta.Include(p => p.Apolice).Include(p => p.ApoliceVida).SingleAsync(p => p.PublicId == id);
        if (tipo == "sem_pedido") { p.CancelamentoSolicitadoEm = null; p.DataCancelamentoVigencia = null; }
        if (tipo == "legado") p.LegadoId = 729981;
        if (tipo == "oculta") p.VisivelOperacional = false;
        if (tipo == "excluida") p.DeletedAt = relogio.Agora.UtcDateTime;
        if (tipo == "fora_apolice") p.Apolice!.DataFimVigencia = new(2026, 11, 13);
        if (tipo == "fora_vida") p.ApoliceVida!.DataFimVigencia = new(2026, 11, 13);
        await fixture.DbContext.SaveChangesAsync(); fixture.DbContext.ChangeTracker.Clear();
        var antes = await AssinaturaPropostaAsync(id); var historicos = await fixture.DbContext.PropostaHistoricos.CountAsync(h => h.PropostaNova.PublicId == id);
        var auditorias = await fixture.Auditoria.RegistrosAuditoria.CountAsync(a => a.RecursoId == id.ToString());
        int? revisao = tipo == "sem_revisao" ? null : tipo == "revisao" ? 2 : tipo == "efetivada" ? 4 : 3;
        var observacao = tipo == "sem_justificativa" ? " " : tipo == "texto_longo" ? new string('x', 1001) : "Alterar";
        DateOnly? data = tipo switch { "sem_data" => null, "igual" => new(2026, 11, 13), "antes_inicio" => new(2026, 10, 31), _ => new(2026, 11, 14) };
        await Assert.ThrowsAsync<ValidacaoException>(() => AlteradorCancelamento(codigo, relogio).Handle(new(id, new(revisao, data, observacao)), default));
        fixture.DbContext.ChangeTracker.Clear();
        if (tipo is not ("sem_data" or "igual" or "antes_inicio" or "fora_apolice" or "fora_vida"))
            await Assert.ThrowsAsync<ValidacaoException>(() => RetiradorCancelamento(codigo, relogio).Handle(new(id, new(revisao, observacao)), default));
        Assert.Equal(antes, await AssinaturaPropostaAsync(id));
        Assert.Equal(historicos, await fixture.DbContext.PropostaHistoricos.CountAsync(h => h.PropostaNova.PublicId == id));
        Assert.Equal(auditorias, await fixture.Auditoria.RegistrosAuditoria.CountAsync(a => a.RecursoId == id.ToString()));
    }

    [Fact]
    public async Task AlteracaoFuturaExigeProcessadorMasImediataPermaneceDisponivel()
    {
        var relogio = new RelogioPropostas(); var (id, codigo) = await PedidoCancelamentoAsync(relogio);
        var handler = new AlterarCancelamentoVigenciaHandler(new PropostasRepository(fixture.DbContext, new ClientesPropostas(fixture.Cadastro)), Operacao(new RegistradorAuditoria(fixture.Auditoria)), ConfigConfirmacao(codigo, false), relogio);
        await Assert.ThrowsAsync<ValidacaoException>(() => handler.Handle(new(id, new(3, new(2026, 11, 14), "Adiar")), default)); fixture.DbContext.ChangeTracker.Clear();
        Assert.True((await handler.Handle(new(id, new(3, new(2026, 11, 12), "Antecipar")), default)).Cancelada);
    }

    [Theory]
    [InlineData("retirar")] [InlineData("futura")] [InlineData("imediata")]
    public async Task FalhaDeAuditoriaReverteGestaoDoPedido(string tipo)
    {
        var relogio = new RelogioPropostas(); var (id, codigo) = await PedidoCancelamentoAsync(relogio);
        var antes = await AssinaturaPropostaAsync(id); var historicos = await fixture.DbContext.PropostaHistoricos.CountAsync(h => h.PropostaNova.PublicId == id);
        var auditor = new Mock<IRegistradorAuditoria>(); auditor.Setup(a => a.RegistrarAsync(It.IsAny<WebApolice.Auditoria.Domain.RegistroAuditoria>(), It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("Falha artificial"));
        Task Executar() => tipo == "retirar" ? RetiradorCancelamento(codigo, relogio, auditor.Object).Handle(new(id, new(3, "Manter")), default)
            : AlteradorCancelamento(codigo, relogio, auditor.Object).Handle(new(id, new(3, new(2026, 11, tipo == "futura" ? 14 : 12), "Alterar")), default);
        await Assert.ThrowsAsync<InvalidOperationException>(Executar);
        Assert.Equal(antes, await AssinaturaPropostaAsync(id)); await using var leitura = LeituraNovaConexao();
        Assert.Equal(historicos, await leitura.PropostaHistoricos.CountAsync(h => h.PropostaNova.PublicId == id));
    }

    [Theory]
    [InlineData(true, true)] [InlineData(true, false)] [InlineData(false, true)] [InlineData(false, false)]
    public async Task SerializaGestaoDoPedidoComEfetivacaoNasDuasOrdens(bool gerirPrimeiro, bool retirar)
    {
        var relogio = new RelogioPropostas(); var (id, codigo) = await PedidoCancelamentoAsync(relogio); relogio.Agora = relogio.Agora.AddDays(1);
        var chegou = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously); var liberar = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var auditor = new Mock<IRegistradorAuditoria>();
        auditor.Setup(a => a.RegistrarAsync(It.IsAny<WebApolice.Auditoria.Domain.RegistroAuditoria>(), It.IsAny<CancellationToken>()))
            .Returns(async (WebApolice.Auditoria.Domain.RegistroAuditoria registro, CancellationToken ct) => { chegou.TrySetResult(); await liberar.Task.WaitAsync(ct); await new RegistradorAuditoria(fixture.Auditoria).RegistrarAsync(registro, ct); });
        Task Gerir(IRegistradorAuditoria a) => retirar ? RetiradorCancelamento(codigo, relogio, a).Handle(new(id, new(3, "Manter")), default)
            : AlteradorCancelamento(codigo, relogio, a).Handle(new(id, new(3, new(2026, 11, 15), "Adiar")), default);
        Task primeira = gerirPrimeiro ? Gerir(auditor.Object) : EfetivadorCancelamento(codigo, relogio, auditor.Object).Handle(new(id, 3), default);
        Task? segunda = null; await using var services = ServicosAgendamento(); await using var scope = services.CreateAsyncScope(); var sp = scope.ServiceProvider;
        try
        {
            await chegou.Task.WaitAsync(TimeSpan.FromSeconds(10));
            var operacao = new OperacaoPropostas(sp.GetRequiredService<SeguroDbContext>(), new OperacaoAuditada(sp.GetRequiredService<AuditoriaDbContext>(), sp.GetRequiredService<IRegistradorAuditoria>(), new Mock<IContextoAuditoria>().Object));
            segunda = gerirPrimeiro ? new EfetivarCancelamentoVigenciaHandler(sp.GetRequiredService<IPropostasRepository>(), operacao, ConfigConfirmacao(codigo), relogio).Handle(new(id, 3), default)
                : retirar ? new RetirarCancelamentoVigenciaHandler(sp.GetRequiredService<IPropostasRepository>(), operacao, ConfigConfirmacao(codigo), relogio).Handle(new(id, new(3, "Manter")), default)
                : new AlterarCancelamentoVigenciaHandler(sp.GetRequiredService<IPropostasRepository>(), operacao, ConfigConfirmacao(codigo), relogio).Handle(new(id, new(3, new(2026, 11, 15), "Adiar")), default);
            Assert.NotSame(segunda, await Task.WhenAny(segunda, Task.Delay(150)));
        }
        finally { liberar.TrySetResult(); await primeira; }
        Assert.NotNull(segunda); await Assert.ThrowsAsync<ValidacaoException>(async () => await segunda);
        await using var leitura = LeituraNovaConexao(); var p = await leitura.Proposta.SingleAsync(p => p.PublicId == id);
        Assert.Equal(gerirPrimeiro, p.Vigente); Assert.Equal(4, p.RevisaoPreenchimento);
        if (gerirPrimeiro) Assert.Equal(retirar ? null : new DateOnly(2026, 11, 15), p.DataCancelamentoVigencia);
        else Assert.Equal(new DateOnly(2026, 11, 13), p.DataFimVigencia);
    }

    [Fact]
    public async Task ProcessadorReleDataAlteradaENaoCancelaPedidoRetirado()
    {
        var relogio = new RelogioPropostas(); var (alterada, codigo) = await PedidoCancelamentoAsync(relogio); var (retirada, _) = await PedidoCancelamentoAsync(relogio);
        await AlteradorCancelamento(codigo, relogio).Handle(new(alterada, new(3, new(2026, 11, 15), "Adiar")), default); fixture.DbContext.ChangeTracker.Clear();
        await RetiradorCancelamento(codigo, relogio).Handle(new(retirada, new(3, "Manter")), default); fixture.DbContext.ChangeTracker.Clear();
        await using var services = ServicosAgendamento();
        var processador = new ProcessadorVigenciasPropostas(services.GetRequiredService<IServiceScopeFactory>(), ConfigConfirmacao(codigo), relogio, NullLogger<ProcessadorVigenciasPropostas>.Instance);
        relogio.Agora = new(2026, 11, 15, 2, 59, 0, TimeSpan.Zero); await processador.ProcessarAsync(default);
        await using (var leitura = LeituraNovaConexao()) Assert.True((await leitura.Proposta.SingleAsync(p => p.PublicId == alterada)).Vigente);
        relogio.Agora = relogio.Agora.AddMinutes(1); await processador.ProcessarAsync(default); await processador.ProcessarAsync(default);
        await using var final = LeituraNovaConexao();
        Assert.False((await final.Proposta.SingleAsync(p => p.PublicId == alterada)).Vigente); Assert.True((await final.Proposta.SingleAsync(p => p.PublicId == retirada)).Vigente);
        Assert.Equal(1, await fixture.Auditoria.RegistrosAuditoria.CountAsync(a => a.RecursoId == alterada.ToString() && a.UsuarioIdExterno == "sistema:vigencias-propostas"));
    }
}
