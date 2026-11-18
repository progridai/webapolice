using Microsoft.EntityFrameworkCore;
using Moq;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Queries;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas.SimularValorMensal;
using WebApolice.Modulos.Seguro.Domain.Modelos;
using WebApolice.Modulos.Seguro.Infrastructure.Persistence.Repositories;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Integration.Tests.Modulos.Seguro;

public partial class PropostasPostgresTests
{
    private PropostasRepository RepositorySimulacao() => new(fixture.DbContext, new ClientesPropostas(fixture.Cadastro));
    private async Task<Guid> RamoSimulacaoAsync(Guid proposta, decimal? aliquota = 2.5m)
    {
        var db = fixture.DbContext;
        var p = await db.Proposta.SingleAsync(p => p.PublicId == proposta);
        var ramo = new RamoModel { PublicId = Guid.NewGuid(), Codigo = Guid.NewGuid().ToString("N"), Nome = "Ramo da Simulação", Ativo = true };
        db.Ramos.Add(ramo); await db.SaveChangesAsync();
        var vinculo = new ApoliceRamoModel { PublicId = Guid.NewGuid(), ApoliceId = p.ApoliceId!.Value, RamoId = ramo.Id, IofPercentual = aliquota, Ativo = true };
        db.ApoliceRamos.Add(vinculo); await db.SaveChangesAsync(); db.ChangeTracker.Clear();
        return vinculo.PublicId;
    }

    [Theory]
    [InlineData(false, "0.00", "5.00", "0.13", "5.13")]
    [InlineData(true, "2.00", "7.00", "0.18", "7.18")]
    public async Task SimulaPremiosRegistradosSemGravarFinanceiroOuAuditoria(bool inclui, string conjuge, string liquido, string iof, string total)
    {
        var (id, codigo, cobertura) = await RascunhoAsync();
        await Preenchedor(codigo).Handle(new(id, Dados(cobertura, premio: 5m) with { IncluiConjuge = inclui, Coberturas = [new(cobertura, 5m, 2m, "Ajuste Justificado")] }), default);
        var ramo = await RamoSimulacaoAsync(id);
        // Alterar os preços atuais do Módulo não deve reprecificar a proposta.
        var c = await fixture.DbContext.PropostaCoberturas.Include(c => c.ApoliceModuloCobertura).SingleAsync(c => c.PublicId == cobertura);
        c.ApoliceModuloCobertura!.PremioTitular = 999m;
        await fixture.DbContext.SaveChangesAsync(); fixture.DbContext.ChangeTracker.Clear();
        var antes = await fixture.DbContext.Database.SqlQueryRaw<string>("SELECT md5(to_jsonb(p)::text) AS \"Value\" FROM seguro.proposta p WHERE public_id={0}", id).SingleAsync();
        var auditorias = await fixture.Auditoria.RegistrosAuditoria.CountAsync();
        var r = await new SimularValorMensalHandler(RepositorySimulacao(), new RelogioPropostas()).Handle(new(id, new(1, ramo)), default);
        Assert.Equal("5.00", r.LiquidoTitular); Assert.Equal(conjuge, r.LiquidoConjuge);
        Assert.Equal(liquido, r.LiquidoTotal); Assert.Equal(iof, r.Iof); Assert.Equal(total, r.TotalMensal);
        Assert.True(Assert.Single(r.Coberturas).PremioAjustado); Assert.Equal(inclui, r.IncluiConjuge);
        Assert.Equal(2.5m, r.Ramo.IofPercentual); Assert.Equal(1, r.RevisaoPreenchimento);
        Assert.Equal(antes, await fixture.DbContext.Database.SqlQueryRaw<string>("SELECT md5(to_jsonb(p)::text) AS \"Value\" FROM seguro.proposta p WHERE public_id={0}", id).SingleAsync());
        Assert.Equal(auditorias, await fixture.Auditoria.RegistrosAuditoria.CountAsync());
        Assert.False(await fixture.DbContext.PropostaMovimentos.AnyAsync(m => m.Proposta!.PublicId == id));
        Assert.DoesNotContain(fixture.DbContext.ChangeTracker.Entries(), e => e.State != EntityState.Unchanged);
    }

    [Theory]
    [InlineData("sem_escolha")][InlineData("revisao")][InlineData("sem_ramo")][InlineData("outro_ramo")]
    [InlineData("inativo")][InlineData("excluido")][InlineData("cadastro_inativo")][InlineData("sem_iof")][InlineData("premio_ausente")]
    public async Task RecusaSimulacaoIncompletaOuDesatualizada(string caso)
    {
        var (id, codigo, cobertura) = await RascunhoAsync();
        await Preenchedor(codigo).Handle(new(id, Dados(cobertura) with { IncluiConjuge = caso == "sem_escolha" ? null : true }), default);
        var ramo = await RamoSimulacaoAsync(id, caso == "sem_iof" ? null : 2.5m);
        var v = await fixture.DbContext.ApoliceRamos.Include(v => v.Ramo).SingleAsync(v => v.PublicId == ramo);
        if (caso == "inativo") v.Ativo = false;
        if (caso == "excluido") v.DeletedAt = DateTimeOffset.UtcNow;
        if (caso == "cadastro_inativo") v.Ramo!.Ativo = false;
        if (caso == "premio_ausente") (await fixture.DbContext.PropostaCoberturas.SingleAsync(c => c.PublicId == cobertura)).PremioConjuge = null;
        await fixture.DbContext.SaveChangesAsync(); fixture.DbContext.ChangeTracker.Clear();
        var selecao = ramo;
        if (caso == "outro_ramo")
        {
            var outra = await RascunhoAsync(); selecao = await RamoSimulacaoAsync(outra.Id);
        }
        var opcoes = await new OpcoesSimulacaoMensalHandler(RepositorySimulacao()).Handle(new(id), default);
        if (caso is "inativo" or "excluido" or "cadastro_inativo") Assert.DoesNotContain(opcoes, r => r.PublicId == ramo);
        if (caso == "outro_ramo") Assert.DoesNotContain(opcoes, r => r.PublicId == selecao);
        var dados = new SimulacaoMensalDados(caso == "revisao" ? 0 : 1, caso == "sem_ramo" ? null : selecao);
        await Assert.ThrowsAsync<ValidacaoException>(() => new SimularValorMensalHandler(RepositorySimulacao(), TimeProvider.System).Handle(new(id, dados), default));
    }

    [Fact]
    public async Task PayloadAntigoPreservaEscolhaAnteriorSemInferirNao()
    {
        var (id, codigo, cobertura) = await RascunhoAsync();
        await Preenchedor(codigo).Handle(new(id, Dados(cobertura) with { IncluiConjuge = true }), default);
        fixture.DbContext.ChangeTracker.Clear();
        await Preenchedor(codigo).Handle(new(id, Dados(cobertura, 1) with { IncluiConjuge = null }), default);
        fixture.DbContext.ChangeTracker.Clear();
        var p = await fixture.DbContext.Proposta.SingleAsync(p => p.PublicId == id);
        Assert.True(p.IncluiConjuge); Assert.Equal(2, p.RevisaoPreenchimento);
    }

    [Fact]
    public async Task ConjugeNaoContratadoPermitePremioConjugeAusenteEZeroIofExplicito()
    {
        var (id, codigo, cobertura) = await RascunhoAsync();
        await Preenchedor(codigo).Handle(new(id, Dados(cobertura) with { Coberturas = [new(cobertura, 10m, null, "Sem Cônjuge")] }), default);
        var ramo = await RamoSimulacaoAsync(id, 0);
        var r = await new SimularValorMensalHandler(RepositorySimulacao(), TimeProvider.System).Handle(new(id, new(1, ramo)), default);
        Assert.Equal("0.00", r.Iof); Assert.Equal("0.00", r.LiquidoConjuge); Assert.Equal("10.00", r.TotalMensal);
    }

    [Fact]
    public async Task ConfirmacaoExigeEscolhaMasAtivacaoAnteriorSemEscolhaContinuaPermitida()
    {
        var (id, codigo, _) = await ProntaConfirmarAsync();
        var p = await fixture.DbContext.Proposta.SingleAsync(p => p.PublicId == id); p.IncluiConjuge = null;
        await fixture.DbContext.SaveChangesAsync(); fixture.DbContext.ChangeTracker.Clear();
        var erro = await Assert.ThrowsAsync<ValidacaoException>(() => Confirmador(codigo, new RelogioPropostas()).Handle(new(id, new(1, new(2026, 11, 13))), default));
        Assert.Contains("cônjuge", erro.Message);
        p = await fixture.DbContext.Proposta.SingleAsync(p => p.PublicId == id); p.IncluiConjuge = false;
        await fixture.DbContext.SaveChangesAsync(); fixture.DbContext.ChangeTracker.Clear();
        await Confirmador(codigo, new RelogioPropostas()).Handle(new(id, new(1, new(2026, 11, 13))), default);
        p = await fixture.DbContext.Proposta.SingleAsync(p => p.PublicId == id); p.IncluiConjuge = null;
        await fixture.DbContext.SaveChangesAsync(); fixture.DbContext.ChangeTracker.Clear();
        Assert.True((await Efetivador(codigo, new RelogioPropostas { Agora = new(2026, 11, 13, 12, 0, 0, TimeSpan.Zero) }).Handle(new(id, 2), default)).Ativada);
    }

    [Fact]
    public async Task SimulacaoRecusaAlteracaoConcorrenteSemSalvar()
    {
        var p = new Propostum { PublicId = Guid.NewGuid(), ApoliceId = 1, ApoliceVidaId = 2, VisivelOperacional = true,
            Status = new PropostaStatus { VisivelOperacional = true }, IncluiConjuge = false };
        p.PropostaItems.Add(new PropostaItem { Id = 10, Ativo = true, ApoliceModuloPlanoId = 20 });
        p.PropostaCoberturas.Add(new PropostaCobertura { Ativo = true, PropostaItemId = 10, CoberturaId = 30, PremioTitular = 5 });
        var ramo = new ApoliceRamoModel { PublicId = Guid.NewGuid(), Ramo = new RamoModel(), IofPercentual = 2.5m };
        var repo = new Mock<IPropostasRepository>(MockBehavior.Strict);
        repo.Setup(r => r.ObterParaComparacaoAsync(p.PublicId, default)).ReturnsAsync(p);
        repo.Setup(r => r.ObterRamosSimulacaoAsync(1, default)).ReturnsAsync([ramo]);
        repo.Setup(r => r.ObterRevisaoAtualAsync(p.PublicId, default)).ReturnsAsync(1);
        await Assert.ThrowsAsync<ValidacaoException>(() => new SimularValorMensalHandler(repo.Object, TimeProvider.System).Handle(new(p.PublicId, new(0, ramo.PublicId)), default));
    }

    [Theory]
    [InlineData(false)][InlineData(true)][InlineData(null)]
    public async Task NovasVersoesEReinclusoesCopiamEscolhaSemModificarOrigem(bool? escolha)
    {
        var relogio = new RelogioPropostas();
        var (id, codigo) = await VigenteParaEncerrarAsync(relogio);
        var p = await fixture.DbContext.Proposta.Include(p => p.Apolice).Include(p => p.ApoliceVida).SingleAsync(p => p.PublicId == id);
        p.IncluiConjuge = escolha; await fixture.DbContext.SaveChangesAsync(); fixture.DbContext.ChangeTracker.Clear();
        var antes = await AssinaturaFamiliaReinclusaoAsync(id);
        var nova = await CriarHandler(codigo, new WebApolice.Auditoria.Infrastructure.RegistradorAuditoria(fixture.Auditoria)).Handle(new()
        {
            ClienteId = p.ClienteId, ApoliceId = p.Apolice!.PublicId, ApoliceVidaId = p.ApoliceVida!.PublicId, PropostaOrigemPublicId = id
        }, default);
        fixture.DbContext.ChangeTracker.Clear();
        Assert.Equal(escolha, (await fixture.DbContext.Proposta.SingleAsync(p => p.PublicId == nova)).IncluiConjuge);
        Assert.Equal(antes, await AssinaturaFamiliaReinclusaoAsync(id));
        // Outra família permite verificar as duas fontes de reinclusão sem propostas concorrentes.
        foreach (var fonte in new[] { "ANTERIOR", "MODULO" })
        {
            var encerrada = await EncerradaParaReincluirAsync(relogio);
            p = await fixture.DbContext.Proposta.SingleAsync(p => p.PublicId == encerrada.Id);
            p.IncluiConjuge = escolha; await fixture.DbContext.SaveChangesAsync(); fixture.DbContext.ChangeTracker.Clear();
            antes = await AssinaturaFamiliaReinclusaoAsync(encerrada.Id);
            var reinclusao = await PreparadorReinclusao(encerrada.Codigo).Handle(new(encerrada.Id, new(3, fonte)), default);
            fixture.DbContext.ChangeTracker.Clear();
            Assert.Equal(escolha, (await fixture.DbContext.Proposta.SingleAsync(p => p.PublicId == reinclusao)).IncluiConjuge);
            Assert.Equal(antes, await AssinaturaFamiliaReinclusaoAsync(encerrada.Id));
        }
    }

    [Fact]
    public async Task SomaSomenteCoberturasAtivasEAplicaIofSobreTotal()
    {
        var (id, codigo, cobertura) = await RascunhoAsync();
        await Preenchedor(codigo).Handle(new(id, Dados(cobertura, premio: 5m)), default);
        var ramo = await RamoSimulacaoAsync(id);
        // Reutiliza o agregado real com uma segunda cobertura ativa e uma condição histórica.
        var p = (await RepositorySimulacao().ObterParaComparacaoAsync(id, default))!;
        p.PropostaCoberturas.Add(new PropostaCobertura { PublicId = Guid.NewGuid(), Ativo = true, PropostaItemId = p.PropostaItems.Single().Id, CoberturaId = 987654, PremioTitular = 5m });
        p.PropostaCoberturas.Add(new PropostaCobertura { PublicId = Guid.NewGuid(), Ativo = false, PremioTitular = 999m });
        var repo = new Mock<IPropostasRepository>(MockBehavior.Strict);
        repo.Setup(r => r.ObterParaComparacaoAsync(id, default)).ReturnsAsync(p);
        var ramos = await RepositorySimulacao().ObterRamosSimulacaoAsync(p.ApoliceId!.Value, default);
        repo.Setup(r => r.ObterRamosSimulacaoAsync(p.ApoliceId.Value, default)).ReturnsAsync(ramos);
        repo.Setup(r => r.ObterRevisaoAtualAsync(id, default)).ReturnsAsync(1);
        var r = await new SimularValorMensalHandler(repo.Object, TimeProvider.System).Handle(new(id, new(1, ramo)), default);
        Assert.Equal(2, r.Coberturas.Count); Assert.Equal("10.00", r.LiquidoTotal); Assert.Equal("0.25", r.Iof); Assert.Equal("10.25", r.TotalMensal);
    }
}

public sealed class CalculoMensalPropostaTests
{
    [Theory]
    [InlineData("5", "2.5", "0.13")][InlineData("0", "2.5", "0.00")]
    [InlineData("10", "0", "0.00")][InlineData("1", "100", "1.00")]
    public void ArredondaImpostoUmaVez(string liquido, string percentual, string esperado) =>
        Assert.Equal(esperado, CalculoMensalProposta.Moeda(CalculoMensalProposta.Imposto(decimal.Parse(liquido, System.Globalization.CultureInfo.InvariantCulture), decimal.Parse(percentual, System.Globalization.CultureInfo.InvariantCulture))));
    [Theory]
    [InlineData(null)][InlineData("-1")][InlineData("100.0001")][InlineData("0.00001")]
    public void RecusaAliquotaInvalida(string? aliquota) => Assert.Throws<ValidacaoException>(() => CalculoMensalProposta.Imposto(5, aliquota == null ? null : decimal.Parse(aliquota, System.Globalization.CultureInfo.InvariantCulture)));
    [Fact]
    public void PreservaCentavosGrandesERecusaTotalAcimaDoLimite()
    {
        Assert.Equal("9999999999999999.99", CalculoMensalProposta.Moeda(CalculoMensalProposta.Premio(CalculoMensalProposta.Maximo)));
        Assert.Throws<ValidacaoException>(() => CalculoMensalProposta.Somar(CalculoMensalProposta.Maximo, 0.01m));
    }
}
