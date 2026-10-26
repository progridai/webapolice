using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using WebApolice.Modulos.Cadastro.Contracts;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas.ListarPropostas;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Propostas.AdicionarItem;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Propostas.CriarProposta;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Integration.Tests.Modulos.Seguro;

// Provider em memória para regras e IDs. Não comprova FKs, migrations ou transações PostgreSQL.
public class PropostasConsolidacaoTests
{
    [Fact]
    public async Task CriacaoResolvePessoaVinculoEStatusSemCopiarIdDoCliente()
    {
        await using var db = CriarDb();
        var vida = await PrepararVidaAsync(db, 50);
        var clientes = new Mock<IClientesPropostas>();
        clientes.Setup(c => c.ResolverAsync(10, 50, It.IsAny<CancellationToken>())).ReturnsAsync(new IdentidadeClienteProposta(10, 99, 50));
        var handler = CriarHandler(db, clientes.Object);
        var id = await handler.Handle(new CriarPropostaCommand { ClienteId = 10, ApoliceId = vida.Apolice!.PublicId, ApoliceVidaId = vida.PublicId }, default);
        var proposta = await db.Proposta.SingleAsync(p => p.PublicId == id);
        Assert.Equal(99, proposta.PessoaId);
        Assert.Equal(50, proposta.ClienteVinculoId);
        Assert.Equal(7, proposta.StatusId);
        Assert.Null(proposta.LegadoId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0L)]
    [InlineData(50L)]
    public async Task AusenciaOuIncompatibilidadeDeVinculoImpedeCriacao(long? vinculo)
    {
        await using var db = CriarDb();
        var vida = await PrepararVidaAsync(db, vinculo);
        var handler = CriarHandler(db, Mock.Of<IClientesPropostas>());
        await Assert.ThrowsAsync<ValidacaoException>(() => handler.Handle(new CriarPropostaCommand
            { ClienteId = 10, ApoliceId = vida.Apolice!.PublicId, ApoliceVidaId = vida.PublicId }, default));
        Assert.Empty(db.Proposta);
    }

    [Fact]
    public async Task AdicionarItemRetornaIdentificadorPersistidoENaoInventaLegado()
    {
        await using var db = CriarDb();
        var vida = await PrepararVidaAsync(db, 50);
        var proposta = new Propostum { PublicId = Guid.NewGuid(), ApoliceId = vida.ApoliceId, StatusId = 7 };
        db.Proposta.Add(proposta);
        var produto = new ApoliceProdutoModel { ApoliceId = vida.ApoliceId, ProdutoId = 100 };
        db.ApoliceProdutos.Add(produto);
        await db.SaveChangesAsync();
        db.ApolicePlanos.Add(new ApolicePlanoModel { ApoliceProdutoId = produto.Id, PlanoId = 200 });
        await db.SaveChangesAsync();
        var handler = new AdicionarPropostaItemHandler(db, new OperacaoSemPersistencia());
        var command = new AdicionarPropostaItemCommand { PropostaId = proposta.PublicId, ProdutoId = 100, PlanoId = 200 };
        var primeiro = await handler.Handle(command, default);
        var segundo = await handler.Handle(command, default);
        Assert.NotEqual(primeiro, segundo);
        Assert.Equal(2, await db.PropostaItems.CountAsync());
        Assert.Contains(await db.PropostaItems.ToListAsync(), i => i.PublicId == primeiro);
        Assert.Contains(await db.PropostaItems.ToListAsync(), i => i.PublicId == segundo);
        Assert.All(await db.PropostaItems.ToListAsync(), i => Assert.Null(i.LegadoId));
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    [InlineData(int.MaxValue, 100)]
    public async Task PaginacaoInvalidaEhRejeitada(int pagina, int tamanho)
    {
        await using var db = CriarDb();
        await Assert.ThrowsAsync<ValidacaoException>(() => new ObterPropostasHandler(db).Handle(
            new ObterPropostasQuery { Pagina = pagina, TamanhoPagina = tamanho }, default));
    }

    private static SeguroDbContext CriarDb() => new(new DbContextOptionsBuilder<SeguroDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static CriarPropostaHandler CriarHandler(SeguroDbContext db, IClientesPropostas clientes) => new(db, clientes,
        Options.Create(new ConfiguracaoPropostas { StatusInicialCodigo = "STATUS_DE_TESTE" }), new OperacaoSemPersistencia());

    private static async Task<ApoliceVidaModel> PrepararVidaAsync(SeguroDbContext db, long? vinculo)
    {
        db.PropostaStatuses.Add(new PropostaStatus { Id = 7, Codigo = "STATUS_DE_TESTE", Nome = "Teste" });
        var apolice = new ApoliceModel { PublicId = Guid.NewGuid() };
        var vida = new ApoliceVidaModel { PublicId = Guid.NewGuid(), Apolice = apolice, ClienteId = 10, ClienteVinculoId = vinculo, Ativo = true };
        db.ApoliceVidas.Add(vida);
        await db.SaveChangesAsync();
        return vida;
    }

    private sealed class OperacaoSemPersistencia : IOperacaoPropostas
    {
        public async Task<T> ExecutarAsync<T>(string acao,
            Func<CancellationToken, Task<(T Resultado, string PublicId, object? Antes, object? Depois)>> executar,
            CancellationToken cancellationToken) => (await executar(cancellationToken)).Resultado;
    }
}
