using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using WebApolice.Auditoria.Contracts;
using WebApolice.Auditoria.Infrastructure;
using WebApolice.Modulos.Cadastro.Contracts;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Queries;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas.EfetivarVigencia;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas.GerirAgendamento;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas.ObterProposta;
using WebApolice.Modulos.Seguro.Infrastructure.Persistence;
using WebApolice.Modulos.Seguro.Infrastructure.Persistence.Repositories;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Propostas.CriarProposta;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Integration.Tests.Modulos.Seguro;

public partial class PropostasPostgresTests
{
    private ReagendarPropostaHandler Reagendador(string codigo, TimeProvider relogio, IRegistradorAuditoria? auditor = null) => new(
        new PropostasRepository(fixture.DbContext, new ClientesPropostas(fixture.Cadastro)), Operacao(auditor ?? new RegistradorAuditoria(fixture.Auditoria)),
        new ClientesPropostas(fixture.Cadastro), Setup.SeguroPersistenciaTestes.ConsultasCadastro(fixture.DbContext), ConfigConfirmacao(codigo), relogio);
    private CancelarAgendamentoHandler Cancelador(string codigo, TimeProvider relogio, IRegistradorAuditoria? auditor = null) => new(
        new PropostasRepository(fixture.DbContext, new ClientesPropostas(fixture.Cadastro)), Operacao(auditor ?? new RegistradorAuditoria(fixture.Auditoria)), ConfigConfirmacao(codigo), relogio);
    private async Task<(Guid Origem, Guid Nova, string Codigo)> AgendadaAsync(RelogioPropostas relogio)
    {
        var resultado = await SubstituicaoAsync(relogio);
        await Confirmador(resultado.Codigo, relogio).Handle(new(resultado.Nova, new(1, new(2026, 11, 13))), default);
        fixture.DbContext.ChangeTracker.Clear(); return resultado;
    }
    private async Task<CriarPropostaCommand> CriacaoDaOrigemAsync(Guid id)
    {
        var p = await fixture.DbContext.Proposta.Include(p => p.Apolice).Include(p => p.ApoliceVida).SingleAsync(p => p.PublicId == id);
        return new() { ClienteId = p.ClienteId, ApoliceId = p.Apolice!.PublicId, ApoliceVidaId = p.ApoliceVida!.PublicId, PropostaOrigemPublicId = id };
    }

    [Theory]
    [InlineData("rascunho")] [InlineData("vigente")] [InlineData("legado")] [InlineData("oculta")]
    [InlineData("excluida")] [InlineData("revisao")] [InlineData("sem_justificativa")]
    public async Task CancelamentoRecusaEstadoOuComandoInvalidoSemAuditar(string tipo)
    {
        var relogio = new RelogioPropostas(); var (id, codigo, _) = await ProntaConfirmarAsync();
        if (tipo != "rascunho") await Confirmador(codigo, relogio).Handle(new(id, new(1, tipo == "vigente" ? new(2026, 11, 12) : new(2026, 11, 13))), default);
        var p = await fixture.DbContext.Proposta.SingleAsync(p => p.PublicId == id);
        if (tipo == "legado") p.LegadoId = 881199;
        if (tipo == "oculta") p.VisivelOperacional = false;
        if (tipo == "excluida") p.DeletedAt = relogio.Agora.UtcDateTime;
        await fixture.DbContext.SaveChangesAsync(); fixture.DbContext.ChangeTracker.Clear();
        var auditorias = await fixture.Auditoria.RegistrosAuditoria.CountAsync(a => a.RecursoId == id.ToString());
        await Assert.ThrowsAsync<ValidacaoException>(() => Cancelador(codigo, relogio).Handle(new(id, new(tipo == "revisao" ? 1 : 2, tipo == "sem_justificativa" ? " " : "Desistência")), default));
        await using var leitura = LeituraNovaConexao(); Assert.Null((await leitura.Proposta.SingleAsync(p => p.PublicId == id)).CanceladaEm);
        Assert.Equal(auditorias, await fixture.Auditoria.RegistrosAuditoria.CountAsync(a => a.RecursoId == id.ToString()));
    }

    [Fact]
    public async Task ConfiguracaoAusenteOuIncompativelBloqueiaCancelamentoEProcessadorDesabilitadoBloqueiaReagendamento()
    {
        var relogio = new RelogioPropostas(); var (_, id, codigo) = await AgendadaAsync(relogio);
        foreach (var invalido in new[] { "", "CONFIRMADA", "RASCUNHO", "INEXISTENTE" })
        {
            var config = ConfigConfirmacao(codigo); config.Value.StatusCanceladaCodigo = invalido;
            var handler = new CancelarAgendamentoHandler(new PropostasRepository(fixture.DbContext, new ClientesPropostas(fixture.Cadastro)), Operacao(new RegistradorAuditoria(fixture.Auditoria)), config, relogio);
            await Assert.ThrowsAsync<ValidacaoException>(() => handler.Handle(new(id, new(2, "Desistência")), default)); fixture.DbContext.ChangeTracker.Clear();
        }
        var reagendador = new ReagendarPropostaHandler(new PropostasRepository(fixture.DbContext, new ClientesPropostas(fixture.Cadastro)), Operacao(new RegistradorAuditoria(fixture.Auditoria)), new ClientesPropostas(fixture.Cadastro), Setup.SeguroPersistenciaTestes.ConsultasCadastro(fixture.DbContext), ConfigConfirmacao(codigo, false), relogio);
        await Assert.ThrowsAsync<ValidacaoException>(() => reagendador.Handle(new(id, new(2, new(2026, 11, 20), "Nova Data")), default));
    }

    [Fact]
    public async Task ReagendamentoDePendenciaCorrigidaLimpaMotivoSemAnteciparVigencia()
    {
        var relogio = new RelogioPropostas(); var (origem, id, codigo) = await AgendadaAsync(relogio);
        var vida = await fixture.DbContext.ApoliceVidas.SingleAsync(v => v.Propostas.Any(p => p.PublicId == id)); vida.Ativo = false;
        await fixture.DbContext.SaveChangesAsync(); fixture.DbContext.ChangeTracker.Clear(); relogio.Agora = new(2026, 11, 14, 12, 0, 0, TimeSpan.Zero);
        await Efetivador(codigo, relogio).Handle(new(id, 2), default); fixture.DbContext.ChangeTracker.Clear();
        vida = await fixture.DbContext.ApoliceVidas.SingleAsync(v => v.Id == vida.Id); vida.Ativo = true; await fixture.DbContext.SaveChangesAsync(); fixture.DbContext.ChangeTracker.Clear();
        await Reagendador(codigo, relogio).Handle(new(id, new(3, new(2026, 11, 20), "Nova Data Após Correção")), default);
        await using var leitura = LeituraNovaConexao(); var p = await leitura.Proposta.SingleAsync(p => p.PublicId == id);
        Assert.Null(p.MotivoPendenciaVigencia); Assert.Null(p.PendenciaVigenciaEm); Assert.False(p.Vigente);
        Assert.True((await leitura.Proposta.SingleAsync(p => p.PublicId == origem)).Vigente);
    }

    [Fact]
    public async Task ReagendaComHistoricoPreservandoConfirmacaoCondicoesEVigente()
    {
        var relogio = new RelogioPropostas(); var (origem, nova, codigo) = await AgendadaAsync(relogio);
        var anterior = await fixture.DbContext.Database.SqlQuery<string>($"SELECT md5(to_jsonb(p)::text) AS \"Value\" FROM seguro.proposta p WHERE public_id={origem}").SingleAsync();
        var confirmada = (await fixture.DbContext.Proposta.SingleAsync(p => p.PublicId == nova)).ConfirmadaEm;
        var resultado = await Reagendador(codigo, relogio).Handle(new(nova, new(2, new(2026, 11, 20), "AÇÃO DE PROTEÇÃO")), default);
        Assert.Equal(3, resultado.RevisaoPreenchimento); fixture.DbContext.ChangeTracker.Clear();
        var repo = new PropostasRepository(fixture.DbContext, new ClientesPropostas(fixture.Cadastro));
        var dto = await new ObterPropostaHandler(repo, ConfigConfirmacao(codigo), relogio).Handle(new(nova), default);
        Assert.NotNull(dto); Assert.Equal("AGENDADA", dto.SituacaoVigenciaCodigo); Assert.True(dto.PodeReagendar); Assert.True(dto.PodeCancelarAgendamento);
        Assert.Equal(new DateOnly(2026, 11, 20), dto.DataInicioVigencia); Assert.Equal(confirmada, dto.ConfirmadaEm);
        Assert.False(dto.Vigente); Assert.Equal(100m, dto.Beneficiarios.Single().PercentualParticipacao); Assert.Equal(15.12m, dto.Coberturas.Single().PremioTitular);
        var evento = Assert.Single(dto.Historico, h => h.Motivo == "Reagendamento de Proposta");
        Assert.Contains("13/11/2026", evento.Observacao); Assert.Contains("20/11/2026", evento.Observacao); Assert.Contains("Ação de Proteção", evento.Observacao);
        Assert.Equal(anterior, await fixture.DbContext.Database.SqlQuery<string>($"SELECT md5(to_jsonb(p)::text) AS \"Value\" FROM seguro.proposta p WHERE public_id={origem}").SingleAsync());
        await Assert.ThrowsAsync<ValidacaoException>(() => Reagendador(codigo, relogio).Handle(new(nova, new(2, new(2026, 11, 21), "Repetição")), default));
    }

    [Theory]
    [InlineData("hoje")] [InlineData("passada")] [InlineData("mesma")] [InlineData("fora_periodo")]
    [InlineData("sem_data")] [InlineData("sem_justificativa")] [InlineData("texto_longo")] [InlineData("contexto")]
    public async Task RecusaReagendamentoInvalidoSemAlterarAgendamento(string tipo)
    {
        var relogio = new RelogioPropostas(); var (_, nova, codigo) = await AgendadaAsync(relogio);
        if (tipo == "contexto") { var vida = await fixture.DbContext.ApoliceVidas.SingleAsync(v => v.Propostas.Any(p => p.PublicId == nova)); vida.Ativo = false; await fixture.DbContext.SaveChangesAsync(); fixture.DbContext.ChangeTracker.Clear(); }
        DateOnly? data = tipo switch { "hoje" => new(2026, 11, 12), "passada" => new(2026, 11, 11), "mesma" => new(2026, 11, 13), "fora_periodo" => new(2200, 1, 1), "sem_data" => null, _ => new(2026, 11, 20) };
        var texto = tipo == "sem_justificativa" ? "  " : tipo == "texto_longo" ? new string('a', 1001) : "Ajuste da Data";
        if (tipo == "fora_periodo") { var apolice = await fixture.DbContext.Apolices.SingleAsync(a => a.Propostas.Any(p => p.PublicId == nova)); apolice.DataFimVigencia = new(2026, 12, 31); await fixture.DbContext.SaveChangesAsync(); fixture.DbContext.ChangeTracker.Clear(); }
        var auditorias = await fixture.Auditoria.RegistrosAuditoria.CountAsync(a => a.RecursoId == nova.ToString());
        await Assert.ThrowsAsync<ValidacaoException>(() => Reagendador(codigo, relogio).Handle(new(nova, new(2, data, texto)), default));
        await using var leitura = LeituraNovaConexao(); var p = await leitura.Proposta.SingleAsync(p => p.PublicId == nova);
        Assert.Equal(2, p.RevisaoPreenchimento); Assert.Equal(new DateOnly(2026, 11, 13), p.DataInicioVigencia);
        Assert.Equal(auditorias, await fixture.Auditoria.RegistrosAuditoria.CountAsync(a => a.RecursoId == nova.ToString()));
    }

    [Fact]
    public async Task CancelaMesmoComContextoInativoPreservaPendenciaENaoVoltaParaFila()
    {
        var relogio = new RelogioPropostas(); var (origem, nova, codigo) = await AgendadaAsync(relogio);
        var vida = await fixture.DbContext.ApoliceVidas.SingleAsync(v => v.Propostas.Any(p => p.PublicId == nova)); vida.Ativo = false;
        await fixture.DbContext.SaveChangesAsync(); fixture.DbContext.ChangeTracker.Clear();
        relogio.Agora = new(2026, 11, 14, 12, 0, 0, TimeSpan.Zero);
        await Efetivador(codigo, relogio).Handle(new(nova, 2), default); fixture.DbContext.ChangeTracker.Clear();
        await Cancelador(codigo, relogio).Handle(new(nova, new(3, "DESISTÊNCIA DA ADESÃO")), default); fixture.DbContext.ChangeTracker.Clear();
        var repo = new PropostasRepository(fixture.DbContext, new ClientesPropostas(fixture.Cadastro));
        var dto = await new ObterPropostaHandler(repo, ConfigConfirmacao(codigo), relogio).Handle(new(nova), default);
        Assert.NotNull(dto); Assert.Equal("CANCELADA", dto.StatusCodigo); Assert.Equal("CANCELADA", dto.SituacaoVigenciaCodigo);
        Assert.Equal(relogio.Agora.UtcDateTime, dto.CanceladaEm); Assert.NotNull(dto.MotivoPendenciaVigencia); Assert.Equal(new DateOnly(2026, 11, 13), dto.DataInicioVigencia);
        Assert.False(dto.PodeReagendar); Assert.False(dto.PodeCancelarAgendamento); Assert.False(dto.PodeEfetivarVigencia); Assert.False(dto.PodeEditarRascunho); Assert.False(dto.PodeConfirmar);
        Assert.Contains(dto.Historico, h => h.Motivo == "Cancelamento de Agendamento" && h.Observacao == "Desistência da Adesão");
        Assert.DoesNotContain(await repo.ListarAgendadasAsync(new(2026, 11, 30), "CONFIRMADA", 0, default), p => p.PublicId == nova);
        await Assert.ThrowsAsync<ValidacaoException>(() => Efetivador(codigo, relogio).Handle(new(nova, 4), default));
        await Assert.ThrowsAsync<ValidacaoException>(() => Cancelador(codigo, relogio).Handle(new(nova, new(4, "Repetição")), default));
        await using var leitura = LeituraNovaConexao(); var p = await leitura.Proposta.SingleAsync(p => p.PublicId == nova);
        Assert.Null(p.DataFimVigencia); Assert.Null(p.SubstituidaEm); Assert.False(p.Vigente); Assert.NotNull(p.ConfirmadaEm); Assert.Equal(4, p.RevisaoPreenchimento);
        Assert.True((await leitura.Proposta.SingleAsync(p => p.PublicId == origem)).Vigente); Assert.False(await leitura.PropostaMovimentos.AnyAsync(m => m.PropostaId == p.Id));
    }

    [Fact]
    public async Task NovaTentativaAposCancelamentoNaoReutilizaVersaoENaoBloqueiaConfirmacao()
    {
        var relogio = new RelogioPropostas(); var (origem, nova, codigo) = await AgendadaAsync(relogio);
        var comando = await CriacaoDaOrigemAsync(origem);
        await Assert.ThrowsAsync<ValidacaoException>(() => CriarHandler(codigo, new RegistradorAuditoria(fixture.Auditoria)).Handle(comando, default));
        fixture.DbContext.ChangeTracker.Clear();
        await Cancelador(codigo, relogio).Handle(new(nova, new(2, "Nova Tentativa")), default); fixture.DbContext.ChangeTracker.Clear();
        var tentativa = await CriarHandler(codigo, new RegistradorAuditoria(fixture.Auditoria)).Handle(comando, default); fixture.DbContext.ChangeTracker.Clear();
        var p = await fixture.DbContext.Proposta.Include(p => p.PropostaCoberturas).SingleAsync(p => p.PublicId == tentativa);
        Assert.Equal(3, p.Versao); var cobertura = p.PropostaCoberturas.Single().PublicId;
        await Preenchedor(codigo).Handle(new(tentativa, Dados(cobertura) with { Beneficiarios = [new(null, "Maria", "Filha", 100)] }), default); fixture.DbContext.ChangeTracker.Clear();
        Assert.True((await Confirmador(codigo, relogio).Handle(new(tentativa, new(1, new(2026, 11, 12))), default)).Vigente);
        await using var leitura = LeituraNovaConexao();
        Assert.False((await leitura.Proposta.SingleAsync(p => p.PublicId == origem)).Vigente); Assert.NotNull((await leitura.Proposta.SingleAsync(p => p.PublicId == nova)).CanceladaEm);
    }

    [Fact]
    public async Task PrimeiraPropostaCanceladaPermiteNovaTentativaSemInventarOrigem()
    {
        var relogio = new RelogioPropostas(); var (id, codigo, _) = await ProntaConfirmarAsync();
        var comando = await CriacaoDaOrigemAsync(id); comando.PropostaOrigemPublicId = null;
        await Confirmador(codigo, relogio).Handle(new(id, new(1, new(2026, 11, 13))), default); fixture.DbContext.ChangeTracker.Clear();
        await Cancelador(codigo, relogio).Handle(new(id, new(2, "Desistência")), default); fixture.DbContext.ChangeTracker.Clear();
        var nova = await CriarHandler(codigo, new RegistradorAuditoria(fixture.Auditoria)).Handle(comando, default);
        await using var leitura = LeituraNovaConexao(); var p = await leitura.Proposta.SingleAsync(p => p.PublicId == nova);
        Assert.Equal(2, p.Versao); Assert.Null(p.PropostaOrigemId);
        var cancelada = await leitura.Proposta.SingleAsync(p => p.PublicId == id);
        var evento = await leitura.PropostaHistoricos.SingleAsync(h => h.PropostaNovaId == cancelada.Id);
        Assert.Equal(cancelada.Id, evento.PropostaAnteriorId);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task FalhaAuditoriaReverteHistoricoEEstadoDoAgendamento(bool cancelar)
    {
        var relogio = new RelogioPropostas(); var (_, nova, codigo) = await AgendadaAsync(relogio);
        var falha = new Mock<IRegistradorAuditoria>();
        falha.Setup(a => a.RegistrarAsync(It.IsAny<WebApolice.Auditoria.Domain.RegistroAuditoria>(), It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("Falha artificial"));
        if (cancelar) await Assert.ThrowsAsync<InvalidOperationException>(() => Cancelador(codigo, relogio, falha.Object).Handle(new(nova, new(2, "Desistência")), default));
        else await Assert.ThrowsAsync<InvalidOperationException>(() => Reagendador(codigo, relogio, falha.Object).Handle(new(nova, new(2, new(2026, 11, 20), "Nova Data")), default));
        await using var leitura = LeituraNovaConexao(); var p = await leitura.Proposta.Include(p => p.Status).SingleAsync(p => p.PublicId == nova);
        Assert.Null(p.CanceladaEm); Assert.Equal("CONFIRMADA", p.Status.Codigo); Assert.Equal(2, p.RevisaoPreenchimento); Assert.Equal(new DateOnly(2026, 11, 13), p.DataInicioVigencia);
        Assert.Equal(2, await leitura.PropostaHistoricos.CountAsync(h => h.PropostaNovaId == p.Id));
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task CancelamentoEAtivacaoConcorrentesPreservamQuemConcluiuPrimeiro(bool cancelarPrimeiro)
    {
        var relogio = new RelogioPropostas(); var (origem, nova, codigo) = await AgendadaAsync(relogio);
        relogio.Agora = new(2026, 11, 13, 12, 0, 0, TimeSpan.Zero);
        var chegou = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously); var liberar = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var auditor = new Mock<IRegistradorAuditoria>();
        auditor.Setup(a => a.RegistrarAsync(It.IsAny<WebApolice.Auditoria.Domain.RegistroAuditoria>(), It.IsAny<CancellationToken>()))
            .Returns(async (WebApolice.Auditoria.Domain.RegistroAuditoria registro, CancellationToken ct) => { chegou.TrySetResult(); await liberar.Task.WaitAsync(ct); await new RegistradorAuditoria(fixture.Auditoria).RegistrarAsync(registro, ct); });
        Task primeira = cancelarPrimeiro ? Cancelador(codigo, relogio, auditor.Object).Handle(new(nova, new(2, "Desistência")), default) : Efetivador(codigo, relogio, auditor.Object).Handle(new(nova, 2), default);
        Task? segunda = null;
        await using var services = ServicosAgendamento(); await using var scope = services.CreateAsyncScope(); var sp = scope.ServiceProvider;
        try
        {
            await chegou.Task.WaitAsync(TimeSpan.FromSeconds(10));
            var operacao = new OperacaoPropostas(sp.GetRequiredService<SeguroDbContext>(), new OperacaoAuditada(sp.GetRequiredService<AuditoriaDbContext>(), sp.GetRequiredService<IRegistradorAuditoria>(), new Mock<IContextoAuditoria>().Object));
            segunda = cancelarPrimeiro
                ? new EfetivarVigenciaPropostaHandler(sp.GetRequiredService<IPropostasRepository>(), operacao, sp.GetRequiredService<IClientesPropostas>(), sp.GetRequiredService<ICadastroSeguroQueries>(), ConfigConfirmacao(codigo), relogio).Handle(new(nova, 2), default)
                : new CancelarAgendamentoHandler(sp.GetRequiredService<IPropostasRepository>(), operacao, ConfigConfirmacao(codigo), relogio).Handle(new(nova, new(2, "Desistência")), default);
            Assert.NotSame(segunda, await Task.WhenAny(segunda, Task.Delay(150)));
        }
        finally { liberar.TrySetResult(); await primeira; }
        Assert.NotNull(segunda); await Assert.ThrowsAsync<ValidacaoException>(async () => await segunda);
        await using var leitura = LeituraNovaConexao(); var p = await leitura.Proposta.SingleAsync(p => p.PublicId == nova);
        Assert.Equal(!cancelarPrimeiro, p.Vigente); Assert.Equal(cancelarPrimeiro, p.CanceladaEm.HasValue);
        Assert.Equal(cancelarPrimeiro, (await leitura.Proposta.SingleAsync(p => p.PublicId == origem)).Vigente);
    }
}
