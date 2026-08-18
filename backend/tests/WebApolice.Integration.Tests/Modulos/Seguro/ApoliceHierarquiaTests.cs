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
    public async Task NaoPermitir_ModuloSemSubestipulante()
    {
        var apolice = new ApoliceModel
        {
            PublicId = Guid.NewGuid(),
            EstipulanteId = 1,
            SeguradoraId = 1,
            Nome = "Grupo Teste",
            DataInicioVigencia = new DateOnly(2025, 1, 1)
        };
        _fixture.DbContext.Apolices.Add(apolice);
        await _fixture.DbContext.SaveChangesAsync();

        var vinculoModulo = new ApoliceSubestipulanteModuloModel
        {
            ModuloId = 1,
            // ApoliceSubestipulanteId propositalmente intocado/zero (FK violation no DB real)
        };
        _fixture.DbContext.ApoliceSubestipulanteModulos.Add(vinculoModulo);

        var act = async () => await _fixture.DbContext.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task Permitir_VidaDiretamenteNaApolice_OuComSubestipulante_OuComModulo()
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

        var modulo = new ApoliceSubestipulanteModuloModel
        {
            ApoliceSubestipulanteId = subestipulante.Id,
            ModuloId = 5,
            Ativo = true
        };
        _fixture.DbContext.ApoliceSubestipulanteModulos.Add(modulo);
        await _fixture.DbContext.SaveChangesAsync();

        var vidaDireta = new ApoliceVidaModel
        {
            PublicId = Guid.NewGuid(),
            ApoliceId = apolice.Id,
            ClienteId = 100,
            Ativo = true
        };
        _fixture.DbContext.ApoliceVidas.Add(vidaDireta);

        var vidaSub = new ApoliceVidaModel
        {
            PublicId = Guid.NewGuid(),
            ApoliceId = apolice.Id,
            ApoliceSubestipulanteId = subestipulante.Id,
            ClienteId = 101,
            Ativo = true
        };
        _fixture.DbContext.ApoliceVidas.Add(vidaSub);

        var vidaModulo = new ApoliceVidaModel
        {
            PublicId = Guid.NewGuid(),
            ApoliceId = apolice.Id,
            ApoliceSubestipulanteId = subestipulante.Id,
            ApoliceSubestipulanteModuloId = modulo.Id,
            ClienteId = 102,
            Ativo = true
        };
        _fixture.DbContext.ApoliceVidas.Add(vidaModulo);

        var result = await _fixture.DbContext.SaveChangesAsync();
        result.Should().BeGreaterThan(0);
    }
}
