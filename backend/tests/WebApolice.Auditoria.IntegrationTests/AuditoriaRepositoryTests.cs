using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WebApolice.Auditoria.Domain;
using WebApolice.Auditoria.Infrastructure;
using Xunit;
using Testcontainers.PostgreSql;

namespace WebApolice.Auditoria.IntegrationTests;

public class AuditoriaRepositoryTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("auditoria_db")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private AuditoriaDbContext? _dbContext;

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        
        var options = new DbContextOptionsBuilder<AuditoriaDbContext>()
            .UseNpgsql(_dbContainer.GetConnectionString(), o => 
            {
                o.MigrationsHistoryTable("__EFMigrationsHistory", "auditoria");
            })
            .UseSnakeCaseNamingConvention()
            .Options;
            
        _dbContext = new AuditoriaDbContext(options);
        
        // Aplica as migrations em um banco de dados vazio
        await _dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        if (_dbContext != null)
        {
            await _dbContext.DisposeAsync();
        }
        await _dbContainer.DisposeAsync();
    }

    [Fact]
    public async Task Deve_Gravar_RegistroDeAuditoria_ComSucesso_NoBancoPostgreSQL()
    {
        // Arrange
        var jsonStr = """{ "teste": "valor" }""";
        var doc = JsonDocument.Parse(jsonStr);

        var registro = new RegistroAuditoria
        {
            Acao = "criar",
            Modulo = "clientes",
            Recurso = "cliente",
            Resultado = ResultadoAuditoria.Sucesso,
            DataHoraUtc = DateTime.UtcNow,
            DadosAnteriores = doc,
            EnderecoIp = "127.0.0.1",
            TraceId = "trace-123"
        };

        var registrador = new RegistradorAuditoria(_dbContext!);

        // Act
        await registrador.RegistrarAsync(registro);

        // Assert
        var registroSalvo = await _dbContext!.RegistrosAuditoria.SingleOrDefaultAsync(r => r.TraceId == "trace-123");
        Assert.NotNull(registroSalvo);
        Assert.Equal("criar", registroSalvo.Acao);
        Assert.NotNull(registroSalvo.DadosAnteriores);
        Assert.Contains("valor", registroSalvo.DadosAnteriores.RootElement.GetRawText());
    }

    [Fact]
    public async Task Nomes_Fisicos_Devem_Estar_Em_Portugues_SnakeCase()
    {
        // Assert
        var conn = _dbContext!.Database.GetDbConnection();
        await conn.OpenAsync();
        
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT table_name FROM information_schema.tables WHERE table_schema = 'auditoria' AND table_name = 'registros_auditoria';";
        
        var result = await cmd.ExecuteScalarAsync();
        Assert.Equal("registros_auditoria", result);
    }
}
