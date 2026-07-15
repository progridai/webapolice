using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using WebApolice.Modulos.Clientes.Application.Ports;
using WebApolice.Modulos.Clientes.Application.UseCases.AlterarCliente;
using WebApolice.Modulos.Clientes.Domain;
using WebApolice.Modulos.Clientes.Domain.Exceptions;
using WebApolice.Modulos.Clientes.Infrastructure.Persistence;
using WebApolice.Modulos.Clientes.Infrastructure.Persistence.Models;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace WebApolice.Modulos.Clientes.Tests.Application.UseCases.AlterarCliente;

public class AlterarClienteHandlerTests
{
    private readonly Mock<IClienteRepository> _repositoryMock;
    private readonly Mock<ClientesDbContext> _dbContextMock;
    private readonly AlterarClienteHandler _handler;

    public AlterarClienteHandlerTests()
    {
        _repositoryMock = new Mock<IClienteRepository>();
        var options = new DbContextOptionsBuilder<ClientesDbContext>().Options;
        _dbContextMock = new Mock<ClientesDbContext>(options);
        
        var transactionMock = new Mock<IDbContextTransaction>();
        _dbContextMock.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(transactionMock.Object);

        _handler = new AlterarClienteHandler(_repositoryMock.Object, _dbContextMock.Object);
    }

    [Fact]
    public async Task Handle_DeveLancarExcecao_QuandoClienteNaoExistir()
    {
        // Arrange
        var command = new AlterarClienteCommand(Guid.NewGuid(), "Nome", null, new DateOnly(1990, 1, 1), null, null, false, null, "Email", "Tel", "Cel", null);
        _repositoryMock.Setup(x => x.ObterParaEdicaoPorPublicIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ClienteNaoEncontradoException>(() =>
            _handler.Handle(command, "usuario", CancellationToken.None));
        
        exception.Message.Should().Be("Cliente não encontrado ou excluído.");
    }

    [Fact]
    public async Task Handle_DeveLancarExcecao_QuandoPessoaCompartilhada()
    {
        // Arrange
        var cliente = new Cliente(1, 1);
        var pessoa = new PessoaModel(1, "Nome", "12345678901", "12345678901", true, new DateOnly(1990, 1, 1), 1, "");
        var command = new AlterarClienteCommand(cliente.PublicId, "Novo Nome", null, new DateOnly(1990, 1, 1), null, null, false, null, null, null, null, null);

        _repositoryMock.Setup(x => x.ObterParaEdicaoPorPublicIdAsync(command.Id, It.IsAny<CancellationToken>())).ReturnsAsync(cliente);
        _repositoryMock.Setup(x => x.LocalizarPessoaPorIdAsync(cliente.PessoaId, It.IsAny<CancellationToken>())).ReturnsAsync(pessoa);
        _repositoryMock.Setup(x => x.VerificarPessoaCompartilhadaAsync(pessoa.Id, cliente.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ClienteInvalidoException>(() =>
            _handler.Handle(command, "usuario", CancellationToken.None));
        
        exception.Message.Should().Be("Os dados dessa pessoa são compartilhados com outros papéis no sistema e não podem ser alterados diretamente por aqui.");
    }

    [Fact]
    public async Task Handle_DeveAlterarDadosBasicos_QuandoSucesso()
    {
        // Arrange
        var cliente = new Cliente(1, 1);
        var pessoa = new PessoaModel(1, "Nome Antigo", "12345678901", "12345678901", true, new DateOnly(1990, 1, 1), 1, "");
        var command = new AlterarClienteCommand(cliente.PublicId, "Nome Novo", null, new DateOnly(1990, 1, 1), 2, null, false, null, null, null, null, null);

        _repositoryMock.Setup(x => x.ObterParaEdicaoPorPublicIdAsync(command.Id, It.IsAny<CancellationToken>())).ReturnsAsync(cliente);
        _repositoryMock.Setup(x => x.LocalizarPessoaPorIdAsync(cliente.PessoaId, It.IsAny<CancellationToken>())).ReturnsAsync(pessoa);
        _repositoryMock.Setup(x => x.VerificarPessoaCompartilhadaAsync(pessoa.Id, cliente.Id, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var transactionMock = new Mock<IDbContextTransaction>();
        _dbContextMock.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(transactionMock.Object);

        // Act
        await _handler.Handle(command, "usuario", CancellationToken.None);

        // Assert
        pessoa.Nome.Should().Be("Nome Novo");
        pessoa.Sexo.Should().Be(2);
        _repositoryMock.Verify(x => x.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DeveInativarContato_QuandoValorVazioOuNulo()
    {
        // Arrange
        var cliente = new Cliente(1, 1);
        var pessoa = new PessoaModel(1, "Nome", "12345678901", "12345678901", true, new DateOnly(1990, 1, 1), 1, "");
        var command = new AlterarClienteCommand(cliente.PublicId, "Nome", null, new DateOnly(1990, 1, 1), null, null, false, null, "", null, null, null);

        var contatoEmailExistente = new PessoaContatoModel(pessoa.Id, "EMAIL", "teste@teste.com", "TESTE@TESTE.COM", true);

        _repositoryMock.Setup(x => x.ObterParaEdicaoPorPublicIdAsync(command.Id, It.IsAny<CancellationToken>())).ReturnsAsync(cliente);
        _repositoryMock.Setup(x => x.LocalizarPessoaPorIdAsync(cliente.PessoaId, It.IsAny<CancellationToken>())).ReturnsAsync(pessoa);
        _repositoryMock.Setup(x => x.VerificarPessoaCompartilhadaAsync(pessoa.Id, cliente.Id, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _repositoryMock.Setup(x => x.ObterContatoPrincipalAsync(pessoa.Id, "EMAIL", It.IsAny<CancellationToken>())).ReturnsAsync(contatoEmailExistente);

        var transactionMock = new Mock<IDbContextTransaction>();
        _dbContextMock.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(transactionMock.Object);

        // Act
        await _handler.Handle(command, "usuario", CancellationToken.None);

        // Assert
        contatoEmailExistente.Ativo.Should().BeFalse();
        _repositoryMock.Verify(x => x.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
