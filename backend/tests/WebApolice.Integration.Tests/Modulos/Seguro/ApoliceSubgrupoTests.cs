using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WebApolice.Integration.Tests.Setup;
using WebApolice.Modulos.Seguro.Application.UseCases.Apolices.CriarSubgrupo;
using WebApolice.Modulos.Seguro.Application.UseCases.Apolices.AlterarSubgrupo;
using WebApolice.Modulos.Seguro.Application.UseCases.Apolices.InativarSubgrupo;
using WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarSubgrupos;
using WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ObterSubgrupo;
using WebApolice.Modulos.Seguro.Infrastructure.Persistence.Queries;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;
using WebApolice.SharedKernel.Application.Exceptions;
using Xunit;

namespace WebApolice.Integration.Tests.Modulos.Seguro;

/// <summary>
/// Testes de integração para Subgrupos da Apólice.
/// Subgrupo é uma divisão contextual da Apólice — não é cadastro global.
/// O sistema não interpreta o nome: "Matriz", "Filial", "Débito em Conta"
/// são apenas identificações funcionais definidas pelo negócio.
/// </summary>
[Collection("SeguroTests")]
public class ApoliceSubgrupoTests : IClassFixture<SeguroIntegrationTestFixture>
{
    private readonly SeguroIntegrationTestFixture _fixture;

    public ApoliceSubgrupoTests(SeguroIntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    private ApoliceModel CriarApolice(string nome = "Apólice Subgrupo Teste") =>
        new ApoliceModel
        {
            PublicId = Guid.NewGuid(),
            EstipulanteId = 1,
            SeguradoraId = 1,
            Nome = nome,
            DataInicioVigencia = new DateOnly(2025, 1, 1)
        };

    // ── Criação ────────────────────────────────────────────────────────────

    [Fact]
    public async Task CriarSubgrupo_ComNome_DeveRetornarPublicId()
    {
        var apolice = CriarApolice("Apólice Criar Sub 1");
        _fixture.DbContext.Apolices.Add(apolice);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new CriarSubgrupoApoliceHandler(_fixture.DbContext);
        var command = new CriarSubgrupoApoliceCommand
        {
            ApolicePublicId = apolice.PublicId,
            Nome = "Matriz"
        };

        var publicId = await handler.Handle(command, CancellationToken.None);

        publicId.Should().NotBeEmpty();

        var subgrupo = await _fixture.DbContext.ApoliceSubgrupos
            .FirstOrDefaultAsync(s => s.PublicId == publicId);

        subgrupo.Should().NotBeNull();
        subgrupo!.Nome.Should().Be("Matriz");
        subgrupo.Ativo.Should().BeTrue();
        subgrupo.ApoliceId.Should().Be(apolice.Id);
        subgrupo.Observacao.Should().BeNull();
    }

    [Fact]
    public async Task CriarSubgrupo_ComNomeEObservacao_DevePersistirAmbos()
    {
        var apolice = CriarApolice("Apólice Criar Sub 2");
        _fixture.DbContext.Apolices.Add(apolice);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new CriarSubgrupoApoliceHandler(_fixture.DbContext);
        var command = new CriarSubgrupoApoliceCommand
        {
            ApolicePublicId = apolice.PublicId,
            Nome = "Filial Centro",
            Observacao = "Filial localizada no centro da cidade."
        };

        var publicId = await handler.Handle(command, CancellationToken.None);

        var subgrupo = await _fixture.DbContext.ApoliceSubgrupos
            .FirstOrDefaultAsync(s => s.PublicId == publicId);

        subgrupo!.Nome.Should().Be("Filial Centro");
        subgrupo.Observacao.Should().Be("Filial localizada no centro da cidade.");
    }

    [Fact]
    public async Task CriarSubgrupo_NomeVazio_DeveLancarValidacaoException()
    {
        var apolice = CriarApolice("Apólice Criar Sub Vazio");
        _fixture.DbContext.Apolices.Add(apolice);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new CriarSubgrupoApoliceHandler(_fixture.DbContext);
        var command = new CriarSubgrupoApoliceCommand
        {
            ApolicePublicId = apolice.PublicId,
            Nome = ""
        };

        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ValidacaoException>()
            .WithMessage("*nome*obrigatório*");
    }

    [Fact]
    public async Task CriarSubgrupo_ApoliceInexistente_DeveLancarValidacaoException()
    {
        var handler = new CriarSubgrupoApoliceHandler(_fixture.DbContext);
        var command = new CriarSubgrupoApoliceCommand
        {
            ApolicePublicId = Guid.NewGuid(), // apólice que não existe
            Nome = "Subgrupo Órfão"
        };

        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ValidacaoException>()
            .WithMessage("*Apólice*não encontrada*");
    }

    // ── Isolamento entre Apólices ──────────────────────────────────────────

    [Fact]
    public async Task IsolamentoPorApolice_SubgrupoNaoAcessivelPorOutraApolice()
    {
        var apolice1 = CriarApolice("Apólice A - Isolamento");
        var apolice2 = CriarApolice("Apólice B - Isolamento");
        _fixture.DbContext.Apolices.AddRange(apolice1, apolice2);
        await _fixture.DbContext.SaveChangesAsync();

        // Cria Subgrupo na Apólice A
        var criarHandler = new CriarSubgrupoApoliceHandler(_fixture.DbContext);
        var publicIdSub = await criarHandler.Handle(new CriarSubgrupoApoliceCommand
        {
            ApolicePublicId = apolice1.PublicId,
            Nome = "Funcionários"
        }, CancellationToken.None);

        // Tenta alterar o Subgrupo usando contexto da Apólice B
        var alterarHandler = new AlterarSubgrupoApoliceHandler(_fixture.DbContext);
        Func<Task> act = async () => await alterarHandler.Handle(new AlterarSubgrupoApoliceCommand
        {
            ApolicePublicId = apolice2.PublicId, // outra apólice!
            SubgrupoPublicId = publicIdSub,
            Nome = "Tentativa Indevida"
        }, CancellationToken.None);

        await act.Should().ThrowAsync<ValidacaoException>()
            .WithMessage("*não encontrado*");
    }

    // ── Alteração ─────────────────────────────────────────────────────────

    [Fact]
    public async Task AlterarSubgrupo_ComNovoNome_DeveAtualizar()
    {
        var apolice = CriarApolice("Apólice Alterar Sub");
        _fixture.DbContext.Apolices.Add(apolice);
        await _fixture.DbContext.SaveChangesAsync();

        var criarHandler = new CriarSubgrupoApoliceHandler(_fixture.DbContext);
        var publicId = await criarHandler.Handle(new CriarSubgrupoApoliceCommand
        {
            ApolicePublicId = apolice.PublicId,
            Nome = "Nome Original"
        }, CancellationToken.None);

        var alterarHandler = new AlterarSubgrupoApoliceHandler(_fixture.DbContext);
        await alterarHandler.Handle(new AlterarSubgrupoApoliceCommand
        {
            ApolicePublicId = apolice.PublicId,
            SubgrupoPublicId = publicId,
            Nome = "Nome Atualizado",
            Observacao = "Observação nova"
        }, CancellationToken.None);

        var subgrupo = await _fixture.DbContext.ApoliceSubgrupos
            .FirstOrDefaultAsync(s => s.PublicId == publicId);

        subgrupo!.Nome.Should().Be("Nome Atualizado");
        subgrupo.Observacao.Should().Be("Observação nova");
    }

    [Fact]
    public async Task AlterarSubgrupo_NomeVazio_DeveLancarValidacaoException()
    {
        var apolice = CriarApolice("Apólice Alterar Sub Vazio");
        _fixture.DbContext.Apolices.Add(apolice);
        await _fixture.DbContext.SaveChangesAsync();

        var criarHandler = new CriarSubgrupoApoliceHandler(_fixture.DbContext);
        var publicId = await criarHandler.Handle(new CriarSubgrupoApoliceCommand
        {
            ApolicePublicId = apolice.PublicId,
            Nome = "Nome Valido"
        }, CancellationToken.None);

        var alterarHandler = new AlterarSubgrupoApoliceHandler(_fixture.DbContext);
        Func<Task> act = async () => await alterarHandler.Handle(new AlterarSubgrupoApoliceCommand
        {
            ApolicePublicId = apolice.PublicId,
            SubgrupoPublicId = publicId,
            Nome = "   "
        }, CancellationToken.None);

        await act.Should().ThrowAsync<ValidacaoException>()
            .WithMessage("*nome*obrigatório*");
    }

    // ── Inativação ────────────────────────────────────────────────────────

    [Fact]
    public async Task InativarSubgrupo_Ativo_DeveInativar()
    {
        var apolice = CriarApolice("Apólice Inativar Sub");
        _fixture.DbContext.Apolices.Add(apolice);
        await _fixture.DbContext.SaveChangesAsync();

        var criarHandler = new CriarSubgrupoApoliceHandler(_fixture.DbContext);
        var publicId = await criarHandler.Handle(new CriarSubgrupoApoliceCommand
        {
            ApolicePublicId = apolice.PublicId,
            Nome = "Diretoria"
        }, CancellationToken.None);

        var inativarHandler = new InativarSubgrupoApoliceHandler(_fixture.DbContext);
        await inativarHandler.Handle(new InativarSubgrupoApoliceCommand
        {
            ApolicePublicId = apolice.PublicId,
            SubgrupoPublicId = publicId
        }, CancellationToken.None);

        var subgrupo = await _fixture.DbContext.ApoliceSubgrupos
            .FirstOrDefaultAsync(s => s.PublicId == publicId);

        subgrupo!.Ativo.Should().BeFalse();
    }

    [Fact]
    public async Task InativarSubgrupo_JaInativo_DeveLancarValidacaoException()
    {
        var apolice = CriarApolice("Apólice Inativar Já Inativo");
        _fixture.DbContext.Apolices.Add(apolice);
        await _fixture.DbContext.SaveChangesAsync();

        var criarHandler = new CriarSubgrupoApoliceHandler(_fixture.DbContext);
        var publicId = await criarHandler.Handle(new CriarSubgrupoApoliceCommand
        {
            ApolicePublicId = apolice.PublicId,
            Nome = "Grupo A"
        }, CancellationToken.None);

        var inativarHandler = new InativarSubgrupoApoliceHandler(_fixture.DbContext);

        // Primeira inativação — OK
        await inativarHandler.Handle(new InativarSubgrupoApoliceCommand
        {
            ApolicePublicId = apolice.PublicId,
            SubgrupoPublicId = publicId
        }, CancellationToken.None);

        // Segunda inativação — deve falhar
        Func<Task> act = async () => await inativarHandler.Handle(new InativarSubgrupoApoliceCommand
        {
            ApolicePublicId = apolice.PublicId,
            SubgrupoPublicId = publicId
        }, CancellationToken.None);

        await act.Should().ThrowAsync<ValidacaoException>()
            .WithMessage("*já está inativo*");
    }

    // ── Queries ───────────────────────────────────────────────────────────

    [Fact]
    public async Task ListarSubgrupos_DeveRetornarSomenteSubgruposDaApolice()
    {
        var apoliceA = CriarApolice("Apólice A Listar");
        var apoliceB = CriarApolice("Apólice B Listar");
        _fixture.DbContext.Apolices.AddRange(apoliceA, apoliceB);
        await _fixture.DbContext.SaveChangesAsync();

        var criarHandler = new CriarSubgrupoApoliceHandler(_fixture.DbContext);

        // Subgrupos da Apólice A
        await criarHandler.Handle(new CriarSubgrupoApoliceCommand { ApolicePublicId = apoliceA.PublicId, Nome = "Sub A1" }, CancellationToken.None);
        await criarHandler.Handle(new CriarSubgrupoApoliceCommand { ApolicePublicId = apoliceA.PublicId, Nome = "Sub A2" }, CancellationToken.None);

        // Subgrupo da Apólice B — não deve aparecer na listagem de A
        await criarHandler.Handle(new CriarSubgrupoApoliceCommand { ApolicePublicId = apoliceB.PublicId, Nome = "Sub B1" }, CancellationToken.None);

        var queries = new ApolicesQueries(_fixture.DbContext);
        var handler = new ListarApoliceSubgruposHandler(queries);

        var result = await handler.Handle(new ListarApoliceSubgruposQuery(apoliceA.PublicId), CancellationToken.None);

        result.Should().HaveCount(2);
        result.Select(s => s.Nome).Should().BeEquivalentTo(["Sub A1", "Sub A2"]);
        result.All(s => s.Ativo).Should().BeTrue();
    }

    [Fact]
    public async Task ObterSubgrupo_PublicIdCorreto_DeveRetornar()
    {
        var apolice = CriarApolice("Apólice Obter Sub");
        _fixture.DbContext.Apolices.Add(apolice);
        await _fixture.DbContext.SaveChangesAsync();

        var criarHandler = new CriarSubgrupoApoliceHandler(_fixture.DbContext);
        var publicId = await criarHandler.Handle(new CriarSubgrupoApoliceCommand
        {
            ApolicePublicId = apolice.PublicId,
            Nome = "Folha"
        }, CancellationToken.None);

        var queries = new ApolicesQueries(_fixture.DbContext);
        var handler = new ObterApoliceSubgrupoHandler(queries);

        var result = await handler.Handle(
            new ObterApoliceSubgrupoPorPublicIdQuery(apolice.PublicId, publicId),
            CancellationToken.None);

        result.Should().NotBeNull();
        result!.SubgrupoPublicId.Should().Be(publicId);
        result.Nome.Should().Be("Folha");
        result.Ativo.Should().BeTrue();
    }

    [Fact]
    public async Task ObterSubgrupo_PublicIdDeOutraApolice_DeveRetornarNull()
    {
        var apoliceA = CriarApolice("Apólice A Obter Isolamento");
        var apoliceB = CriarApolice("Apólice B Obter Isolamento");
        _fixture.DbContext.Apolices.AddRange(apoliceA, apoliceB);
        await _fixture.DbContext.SaveChangesAsync();

        var criarHandler = new CriarSubgrupoApoliceHandler(_fixture.DbContext);
        var publicIdSubA = await criarHandler.Handle(new CriarSubgrupoApoliceCommand
        {
            ApolicePublicId = apoliceA.PublicId,
            Nome = "Sub da A"
        }, CancellationToken.None);

        // Tenta obter Sub da A usando contexto da Apólice B
        var queries = new ApolicesQueries(_fixture.DbContext);
        var handler = new ObterApoliceSubgrupoHandler(queries);

        var result = await handler.Handle(
            new ObterApoliceSubgrupoPorPublicIdQuery(apoliceB.PublicId, publicIdSubA),
            CancellationToken.None);

        result.Should().BeNull(); // isolamento correto
    }
}
