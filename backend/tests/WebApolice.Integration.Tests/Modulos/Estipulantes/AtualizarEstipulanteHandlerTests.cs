using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using WebApolice.Auditoria.Contracts;
using WebApolice.Modulos.Estipulantes.Application.Ports;
using WebApolice.Modulos.Estipulantes.Application.UseCases.AtualizarEstipulante;
using WebApolice.Modulos.Estipulantes.Domain.Exceptions;
using WebApolice.Modulos.Estipulantes.Infrastructure.Persistence.Models;
using Xunit;

namespace WebApolice.Integration.Tests.Modulos.Estipulantes;

public class AtualizarEstipulanteHandlerTests
{
    private readonly Mock<IEstipulanteRepository> _repositoryMock;
    private readonly Mock<IRegistradorAuditoria> _auditoriaMock;
    private readonly Mock<IDbContextTransaction> _transactionMock;
    private readonly AtualizarEstipulanteHandler _handler;

    public AtualizarEstipulanteHandlerTests()
    {
        _repositoryMock = new Mock<IEstipulanteRepository>();
        _auditoriaMock = new Mock<IRegistradorAuditoria>();
        _transactionMock = new Mock<IDbContextTransaction>();

        _repositoryMock.Setup(r => r.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_transactionMock.Object);

        _handler = new AtualizarEstipulanteHandler(_repositoryMock.Object, _auditoriaMock.Object);
    }

    [Fact]
    public async Task NaoDeveAlterarRazaoSocialSePessoaCompartilhada()
    {
        var publicId = Guid.NewGuid();
        var command = new AtualizarEstipulanteCommand(publicId, "Nova Razao", "Novo Fantasia", null, null, null, null, null, null);

        var estipulante = new EstipulanteModel { Id = 1, PessoaId = 1, Ativo = true };
        var pessoa = new PessoaModel { Id = 1, Nome = "Razao Antiga" };

        _repositoryMock.Setup(r => r.ObterParaEdicaoPorPublicIdAsync(publicId, It.IsAny<CancellationToken>())).ReturnsAsync(estipulante);
        _repositoryMock.Setup(r => r.LocalizarPessoaPorIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(pessoa);
        _repositoryMock.Setup(r => r.VerificarPessoaCompartilhadaAsync(1, 1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<EstipulanteConflitoException>();
        _transactionMock.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeveAlterarFantasiaMesmoComPessoaCompartilhada()
    {
        var publicId = Guid.NewGuid();
        var command = new AtualizarEstipulanteCommand(publicId, "Razao Antiga", "Novo Fantasia", null, null, null, null, null, null);

        var estipulante = new EstipulanteModel { Id = 1, PessoaId = 1, Ativo = true };
        var pessoa = new PessoaModel { Id = 1, Nome = "Razao Antiga" };

        _repositoryMock.Setup(r => r.ObterParaEdicaoPorPublicIdAsync(publicId, It.IsAny<CancellationToken>())).ReturnsAsync(estipulante);
        _repositoryMock.Setup(r => r.LocalizarPessoaPorIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(pessoa);
        _repositoryMock.Setup(r => r.VerificarPessoaCompartilhadaAsync(1, 1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await _handler.Handle(command, CancellationToken.None);

        estipulante.Nome.Should().Be("Novo Fantasia");
        _transactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
