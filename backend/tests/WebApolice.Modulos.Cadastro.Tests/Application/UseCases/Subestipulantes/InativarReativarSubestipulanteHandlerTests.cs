using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using WebApolice.Modulos.Cadastro.Application.Ports;
using WebApolice.Modulos.Cadastro.Application.UseCases.InativarSubestipulante;
using WebApolice.Modulos.Cadastro.Application.UseCases.ReativarSubestipulante;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Models.Vinculos;
using Xunit;

namespace WebApolice.Modulos.Cadastro.Tests.Application.UseCases.Subestipulantes;

public class InativarReativarSubestipulanteHandlerTests
{
    private readonly Mock<ISubestipulanteRepository> _repositoryMock;

    public InativarReativarSubestipulanteHandlerTests()
    {
        _repositoryMock = new Mock<ISubestipulanteRepository>();
    }

    [Fact]
    public async Task Inativar_SubestipulanteAtivo_DeveInativarESalvar()
    {
        // Arrange
        var publicId = Guid.NewGuid();
        var handler = new InativarSubestipulanteHandler(_repositoryMock.Object);
        var subestipulante = new SubestipulanteModel { PublicId = publicId, Ativo = true };

        _repositoryMock.Setup(r => r.ObterPorPublicIdAsync(publicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subestipulante);

        // Act
        await handler.Handle(new InativarSubestipulanteCommand(publicId), CancellationToken.None);

        // Assert
        Assert.False(subestipulante.Ativo);
        _repositoryMock.Verify(r => r.Atualizar(subestipulante), Times.Once);
        _repositoryMock.Verify(r => r.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Reativar_SubestipulanteInativo_DeveReativarESalvar()
    {
        // Arrange
        var publicId = Guid.NewGuid();
        var handler = new ReativarSubestipulanteHandler(_repositoryMock.Object);
        var subestipulante = new SubestipulanteModel { PublicId = publicId, Ativo = false };

        _repositoryMock.Setup(r => r.ObterPorPublicIdAsync(publicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subestipulante);

        // Act
        await handler.Handle(new ReativarSubestipulanteCommand(publicId), CancellationToken.None);

        // Assert
        Assert.True(subestipulante.Ativo);
        _repositoryMock.Verify(r => r.Atualizar(subestipulante), Times.Once);
        _repositoryMock.Verify(r => r.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
