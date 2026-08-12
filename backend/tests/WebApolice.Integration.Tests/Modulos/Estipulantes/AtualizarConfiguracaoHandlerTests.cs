using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using WebApolice.Auditoria.Contracts;
using WebApolice.Modulos.Estipulantes.Application.Ports;
using WebApolice.Modulos.Estipulantes.Application.UseCases.AtualizarConfiguracao;
using WebApolice.Modulos.Estipulantes.Infrastructure.Persistence.Models;
using Xunit;

namespace WebApolice.Integration.Tests.Modulos.Estipulantes;

public class AtualizarConfiguracaoHandlerTests
{
    private readonly Mock<IEstipulanteRepository> _repositoryMock;
    private readonly Mock<IRegistradorAuditoria> _auditoriaMock;
    private readonly Mock<IDbContextTransaction> _transactionMock;
    private readonly AtualizarConfiguracaoHandler _handler;

    public AtualizarConfiguracaoHandlerTests()
    {
        _repositoryMock = new Mock<IEstipulanteRepository>();
        _auditoriaMock = new Mock<IRegistradorAuditoria>();
        _transactionMock = new Mock<IDbContextTransaction>();

        _repositoryMock.Setup(r => r.BeginTransactionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(_transactionMock.Object);

        _handler = new AtualizarConfiguracaoHandler(_repositoryMock.Object, _auditoriaMock.Object);
    }

    [Fact]
    public async Task DeveCriarConfiguracaoSeNaoExistir()
    {
        var publicId = Guid.NewGuid();
        var command = new AtualizarConfiguracaoCommand(publicId, DateOnly.FromDateTime(DateTime.UtcNow), null, 30, "TESTE", "TESTE", "TESTE");

        var estipulante = new EstipulanteModel { Id = 1, PessoaId = 1, Ativo = true };

        _repositoryMock.Setup(r => r.ObterParaEdicaoPorPublicIdAsync(publicId, It.IsAny<CancellationToken>())).ReturnsAsync(estipulante);
        _repositoryMock.Setup(r => r.ObterConfiguracaoPorEstipulanteIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((EstipulanteConfiguracaoModel?)null);

        await _handler.Handle(command, CancellationToken.None);

        _repositoryMock.Verify(r => r.AdicionarConfiguracao(It.IsAny<EstipulanteConfiguracaoModel>()), Times.Once);
        _transactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
