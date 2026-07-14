using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace WebApolice.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LocalidadesController : ControllerBase
{
    private readonly string _connectionString;

    public LocalidadesController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("PostgreSql") 
            ?? throw new System.InvalidOperationException("Connection string 'PostgreSql' not found.");
    }

    [HttpGet("cidades")]
    public async Task<IActionResult> ObterCidades([FromQuery] string uf, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(uf) || uf.Length != 2)
        {
            return BadRequest(new { Message = "O parâmetro 'uf' é obrigatório e deve conter 2 caracteres." });
        }

        var cidades = new List<CidadeResult>();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        // Busca o ID do estado pela sigla e depois as cidades correspondentes
        const string sql = @"
            SELECT c.id, c.nome 
            FROM core.cidade c
            INNER JOIN core.estado e ON c.estado_id = e.id
            WHERE upper(e.uf) = upper(@uf)
            ORDER BY c.nome";

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("uf", uf);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            cidades.Add(new CidadeResult(
                reader.GetInt64(0),
                reader.GetString(1)
            ));
        }

        return Ok(cidades);
    }
}

public record CidadeResult(long Id, string Nome);
