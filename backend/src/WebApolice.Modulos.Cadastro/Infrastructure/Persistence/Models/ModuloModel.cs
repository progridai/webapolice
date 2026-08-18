using System;

namespace WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Models;

public class ModuloModel
{
    public long Id { get; set; }
    public Guid PublicId { get; set; }
    
    public string Nome { get; set; } = null!;
    public string? Descricao { get; set; }
    
    public bool Ativo { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
