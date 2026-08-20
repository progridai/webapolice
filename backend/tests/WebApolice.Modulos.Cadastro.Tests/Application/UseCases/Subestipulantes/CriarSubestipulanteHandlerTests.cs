using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using WebApolice.Modulos.Cadastro.Application.Ports;
using WebApolice.Modulos.Cadastro.Application.UseCases.CriarSubestipulante;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Models;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Models.Vinculos;
using Xunit;

namespace WebApolice.Modulos.Cadastro.Tests.Application.UseCases.Subestipulantes;

public class CriarSubestipulanteHandlerTests
{
    private readonly Mock<ISubestipulanteRepository> _repositoryMock;
    private readonly Mock<IClienteRepository> _clienteRepositoryMock;
    private readonly Mock<ICadastroTransactionManager> _transactionManagerMock;
    private readonly Mock<System.Data.Common.DbTransaction> _transactionMock;
    private readonly CriarSubestipulanteHandler _handler;

    public CriarSubestipulanteHandlerTests()
    {
        _repositoryMock = new Mock<ISubestipulanteRepository>();
        _clienteRepositoryMock = new Mock<IClienteRepository>();
        _transactionManagerMock = new Mock<ICadastroTransactionManager>();
        _transactionMock = new Mock<System.Data.Common.DbTransaction>();

        _transactionManagerMock.Setup(m => m.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_transactionMock.Object);

        _handler = new CriarSubestipulanteHandler(
            _repositoryMock.Object,
            _clienteRepositoryMock.Object,
            _transactionManagerMock.Object);
    }

    [Fact]
    public async Task Handle_SubestipulanteValidoSemCnpj_DeveCriarPessoaESubestipulante()
    {
        // Arrange
        var command = new CriarSubestipulanteCommand
        {
            Nome = "Subestipulante Teste",
            Codigo = "SUB-001"
        };

        // Act
        var resultId = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, resultId);
        _clienteRepositoryMock.Verify(r => r.AdicionarPessoa(It.Is<PessoaModel>(p => p.Nome == "Subestipulante Teste" && p.DocumentoPrincipal == null)), Times.Once);
        _repositoryMock.Verify(r => r.Adicionar(It.Is<SubestipulanteModel>(s => s.Codigo == "SUB-001" && s.Ativo)), Times.Once);
        _transactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CnpjJaExistenteParaSubestipulante_DeveLancarExcecao()
    {
        // Arrange
        var command = new CriarSubestipulanteCommand
        {
            Nome = "Subestipulante Teste",
            Cnpj = "12.345.678/0001-99"
        };

        _repositoryMock.Setup(r => r.CnpjJaExisteAsync("12345678000199", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));
        Assert.Equal("Já existe um subestipulante cadastrado com este CNPJ.", ex.Message);
    }

    [Fact]
    public async Task Handle_CnpjExistenteParaPessoaComNomeDiferente_DeveLancarExcecao()
    {
        // Arrange
        var command = new CriarSubestipulanteCommand
        {
            Nome = "Nome Diferente",
            Cnpj = "12.345.678/0001-99"
        };

        _repositoryMock.Setup(r => r.CnpjJaExisteAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _clienteRepositoryMock.Setup(r => r.LocalizarPessoaPorDocumentoAsync("12345678000199", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PessoaModel(2, "Nome Original", "12.345.678/0001-99", "12345678000199", true, null, null, null));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));
        Assert.Contains("nome divergente", ex.Message);
        _transactionMock.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
