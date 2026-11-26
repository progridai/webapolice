using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using WebApolice.Integration.Tests.Setup;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence;
using WebApolice.Modulos.Cadastro.Domain.Modelos;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Queries;
using WebApolice.Modulos.Cadastro.Domain.Clientes;
using WebApolice.Modulos.Cadastro.Domain.Modelos.Vinculos;

namespace WebApolice.Integration.Tests.Modulos.Estipulantes;

public class EstipulantesQueriesTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:18.4").Build();
    public Task InitializeAsync() => _postgres.StartAsync();
    public Task DisposeAsync() => _postgres.DisposeAsync().AsTask();

    [Fact]
    public async Task ConsultaRegistroCriadoNaBaseEfemera()
    {
        await using var db = new CadastroEfemeroDbContext(new DbContextOptionsBuilder<CadastroDbContext>()
            .UseNpgsql(_postgres.GetConnectionString()).UseSnakeCaseNamingConvention().Options);
        await db.Database.EnsureCreatedAsync();
        var estipulante = new EstipulanteModel { PublicId = Guid.NewGuid(), Nome = "Empresa Teste", Ativo = true };
        db.Estipulantes.Add(estipulante);
        await db.SaveChangesAsync();
        var queries = new EstipulantesQueries(db);
        var result = await queries.ObterPorPublicIdAsync(estipulante.PublicId, default);
        Assert.NotNull(result);
        Assert.Equal(estipulante.PublicId, result.PublicId);
        Assert.Equal("Empresa Teste", result.RazaoSocial);
        Assert.Null(result.SeguradoraNome);
        Assert.Null(await queries.ObterConfiguracaoPorPublicIdAsync(estipulante.PublicId, default));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DetalheRetornaNomeEUuidDaSeguradoraVinculadaMesmoInativa(bool ativa)
    {
        await using var db = new CadastroEfemeroDbContext(new DbContextOptionsBuilder<CadastroDbContext>()
            .UseNpgsql(_postgres.GetConnectionString()).UseSnakeCaseNamingConvention().Options);
        await db.Database.EnsureCreatedAsync();
        var pessoa = new PessoaModel { PublicId = Guid.NewGuid(), TipoPessoa = 2, Nome = "SEGURADORA DE PROTEÇÃO" };
        db.Pessoas.Add(pessoa);
        await db.SaveChangesAsync();
        var seguradora = new SeguradoraModel { PublicId = Guid.NewGuid(), PessoaId = pessoa.Id, Ativo = ativa };
        db.Seguradoras.Add(seguradora);
        await db.SaveChangesAsync();
        var estipulante = new EstipulanteModel { PublicId = Guid.NewGuid(), Nome = "Empresa Teste", Ativo = true, SeguradoraId = seguradora.Id };
        db.Estipulantes.Add(estipulante);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var result = await new EstipulantesQueries(db).ObterPorPublicIdAsync(estipulante.PublicId, default);

        Assert.NotNull(result);
        Assert.Equal(seguradora.PublicId, result.SeguradoraPublicId);
        Assert.Equal("Seguradora de Proteção", result.SeguradoraNome);
        Assert.Equal(seguradora.Id, (await db.Estipulantes.FindAsync(estipulante.Id))!.SeguradoraId);
    }
}
