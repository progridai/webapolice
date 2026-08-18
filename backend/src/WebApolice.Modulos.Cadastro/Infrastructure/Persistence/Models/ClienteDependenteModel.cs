using System;

namespace WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Models;

/// <summary>
/// Modelo de persistência para leitura parcial da tabela cadastro.cliente_dependente.
/// </summary>
public sealed class ClienteDependenteModel
{
    public long Id { get; private set; }
    public long ClienteId { get; private set; }
    public long? PessoaId { get; private set; }
    public string TipoRelacao { get; private set; } = null!;
    public string Nome { get; private set; } = null!;
    public string? Cpf { get; private set; }
    public DateOnly? DataNascimento { get; private set; }
    public DateTime CreatedAt { get; private set; }
}
