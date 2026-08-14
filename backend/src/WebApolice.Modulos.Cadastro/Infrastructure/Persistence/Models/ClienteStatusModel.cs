namespace WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Models;

/// <summary>
/// Modelo de persistÃªncia para leitura da tabela cadastro.cliente_status.
/// Mapeamento parcial fiel aos tipos da base oficial.
/// </summary>
public sealed class ClienteStatusModel
{
    public short Id { get; private set; }
    public string Codigo { get; private set; } = null!;
    public string Nome { get; private set; } = null!;
    public bool Ativo { get; private set; }
}
