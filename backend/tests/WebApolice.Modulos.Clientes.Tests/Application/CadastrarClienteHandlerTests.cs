using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using WebApolice.Auditoria.Contracts;
using WebApolice.Auditoria.Domain;
using WebApolice.Modulos.Clientes.Application.Ports;
using WebApolice.Modulos.Clientes.Application.UseCases.CadastrarCliente;
using WebApolice.Modulos.Clientes.Domain;
using WebApolice.Modulos.Clientes.Domain.Exceptions;
using Xunit;

namespace WebApolice.Modulos.Clientes.Tests.Application;

public class CadastrarClienteHandlerTests
{
    private readonly Mock<IClientesRepository> _repositoryMock;
    private readonly Mock<IRegistradorAuditoria> _auditoriaMock;
    private readonly Mock<IClientesTransactionManager> _transactionManagerMock;
    private readonly CadastrarClienteHandler _handler;

    public CadastrarClienteHandlerTests()
    {
        _repositoryMock = new Mock<IClientesRepository>();
        _auditoriaMock = new Mock<IRegistradorAuditoria>();
        _transactionManagerMock = new Mock<IClientesTransactionManager>();

        _transactionManagerMock.Setup(t => t.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
            .Callback<Func<Task>, CancellationToken>(async (action, ct) => await action())
            .Returns(Task.CompletedTask);

        _handler = new CadastrarClienteHandler(
            _repositoryMock.Object, 
            _auditoriaMock.Object, 
            _transactionManagerMock.Object);
    }

    [Fact]
    public async Task Handle_ComDadosValidos_DeveCadastrarEAuditar()
    {
        // Arrange
        var command = new CadastrarClienteCommand("Fulano", "01821765419", null, null, null, null);
        _repositoryMock.Setup(r => r.ExisteCpfAsync("01821765419", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, "user123", CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Nome.Should().Be("Fulano");
        result.CpfMascarado.Should().Be("***.***.***-19");
        result.Status.Should().Be("ativo");

        _repositoryMock.Verify(r => r.AdicionarAsync(It.Is<Cliente>(c => c.Nome == "Fulano"), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _auditoriaMock.Verify(a => a.RegistrarAsync(It.Is<RegistroAuditoria>(r => r.Acao == "cadastrar" && r.Modulo == "clientes"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CpfJaExistente_DeveLancarConflito()
    {
        // Arrange
        var command = new CadastrarClienteCommand("Fulano", "01821765419", null, null, null, null);
        _repositoryMock.Setup(r => r.ExisteCpfAsync("01821765419", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        var act = () => _handler.Handle(command, "user123", CancellationToken.None);
        await act.Should().ThrowAsync<ClienteJaCadastradoException>();

        _repositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
