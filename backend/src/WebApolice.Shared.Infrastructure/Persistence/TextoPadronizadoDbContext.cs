using Microsoft.EntityFrameworkCore;
using WebApolice.SharedKernel.Text;

namespace WebApolice.Shared.Infrastructure.Persistence;

/// <summary>Base obrigatória para persistência de dados de negócio, inclusive ferramentas de importação EF.</summary>
public abstract class TextoPadronizadoDbContext : DbContext
{
    protected TextoPadronizadoDbContext() { }
    protected TextoPadronizadoDbContext(DbContextOptions options) : base(options) { }

    public void PadronizarTextos()
    {
        ChangeTracker.DetectChanges();
        foreach (var entry in ChangeTracker.Entries().Where(e => e.State is EntityState.Added or EntityState.Modified))
        {
            // Registros históricos não podem ser reescritos.
            if (entry.Metadata.ClrType.Name.Contains("Auditoria", StringComparison.OrdinalIgnoreCase) ||
                entry.Metadata.ClrType.Name.EndsWith("MigrationMap", StringComparison.Ordinal)) continue;
            foreach (var property in entry.Properties)
            {
                if (property.Metadata.ClrType == typeof(string) && PadronizacaoTexto.CampoHumano(property.Metadata.Name))
                    property.CurrentValue = PadronizacaoTexto.TitleCase((string?)property.CurrentValue);
            }
        }
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        PadronizarTextos();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        PadronizarTextos();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
}
