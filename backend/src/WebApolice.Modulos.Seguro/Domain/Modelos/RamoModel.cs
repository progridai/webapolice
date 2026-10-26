using System;

namespace WebApolice.Modulos.Seguro.Domain.Modelos;

public class RamoModel
{
    public long Id { get; set; }
    public Guid PublicId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public bool Ativo { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
