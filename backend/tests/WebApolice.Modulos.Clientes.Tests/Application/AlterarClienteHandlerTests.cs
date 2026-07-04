using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using WebApolice.Auditoria.Contracts;
using WebApolice.Auditoria.Domain;
using WebApolice.Modulos.Clientes.Application.Ports;
using WebApolice.Modulos.Clientes.Application.UseCases.AlterarCliente;
using WebApolice.Modulos.Clientes.Domain;
using WebApolice.Modulos.Clientes.Domain.Exceptions;
using Xunit;

namespace WebApolice.Modulos.Clientes.Tests.Application;

public class AlterarClienteHandlerTests
{
    private readonly Mock<IClientesRepository> _repositoryMock;
    private readonly Mock<IRegistradorAuditoria> _auditoriaMock;
    private readonly Mock<IClientesTransactionManager> _transactionManagerMock;
    private readonly AlterarClienteHandler _handler;

    public AlterarClienteHandlerTests()
    {
        _repositoryMock = new Mock<IClientesRepository>();
        _auditoriaMock = new Mock<IRegistradorAuditoria>();
        _transactionManagerMock = new Mock<IClientesTransactionManager>();

        _transactionManagerMock.Setup(t => t.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
            .Callback<Func<Task>, CancellationToken>(async (action, ct) => await action())
            .Returns(Task.CompletedTask);

        _handler = new AlterarClienteHandler(
            _repositoryMock.Object, 
            _auditoriaMock.Object, 
            _transactionManagerMock.Object);
    }

    [Fact]
    public async Task Handle_ComDadosValidos_DeveAlterarEAuditar()
    {
        // Arrange
        var clienteExistente = new Cliente("Fulano", "01821765419", null, null, null, null);
        _repositoryMock.Setup(r => r.ObterPorIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(clienteExistente);

        var command = new AlterarClienteCommand(1, "Fulano Editado", null, "edit@teste.com", null);

        // Act
        await _handler.Handle(command, "user123", CancellationToken.None);

        // Assert
        clienteExistente.Nome.Should().Be("Fulano Editado");
        clienteExistente.Email.Should().Be("edit@teste.com");

        _repositoryMock.Verify(r => r.AtualizarAsync(clienteExistente, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _auditoriaMock.Verify(a => a.RegistrarAsync(It.Is<RegistroAuditoria>(r => r.Acao == "alterar" && r.Modulo == "clientes"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ClienteInexistente_DeveLancarNaoEncontrado()
    {
        // Arrange
        _repositoryMock.Setup(r => r.ObterPorIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente?)null);

        var command = new AlterarClienteCommand(99, "Teste", null, null, null);

        // Act & Assert
        var act = () => _handler.Handle(command, "user123", CancellationToken.None);
        await act.Should().ThrowAsync<ClienteNaoEncontradoException>();
    }
}
