using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using WebApolice.Modulos.Clientes.Application.Ports;
using WebApolice.Modulos.Clientes.Application.UseCases.AlterarCliente;
using WebApolice.Modulos.Clientes.Domain.Exceptions;
using Xunit;

namespace WebApolice.Modulos.Clientes.Tests.Application;

public class AlterarClienteHandlerTests
{
    private readonly AlterarClienteHandler _handler;

    public AlterarClienteHandlerTests()
    {
        var repoMock = new Mock<IClienteRepository>();
        _handler = new AlterarClienteHandler(repoMock.Object, null!);
    }

    [Fact]
    public async Task Handle_NomeVazio_LancaClienteInvalidoException()
    {
        // Arrange
        var command = new AlterarClienteCommand(Guid.NewGuid(), "", null, null, null, false, null, null, null, null, null);

        // Act
        var act = () => _handler.Handle(command, "user123", CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ClienteInvalidoException>().WithMessage("O nome é obrigatório.");
    }
}
