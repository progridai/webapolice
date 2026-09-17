using System;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarModulos;

public class ModuloApoliceDto
{
    public Guid PublicId { get; set; }
    public Guid ModuloPublicId { get; set; }
    public string Nome { get; set; } = null!;
    public string? Descricao { get; set; }
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public string? Observacao { get; set; }
    public bool Ativo { get; set; }
}
