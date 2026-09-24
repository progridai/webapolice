using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WebApolice.Integration.Tests.Setup;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;
using Xunit;

namespace WebApolice.Integration.Tests.Modulos.Seguro;

[Collection("SeguroTests")]
public class ApoliceHierarquiaTests : IClassFixture<SeguroIntegrationTestFixture>
{
    private readonly SeguroIntegrationTestFixture _fixture;

    public ApoliceHierarquiaTests(SeguroIntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Permitir_VidaNaApolice_ESubestipulanteNaApolice()
    {
        var apolice = new ApoliceModel
        {
            PublicId = Guid.NewGuid(),
            EstipulanteId = 2,
            SeguradoraId = 2,
            Nome = "Grupo Teste 2",
            DataInicioVigencia = new DateOnly(2025, 1, 1)
        };
        _fixture.DbContext.Apolices.Add(apolice);
        await _fixture.DbContext.SaveChangesAsync();

        var subestipulante = new ApoliceSubestipulanteModel
        {
            ApoliceId = apolice.Id,
            SubestipulanteId = 10
        };
        _fixture.DbContext.ApoliceSubestipulantes.Add(subestipulante);
        await _fixture.DbContext.SaveChangesAsync();

        var vidaDireta = new ApoliceVidaModel
        {
            PublicId = Guid.NewGuid(),
            ApoliceId = apolice.Id,
            ClienteId = 100,
            Ativo = true
        };
        _fixture.DbContext.ApoliceVidas.Add(vidaDireta);


        var result = await _fixture.DbContext.SaveChangesAsync();
        result.Should().BeGreaterThan(0);
    }
}
