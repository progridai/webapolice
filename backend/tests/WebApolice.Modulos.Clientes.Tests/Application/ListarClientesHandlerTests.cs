using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using WebApolice.Modulos.Clientes.Application.Ports;
using WebApolice.Modulos.Clientes.Application.UseCases.ListarClientes;
using WebApolice.Modulos.Clientes.Domain;
using Xunit;

namespace WebApolice.Modulos.Clientes.Tests.Application;

public class ListarClientesHandlerTests
{
    private readonly Mock<IClientesRepository> _repositoryMock;
    private readonly ListarClientesHandler _handler;

    public ListarClientesHandlerTests()
    {
        _repositoryMock = new Mock<IClientesRepository>();
        _handler = new ListarClientesHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_DeveRetornarCpfMascarado()
    {
        // Arrange
        var cliente = new Cliente("Fulano", "01821765419", null, null, null, null);
        var lista = new List<Cliente> { cliente };
        
        _repositoryMock.Setup(r => r.ListarPaginadoAsync(1, 20, null, null, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((lista, 1, 1));

        var query = new ListarClientesQuery(1, 20, null, null, null, null, null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Itens.Should().HaveCount(1);
        result.Itens.First().CpfMascarado.Should().Be("***.***.***-19");
    }
}
