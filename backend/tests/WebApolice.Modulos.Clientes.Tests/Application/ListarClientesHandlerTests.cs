using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using WebApolice.Modulos.Clientes.Application.Ports;
using WebApolice.Modulos.Clientes.Application.UseCases.ListarClientes;
using Xunit;

namespace WebApolice.Modulos.Clientes.Tests.Application;

public class ListarClientesHandlerTests
{
    private readonly Mock<IClientesQueries> _queriesMock;
    private readonly ListarClientesHandler _handler;

    public ListarClientesHandlerTests()
    {
        _queriesMock = new Mock<IClientesQueries>();
        _handler = new ListarClientesHandler(_queriesMock.Object);
    }

    [Fact]
    public async Task Handle_DeveRetornarCpfMascarado()
    {
        // Arrange
        var clienteItem = new ClienteListagemItemResult(Guid.NewGuid(), "Fulano", "***.***.***-19", "Ativo", DateTime.UtcNow);
        var lista = new List<ClienteListagemItemResult> { clienteItem };
        
        var expectedResult = (lista.ToArray(), 1, 1);
        _queriesMock.Setup(q => q.ListarPaginadoAsync(1, 20, null, null, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var query = new ListarClientesQuery(1, 20, null, null, null, null, null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Itens.Should().HaveCount(1);
        result.Itens.First().DocumentoMascarado.Should().Be("***.***.***-19");
    }
}
