using System;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.ListarCorretoras;

public class CorretoraListagemItemResult
{
    public Guid PublicId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Cnpj { get; set; }
    public string? Codigo { get; set; }
    public string? CodigoProtheus { get; set; }
    public bool Ativo { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
