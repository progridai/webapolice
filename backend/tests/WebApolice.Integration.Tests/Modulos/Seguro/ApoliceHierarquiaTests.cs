using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WebApolice.Integration.Tests.Setup;
using WebApolice.Modulos.Seguro.Domain.Modelos;
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

        var pessoa = new WebApolice.Modulos.Cadastro.Domain.Clientes.PessoaModel(1, "Pessoa Hierarquia", null, null, false, null, null, null);
        _fixture.Cadastro.Pessoas.Add(pessoa); await _fixture.Cadastro.SaveChangesAsync();
        var cliente = new WebApolice.Modulos.Cadastro.Domain.Cliente(pessoa.Id, 1);
        _fixture.Cadastro.Clientes.Add(cliente);
        _fixture.Cadastro.Estipulantes.Add(new() { Id = 2, PublicId = Guid.NewGuid(), Nome = "Estipulante Hierarquia", Ativo = true });
        await _fixture.Cadastro.SaveChangesAsync();
        var vinculo = new WebApolice.Modulos.Cadastro.Domain.Modelos.ClienteVinculoModel(cliente.Id, pessoa.Id, 2, "H-100");
        _fixture.Cadastro.Vinculos.Add(vinculo); await _fixture.Cadastro.SaveChangesAsync();
        var vidaDireta = new ApoliceVidaModel
        {
            PublicId = Guid.NewGuid(),
            ApoliceId = apolice.Id,
            ClienteId = cliente.Id,
            ClienteVinculoId = vinculo.Id,
            Ativo = true
        };
        _fixture.DbContext.ApoliceVidas.Add(vidaDireta);


        var result = await _fixture.DbContext.SaveChangesAsync();
        result.Should().BeGreaterThan(0);
    }
}
