using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using WebApolice.Auditoria.Contracts;
using WebApolice.Auditoria.Infrastructure;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Queries;
using WebApolice.Modulos.Cadastro.Contracts;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas.ObterProposta;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas.ReincluirProposta;
using WebApolice.Modulos.Seguro.Infrastructure.Persistence;
using WebApolice.Modulos.Seguro.Infrastructure.Persistence.Repositories;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Propostas.CriarProposta;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Integration.Tests.Modulos.Seguro;

public partial class PropostasPostgresTests
{
    private PrepararReinclusaoHandler PreparadorReinclusao(string codigo, IRegistradorAuditoria? auditor = null)
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<IRequest<Guid>>(), It.IsAny<CancellationToken>()))
            .Returns((IRequest<Guid> comando, CancellationToken ct) => CriarHandler(codigo, auditor ?? new RegistradorAuditoria(fixture.Auditoria)).Handle(Assert.IsType<CriarPropostaCommand>(comando), ct));
        return new(new PropostasRepository(fixture.DbContext, new ClientesPropostas(fixture.Cadastro)), mediator.Object);
    }
    private async Task<(Guid Id, string Codigo)> EncerradaParaReincluirAsync(RelogioPropostas relogio)
    {
        var r = await VigenteParaEncerrarAsync(relogio);
        await SolicitadorCancelamento(r.Codigo, relogio).Handle(new(r.Id, new(2, new(2026, 11, 10), "Encerramento")), default);
        fixture.DbContext.ChangeTracker.Clear(); return r;
    }
    private async Task<string> AssinaturaFamiliaReinclusaoAsync(Guid id) => await fixture.DbContext.Database.SqlQuery<string>($"""
        SELECT md5(jsonb_build_array(to_jsonb(p),
          (SELECT jsonb_agg(to_jsonb(i) ORDER BY i.id) FROM seguro.proposta_item i WHERE i.proposta_id=p.id),
          (SELECT jsonb_agg(to_jsonb(c) ORDER BY c.id) FROM seguro.proposta_cobertura c WHERE c.proposta_id=p.id),
          (SELECT jsonb_agg(to_jsonb(b) ORDER BY b.id) FROM seguro.proposta_beneficiario b WHERE b.proposta_id=p.id))::text) AS "Value"
        FROM seguro.proposta p WHERE p.public_id={id}
        """).SingleAsync();

    [Theory]
    [InlineData("ANTERIOR")] [InlineData("MODULO")]
    public async Task PreparaReinclusaoComEscolhaExplicitaPreservandoEncerradaECopiandoBeneficiarios(string escolha)
    {
        var relogio = new RelogioPropostas(); var (id, codigo) = await EncerradaParaReincluirAsync(relogio);
        var modulo = await fixture.DbContext.ApoliceModuloCoberturas.SingleAsync(c => fixture.DbContext.PropostaCoberturas.Any(p => p.ApoliceModuloCoberturaId == c.Id && p.Proposta.PublicId == id));
        modulo.PremioTitular = 40m; await fixture.DbContext.SaveChangesAsync(); fixture.DbContext.ChangeTracker.Clear();
        var antes = await AssinaturaFamiliaReinclusaoAsync(id);
        var nova = await PreparadorReinclusao(codigo).Handle(new(id, new(3, escolha)), default); fixture.DbContext.ChangeTracker.Clear();
        var dto = await new ObterPropostaHandler(new PropostasRepository(fixture.DbContext, new ClientesPropostas(fixture.Cadastro)), ConfigConfirmacao(codigo), relogio).Handle(new(nova), default);
        Assert.NotNull(dto); Assert.True(dto.Reinclusao); Assert.True(dto.PodeEditarRascunho); Assert.True(dto.PodeConfirmar); Assert.False(dto.Vigente);
        Assert.Equal(2, dto.Versao); Assert.Equal(id, dto.PropostaOrigemPublicId); Assert.Equal(new DateOnly(2026, 11, 10), dto.DataFimVigenciaOrigem);
        Assert.Null(dto.ConfirmadaEm); Assert.Null(dto.CanceladaEm); Assert.Null(dto.DataInicioVigencia); Assert.Null(dto.DataFimVigencia);
        Assert.Null(dto.PremioLiquido); Assert.Null(dto.ValorParcela);
        var cobertura = Assert.Single(dto.Coberturas); Assert.Equal(escolha == "ANTERIOR" ? 15.12m : 40m, cobertura.PremioTitular);
        Assert.Equal(escolha == "ANTERIOR", cobertura.PremioAjustado);
        Assert.Equal(escolha == "ANTERIOR" ? 12.34m : null, cobertura.PremioTitularInicial);
        Assert.Equal(escolha == "ANTERIOR" ? "Ajuste de Proteção" : null, cobertura.JustificativaAjustePremio);
        var b = Assert.Single(dto.Beneficiarios); Assert.Equal("Maria d’Ávila", b.Nome); Assert.Equal(100m, b.PercentualParticipacao); Assert.True(b.PodeAlterar);
        var anterior = await new PropostasRepository(fixture.DbContext, new ClientesPropostas(fixture.Cadastro)).ObterDetalheAsync(id, default);
        Assert.NotNull(anterior); Assert.NotEqual(anterior.Beneficiarios.Single().PublicId, b.PublicId);
        Assert.NotEqual(anterior.Coberturas.Single().PublicId, cobertura.PublicId); Assert.NotEqual(anterior.Itens.Single().PublicId, dto.Itens.Single().PublicId);
        Assert.Equal(antes, await AssinaturaFamiliaReinclusaoAsync(id));
        Assert.Contains(dto.Historico, h => h.Motivo == "Preparação de Reinclusão" && h.Observacao != null && h.Observacao.Contains(escolha == "ANTERIOR" ? "Proposta Anterior" : "Atuais do Módulo"));
        Assert.False((await new ObterPropostaHandler(new PropostasRepository(fixture.DbContext, new ClientesPropostas(fixture.Cadastro)), ConfigConfirmacao(codigo), relogio).Handle(new(id), default))!.PodePrepararReinclusao);
    }

    [Theory]
    [InlineData(10)] [InlineData(11)] [InlineData(12)] [InlineData(13)]
    public async Task ConfirmaReinclusaoComInicioInformadoSemReabrirOuReescreverOrigem(int dia)
    {
        var relogio = new RelogioPropostas(); var (id, codigo) = await EncerradaParaReincluirAsync(relogio); var antes = await AssinaturaFamiliaReinclusaoAsync(id);
        var nova = await PreparadorReinclusao(codigo).Handle(new(id, new(3, "ANTERIOR")), default); fixture.DbContext.ChangeTracker.Clear();
        var resultado = await Confirmador(codigo, relogio).Handle(new(nova, new(0, new(2026, 11, dia))), default);
        Assert.Equal(dia > 12, resultado.Agendada); Assert.Equal(dia <= 12, resultado.Vigente);
        if (dia > 12)
        {
            fixture.DbContext.ChangeTracker.Clear(); relogio.Agora = relogio.Agora.AddDays(2);
            await using var services = ServicosAgendamento();
            var processador = new ProcessadorVigenciasPropostas(services.GetRequiredService<IServiceScopeFactory>(), ConfigConfirmacao(codigo), relogio, NullLogger<ProcessadorVigenciasPropostas>.Instance);
            await Task.WhenAll(processador.ProcessarAsync(default), processador.ProcessarAsync(default)); await processador.ProcessarAsync(default);
        }
        Assert.Equal(antes, await AssinaturaFamiliaReinclusaoAsync(id));
        await using var leitura = LeituraNovaConexao(); var p = await leitura.Proposta.SingleAsync(p => p.PublicId == nova);
        Assert.True(p.Vigente); Assert.Equal(new DateOnly(2026, 11, dia), p.DataInicioVigencia); Assert.Null(p.DataFimVigencia);
        Assert.False(await leitura.PropostaMovimentos.AnyAsync(m => m.PropostaId == p.Id));
        Assert.Equal(1, await leitura.PropostaHistoricos.CountAsync(h => h.PropostaNovaId == p.Id && h.Motivo == "Efetivação de Reinclusão"));
        Assert.False(await leitura.PropostaHistoricos.AnyAsync(h => h.PropostaNovaId == p.Id && h.Motivo == "Substituição de Proposta"));
    }

    [Theory]
    [InlineData("sobreposicao")] [InlineData("fora_periodo")] [InlineData("vida_inativa")] [InlineData("cobertura_inativa")] [InlineData("beneficiarios")]
    public async Task ConfirmaReinclusaoSomenteComDatasEContextoElegiveis(string tipo)
    {
        var relogio = new RelogioPropostas(); var (id, codigo) = await EncerradaParaReincluirAsync(relogio);
        var nova = await PreparadorReinclusao(codigo).Handle(new(id, new(3, "MODULO")), default); fixture.DbContext.ChangeTracker.Clear();
        var p = await fixture.DbContext.Proposta.Include(p => p.Apolice).Include(p => p.ApoliceVida).Include(p => p.PropostaBeneficiarios).Include(p => p.PropostaCoberturas).ThenInclude(c => c.Cobertura).SingleAsync(p => p.PublicId == nova);
        if (tipo == "fora_periodo") p.Apolice!.DataFimVigencia = new(2026, 11, 11);
        if (tipo == "vida_inativa") p.ApoliceVida!.Ativo = false;
        if (tipo == "cobertura_inativa") p.PropostaCoberturas.Single().Cobertura!.Ativo = false;
        if (tipo == "beneficiarios") p.PropostaBeneficiarios.Single().PercentualParticipacao = 50;
        await fixture.DbContext.SaveChangesAsync(); fixture.DbContext.ChangeTracker.Clear(); var antes = await AssinaturaFamiliaReinclusaoAsync(id);
        await Assert.ThrowsAsync<ValidacaoException>(() => Confirmador(codigo, relogio).Handle(new(nova, new(0, new(2026, 11, tipo == "sobreposicao" ? 9 : 12))), default));
        Assert.Equal(antes, await AssinaturaFamiliaReinclusaoAsync(id));
        await using var leitura = LeituraNovaConexao(); Assert.False((await leitura.Proposta.SingleAsync(p => p.PublicId == nova)).Vigente);
    }

    [Theory]
    [InlineData("rascunho")] [InlineData("vigente")] [InlineData("agendada")] [InlineData("cancelada_sem_vigencia")]
    [InlineData("legado")] [InlineData("oculta")] [InlineData("excluida")] [InlineData("revisao")] [InlineData("sem_revisao")] [InlineData("escolha")]
    public async Task RecusaPreparacaoDeReinclusaoInvalidaSemNovaVersao(string tipo)
    {
        var relogio = new RelogioPropostas(); Guid id; string codigo; int? revisao = 3;
        if (tipo is "rascunho" or "agendada" or "cancelada_sem_vigencia")
        {
            var r = await ProntaConfirmarAsync(); id = r.Id; codigo = r.Codigo; revisao = 1;
            if (tipo != "rascunho") { await Confirmador(codigo, relogio).Handle(new(id, new(1, new(2026, 11, 13))), default); revisao = 2; }
            if (tipo == "cancelada_sem_vigencia") { fixture.DbContext.ChangeTracker.Clear(); await Cancelador(codigo, relogio).Handle(new(id, new(2, "Desistência")), default); revisao = 3; }
        }
        else if (tipo == "vigente") { (id, codigo) = await VigenteParaEncerrarAsync(relogio); revisao = 2; }
        else (id, codigo) = await EncerradaParaReincluirAsync(relogio);
        fixture.DbContext.ChangeTracker.Clear(); var p = await fixture.DbContext.Proposta.SingleAsync(p => p.PublicId == id);
        if (tipo == "legado") p.LegadoId = 724809;
        if (tipo == "oculta") p.VisivelOperacional = false;
        if (tipo == "excluida") p.DeletedAt = relogio.Agora.UtcDateTime;
        if (tipo == "revisao") revisao = 2;
        if (tipo == "sem_revisao") revisao = null;
        await fixture.DbContext.SaveChangesAsync(); fixture.DbContext.ChangeTracker.Clear(); var antes = await AssinaturaFamiliaReinclusaoAsync(id);
        var quantidade = await fixture.DbContext.Proposta.CountAsync(v => v.ApoliceVidaId == p.ApoliceVidaId);
        await Assert.ThrowsAsync<ValidacaoException>(() => PreparadorReinclusao(codigo).Handle(new(id, new(revisao, tipo == "escolha" ? "anterior" : "ANTERIOR")), default));
        Assert.Equal(antes, await AssinaturaFamiliaReinclusaoAsync(id)); Assert.Equal(quantidade, await fixture.DbContext.Proposta.CountAsync(v => v.ApoliceVidaId == p.ApoliceVidaId));
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task NovaTentativaDeReinclusaoAposDescarteOuCancelamentoNaoReutilizaVersao(bool agendada)
    {
        var relogio = new RelogioPropostas(); var (id, codigo) = await EncerradaParaReincluirAsync(relogio); var antes = await AssinaturaFamiliaReinclusaoAsync(id);
        var tentativa = await PreparadorReinclusao(codigo).Handle(new(id, new(3, "ANTERIOR")), default); fixture.DbContext.ChangeTracker.Clear();
        await Assert.ThrowsAsync<ValidacaoException>(() => PreparadorReinclusao(codigo).Handle(new(id, new(3, "MODULO")), default)); fixture.DbContext.ChangeTracker.Clear();
        if (agendada)
        {
            await Confirmador(codigo, relogio).Handle(new(tentativa, new(0, new(2026, 11, 13))), default); fixture.DbContext.ChangeTracker.Clear();
            await Reagendador(codigo, relogio).Handle(new(tentativa, new(1, new(2026, 11, 14), "Adiar Reinclusão")), default); fixture.DbContext.ChangeTracker.Clear();
            await Cancelador(codigo, relogio).Handle(new(tentativa, new(2, "Desistência")), default);
        }
        else await Descartador(codigo, relogio).Handle(new(tentativa, new(0, "Revisão")), default);
        fixture.DbContext.ChangeTracker.Clear();
        var nova = await PreparadorReinclusao(codigo).Handle(new(id, new(3, "MODULO")), default); fixture.DbContext.ChangeTracker.Clear();
        await Confirmador(codigo, relogio).Handle(new(nova, new(0, new(2026, 11, 12))), default);
        Assert.Equal(antes, await AssinaturaFamiliaReinclusaoAsync(id)); await using var leitura = LeituraNovaConexao();
        Assert.Equal(3, (await leitura.Proposta.SingleAsync(p => p.PublicId == nova)).Versao);
    }

    [Fact]
    public async Task CopiaSomenteBeneficiariosNativosSemEvidenciasEPermiteCompletarRascunho()
    {
        var relogio = new RelogioPropostas(); var (id, codigo) = await EncerradaParaReincluirAsync(relogio);
        var b = await fixture.DbContext.PropostaBeneficiarios.SingleAsync(b => b.Proposta.PublicId == id); b.CpfOriginal = "EVIDENCIA";
        await fixture.DbContext.SaveChangesAsync(); fixture.DbContext.ChangeTracker.Clear(); var antes = await AssinaturaFamiliaReinclusaoAsync(id);
        var nova = await PreparadorReinclusao(codigo).Handle(new(id, new(3, "MODULO")), default);
        Assert.False(await fixture.DbContext.PropostaBeneficiarios.AnyAsync(b => b.Proposta.PublicId == nova));
        Assert.Equal(antes, await AssinaturaFamiliaReinclusaoAsync(id)); fixture.DbContext.ChangeTracker.Clear();
        await Assert.ThrowsAsync<ValidacaoException>(() => Confirmador(codigo, relogio).Handle(new(nova, new(0, new(2026, 11, 12))), default));
    }

    [Theory]
    [InlineData("preparar")] [InlineData("confirmar")] [InlineData("efetivar")]
    public async Task FalhaDeAuditoriaReverteReinclusaoInteiraEPreservaEncerrada(string etapa)
    {
        var relogio = new RelogioPropostas(); var (id, codigo) = await EncerradaParaReincluirAsync(relogio); var antes = await AssinaturaFamiliaReinclusaoAsync(id);
        var auditor = new Mock<IRegistradorAuditoria>(); auditor.Setup(a => a.RegistrarAsync(It.IsAny<WebApolice.Auditoria.Domain.RegistroAuditoria>(), It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("Falha artificial"));
        if (etapa == "preparar")
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() => PreparadorReinclusao(codigo, auditor.Object).Handle(new(id, new(3, "ANTERIOR")), default));
            await using var leitura = LeituraNovaConexao(); var origemId = (await leitura.Proposta.SingleAsync(p => p.PublicId == id)).Id;
            Assert.False(await leitura.Proposta.AnyAsync(p => p.PropostaOrigemId == origemId));
        }
        else
        {
            var nova = await PreparadorReinclusao(codigo).Handle(new(id, new(3, "ANTERIOR")), default); fixture.DbContext.ChangeTracker.Clear();
            if (etapa == "efetivar") { await Confirmador(codigo, relogio).Handle(new(nova, new(0, new(2026, 11, 13))), default); fixture.DbContext.ChangeTracker.Clear(); relogio.Agora = relogio.Agora.AddDays(1); }
            var novaAntes = await AssinaturaFamiliaReinclusaoAsync(nova);
            Task Executar() => etapa == "confirmar" ? Confirmador(codigo, relogio, auditor.Object).Handle(new(nova, new(0, new(2026, 11, 12))), default) : Efetivador(codigo, relogio, auditor.Object).Handle(new(nova, 1), default);
            await Assert.ThrowsAsync<InvalidOperationException>(Executar); Assert.Equal(novaAntes, await AssinaturaFamiliaReinclusaoAsync(nova));
        }
        Assert.Equal(antes, await AssinaturaFamiliaReinclusaoAsync(id));
    }

    [Fact]
    public async Task CondicoesAnterioresIndisponiveisNaoSaoSubstituidasSilenciosamente()
    {
        var relogio = new RelogioPropostas(); var (id, codigo) = await EncerradaParaReincluirAsync(relogio);
        var c = await fixture.DbContext.ApoliceModuloCoberturas.SingleAsync(c => fixture.DbContext.PropostaCoberturas.Any(p => p.ApoliceModuloCoberturaId == c.Id && p.Proposta.PublicId == id)); c.Ativo = false;
        await fixture.DbContext.SaveChangesAsync(); fixture.DbContext.ChangeTracker.Clear(); var antes = await AssinaturaFamiliaReinclusaoAsync(id);
        await Assert.ThrowsAsync<ValidacaoException>(() => PreparadorReinclusao(codigo).Handle(new(id, new(3, "ANTERIOR")), default));
        Assert.Equal(antes, await AssinaturaFamiliaReinclusaoAsync(id));
    }

    [Fact]
    public async Task PreparacoesConcorrentesDeReinclusaoCriamSomenteUmaTentativa()
    {
        var relogio = new RelogioPropostas(); var (id, codigo) = await EncerradaParaReincluirAsync(relogio);
        var chegou = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously); var liberar = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var auditor = new Mock<IRegistradorAuditoria>();
        auditor.Setup(a => a.RegistrarAsync(It.IsAny<WebApolice.Auditoria.Domain.RegistroAuditoria>(), It.IsAny<CancellationToken>()))
            .Returns(async (WebApolice.Auditoria.Domain.RegistroAuditoria registro, CancellationToken ct) => { chegou.TrySetResult(); await liberar.Task.WaitAsync(ct); await new RegistradorAuditoria(fixture.Auditoria).RegistrarAsync(registro, ct); });
        var primeira = PreparadorReinclusao(codigo, auditor.Object).Handle(new(id, new(3, "ANTERIOR")), default);
        Task<Guid>? segunda = null; await using var services = ServicosAgendamento(); await using var scope = services.CreateAsyncScope(); var sp = scope.ServiceProvider;
        try
        {
            await chegou.Task.WaitAsync(TimeSpan.FromSeconds(10));
            var operacao = new OperacaoPropostas(sp.GetRequiredService<SeguroDbContext>(), new OperacaoAuditada(sp.GetRequiredService<AuditoriaDbContext>(), sp.GetRequiredService<IRegistradorAuditoria>(), new Mock<IContextoAuditoria>().Object));
            var criador = new CriarPropostaHandler(sp.GetRequiredService<IPropostasRepository>(), sp.GetRequiredService<IClientesPropostas>(), ConfigConfirmacao(codigo), operacao, sp.GetRequiredService<WebApolice.Modulos.Cadastro.Contracts.ICadastroSeguroQueries>());
            var mediator = new Mock<IMediator>();
            mediator.Setup(m => m.Send(It.IsAny<IRequest<Guid>>(), It.IsAny<CancellationToken>()))
                .Returns((IRequest<Guid> comando, CancellationToken ct) => criador.Handle(Assert.IsType<CriarPropostaCommand>(comando), ct));
            segunda = new PrepararReinclusaoHandler(sp.GetRequiredService<IPropostasRepository>(), mediator.Object).Handle(new(id, new(3, "MODULO")), default);
            Assert.NotSame(segunda, await Task.WhenAny(segunda, Task.Delay(150)));
        }
        finally { liberar.TrySetResult(); await primeira; }
        Assert.NotNull(segunda); await Assert.ThrowsAsync<ValidacaoException>(async () => await segunda);
        await using var leitura = LeituraNovaConexao(); var origem = await leitura.Proposta.SingleAsync(p => p.PublicId == id);
        Assert.Equal(1, await leitura.Proposta.CountAsync(p => p.PropostaOrigemId == origem.Id)); Assert.False(origem.Vigente); Assert.Equal(3, origem.RevisaoPreenchimento);
    }

    [Fact]
    public async Task ApenasUltimaPropostaCanceladaComVigenciaPermiteNovaReinclusao()
    {
        var relogio = new RelogioPropostas(); var (id, codigo) = await EncerradaParaReincluirAsync(relogio);
        var nova = await PreparadorReinclusao(codigo).Handle(new(id, new(3, "ANTERIOR")), default); fixture.DbContext.ChangeTracker.Clear();
        await Confirmador(codigo, relogio).Handle(new(nova, new(0, new(2026, 11, 11))), default); fixture.DbContext.ChangeTracker.Clear();
        await SolicitadorCancelamento(codigo, relogio).Handle(new(nova, new(1, new(2026, 11, 12), "Novo Encerramento")), default); fixture.DbContext.ChangeTracker.Clear();
        await Assert.ThrowsAsync<ValidacaoException>(() => PreparadorReinclusao(codigo).Handle(new(id, new(3, "ANTERIOR")), default)); fixture.DbContext.ChangeTracker.Clear();
        var terceira = await PreparadorReinclusao(codigo).Handle(new(nova, new(2, "MODULO")), default);
        await using var leitura = LeituraNovaConexao(); Assert.Equal(3, (await leitura.Proposta.SingleAsync(p => p.PublicId == terceira)).Versao);
    }
}
