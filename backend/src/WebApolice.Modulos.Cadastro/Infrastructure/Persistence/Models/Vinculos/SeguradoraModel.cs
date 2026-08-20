using System;

namespace WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Models.Vinculos;

public class SeguradoraModel
{
    public long Id { get; set; }
    public Guid PublicId { get; set; }
    public long PessoaId { get; set; }
    
    public string? Codigo { get; set; }
    public string? Susep { get; set; }
    
    public bool Ativo { get; set; } = true;
    public string? Observacao { get; set; }
    public int? LegadoId { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    
    public PessoaModel Pessoa { get; set; } = null!;
}
