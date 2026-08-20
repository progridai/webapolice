using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WebApolice.Integration.Tests.Setup;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.VincularRamo;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.AtualizarRamo;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.InativarRamo;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;
using WebApolice.SharedKernel.Application.Exceptions;
using Xunit;

namespace WebApolice.Integration.Tests.Modulos.Seguro;

[Collection("SeguroTests")]
public class ApoliceRamoTests : IClassFixture<SeguroIntegrationTestFixture>
{
    private readonly SeguroIntegrationTestFixture _fixture;

    public ApoliceRamoTests(SeguroIntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task VincularRamo_Valido_DeveSalvarERegistrarHistorico()
    {
        var apolicePublicId = Guid.NewGuid();
        var apolice = new ApoliceModel 
        { 
            PublicId = apolicePublicId, 
            EstipulanteId = 1, 
            SeguradoraId = 1, 
            Nome = "Apolice Teste Ramo",
            DataInicioVigencia = new DateOnly(2025, 1, 1)
        };
        _fixture.DbContext.Apolices.Add(apolice);

        var ramoPublicId = Guid.NewGuid();
        var ramo = new RamoModel
        {
            PublicId = ramoPublicId,
            Codigo = "XYZ",
            Nome = "Ramo XYZ",
            Ativo = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        _fixture.DbContext.Ramos.Add(ramo);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new VincularRamoApoliceHandler(_fixture.DbContext);
        var command = new VincularRamoApoliceCommand
        {
            ApolicePublicId = apolicePublicId,
            RamoPublicId = ramoPublicId,
            NumeroApolice = "123",
            IofPercentual = 7.38m,
            UsuarioPublicId = Guid.NewGuid()
        };

        var id = await handler.Handle(command, CancellationToken.None);

        id.Should().BeGreaterThan(0);

        var vinculo = await _fixture.DbContext.ApoliceRamos.FirstOrDefaultAsync(v => v.Id == id);
        vinculo.Should().NotBeNull();
        vinculo!.Ativo.Should().BeTrue();
        vinculo.NumeroApolice.Should().Be("123");

        var historico = await _fixture.DbContext.ApoliceHistoricos.FirstOrDefaultAsync(h => h.ApoliceId == apolice.Id);
        historico.Should().NotBeNull();
        historico!.Acao.Should().Be("Vínculo de Ramo");
    }

    [Fact]
    public async Task VincularRamo_Inativo_NaoDevePermitir_Silenciosamente()
    {
        var apolicePublicId = Guid.NewGuid();
        var apolice = new ApoliceModel 
        { 
            PublicId = apolicePublicId, 
            EstipulanteId = 1, 
            SeguradoraId = 1, 
            Nome = "Apolice Teste Ramo 2",
            DataInicioVigencia = new DateOnly(2025, 1, 1)
        };
        _fixture.DbContext.Apolices.Add(apolice);

        var ramoPublicId = Guid.NewGuid();
        var ramo = new RamoModel
        {
            PublicId = ramoPublicId,
            Codigo = "XYZ2",
            Nome = "Ramo XYZ2",
            Ativo = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        _fixture.DbContext.Ramos.Add(ramo);
        await _fixture.DbContext.SaveChangesAsync();

        var vinculoAntigo = new ApoliceRamoModel
        {
            ApoliceId = apolice.Id,
            RamoId = ramo.Id,
            Ativo = false
        };
        _fixture.DbContext.ApoliceRamos.Add(vinculoAntigo);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new VincularRamoApoliceHandler(_fixture.DbContext);
        var command = new VincularRamoApoliceCommand
        {
            ApolicePublicId = apolicePublicId,
            RamoPublicId = ramoPublicId,
            NumeroApolice = "123",
            UsuarioPublicId = Guid.NewGuid()
        };

        Func<Task> action = async () => await handler.Handle(command, CancellationToken.None);
        await action.Should().ThrowAsync<ValidacaoException>().WithMessage("*Existe um vínculo inativo*");
    }
}
