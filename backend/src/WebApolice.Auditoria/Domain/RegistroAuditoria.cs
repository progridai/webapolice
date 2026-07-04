using System.Text.Json;

namespace WebApolice.Auditoria.Domain;

public class RegistroAuditoria
{
    public long Id { get; set; }
    public DateTime DataHoraUtc { get; set; }
    public string? UsuarioIdExterno { get; set; }
    public string? UsuarioNome { get; set; }
    public string Acao { get; set; } = string.Empty;
    public string Modulo { get; set; } = string.Empty;
    public string Recurso { get; set; } = string.Empty;
    public string? RecursoId { get; set; }
    public ResultadoAuditoria Resultado { get; set; }
    public string? TraceId { get; set; }
    public string? CorrelationId { get; set; }
    public string? EnderecoIp { get; set; }
    public string? Origem { get; set; }
    
    // JSONB fields
    public JsonDocument? DadosAnteriores { get; set; }
    public JsonDocument? DadosPosteriores { get; set; }
    public JsonDocument? Metadados { get; set; }
    
    public string? MensagemErro { get; set; }
}
