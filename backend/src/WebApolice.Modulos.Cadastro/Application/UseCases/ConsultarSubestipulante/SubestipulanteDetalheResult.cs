using System;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.ConsultarSubestipulante;

public sealed class SubestipulanteDetalheResult
{
    public Guid PublicId { get; set; }
    public string Nome { get; set; } = null!;
    public string? Cnpj { get; set; }
    public string? CnpjLimpo { get; set; }
    public string? Codigo { get; set; }
    public bool Ativo { get; set; }
    public string? Observacao { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
