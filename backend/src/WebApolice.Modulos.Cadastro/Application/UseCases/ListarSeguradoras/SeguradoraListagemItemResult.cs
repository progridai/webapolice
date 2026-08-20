using System;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.ListarSeguradoras;

public class SeguradoraListagemItemResult
{
    public Guid PublicId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Codigo { get; set; }
    public string? Cnpj { get; set; }
    public string? Susep { get; set; }
    public bool Ativo { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
