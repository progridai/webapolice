using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WebApolice.Modulos.Clientes.Infrastructure.Persistence;

public class ClientesDbContextFactory : IDesignTimeDbContextFactory<ClientesDbContext>
{
    public ClientesDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ClientesDbContext>();
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__PostgreSql") ??
            "Host=localhost;Database=webapolice_db;Username=postgres;Password=postgres";

        optionsBuilder.UseNpgsql(connectionString, 
            npgsqlOptions => 
            {
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "clientes");
            });

        return new ClientesDbContext(optionsBuilder.Options);
    }
}
