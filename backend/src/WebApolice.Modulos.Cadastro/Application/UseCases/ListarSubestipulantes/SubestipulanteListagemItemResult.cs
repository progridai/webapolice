using System;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.ListarSubestipulantes;

public sealed class SubestipulanteListagemItemResult
{
    public Guid PublicId { get; set; }
    public string Nome { get; set; } = null!;
    public string? Cnpj { get; set; }
    public string? Codigo { get; set; }
    public bool Ativo { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
