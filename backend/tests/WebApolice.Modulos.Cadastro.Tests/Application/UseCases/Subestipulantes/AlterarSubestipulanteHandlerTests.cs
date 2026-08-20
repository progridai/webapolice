using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using WebApolice.Modulos.Cadastro.Application.Ports;
using WebApolice.Modulos.Cadastro.Application.UseCases.AlterarSubestipulante;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Models;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Models.Vinculos;
using Xunit;

namespace WebApolice.Modulos.Cadastro.Tests.Application.UseCases.Subestipulantes;

public class AlterarSubestipulanteHandlerTests
{
    private readonly Mock<ISubestipulanteRepository> _repositoryMock;
    private readonly Mock<IClienteRepository> _clienteRepositoryMock;
    private readonly Mock<ICadastroTransactionManager> _transactionManagerMock;
    private readonly Mock<System.Data.Common.DbTransaction> _transactionMock;
    private readonly AlterarSubestipulanteHandler _handler;

    public AlterarSubestipulanteHandlerTests()
    {
        _repositoryMock = new Mock<ISubestipulanteRepository>();
        _clienteRepositoryMock = new Mock<IClienteRepository>();
        _transactionManagerMock = new Mock<ICadastroTransactionManager>();
        _transactionMock = new Mock<System.Data.Common.DbTransaction>();

        _transactionManagerMock.Setup(m => m.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_transactionMock.Object);

        _handler = new AlterarSubestipulanteHandler(
            _repositoryMock.Object,
            _clienteRepositoryMock.Object,
            _transactionManagerMock.Object);
    }

    [Fact]
    public async Task Handle_SubestipulanteValido_DeveAtualizarPessoaESubestipulante()
    {
        // Arrange
        var publicId = Guid.NewGuid();
        var command = new AlterarSubestipulanteCommand
        {
            PublicId = publicId,
            Nome = "Nome Atualizado",
            Codigo = "SUB-002"
        };

        var subestipulanteExistente = new SubestipulanteModel { Id = 1, PublicId = publicId, PessoaId = 2 };
        var pessoaExistente = new PessoaModel(2, "Nome Antigo", null, null, false, null, null, null);

        _repositoryMock.Setup(r => r.ObterPorPublicIdAsync(publicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subestipulanteExistente);
        _clienteRepositoryMock.Setup(r => r.LocalizarPessoaPorIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pessoaExistente);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal("Nome Atualizado", pessoaExistente.Nome);
        Assert.Equal("SUB-002", subestipulanteExistente.Codigo);
        
        _clienteRepositoryMock.Verify(r => r.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.Atualizar(subestipulanteExistente), Times.Once);
        _repositoryMock.Verify(r => r.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _transactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
