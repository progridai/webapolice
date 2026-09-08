using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Npgsql;
using WebApolice.SharedKernel.Application.Ports;

namespace WebApolice.Shared.Infrastructure.Persistence;

public class LocalidadeResolver : ILocalidadeResolver
{
    private readonly string _connectionString;

    public LocalidadeResolver(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("PostgreSql") 
            ?? throw new System.InvalidOperationException("Connection string 'PostgreSql' not found.");
    }

    public async Task<(long? CidadeId, long? EstadoId)> ResolverLocalidadeAsync(string cidade, string uf, CancellationToken cancellationToken)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        // Normalize string comparison to avoid accent/case issues where possible, 
        // though basic unaccent/upper is standard.
        const string sql = @"
            SELECT c.id, e.id 
            FROM core.cidade c
            INNER JOIN core.estado e ON c.estado_id = e.id
            WHERE upper(unaccent(c.nome)) = upper(unaccent(@cidade))
              AND upper(e.uf) = upper(@uf)
            LIMIT 1";

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("cidade", cidade);
        cmd.Parameters.AddWithValue("uf", uf);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        
        if (await reader.ReadAsync(cancellationToken))
        {
            return (reader.GetInt64(0), reader.GetInt64(1));
        }

        return (null, null);
    }
}
