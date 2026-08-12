using System;

namespace WebApolice.Modulos.Estipulantes.Infrastructure.Persistence.Models;

public class EstipulanteModel
{
    public long Id { get; set; }
    public Guid PublicId { get; set; }
    public long? PessoaId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? NomeFormatado { get; set; }
    public string? Codigo { get; set; }
    public short? TipoPessoa { get; set; }
    public string? Cnpj { get; set; }
    public string? CnpjLimpo { get; set; }
    public long? CidadeId { get; set; }
    public long? GrupoId { get; set; }
    public long? SeguradoraId { get; set; }
    public bool Ativo { get; set; }
    public string? Observacao { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    
    // Navigation properties for EF
    public PessoaModel? Pessoa { get; set; }
    public EstipulanteConfiguracaoModel? Configuracao { get; set; }
}
