using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Repositories;
using WebApolice.Integration.Tests.Setup;
using Xunit;

namespace WebApolice.Integration.Tests.Modulos.Clientes;

public class ClientesRepositoryTests : IClassFixture<ClientesIntegrationTestFixture>
{
    private readonly ClientesIntegrationTestFixture _fixture;

    public ClientesRepositoryTests(ClientesIntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ObterPorId_DeveRetornarNuloSeNaoEncontrar()
    {
        var repository = new ClienteRepository(_fixture.DbContext);
        var idFake = Guid.NewGuid();
        var result = await repository.ObterParaEdicaoPorPublicIdAsync(idFake, CancellationToken.None);
        result.Should().BeNull();
    }
}
