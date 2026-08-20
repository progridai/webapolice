using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using WebApolice.Modulos.Cadastro.Application.Ports;
using WebApolice.Modulos.Cadastro.Application.UseCases.CriarSeguradora;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Models.Vinculos;
using Xunit;

namespace WebApolice.Integration.Tests.Modulos.Seguradoras;

public class CriarSeguradoraHandlerTests
{
    private readonly Mock<ISeguradoraRepository> _repositoryMock;
    private readonly Mock<IClienteRepository> _clienteRepositoryMock;
    private readonly Mock<ICadastroTransactionManager> _transactionMock;
    private readonly Mock<System.Data.Common.DbTransaction> _dbTransactionMock;
    private readonly CriarSeguradoraHandler _handler;

    public CriarSeguradoraHandlerTests()
    {
        _repositoryMock = new Mock<ISeguradoraRepository>();
        _clienteRepositoryMock = new Mock<IClienteRepository>();
        _transactionMock = new Mock<ICadastroTransactionManager>();
        _dbTransactionMock = new Mock<System.Data.Common.DbTransaction>();

        _transactionMock.Setup(t => t.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_dbTransactionMock.Object);

        _handler = new CriarSeguradoraHandler(_repositoryMock.Object, _clienteRepositoryMock.Object, _transactionMock.Object);
    }

    [Fact]
    public async Task DeveCriarSeguradora_Sucesso()
    {
        var command = new CriarSeguradoraCommand
        {
            Nome = "Seguradora Teste",
            Codigo = "123",
            Susep = "12345",
            Cnpj = "12.345.678/0001-99"
        };

        _repositoryMock.Setup(r => r.CnpjJaExisteAsync("12345678000199", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeEmpty();
        _repositoryMock.Verify(r => r.Adicionar(It.IsAny<SeguradoraModel>()), Times.Once);
        _repositoryMock.Verify(r => r.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _dbTransactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeveFalhar_QuandoCnpjJaExiste()
    {
        var command = new CriarSeguradoraCommand
        {
            Nome = "Seguradora Teste",
            Cnpj = "12.345.678/0001-99"
        };

        _repositoryMock.Setup(r => r.CnpjJaExisteAsync("12345678000199", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Já existe uma seguradora cadastrada com este CNPJ.");
        _repositoryMock.Verify(r => r.Adicionar(It.IsAny<SeguradoraModel>()), Times.Never);
        _dbTransactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeveFalhar_QuandoNomeVazio()
    {
        var command = new CriarSeguradoraCommand
        {
            Nome = ""
        };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("O nome da seguradora é obrigatório.");
        _repositoryMock.Verify(r => r.Adicionar(It.IsAny<SeguradoraModel>()), Times.Never);
    }
}
