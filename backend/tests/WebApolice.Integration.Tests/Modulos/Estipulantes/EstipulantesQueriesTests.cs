using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using WebApolice.Integration.Tests.Setup;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence;
using WebApolice.Modulos.Cadastro.Domain.Modelos;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Queries;

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
        Assert.Null(await queries.ObterConfiguracaoPorPublicIdAsync(estipulante.PublicId, default));
    }
}
