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
        var clienteItem = new ClienteListagemItemResult(Guid.NewGuid(), "Fulano", "123.456.789-19", "Ativo", DateTime.UtcNow);
        var mockResult = (Itens: new[] { clienteItem }, TotalItens: 1, TotalPaginas: 1);
        var query = new ListarClientesQuery(1, 20, null, null, null, null, null);

        _queriesMock.Setup(q => q.ListarPaginadoAsync(
            query.Pagina, query.TamanhoPagina, query.Nome, query.Documento,
            query.StatusId, query.OrdenarPor, query.Direcao, CancellationToken.None
        )).ReturnsAsync(mockResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalItens.Should().Be(1);
        result.Itens.Should().HaveCount(1);
        result.Itens.First().DocumentoMascarado.Should().Be("123.456.789-19");
    }
}
