using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using WebApolice.Auditoria.Contracts;
using WebApolice.Auditoria.Infrastructure;
using WebApolice.Modulos.Cadastro.Domain.Modelos;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Queries;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas.RevisarCondicoes;
using WebApolice.Modulos.Seguro.Domain.Modelos;
using WebApolice.Modulos.Seguro.Infrastructure.Persistence.Repositories;
using WebApolice.Modulos.Seguro.Infrastructure.Persistence;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Cadastro.Contracts;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Integration.Tests.Modulos.Seguro;

public partial class PropostasPostgresTests
{
    private CompararCondicoesHandler Comparador(string codigo, TimeProvider relogio) => new(
        new PropostasRepository(fixture.DbContext, new ClientesPropostas(fixture.Cadastro)), new ClientesPropostas(fixture.Cadastro),
        Setup.SeguroPersistenciaTestes.ConsultasCadastro(fixture.DbContext), ConfigConfirmacao(codigo), relogio);
    private AplicarCondicoesHandler Revisor(string codigo, TimeProvider relogio, IRegistradorAuditoria? auditor = null) => new(
        new PropostasRepository(fixture.DbContext, new ClientesPropostas(fixture.Cadastro)), Operacao(auditor ?? new RegistradorAuditoria(fixture.Auditoria)),
        new ClientesPropostas(fixture.Cadastro), Setup.SeguroPersistenciaTestes.ConsultasCadastro(fixture.DbContext), ConfigConfirmacao(codigo), relogio);
    private async Task<(SelecaoCondicoes Dados, Guid Id, string Codigo)> PrepararRevisaoAsync()
    {
        var (id, codigo, _) = await ProntaConfirmarAsync();
        var p = await fixture.DbContext.Proposta.Include(p => p.ApoliceModulo).Include(p => p.ApoliceSubgrupo).SingleAsync(p => p.PublicId == id);
        var plano = await fixture.DbContext.ApoliceModuloPlanos.Include(p => p.Coberturas).SingleAsync(m => m.ApoliceModuloId == p.ApoliceModuloId);
        plano.Nome = "PLANO REVISADO"; plano.Coberturas.Single(c => c.Ativo).PremioTitular = 40m;
        await fixture.DbContext.SaveChangesAsync();
        var dados = new SelecaoCondicoes(p.RevisaoPreenchimento, p.ApoliceModulo!.PublicId, p.ApoliceSubgrupo!.PublicId);
        fixture.DbContext.ChangeTracker.Clear(); return (dados, id, codigo);
    }
    private static AplicarCondicoesCommand Aplicacao(Guid id, SelecaoCondicoes dados, ComparacaoCondicoes comparacao, string escolha = "MODULO") =>
        new(id, new(dados.RevisaoEsperada, dados.ModuloPublicId, dados.SubgrupoPublicId, comparacao.Comparacao, escolha, "REVISÃO SOLICITADA"));

    [Theory]
    [InlineData("MANTER", false)] [InlineData("MODULO", false)]
    [InlineData("MANTER", true)] [InlineData("MODULO", true)]
    public async Task RevisaPreservandoCondicoesAnterioresBeneficiariosEAgendamento(string escolha, bool agendada)
    {
        var (dados, id, codigo) = await PrepararRevisaoAsync(); var relogio = new RelogioPropostas();
        if (agendada)
        {
            await Confirmador(codigo, relogio).Handle(new(id, new(1, new(2026, 11, 20))), default);
            dados = dados with { RevisaoEsperada = 2 }; fixture.DbContext.ChangeTracker.Clear();
        }
        var p = await fixture.DbContext.Proposta.AsNoTracking().SingleAsync(p => p.PublicId == id);
        var vidaAntes = await fixture.DbContext.Database.SqlQuery<string>($"SELECT md5(to_jsonb(v)::text) AS \"Value\" FROM seguro.apolice_vida v WHERE id={p.ApoliceVidaId}").SingleAsync();
        var beneficiarioAntes = await fixture.DbContext.PropostaBeneficiarios.AsNoTracking().SingleAsync(b => b.PropostaId == p.Id);
        var anterior = await fixture.DbContext.PropostaCoberturas.AsNoTracking().SingleAsync(c => c.PropostaId == p.Id);
        var comparacao = await Comparador(codigo, relogio).Handle(new(id, dados), default);
        Assert.Equal(15.12m, Assert.Single(comparacao.Atual.Coberturas).PremioTitular);
        Assert.Equal(40m, Assert.Single(comparacao.Modulo.Coberturas).PremioTitular);
        Assert.Equal(15.12m, Assert.Single(comparacao.MantendoAjustes.Coberturas).PremioTitular);
        Assert.Equal(p.RevisaoPreenchimento, (await fixture.DbContext.Proposta.AsNoTracking().SingleAsync(p => p.PublicId == id)).RevisaoPreenchimento);
        await Revisor(codigo, relogio).Handle(Aplicacao(id, dados, comparacao, escolha), default);
        fixture.DbContext.ChangeTracker.Clear();
        var depois = await fixture.DbContext.Proposta.Include(p => p.PropostaCoberturas).Include(p => p.PropostaBeneficiarios).SingleAsync(p => p.PublicId == id);
        Assert.True(depois.CondicoesProprias); Assert.Equal(dados.RevisaoEsperada + 1, depois.RevisaoPreenchimento);
        Assert.Equal(agendada, depois.ConfirmadaEm != null); Assert.False(depois.Vigente);
        Assert.Equal(agendada ? new DateOnly(2026, 11, 20) : null, depois.DataInicioVigencia);
        var ativa = Assert.Single(depois.PropostaCoberturas, c => c.Ativo);
        Assert.NotEqual(anterior.PublicId, ativa.PublicId); Assert.Equal(escolha == "MANTER" ? 15.12m : 40m, ativa.PremioTitular);
        Assert.Equal(escolha == "MANTER", ativa.PremioAjustado);
        Assert.Equal(escolha == "MANTER" ? 12.34m : null, ativa.PremioTitularInicial);
        var inativa = Assert.Single(depois.PropostaCoberturas, c => !c.Ativo);
        Assert.Equal(anterior.PublicId, inativa.PublicId); Assert.Equal(anterior.PremioTitular, inativa.PremioTitular);
        Assert.Equal(anterior.JustificativaAjustePremio, inativa.JustificativaAjustePremio);
        Assert.Equal(beneficiarioAntes.PublicId, Assert.Single(depois.PropostaBeneficiarios).PublicId);
        Assert.Equal(vidaAntes, await fixture.DbContext.Database.SqlQuery<string>($"SELECT md5(to_jsonb(v)::text) AS \"Value\" FROM seguro.apolice_vida v WHERE id={p.ApoliceVidaId}").SingleAsync());
        Assert.Contains(await fixture.DbContext.PropostaHistoricos.Where(h => h.PropostaNovaId == p.Id).ToListAsync(), h => h.Motivo == "Revisão de Condições");
        if (agendada)
        {
            relogio.Agora = new(2026, 11, 20, 12, 0, 0, TimeSpan.Zero);
            var resultado = await Efetivador(codigo, relogio).Handle(new(id, depois.RevisaoPreenchimento), default);
            Assert.True(resultado.Ativada);
        }
        else Assert.True((await Confirmador(codigo, relogio).Handle(new(id, new(depois.RevisaoPreenchimento, new(2026, 11, 12))), default)).Vigente);
    }

    [Theory]
    [InlineData("configuracao")] [InlineData("proposta")] [InlineData("token")]
    public async Task RecusaComparacaoAntigaOuAdulteradaSemEscrita(string mudanca)
    {
        var (dados, id, codigo) = await PrepararRevisaoAsync(); var relogio = new RelogioPropostas();
        var comparacao = await Comparador(codigo, relogio).Handle(new(id, dados), default);
        if (mudanca == "configuracao")
        {
            var cobertura = await fixture.DbContext.ApoliceModuloCoberturas.SingleAsync(c => c.PublicId == comparacao.Modulo.Coberturas.Single().VinculoPublicId);
            cobertura.PremioTitular++; await fixture.DbContext.SaveChangesAsync();
        }
        if (mudanca == "proposta") await fixture.DbContext.Database.ExecuteSqlInterpolatedAsync($"UPDATE seguro.proposta SET revisao_preenchimento=revisao_preenchimento+1 WHERE public_id={id}");
        var comando = Aplicacao(id, dados, comparacao);
        if (mudanca == "token") comando = comando with { Dados = comando.Dados with { Comparacao = "INVALIDA" } };
        fixture.DbContext.ChangeTracker.Clear();
        var antes = await AssinaturaFamiliaReinclusaoAsync(id);
        await Assert.ThrowsAsync<ValidacaoException>(() => Revisor(codigo, relogio).Handle(comando, default));
        fixture.DbContext.ChangeTracker.Clear(); Assert.Equal(antes, await AssinaturaFamiliaReinclusaoAsync(id));
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task TrocaModuloESubgrupoSomenteNaPropostaEMantemCorrespondenciaPorCobertura(bool agendar)
    {
        var (dados, id, codigo) = await PrepararRevisaoAsync(); var relogio = new RelogioPropostas();
        var p = await fixture.DbContext.Proposta.SingleAsync(p => p.PublicId == id);
        var global = new ModuloModel { PublicId = Guid.NewGuid(), Nome = "Outro Módulo", Ativo = true };
        fixture.Cadastro.Add(global); await fixture.Cadastro.SaveChangesAsync();
        var modulo = new ApoliceModuloModel { PublicId = Guid.NewGuid(), ApoliceId = p.ApoliceId!.Value, ModuloId = global.Id, Ativo = true };
        var subgrupo = new ApoliceSubgrupoModel { PublicId = Guid.NewGuid(), ApoliceId = p.ApoliceId.Value, Nome = "Outro Subgrupo", Ativo = true };
        var plano = new ApoliceModuloPlanoModel { PublicId = Guid.NewGuid(), ApoliceModulo = modulo, Nome = "Outro Plano", Ativo = true };
        var coberturaId = await fixture.DbContext.PropostaCoberturas.Where(c => c.PropostaId == p.Id).Select(c => c.CoberturaId).SingleAsync();
        plano.Coberturas.Add(new() { PublicId = Guid.NewGuid(), CoberturaId = coberturaId!.Value, PremioTitular = 50, Plano = plano });
        fixture.DbContext.AddRange(plano, subgrupo); await fixture.DbContext.SaveChangesAsync(); fixture.DbContext.ChangeTracker.Clear();
        dados = dados with { ModuloPublicId = modulo.PublicId, SubgrupoPublicId = subgrupo.PublicId };
        var comparacao = await Comparador(codigo, relogio).Handle(new(id, dados), default);
        await Revisor(codigo, relogio).Handle(Aplicacao(id, dados, comparacao, "MANTER"), default); fixture.DbContext.ChangeTracker.Clear();
        var depois = await fixture.DbContext.Proposta.Include(p => p.ApoliceVida).SingleAsync(p => p.PublicId == id);
        Assert.Equal(modulo.Id, depois.ApoliceModuloId); Assert.Equal(subgrupo.Id, depois.ApoliceSubgrupoId);
        Assert.NotEqual(depois.ApoliceModuloId, depois.ApoliceVida!.ApoliceModuloId);
        Assert.NotEqual(depois.ApoliceSubgrupoId, depois.ApoliceVida.ApoliceSubgrupoId);
        var confirmada = await Confirmador(codigo, relogio).Handle(new(id, new(2, new(2026, 11, agendar ? 20 : 12))), default);
        Assert.Equal(!agendar, confirmada.Vigente);
        if (agendar)
        {
            fixture.DbContext.ChangeTracker.Clear(); relogio.Agora = new(2026, 11, 20, 12, 0, 0, TimeSpan.Zero);
            Assert.True((await Efetivador(codigo, relogio).Handle(new(id, 3), default)).Ativada);
        }
    }

    [Fact]
    public async Task SerializaAplicacoesConcorrentesSemDuplicarCondicoesOuHistorico()
    {
        var (dados, id, codigo) = await PrepararRevisaoAsync(); var relogio = new RelogioPropostas();
        var comparacao = await Comparador(codigo, relogio).Handle(new(id, dados), default);
        var chegou = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var liberar = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var auditor = new Mock<IRegistradorAuditoria>();
        auditor.Setup(a => a.RegistrarAsync(It.IsAny<WebApolice.Auditoria.Domain.RegistroAuditoria>(), It.IsAny<CancellationToken>()))
            .Returns(async (WebApolice.Auditoria.Domain.RegistroAuditoria registro, CancellationToken ct) => {
                chegou.TrySetResult(); await liberar.Task.WaitAsync(ct); await new RegistradorAuditoria(fixture.Auditoria).RegistrarAsync(registro, ct);
            });
        var primeira = Revisor(codigo, relogio, auditor.Object).Handle(Aplicacao(id, dados, comparacao), default);
        Task? segunda = null;
        await using var services = ServicosAgendamento(); await using var scope = services.CreateAsyncScope(); var sp = scope.ServiceProvider;
        try
        {
            await chegou.Task.WaitAsync(TimeSpan.FromSeconds(10));
            var operacao = new OperacaoPropostas(sp.GetRequiredService<SeguroDbContext>(), new OperacaoAuditada(sp.GetRequiredService<AuditoriaDbContext>(), sp.GetRequiredService<IRegistradorAuditoria>(), new Mock<IContextoAuditoria>().Object));
            segunda = new AplicarCondicoesHandler(sp.GetRequiredService<IPropostasRepository>(), operacao, sp.GetRequiredService<IClientesPropostas>(),
                sp.GetRequiredService<ICadastroSeguroQueries>(), ConfigConfirmacao(codigo), relogio).Handle(Aplicacao(id, dados, comparacao), default);
            Assert.NotSame(segunda, await Task.WhenAny(segunda, Task.Delay(150)));
        }
        finally { liberar.TrySetResult(); await primeira; }
        Assert.NotNull(segunda); await Assert.ThrowsAsync<ValidacaoException>(async () => await segunda);
        await using var leitura = LeituraNovaConexao(); var p = await leitura.Proposta.SingleAsync(p => p.PublicId == id);
        Assert.Equal(2, p.RevisaoPreenchimento);
        Assert.Equal(1, await leitura.PropostaCoberturas.CountAsync(c => c.PropostaId == p.Id && c.Ativo));
        Assert.Equal(1, await leitura.PropostaHistoricos.CountAsync(h => h.PropostaNovaId == p.Id && h.Motivo == "Revisão de Condições"));
    }

    [Fact]
    public async Task PreservaHistoricoEmRevisoesSucessivasERecusaVinculosDeOutraApolice()
    {
        var (dados, id, codigo) = await PrepararRevisaoAsync(); var relogio = new RelogioPropostas();
        var primeira = await Comparador(codigo, relogio).Handle(new(id, dados), default);
        await Revisor(codigo, relogio).Handle(Aplicacao(id, dados, primeira), default); fixture.DbContext.ChangeTracker.Clear();
        var plano = await fixture.DbContext.ApoliceModuloPlanos.SingleAsync(p => p.PublicId == primeira.Modulo.Plano!.PublicId);
        plano.Nome = "Segunda Revisão"; await fixture.DbContext.SaveChangesAsync(); fixture.DbContext.ChangeTracker.Clear();
        dados = dados with { RevisaoEsperada = 2 };
        var segunda = await Comparador(codigo, relogio).Handle(new(id, dados), default);
        await Revisor(codigo, relogio).Handle(Aplicacao(id, dados, segunda), default); fixture.DbContext.ChangeTracker.Clear();
        var p = await fixture.DbContext.Proposta.Include(p => p.PropostaCoberturas).SingleAsync(p => p.PublicId == id);
        Assert.Equal(3, p.RevisaoPreenchimento); Assert.Equal(2, p.PropostaCoberturas.Count(c => !c.Ativo));
        var outro = await PrepararModuloAsync();
        var outroModulo = await fixture.DbContext.ApoliceModulos.SingleAsync(m => m.Id == outro.Plano.ApoliceModuloId);
        fixture.DbContext.ChangeTracker.Clear();
        await Assert.ThrowsAsync<ValidacaoException>(() => Comparador(codigo, relogio).Handle(new(id, dados with { RevisaoEsperada = 3, ModuloPublicId = outroModulo.PublicId }), default));
    }

    [Fact]
    public async Task ReverteTudoQuandoAuditoriaFalha()
    {
        var (dados, id, codigo) = await PrepararRevisaoAsync(); var relogio = new RelogioPropostas();
        var comparacao = await Comparador(codigo, relogio).Handle(new(id, dados), default);
        var antes = await AssinaturaFamiliaReinclusaoAsync(id);
        var auditor = new Moq.Mock<IRegistradorAuditoria>();
        auditor.Setup(a => a.RegistrarAsync(Moq.It.IsAny<WebApolice.Auditoria.Domain.RegistroAuditoria>(), Moq.It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("Falha de Auditoria"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => Revisor(codigo, relogio, auditor.Object).Handle(Aplicacao(id, dados, comparacao), default));
        fixture.DbContext.ChangeTracker.Clear(); Assert.Equal(antes, await AssinaturaFamiliaReinclusaoAsync(id));
    }

    [Theory]
    [InlineData("hoje")] [InlineData("vigente")] [InlineData("legado")] [InlineData("oculta")] [InlineData("cancelada")]
    public async Task RecusaSituacaoNaoPermitida(string tipo)
    {
        var (dados, id, codigo) = await PrepararRevisaoAsync(); var relogio = new RelogioPropostas();
        if (tipo is "hoje" or "vigente")
        {
            await Confirmador(codigo, relogio).Handle(new(id, new(1, new(2026, 11, tipo == "hoje" ? 13 : 12))), default);
            if (tipo == "hoje") relogio.Agora = new(2026, 11, 13, 12, 0, 0, TimeSpan.Zero);
        }
        else
        {
            var p = await fixture.DbContext.Proposta.SingleAsync(p => p.PublicId == id);
            if (tipo == "legado") p.LegadoId = 987654;
            if (tipo == "oculta") p.VisivelOperacional = false;
            if (tipo == "cancelada") p.CanceladaEm = relogio.GetUtcNow().UtcDateTime;
            await fixture.DbContext.SaveChangesAsync();
        }
        fixture.DbContext.ChangeTracker.Clear();
        await Assert.ThrowsAsync<ValidacaoException>(() => Comparador(codigo, relogio).Handle(new(id, dados), default));
    }
}
