using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WebApolice.Integration.Tests.Setup;
using WebApolice.Modulos.Seguro.Domain.Modelos;
using Xunit;

namespace WebApolice.Integration.Tests.Modulos.Seguro;

[Collection("SeguroTests")]
public class ApoliceUniversoPermitidoTests : IClassFixture<SeguroIntegrationTestFixture>
{
    private readonly SeguroIntegrationTestFixture _fixture;

    public ApoliceUniversoPermitidoTests(SeguroIntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Permitir_CriarUniversoCorreto()
    {
        var apolice = new ApoliceModel 
        { 
            PublicId = Guid.NewGuid(), 
            EstipulanteId = 3, 
            SeguradoraId = 3, 
            Nome = "Universo Teste",
            DataInicioVigencia = new DateOnly(2025, 1, 1)
        };
        _fixture.DbContext.Apolices.Add(apolice);
        await _fixture.DbContext.SaveChangesAsync();

        var produtoGlobal = new Produto { Nome = "Produto da Fixture", Ativo = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var planoGlobal = new Plano { PublicId = Guid.NewGuid(), Nome = "Plano da Fixture", Ativo = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var coberturaGlobal = new Cobertura { PublicId = Guid.NewGuid(), Nome = "Cobertura da Fixture", Ativo = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        _fixture.DbContext.AddRange(produtoGlobal, planoGlobal, coberturaGlobal);
        await _fixture.DbContext.SaveChangesAsync();

        var produto = new ApoliceProdutoModel { ApoliceId = apolice.Id, ProdutoId = produtoGlobal.Id };
        _fixture.DbContext.ApoliceProdutos.Add(produto);
        await _fixture.DbContext.SaveChangesAsync();

        var plano = new ApolicePlanoModel { ApoliceProdutoId = produto.Id, PlanoId = planoGlobal.Id };
        _fixture.DbContext.ApolicePlanos.Add(plano);
        await _fixture.DbContext.SaveChangesAsync();

        var cobertura = new ApoliceCoberturaModel { ApolicePlanoId = plano.Id, CoberturaId = coberturaGlobal.Id };
        _fixture.DbContext.ApoliceCoberturas.Add(cobertura);
        
        var result = await _fixture.DbContext.SaveChangesAsync();
        result.Should().BeGreaterThan(0);
    }
}
