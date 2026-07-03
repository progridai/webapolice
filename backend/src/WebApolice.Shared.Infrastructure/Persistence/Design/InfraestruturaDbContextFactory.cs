using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WebApolice.Shared.Infrastructure.Persistence.Design;

public class InfraestruturaDbContextFactory : IDesignTimeDbContextFactory<InfraestruturaDbContext>
{
    public InfraestruturaDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__PostgreSql");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            // Fornece uma mensagem clara se a connection string não estiver definida no ambiente (útil para uso da CLI localmente).
            throw new InvalidOperationException(
                "A variável de ambiente 'ConnectionStrings__PostgreSql' não foi definida. " +
                "Para rodar as migrations, defina a variável de ambiente, por exemplo: " +
                "$env:ConnectionStrings__PostgreSql='Host=localhost;Port=5432;Database=webapolice;Username=webapolice;Password=alterar'");
        }

        var optionsBuilder = new DbContextOptionsBuilder<InfraestruturaDbContext>();

        optionsBuilder
            .UseNpgsql(connectionString, o => o.MigrationsHistoryTable("__EFMigrationsHistory", "infraestrutura"))
            .UseSnakeCaseNamingConvention();

        return new InfraestruturaDbContext(optionsBuilder.Options);
    }
}
