using System;

namespace WebApolice.Modulos.Cadastro.Domain.Modelos;

/// <summary>
/// Modelo de persistência para leitura parcial da tabela cadastro.cliente_vinculo.
/// </summary>
public sealed class ClienteVinculoModel
{
    public ClienteVinculoModel() { }
    public ClienteVinculoModel(long clienteId, long pessoaId, long estipulanteId, string matricula)
    {
        PublicId = Guid.NewGuid(); ClienteId = clienteId; PessoaId = pessoaId;
        CriterioCriacao = "CADASTRO_MANUAL"; Ativo = true; CreatedAt = DateTime.UtcNow;
        Alterar(estipulanteId, matricula, true);
    }
    public void Alterar(long estipulanteId, string matricula, bool ativo)
    {
        EstipulanteId = estipulanteId; Matricula = matricula.Trim();
        MatriculaNormalizada = Matricula.ToUpperInvariant(); Ativo = ativo; UpdatedAt = DateTime.UtcNow;
    }
    public void DefinirSituacao(bool ativo) { Ativo = ativo; UpdatedAt = DateTime.UtcNow; }
    public void CompletarEstipulante(long estipulanteId) { EstipulanteId = estipulanteId; UpdatedAt = DateTime.UtcNow; }
    public long Id { get; private set; }
    public Guid PublicId { get; private set; } = Guid.NewGuid();
    public string? MatriculaNormalizada { get; private set; }
    public string CriterioCriacao { get; private set; } = "CADASTRO_MANUAL";
    public DateTime UpdatedAt { get; private set; }
    public long ClienteId { get; private set; }
    public long PessoaId { get; private set; }
    public long? EstipulanteId { get; private set; }
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
