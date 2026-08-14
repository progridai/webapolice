using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using WebApolice.Auditoria.Contracts;
using WebApolice.Modulos.Cadastro.Application.Ports;
using WebApolice.Modulos.Cadastro.Application.UseCases.InativarEstipulante;
using WebApolice.Modulos.Cadastro.Application.UseCases.ReativarEstipulante;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Models;
using Xunit;

namespace WebApolice.Integration.Tests.Modulos.Estipulantes;

public class StatusEstipulanteHandlerTests
{
    private readonly Mock<IEstipulanteRepository> _repositoryMock;
    private readonly Mock<IRegistradorAuditoria> _auditoriaMock;
    private readonly Mock<IDbContextTransaction> _transactionMock;
    private readonly InativarEstipulanteHandler _inativarHandler;
    private readonly ReativarEstipulanteHandler _reativarHandler;

    public StatusEstipulanteHandlerTests()
    {
        _repositoryMock = new Mock<IEstipulanteRepository>();
        _auditoriaMock = new Mock<IRegistradorAuditoria>();
        _transactionMock = new Mock<IDbContextTransaction>();

        _repositoryMock.Setup(r => r.BeginTransactionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(_transactionMock.Object);

        _inativarHandler = new InativarEstipulanteHandler(_repositoryMock.Object, _auditoriaMock.Object);
        _reativarHandler = new ReativarEstipulanteHandler(_repositoryMock.Object, _auditoriaMock.Object);
    }

    [Fact]
    public async Task DeveInativarCorretamente()
    {
        var publicId = Guid.NewGuid();
        var estipulante = new EstipulanteModel { Id = 1, Ativo = true };

        _repositoryMock.Setup(r => r.ObterParaEdicaoPorPublicIdAsync(publicId, It.IsAny<CancellationToken>())).ReturnsAsync(estipulante);

        await _inativarHandler.Handle(new InativarEstipulanteCommand(publicId), CancellationToken.None);

        estipulante.Ativo.Should().BeFalse();
        _transactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeveSerIdempotenteAoInativar()
    {
        var publicId = Guid.NewGuid();
        var estipulante = new EstipulanteModel { Id = 1, Ativo = false };

        _repositoryMock.Setup(r => r.ObterParaEdicaoPorPublicIdAsync(publicId, It.IsAny<CancellationToken>())).ReturnsAsync(estipulante);

        await _inativarHandler.Handle(new InativarEstipulanteCommand(publicId), CancellationToken.None);

        estipulante.Ativo.Should().BeFalse();
        _transactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
