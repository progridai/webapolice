using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using WebApolice.Integration.Tests.Setup;
using WebApolice.Modulos.Cadastro.Domain.Clientes;
using WebApolice.Modulos.Cadastro.Domain.Modelos.Vinculos;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Queries;

namespace WebApolice.Integration.Tests.Modulos.Seguradoras;

public class SeguradorasQueriesTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:18.4").Build();
    public Task InitializeAsync() => _postgres.StartAsync();
    public Task DisposeAsync() => _postgres.DisposeAsync().AsTask();

    [Theory]
    [InlineData("SEGURADORA ÁRVORE", "árvore")]
    [InlineData("SEGURADORA ÁRVORE", "ÁRVORE")]
    [InlineData("seguradora árvore", "ÁrVoRe")]
    [InlineData(null, "árvore")]
    [InlineData("SEGURADORA ÁRVORE", "ref-42")]
    [InlineData("SEGURADORA ÁRVORE", "99988")]
    [InlineData("SEGURADORA ÁRVORE", "12345678000190")]
    public async Task BuscaNomeSemDistinguirCaixaEPreservaChaveOriginal(string? chave, string busca)
    {
        await using var db = CriarContexto();
        await db.Database.EnsureCreatedAsync();
        var pessoa = new PessoaModel
        {
            PublicId = Guid.NewGuid(), TipoPessoa = 2, Nome = "SEGURADORA ÁRVORE",
            NomeNormalizado = chave, DocumentoPrincipal = "12.345.678/0001-90",
            DocumentoPrincipalLimpo = "12345678000190"
        };
        var seguradora = new SeguradoraModel
        {
            PublicId = Guid.NewGuid(), Pessoa = pessoa, Codigo = "REF-42", Susep = "99988", Ativo = true
        };
        db.Seguradoras.Add(seguradora);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var resultado = await new SeguradorasQueries(db).ListarPaginadoAsync(1, 20, busca, true, default);

        var item = Assert.Single(resultado.itens);
        Assert.Equal(seguradora.PublicId, item.PublicId);
        Assert.Equal("Seguradora Árvore", item.Nome);
        Assert.Equal(1, resultado.totalItens);
        Assert.Equal(chave, (await db.Pessoas.AsNoTracking().SingleAsync()).NomeNormalizado);
    }

    [Fact]
    public async Task BuscaMantemFiltrosDeSituacaoExclusaoEPaginacao()
    {
        await using var db = CriarContexto();
        await db.Database.EnsureCreatedAsync();
        foreach (var (nome, ativa, excluida) in new[]
        {
            ("Seguradora Alfa", true, false), ("Seguradora Beta", true, false),
            ("Seguradora Gama", false, false), ("Seguradora Delta", true, true)
        })
        {
            db.Seguradoras.Add(new SeguradoraModel
            {
                PublicId = Guid.NewGuid(), Ativo = ativa,
                DeletedAt = excluida ? DateTimeOffset.UtcNow : null,
                Pessoa = new PessoaModel { PublicId = Guid.NewGuid(), TipoPessoa = 2, Nome = nome, NomeNormalizado = nome.ToUpperInvariant() }
            });
        }
        await db.SaveChangesAsync();
        var queries = new SeguradorasQueries(db);

        var primeira = await queries.ListarPaginadoAsync(1, 1, "seguradora", true, default);
        var segunda = await queries.ListarPaginadoAsync(2, 1, "SEGURADORA", true, default);
        var inativas = await queries.ListarPaginadoAsync(1, 20, "seguradora", false, default);
        var vazio = await queries.ListarPaginadoAsync(1, 20, "inexistente", true, default);

        Assert.Equal("Seguradora Alfa", Assert.Single(primeira.itens).Nome);
        Assert.Equal("Seguradora Beta", Assert.Single(segunda.itens).Nome);
        Assert.Equal(2, primeira.totalItens);
        Assert.Equal(2, primeira.totalPaginas);
        Assert.Equal("Seguradora Gama", Assert.Single(inativas.itens).Nome);
        Assert.Empty(vazio.itens);
        Assert.Equal(0, vazio.totalItens);
    }

    private CadastroDbContext CriarContexto() => new CadastroEfemeroDbContext(
        new DbContextOptionsBuilder<CadastroDbContext>().UseNpgsql(_postgres.GetConnectionString())
            .UseSnakeCaseNamingConvention().Options);
}
