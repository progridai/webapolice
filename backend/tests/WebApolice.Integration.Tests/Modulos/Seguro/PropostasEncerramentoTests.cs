using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using WebApolice.Auditoria.Contracts;
using WebApolice.Auditoria.Infrastructure;
using WebApolice.Modulos.Cadastro.Contracts;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Queries;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas.EncerrarProposta;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas.ObterProposta;
using WebApolice.Modulos.Seguro.Infrastructure.Persistence;
using WebApolice.Modulos.Seguro.Infrastructure.Persistence.Repositories;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Propostas.CriarProposta;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Integration.Tests.Modulos.Seguro;

public partial class PropostasPostgresTests
{
    private SolicitarCancelamentoVigenciaHandler SolicitadorCancelamento(string codigo, TimeProvider relogio, IRegistradorAuditoria? auditor = null) => new(
        new PropostasRepository(fixture.DbContext, new ClientesPropostas(fixture.Cadastro)), Operacao(auditor ?? new RegistradorAuditoria(fixture.Auditoria)), ConfigConfirmacao(codigo), relogio);
    private EfetivarCancelamentoVigenciaHandler EfetivadorCancelamento(string codigo, TimeProvider relogio, IRegistradorAuditoria? auditor = null) => new(
        new PropostasRepository(fixture.DbContext, new ClientesPropostas(fixture.Cadastro)), Operacao(auditor ?? new RegistradorAuditoria(fixture.Auditoria)), ConfigConfirmacao(codigo), relogio);
    private DescartarRascunhoHandler Descartador(string codigo, TimeProvider relogio, IRegistradorAuditoria? auditor = null) => new(
        new PropostasRepository(fixture.DbContext, new ClientesPropostas(fixture.Cadastro)), Operacao(auditor ?? new RegistradorAuditoria(fixture.Auditoria)), ConfigConfirmacao(codigo), relogio);
    private async Task<(Guid Id, string Codigo)> VigenteParaEncerrarAsync(RelogioPropostas relogio)
    {
        var (id, codigo, _) = await ProntaConfirmarAsync();
        await Confirmador(codigo, relogio).Handle(new(id, new(1, new(2026, 11, 1))), default);
        fixture.DbContext.ChangeTracker.Clear(); return (id, codigo);
    }
    private async Task<string> AssinaturaPropostaAsync(Guid id) => await fixture.DbContext.Database.SqlQuery<string>(
        $"SELECT md5(to_jsonb(p)::text) AS \"Value\" FROM seguro.proposta p WHERE public_id={id}").SingleAsync();

    [Fact]
    public async Task ExigeConfiguracaoFinalizadoraDistintaEProcessadorParaCancelamentoFuturo()
    {
        var relogio = new RelogioPropostas(); var (id, codigo) = await VigenteParaEncerrarAsync(relogio);
        foreach (var invalido in new[] { "", "CONFIRMADA", "DESCARTADA", "INEXISTENTE" })
        {
            var config = ConfigConfirmacao(codigo); config.Value.StatusCanceladaCodigo = invalido;
            var handler = new SolicitarCancelamentoVigenciaHandler(new PropostasRepository(fixture.DbContext, new ClientesPropostas(fixture.Cadastro)), Operacao(new RegistradorAuditoria(fixture.Auditoria)), config, relogio);
            await Assert.ThrowsAsync<ValidacaoException>(() => handler.Handle(new(id, new(2, new(2026, 11, 12), "Encerramento")), default)); fixture.DbContext.ChangeTracker.Clear();
        }
        var desabilitado = new SolicitarCancelamentoVigenciaHandler(new PropostasRepository(fixture.DbContext, new ClientesPropostas(fixture.Cadastro)), Operacao(new RegistradorAuditoria(fixture.Auditoria)), ConfigConfirmacao(codigo, false), relogio);
        await Assert.ThrowsAsync<ValidacaoException>(() => desabilitado.Handle(new(id, new(2, new(2026, 11, 13), "Agendar")), default));
    }

    [Fact]
    public async Task RevalidaCancelamentoPendenteSemDuplicarAuditoriaDaMesmaCausa()
    {
        var relogio = new RelogioPropostas(); var (id, codigo) = await VigenteParaEncerrarAsync(relogio);
        await SolicitadorCancelamento(codigo, relogio).Handle(new(id, new(2, new(2026, 11, 13), "Encerramento")), default);
        fixture.DbContext.ChangeTracker.Clear(); relogio.Agora = new(2026, 11, 13, 12, 0, 0, TimeSpan.Zero);
        var config = ConfigConfirmacao(codigo); config.Value.StatusCanceladaCodigo = string.Empty;
        var handler = new EfetivarCancelamentoVigenciaHandler(new PropostasRepository(fixture.DbContext, new ClientesPropostas(fixture.Cadastro)), Operacao(new RegistradorAuditoria(fixture.Auditoria)), config, relogio);
        var auditoriasAntes = await fixture.Auditoria.RegistrosAuditoria.CountAsync(a => a.RecursoId == id.ToString());
        var primeira = await handler.Handle(new(id, 3), default); fixture.DbContext.ChangeTracker.Clear(); Assert.False(primeira.Cancelada); Assert.NotNull(primeira.PendenciaVigencia);
        Assert.Equal(auditoriasAntes + 1, await fixture.Auditoria.RegistrosAuditoria.CountAsync(a => a.RecursoId == id.ToString()));
        var repetida = await handler.Handle(new(id, 4), default); fixture.DbContext.ChangeTracker.Clear(); Assert.Equal(primeira, repetida);
        Assert.Equal(auditoriasAntes + 1, await fixture.Auditoria.RegistrosAuditoria.CountAsync(a => a.RecursoId == id.ToString()));
        Assert.True((await EfetivadorCancelamento(codigo, relogio).Handle(new(id, 4), default)).Cancelada);
        await using var leitura = LeituraNovaConexao(); Assert.Null((await leitura.Proposta.SingleAsync(p => p.PublicId == id)).MotivoPendenciaVigencia);
    }

    [Theory]
    [InlineData(10)] [InlineData(12)] [InlineData(13)]
    public async Task CancelaComDataRetroativaHojeOuFuturaPreservandoCondicoesEVida(int dia)
    {
        var relogio = new RelogioPropostas(); var (id, codigo) = await VigenteParaEncerrarAsync(relogio);
        var vidaAntes = await fixture.DbContext.Database.SqlQuery<string>($"SELECT md5(to_jsonb(v)::text) AS \"Value\" FROM seguro.apolice_vida v JOIN seguro.proposta p ON p.apolice_vida_id=v.id WHERE p.public_id={id}").SingleAsync();
        var resultado = await SolicitadorCancelamento(codigo, relogio).Handle(new(id, new(2, new(2026, 11, dia), "DESISTÊNCIA DA ADESÃO")), default);
        Assert.Equal(dia > 12, resultado.Agendada); Assert.Equal(dia <= 12, resultado.Cancelada); Assert.Equal(3, resultado.RevisaoPreenchimento);
        fixture.DbContext.ChangeTracker.Clear();
        var dto = await new ObterPropostaHandler(new PropostasRepository(fixture.DbContext, new ClientesPropostas(fixture.Cadastro)), ConfigConfirmacao(codigo), relogio).Handle(new(id), default);
        Assert.NotNull(dto); Assert.Equal(new DateOnly(2026, 11, dia), dto.DataCancelamentoVigencia); Assert.Equal(relogio.Agora.UtcDateTime, dto.CancelamentoSolicitadoEm);
        Assert.Equal(dia > 12 ? "CANCELAMENTO_AGENDADO" : "CANCELADA", dto.SituacaoVigenciaCodigo); Assert.Equal(dia > 12, dto.Vigente); Assert.False(dto.PodeCancelarVigencia);
        Assert.Equal(15.12m, dto.Coberturas.Single().PremioTitular); Assert.Equal(100m, dto.Beneficiarios.Single().PercentualParticipacao);
        Assert.Contains(dto.Historico, h => h.Observacao != null && h.Observacao.Contains("Desistência da Adesão"));
        await using var leitura = LeituraNovaConexao(); var p = await leitura.Proposta.SingleAsync(p => p.PublicId == id);
        Assert.Equal(new DateOnly(2026, 11, 1), p.DataInicioVigencia); Assert.Null(p.SubstituidaEm);
        Assert.Equal(dia <= 12 ? new DateOnly(2026, 11, dia) : null, p.DataFimVigencia); Assert.Equal(dia <= 12 ? relogio.Agora.UtcDateTime : null, p.CanceladaEm);
        Assert.False(await leitura.PropostaMovimentos.AnyAsync(m => m.PropostaId == p.Id));
        Assert.Equal(vidaAntes, await fixture.DbContext.Database.SqlQuery<string>($"SELECT md5(to_jsonb(v)::text) AS \"Value\" FROM seguro.apolice_vida v JOIN seguro.proposta p ON p.apolice_vida_id=v.id WHERE p.public_id={id}").SingleAsync());
        await Assert.ThrowsAsync<ValidacaoException>(() => SolicitadorCancelamento(codigo, relogio).Handle(new(id, new(3, new(2026, 11, dia), "Repetição")), default));
    }

    [Fact]
    public async Task CancelamentoAgendadoRespeitaFusoRetomaAtrasoENaoDuplicaAuditoria()
    {
        var relogio = new RelogioPropostas(); var (id, codigo) = await VigenteParaEncerrarAsync(relogio);
        var criacao = await CriacaoDaOrigemAsync(id);
        await SolicitadorCancelamento(codigo, relogio).Handle(new(id, new(2, new(2026, 11, 13), "Encerramento Programado")), default); fixture.DbContext.ChangeTracker.Clear();
        await Assert.ThrowsAsync<ValidacaoException>(() => CriarHandler(codigo, new RegistradorAuditoria(fixture.Auditoria)).Handle(criacao, default));
        relogio.Agora = new(2026, 11, 13, 2, 59, 0, TimeSpan.Zero);
        await Assert.ThrowsAsync<ValidacaoException>(() => EfetivadorCancelamento(codigo, relogio).Handle(new(id, 3), default));
        relogio.Agora = new(2026, 11, 14, 12, 0, 0, TimeSpan.Zero);
        await using var services = ServicosAgendamento();
        var processador = new ProcessadorVigenciasPropostas(services.GetRequiredService<IServiceScopeFactory>(), ConfigConfirmacao(codigo), relogio, NullLogger<ProcessadorVigenciasPropostas>.Instance);
        await Task.WhenAll(processador.ProcessarAsync(default), processador.ProcessarAsync(default)); await processador.ProcessarAsync(default);
        fixture.DbContext.ChangeTracker.Clear();
        Assert.True((await EfetivadorCancelamento(codigo, relogio).Handle(new(id, 3), default)).Cancelada);
        await using var leitura = LeituraNovaConexao(); var p = await leitura.Proposta.SingleAsync(p => p.PublicId == id);
        Assert.False(p.Vigente); Assert.Equal(new DateOnly(2026, 11, 13), p.DataFimVigencia); Assert.Equal(relogio.Agora.UtcDateTime, p.CanceladaEm); Assert.Equal(4, p.RevisaoPreenchimento);
        Assert.Equal(1, await fixture.Auditoria.RegistrosAuditoria.CountAsync(a => a.RecursoId == id.ToString() && a.UsuarioIdExterno == "sistema:vigencias-propostas"));
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task BloqueiaComSucessoraAteDescartarRascunhoOuCancelarAgendamento(bool agendada)
    {
        var relogio = new RelogioPropostas(); var (origem, nova, codigo) = await SubstituicaoAsync(relogio);
        if (agendada) await Confirmador(codigo, relogio).Handle(new(nova, new(1, new(2026, 11, 13))), default);
        fixture.DbContext.ChangeTracker.Clear();
        await Assert.ThrowsAsync<ValidacaoException>(() => SolicitadorCancelamento(codigo, relogio).Handle(new(origem, new(2, new(2026, 11, 12), "Desistência")), default));
        fixture.DbContext.ChangeTracker.Clear();
        if (agendada) await Cancelador(codigo, relogio).Handle(new(nova, new(2, "Desistência")), default);
        else await Descartador(codigo, relogio).Handle(new(nova, new(1, "Condições Não Aprovadas")), default);
        fixture.DbContext.ChangeTracker.Clear();
        Assert.True((await SolicitadorCancelamento(codigo, relogio).Handle(new(origem, new(2, new(2026, 11, 12), "Encerramento")), default)).Cancelada);
    }

    [Fact]
    public async Task CancelaSemExigirContextoAtivoENaoReativaVersaoAnterior()
    {
        var relogio = new RelogioPropostas(); var (origem, nova, codigo) = await SubstituicaoAsync(relogio);
        await Confirmador(codigo, relogio).Handle(new(nova, new(1, new(2026, 11, 10))), default); fixture.DbContext.ChangeTracker.Clear();
        var anterior = await AssinaturaPropostaAsync(origem);
        var vida = await fixture.DbContext.ApoliceVidas.SingleAsync(v => v.Propostas.Any(p => p.PublicId == nova)); vida.Ativo = false;
        var apolice = await fixture.DbContext.Apolices.SingleAsync(a => a.Propostas.Any(p => p.PublicId == nova)); apolice.Ativo = false;
        await fixture.DbContext.SaveChangesAsync(); fixture.DbContext.ChangeTracker.Clear();
        Assert.True((await SolicitadorCancelamento(codigo, relogio).Handle(new(nova, new(2, new(2026, 11, 11), "Encerramento Retroativo")), default)).Cancelada);
        Assert.Equal(anterior, await AssinaturaPropostaAsync(origem));
        await using var leitura = LeituraNovaConexao(); Assert.False((await leitura.Proposta.SingleAsync(p => p.PublicId == origem)).Vigente);
        Assert.False((await leitura.ApoliceVidas.SingleAsync(v => v.Id == vida.Id)).Ativo);
    }

    [Theory]
    [InlineData("antes_inicio")] [InlineData("sem_data")] [InlineData("fora_periodo")]
    [InlineData("revisao")] [InlineData("sem_justificativa")] [InlineData("texto_longo")]
    [InlineData("legado")] [InlineData("oculta")] [InlineData("rascunho")]
    public async Task RecusaCancelamentoInvalidoSemMudarDadosOuHistorico(string tipo)
    {
        var relogio = new RelogioPropostas(); Guid id; string codigo;
        if (tipo == "rascunho") { var r = await ProntaConfirmarAsync(); id = r.Id; codigo = r.Codigo; }
        else (id, codigo) = await VigenteParaEncerrarAsync(relogio);
        var p = await fixture.DbContext.Proposta.Include(p => p.Apolice).SingleAsync(p => p.PublicId == id);
        if (tipo == "legado") p.LegadoId = 712992;
        if (tipo == "oculta") p.VisivelOperacional = false;
        if (tipo == "fora_periodo") p.Apolice!.DataFimVigencia = new(2026, 11, 30);
        await fixture.DbContext.SaveChangesAsync(); fixture.DbContext.ChangeTracker.Clear(); var antes = await AssinaturaPropostaAsync(id);
        DateOnly? data = tipo switch { "antes_inicio" => new(2026, 10, 31), "sem_data" => null, "fora_periodo" => new(2026, 12, 1), _ => new(2026, 11, 12) };
        var texto = tipo == "sem_justificativa" ? " " : tipo == "texto_longo" ? new string('a', 1001) : "Encerramento";
        await Assert.ThrowsAsync<ValidacaoException>(() => SolicitadorCancelamento(codigo, relogio).Handle(new(id, new(tipo == "revisao" ? 1 : 2, data, texto)), default));
        Assert.Equal(antes, await AssinaturaPropostaAsync(id));
    }

    [Fact]
    public async Task DescarteDeRascunhoIncompletoPreservaDadosEPermiteNovaTentativa()
    {
        var relogio = new RelogioPropostas(); var (id, codigo, _) = await RascunhoAsync();
        var criacao = await CriacaoDaOrigemAsync(id); criacao.PropostaOrigemPublicId = null;
        var vida = await fixture.DbContext.ApoliceVidas.SingleAsync(v => v.Propostas.Any(p => p.PublicId == id)); vida.Ativo = false;
        await fixture.DbContext.SaveChangesAsync(); fixture.DbContext.ChangeTracker.Clear();
        await Descartador(codigo, relogio).Handle(new(id, new(0, "CONDIÇÕES NÃO APROVADAS")), default); fixture.DbContext.ChangeTracker.Clear();
        var p = await fixture.DbContext.Proposta.Include(p => p.Status).SingleAsync(p => p.PublicId == id);
        Assert.Equal("DESCARTADA", p.Status.Codigo); Assert.Equal(relogio.Agora.UtcDateTime, p.DescartadaEm); Assert.Null(p.ConfirmadaEm); Assert.Null(p.CanceladaEm); Assert.Null(p.DataInicioVigencia); Assert.Null(p.DataFimVigencia); Assert.False(p.Vigente);
        var evento = await fixture.DbContext.PropostaHistoricos.SingleAsync(h => h.PropostaNovaId == p.Id); Assert.Equal(p.Id, evento.PropostaAnteriorId); Assert.Equal("Condições Não Aprovadas", evento.Observacao);
        await Assert.ThrowsAsync<ValidacaoException>(() => Descartador(codigo, relogio).Handle(new(id, new(1, "Repetição")), default));
        fixture.DbContext.ChangeTracker.Clear(); vida = await fixture.DbContext.ApoliceVidas.SingleAsync(v => v.Id == vida.Id); vida.Ativo = true; await fixture.DbContext.SaveChangesAsync(); fixture.DbContext.ChangeTracker.Clear();
        var nova = await CriarHandler(codigo, new RegistradorAuditoria(fixture.Auditoria)).Handle(criacao, default);
        await using var leitura = LeituraNovaConexao(); Assert.Equal(2, (await leitura.Proposta.SingleAsync(p => p.PublicId == nova)).Versao);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task NaoDescartaPropostaConfirmadaMesmoAntesDaVigencia(bool vigente)
    {
        var relogio = new RelogioPropostas(); var (id, codigo, _) = await ProntaConfirmarAsync();
        await Confirmador(codigo, relogio).Handle(new(id, new(1, new(2026, 11, vigente ? 12 : 13))), default); fixture.DbContext.ChangeTracker.Clear();
        var antes = await AssinaturaPropostaAsync(id);
        await Assert.ThrowsAsync<ValidacaoException>(() => Descartador(codigo, relogio).Handle(new(id, new(2, "Descarte Indevido")), default));
        Assert.Equal(antes, await AssinaturaPropostaAsync(id));
    }

    [Fact]
    public async Task DescartarSucessoraLiberaOutraVersaoPreservandoAVigente()
    {
        var relogio = new RelogioPropostas(); var (origem, nova, codigo) = await SubstituicaoAsync(relogio);
        var criacao = await CriacaoDaOrigemAsync(origem); var antes = await AssinaturaPropostaAsync(origem);
        await Descartador(codigo, relogio).Handle(new(nova, new(1, "Condições Não Aprovadas")), default); fixture.DbContext.ChangeTracker.Clear();
        var tentativa = await CriarHandler(codigo, new RegistradorAuditoria(fixture.Auditoria)).Handle(criacao, default);
        Assert.Equal(antes, await AssinaturaPropostaAsync(origem));
        await using var leitura = LeituraNovaConexao(); var p = await leitura.Proposta.SingleAsync(p => p.PublicId == tentativa);
        Assert.Equal(3, p.Versao); Assert.Equal((await leitura.Proposta.SingleAsync(p => p.PublicId == origem)).Id, p.PropostaOrigemId);
    }

    [Theory]
    [InlineData("imediato")] [InlineData("futuro")] [InlineData("efetivar")] [InlineData("descartar")]
    public async Task FalhaDeAuditoriaReverteEncerramentoCompleto(string tipo)
    {
        var relogio = new RelogioPropostas(); Guid id; string codigo; var revisao = 2;
        if (tipo == "descartar") { var r = await RascunhoAsync(); id = r.Id; codigo = r.Codigo; revisao = 0; }
        else (id, codigo) = await VigenteParaEncerrarAsync(relogio);
        if (tipo == "efetivar") { await SolicitadorCancelamento(codigo, relogio).Handle(new(id, new(2, new(2026, 11, 13), "Agendar")), default); fixture.DbContext.ChangeTracker.Clear(); relogio.Agora = new(2026, 11, 13, 12, 0, 0, TimeSpan.Zero); revisao = 3; }
        var antes = await AssinaturaPropostaAsync(id); var historicos = await fixture.DbContext.PropostaHistoricos.CountAsync(h => h.PropostaNova.PublicId == id);
        var auditor = new Mock<IRegistradorAuditoria>(); auditor.Setup(a => a.RegistrarAsync(It.IsAny<WebApolice.Auditoria.Domain.RegistroAuditoria>(), It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("Falha artificial"));
        Task Executar() => tipo switch {
            "descartar" => Descartador(codigo, relogio, auditor.Object).Handle(new(id, new(revisao, "Descarte")), default),
            "efetivar" => EfetivadorCancelamento(codigo, relogio, auditor.Object).Handle(new(id, revisao), default),
            _ => SolicitadorCancelamento(codigo, relogio, auditor.Object).Handle(new(id, new(revisao, new(2026, 11, tipo == "futuro" ? 13 : 12), "Encerramento")), default)
        };
        await Assert.ThrowsAsync<InvalidOperationException>(Executar);
        Assert.Equal(antes, await AssinaturaPropostaAsync(id));
        await using var leitura = LeituraNovaConexao(); Assert.Equal(historicos, await leitura.PropostaHistoricos.CountAsync(h => h.PropostaNova.PublicId == id));
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task SerializaSolicitacaoDeCancelamentoECriacaoDeSucessora(bool cancelarPrimeiro)
    {
        var relogio = new RelogioPropostas(); var (id, codigo) = await VigenteParaEncerrarAsync(relogio); var criacao = await CriacaoDaOrigemAsync(id);
        var chegou = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously); var liberar = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var auditor = new Mock<IRegistradorAuditoria>();
        auditor.Setup(a => a.RegistrarAsync(It.IsAny<WebApolice.Auditoria.Domain.RegistroAuditoria>(), It.IsAny<CancellationToken>()))
            .Returns(async (WebApolice.Auditoria.Domain.RegistroAuditoria registro, CancellationToken ct) => { chegou.TrySetResult(); await liberar.Task.WaitAsync(ct); await new RegistradorAuditoria(fixture.Auditoria).RegistrarAsync(registro, ct); });
        Task primeira = cancelarPrimeiro ? SolicitadorCancelamento(codigo, relogio, auditor.Object).Handle(new(id, new(2, new(2026, 11, 13), "Encerramento")), default) : CriarHandler(codigo, auditor.Object).Handle(criacao, default);
        Task? segunda = null; await using var services = ServicosAgendamento(); await using var scope = services.CreateAsyncScope(); var sp = scope.ServiceProvider;
        try {
            await chegou.Task.WaitAsync(TimeSpan.FromSeconds(10));
            var operacao = new OperacaoPropostas(sp.GetRequiredService<SeguroDbContext>(), new OperacaoAuditada(sp.GetRequiredService<AuditoriaDbContext>(), sp.GetRequiredService<IRegistradorAuditoria>(), new Mock<IContextoAuditoria>().Object));
            segunda = cancelarPrimeiro ? new CriarPropostaHandler(sp.GetRequiredService<IPropostasRepository>(), sp.GetRequiredService<IClientesPropostas>(), ConfigConfirmacao(codigo), operacao, sp.GetRequiredService<ICadastroSeguroQueries>()).Handle(criacao, default)
                : new SolicitarCancelamentoVigenciaHandler(sp.GetRequiredService<IPropostasRepository>(), operacao, ConfigConfirmacao(codigo), relogio).Handle(new(id, new(2, new(2026, 11, 13), "Encerramento")), default);
            Assert.NotSame(segunda, await Task.WhenAny(segunda, Task.Delay(150)));
        } finally { liberar.TrySetResult(); await primeira; }
        Assert.NotNull(segunda); await Assert.ThrowsAsync<ValidacaoException>(async () => await segunda);
        await using var leitura = LeituraNovaConexao(); var p = await leitura.Proposta.SingleAsync(p => p.PublicId == id);
        Assert.True(p.Vigente); Assert.Equal(cancelarPrimeiro, p.CancelamentoSolicitadoEm.HasValue);
        Assert.Equal(cancelarPrimeiro ? 0 : 1, await leitura.Proposta.CountAsync(v => v.PropostaOrigemId == p.Id));
    }
}
