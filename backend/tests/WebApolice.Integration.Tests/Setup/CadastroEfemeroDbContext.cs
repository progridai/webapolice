using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence;

namespace WebApolice.Integration.Tests.Setup;

// Somente banco vazio de testes. O schema implantado/legado precisa de homologação separada.
public sealed class CadastroEfemeroDbContext(DbContextOptions<CadastroDbContext> options) : CadastroDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        foreach (var entity in modelBuilder.Model.GetEntityTypes()) entity.SetIsTableExcludedFromMigrations(false);
    }
}
