using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using WebApolice.Auditoria.Contracts;
using WebApolice.Auditoria.Domain;
using WebApolice.Modulos.Estipulantes.Application.Ports;
using WebApolice.Modulos.Estipulantes.Application.UseCases.CriarEstipulante;
using WebApolice.Modulos.Estipulantes.Domain.Exceptions;
using WebApolice.Modulos.Estipulantes.Infrastructure.Persistence.Models;
using Xunit;

namespace WebApolice.Integration.Tests.Modulos.Estipulantes;

public class CriarEstipulanteHandlerTests
{
    private readonly Mock<IEstipulanteRepository> _repositoryMock;
    private readonly Mock<IRegistradorAuditoria> _auditoriaMock;
    private readonly Mock<IDbContextTransaction> _transactionMock;
    private readonly CriarEstipulanteHandler _handler;

    public CriarEstipulanteHandlerTests()
    {
        _repositoryMock = new Mock<IEstipulanteRepository>();
        _auditoriaMock = new Mock<IRegistradorAuditoria>();
        _transactionMock = new Mock<IDbContextTransaction>();

        _repositoryMock.Setup(r => r.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_transactionMock.Object);
            
        _repositoryMock.Setup(r => r.CidadeExisteAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _handler = new CriarEstipulanteHandler(_repositoryMock.Object, _auditoriaMock.Object);
    }

    [Fact]
    public async Task DeveCriarEstipulante_SucessoCompleto()
    {
        var command = CriarCommandValido();
        
        _repositoryMock.Setup(r => r.LocalizarPessoaPorDocumentoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PessoaModel?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        _repositoryMock.Verify(r => r.AdicionarPessoa(It.IsAny<PessoaModel>()), Times.Once);
        _repositoryMock.Verify(r => r.AdicionarEstipulante(It.IsAny<EstipulanteModel>()), Times.Once);
        _repositoryMock.Verify(r => r.AdicionarEndereco(It.IsAny<PessoaEnderecoModel>()), Times.Once);
        _repositoryMock.Verify(r => r.AdicionarContato(It.IsAny<PessoaContatoModel>()), Times.Exactly(2)); // Email + Tel
        _repositoryMock.Verify(r => r.AdicionarConfiguracao(It.IsAny<EstipulanteConfiguracaoModel>()), Times.Once);
        _repositoryMock.Verify(r => r.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
        _transactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _auditoriaMock.Verify(a => a.RegistrarAsync(It.IsAny<RegistroAuditoria>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeveCriarEstipulante_ApenasCamposObrigatorios()
    {
        var command = new CriarEstipulanteCommand(
            "Empresa Teste", null, "12345678000199", null, null, null, null, null, null,
            new CriarEstipulanteConfiguracaoCommand(DateOnly.FromDateTime(DateTime.UtcNow), null, null, null, null, null));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        _repositoryMock.Verify(r => r.AdicionarEstipulante(It.IsAny<EstipulanteModel>()), Times.Once);
        _repositoryMock.Verify(r => r.AdicionarEndereco(It.IsAny<PessoaEnderecoModel>()), Times.Never);
        _repositoryMock.Verify(r => r.AdicionarContato(It.IsAny<PessoaContatoModel>()), Times.Never);
        _transactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeveFalhar_CnpjInvalido_NaoSalvaNada()
    {
        var command = CriarCommandValido() with { Cnpj = "123" };
        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<EstipulanteInvalidoException>().WithMessage("CNPJ inválido.");
        _repositoryMock.Verify(r => r.AdicionarPessoa(It.IsAny<PessoaModel>()), Times.Never);
    }

    [Fact]
    public async Task DeveFalhar_GrupoInexistente_RollbackCompleto()
    {
        var command = CriarCommandValido() with { GrupoId = 999 };
        _repositoryMock.Setup(r => r.GrupoExisteAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<EstipulanteInvalidoException>().WithMessage("Grupo informado não existe.");
        _transactionMock.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeveFalhar_SeguradoraInexistente_RollbackCompleto()
    {
        var segPublicId = Guid.NewGuid();
        var command = CriarCommandValido() with { SeguradoraPublicId = segPublicId };
        _repositoryMock.Setup(r => r.ObterSeguradoraIdPorPublicIdAsync(segPublicId, It.IsAny<CancellationToken>())).ReturnsAsync((long?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<EstipulanteInvalidoException>().WithMessage("Seguradora informada não existe.");
        _transactionMock.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeveFalhar_CidadeInexistente_RollbackCompleto()
    {
        var command = CriarCommandValido();
        _repositoryMock.Setup(r => r.CidadeExisteAsync(It.IsAny<long>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<EstipulanteInvalidoException>().WithMessage("A Cidade informada não existe.");
        _transactionMock.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeveFalhar_ConfiguracaoInvalida_GaranteRollback()
    {
        var command = CriarCommandValido() with 
        { 
            Configuracao = new CriarEstipulanteConfiguracaoCommand(DateOnly.FromDateTime(DateTime.UtcNow), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)), null, null, null, null) 
        };
        
        var act = () => _handler.Handle(command, CancellationToken.None);
        
        await act.Should().ThrowAsync<EstipulanteInvalidoException>();
        _repositoryMock.Verify(r => r.AdicionarPessoa(It.IsAny<PessoaModel>()), Times.Never);
    }

    [Fact]
    public async Task ReutilizacaoSegura_PessoaExistenteSemEstipulante()
    {
        var command = CriarCommandValido();
        var pessoaExistente = new PessoaModel { Id = 1, Nome = "Empresa Teste", TipoPessoa = 2 };
        
        _repositoryMock.Setup(r => r.LocalizarPessoaPorDocumentoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pessoaExistente);
            
        _repositoryMock.Setup(r => r.LocalizarEstipulantePorPessoaIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EstipulanteModel?)null);

        await _handler.Handle(command, CancellationToken.None);

        _repositoryMock.Verify(r => r.AdicionarPessoa(It.IsAny<PessoaModel>()), Times.Never);
        _repositoryMock.Verify(r => r.AdicionarEstipulante(It.IsAny<EstipulanteModel>()), Times.Once);
        _transactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Conflito_EstipulanteExistente_Retorna409()
    {
        var command = CriarCommandValido();
        var pessoaExistente = new PessoaModel { Id = 1, Nome = "Empresa Teste", TipoPessoa = 2 };
        var estipulanteExistente = new EstipulanteModel { PessoaId = 1, Ativo = true };
        
        _repositoryMock.Setup(r => r.LocalizarPessoaPorDocumentoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pessoaExistente);
        _repositoryMock.Setup(r => r.LocalizarEstipulantePorPessoaIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(estipulanteExistente);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<EstipulanteConflitoException>().WithMessage("Já existe um Estipulante ativo para o CNPJ informado.");
        _transactionMock.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Conflito_DadosDivergentesPessoaExistente_Retorna409()
    {
        var command = CriarCommandValido();
        var pessoaExistente = new PessoaModel { Id = 1, Nome = "Outra Razão Social", TipoPessoa = 2 };
        
        _repositoryMock.Setup(r => r.LocalizarPessoaPorDocumentoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pessoaExistente);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<EstipulanteConflitoException>().WithMessage("O CNPJ informado já pertence a outra pessoa com Razão Social divergente no sistema.");
        _transactionMock.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static CriarEstipulanteCommand CriarCommandValido()
    {
        return new CriarEstipulanteCommand(
            "Empresa Teste",
            "Fantasia",
            "12345678000199",
            "COD123",
            null,
            null,
            "Obs",
            new CriarEstipulanteEnderecoCommand("00000000", "Rua", "123", "", "Bairro", 1, "RS"),
            new CriarEstipulanteContatoCommand("teste@teste.com", "51999999999"),
            new CriarEstipulanteConfiguracaoCommand(DateOnly.FromDateTime(DateTime.UtcNow), null, 0, "TESTE", "TESTE", "TESTE")
        );
    }
}
