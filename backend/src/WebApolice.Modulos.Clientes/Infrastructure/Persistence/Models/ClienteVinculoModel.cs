using System;

namespace WebApolice.Modulos.Clientes.Infrastructure.Persistence.Models;

/// <summary>
/// Modelo de persistência para leitura parcial da tabela cadastro.cliente_vinculo.
/// </summary>
public sealed class ClienteVinculoModel
{
    public long Id { get; private set; }
    public long ClienteId { get; private set; }
    public long PessoaId { get; private set; }
    public long? EstipulanteId { get; private set; }
    public long? SubestipulanteId { get; private set; }
    public long? GrupoId { get; private set; }
    public long? SubgrupoId { get; private set; }
    public long? LotacaoId { get; private set; }
    public string? Matricula { get; private set; }
    public long? BancoId { get; private set; }
    public string? Agencia { get; private set; }
    public string? ContaCorrente { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime CreatedAt { get; private set; }
}
