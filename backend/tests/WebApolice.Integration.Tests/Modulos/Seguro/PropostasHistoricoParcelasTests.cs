using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas.ListarParcelas;
using WebApolice.Modulos.Seguro.Domain.Modelos;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Integration.Tests.Modulos.Seguro;

public partial class PropostasPostgresTests
{
    [Fact]
    public async Task HistoricoReuneVersoesDaVidaSemMisturarOutraVidaOuRecalcularValores()
    {
        var (origem, nova, _) = await SubstituicaoAsync(new RelogioPropostas());
        var outra = await RascunhoAsync();
        var anterior = await fixture.DbContext.Proposta.SingleAsync(p => p.PublicId == origem);
        var sucessora = await fixture.DbContext.Proposta.SingleAsync(p => p.PublicId == nova);
        var externa = await fixture.DbContext.Proposta.SingleAsync(p => p.PublicId == outra.Id);
        fixture.DbContext.PropostaMovimentos.AddRange(
            new PropostaMovimento { PublicId = Guid.NewGuid(), PropostaId = anterior.Id, Parcela = 1, CompetenciaInt = 202611, PremioLiquido = 5, Iof = 0.13m, PremioTotal = 5.13m },
            new PropostaMovimento { PublicId = Guid.NewGuid(), PropostaId = sucessora.Id, Parcela = 2, CompetenciaInt = 202612, PremioTotal = -1.23m },
            new PropostaMovimento { PublicId = Guid.NewGuid(), PropostaId = externa.Id, Parcela = 90, PremioTotal = 100 });
        await fixture.DbContext.SaveChangesAsync(); fixture.DbContext.ChangeTracker.Clear();
        var auditorias = await fixture.Auditoria.RegistrosAuditoria.CountAsync();
        var handler = new ListarParcelasHandler(RepositorySimulacao());
        var primeira = await handler.Handle(new(nova, 1, 1), default);
        Assert.Equal(2, primeira.TotalCount); var p = Assert.Single(primeira.Items);
        Assert.Equal(origem, p.PropostaPublicId); Assert.Equal(1, p.Parcela); Assert.Equal("5.13", p.PremioTotal);
        var segunda = await handler.Handle(new(origem, 2, 1), default);
        p = Assert.Single(segunda.Items); Assert.Equal(nova, p.PropostaPublicId); Assert.Equal(2, p.Posicao); Assert.Equal("-1.23", p.PremioTotal);
        Assert.Equal(auditorias, await fixture.Auditoria.RegistrosAuditoria.CountAsync());
        anterior = await fixture.DbContext.Proposta.SingleAsync(p => p.PublicId == origem); anterior.VisivelOperacional = false;
        await fixture.DbContext.SaveChangesAsync(); fixture.DbContext.ChangeTracker.Clear();
        Assert.Equal(1, (await handler.Handle(new(nova), default)).TotalCount);
        await Assert.ThrowsAsync<ValidacaoException>(() => handler.Handle(new(origem), default));
    }

    [Fact]
    public async Task HistoricoPreservaLegadoIncompletoEValoresGrandesSemInventarCampos()
    {
        var r = await RascunhoAsync(); var p = await fixture.DbContext.Proposta.SingleAsync(p => p.PublicId == r.Id);
        fixture.DbContext.PropostaMovimentos.Add(new PropostaMovimento { LegadoId = checked((int)p.Id) + 900000, PropostaId = p.Id, PremioTotal = 9_999_999_999_999_999.99m });
        await fixture.DbContext.SaveChangesAsync(); fixture.DbContext.ChangeTracker.Clear();
        var item = Assert.Single((await new ListarParcelasHandler(RepositorySimulacao()).Handle(new(r.Id), default)).Items);
        Assert.True(item.Legado); Assert.Null(item.PublicId); Assert.Null(item.CompetenciaInt); Assert.Null(item.Parcela);
        Assert.Null(item.PremioLiquido); Assert.Null(item.Iof); Assert.Equal("9999999999999999.99", item.PremioTotal);
    }

    [Theory]
    [InlineData(0, 20)][InlineData(1, 0)][InlineData(1, 101)][InlineData(int.MaxValue, 100)]
    public async Task HistoricoRecusaPaginacaoInvalida(int pagina, int tamanho) =>
        await Assert.ThrowsAsync<ValidacaoException>(() => new ListarParcelasHandler(RepositorySimulacao()).Handle(new(Guid.NewGuid(), pagina, tamanho), default));

    [Fact]
    public async Task SemVidaNaoAgrupaPropostasPelaPessoaOuCliente()
    {
        var uma = Guid.NewGuid(); var outra = Guid.NewGuid();
        await fixture.DbContext.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO seguro.proposta(public_id,pessoa_id,cliente_id,cliente_vinculo_id,estipulante_id,status_id,versao,vigente,visivel_operacional,legado_id)
              SELECT {uma},10,20,30,40,id,1,false,true,950001 FROM seguro.proposta_status WHERE codigo='CONFIRMADA';
            INSERT INTO seguro.proposta(public_id,pessoa_id,cliente_id,cliente_vinculo_id,estipulante_id,status_id,versao,vigente,visivel_operacional,legado_id)
              SELECT {outra},10,20,30,40,id,1,false,true,950002 FROM seguro.proposta_status WHERE codigo='CONFIRMADA';
            """);
        var p1 = await fixture.DbContext.Proposta.SingleAsync(p => p.PublicId == uma);
        var p2 = await fixture.DbContext.Proposta.SingleAsync(p => p.PublicId == outra);
        fixture.DbContext.PropostaMovimentos.AddRange(new PropostaMovimento { PropostaId = p1.Id, LegadoId = 950001, PremioTotal = 1 },
            new PropostaMovimento { PropostaId = p2.Id, LegadoId = 950002, PremioTotal = 2 });
        await fixture.DbContext.SaveChangesAsync(); fixture.DbContext.ChangeTracker.Clear();
        var resultado = await new ListarParcelasHandler(RepositorySimulacao()).Handle(new(uma), default);
        Assert.Equal(1, resultado.TotalCount); Assert.Equal(uma, Assert.Single(resultado.Items).PropostaPublicId);
    }
}
