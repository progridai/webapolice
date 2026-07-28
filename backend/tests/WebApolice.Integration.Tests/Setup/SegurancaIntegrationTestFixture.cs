using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using WebApolice.Modulos.Seguranca.Infrastructure.Persistence;
using Xunit;

namespace WebApolice.Integration.Tests.Setup;

public class SegurancaIntegrationTestFixture : IAsyncLifetime
{
    public SegurancaDbContext DbContext { get; private set; } = default!;
    public bool IsSharedDatabase { get; private set; }

    public async Task InitializeAsync()
    {
        var testConnString = Environment.GetEnvironmentVariable("ConnectionStrings__PostgreSqlTestes");
        var devConnString = Environment.GetEnvironmentVariable("ConnectionStrings__PostgreSql");
        var sharedAllowed = Environment.GetEnvironmentVariable("Testes__PermitirBancoCompartilhado");

        if (string.IsNullOrWhiteSpace(testConnString))
        {
            throw new InvalidOperationException("A connection string de testes (ConnectionStrings__PostgreSqlTestes) não está configurada.");
        }

        var testBuilder = new NpgsqlConnectionStringBuilder(testConnString);
        
        if (testBuilder.Host?.ToLowerInvariant() == "producao" || testBuilder.Database?.ToLowerInvariant() == "producao" || testBuilder.Host?.Contains("prod") == true)
        {
            throw new InvalidOperationException("O banco alvo parece ser de produção. Abortando testes.");
        }

        if (!string.IsNullOrWhiteSpace(devConnString))
        {
            var devBuilder = new NpgsqlConnectionStringBuilder(devConnString);
            bool areSame = testBuilder.Host == devBuilder.Host && 
                           testBuilder.Port == devBuilder.Port && 
                           testBuilder.Database == devBuilder.Database;

            if (areSame)
            {
                if (sharedAllowed != "true")
                {
                    throw new InvalidOperationException("Os bancos de desenvolvimento e testes são iguais, mas Testes__PermitirBancoCompartilhado não está habilitado explicitamente para 'true'.");
                }
                IsSharedDatabase = true;
            }
        }

        var options = new DbContextOptionsBuilder<SegurancaDbContext>()
            .UseNpgsql(testConnString)
            .UseSnakeCaseNamingConvention()
            .Options;

        DbContext = new SegurancaDbContext(options);

        // Se o banco for novo (não compartilhado), poderia migrar. 
        // Como o usuário instruiu não recriar banco nem dar Ensures, apenas garantimos que as migrations estão aplicadas se for possível.
        // Num banco compartilhado a API principal já migrou, mas MigrateAsync() é idempotente.
        await DbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        if (DbContext != null)
        {
            await DbContext.DisposeAsync();
        }
    }
}
