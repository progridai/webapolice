using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WebApolice.Auditoria.Infrastructure;

public class AuditoriaDbContextFactory : IDesignTimeDbContextFactory<AuditoriaDbContext>
{
    public AuditoriaDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AuditoriaDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=dummy;Username=postgres;Password=postgres", 
            o => 
            {
                o.MigrationsHistoryTable("__EFMigrationsHistory", "auditoria");
            })
            .UseSnakeCaseNamingConvention();

        return new AuditoriaDbContext(optionsBuilder.Options);
    }
}
