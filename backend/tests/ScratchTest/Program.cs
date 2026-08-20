using System;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Seguranca.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace ScratchTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "../../src/WebApolice.Api")))
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var connectionString = config.GetConnectionString("PostgreSql");
            
            var optionsBuilder = new DbContextOptionsBuilder<SegurancaDbContext>();
            optionsBuilder.UseNpgsql(connectionString).UseSnakeCaseNamingConvention();

            using var context = new SegurancaDbContext(optionsBuilder.Options);

            // Registra a migration que foi aplicada manualmente no histórico do EF Core
            // para que a cadeia de migrations fique consistente.
            context.Database.ExecuteSqlRaw(@"
INSERT INTO public.""__EFMigrationsHistory"" (""MigrationId"", ""ProductVersion"")
VALUES ('20260818225000_RestaurarCatalogoCatalogo', '10.0.4')
ON CONFLICT (""MigrationId"") DO NOTHING;
            ");

            Console.WriteLine("Migration registrada no __EFMigrationsHistory.");

            // Verificar estado atual do histórico
            var historico = context.Database.SqlQueryRaw<MigrationEntry>(
                @"SELECT ""MigrationId"", ""ProductVersion"" FROM public.""__EFMigrationsHistory"" ORDER BY ""MigrationId"""
            ).ToList();

            Console.WriteLine("\n=== HISTORICO DE MIGRATIONS ===");
            foreach(var h in historico)
                Console.WriteLine($"  {h.MigrationId}");
        }
    }

    public class MigrationEntry
    {
        public string MigrationId { get; set; } = null!;
        public string ProductVersion { get; set; } = null!;
    }
}
