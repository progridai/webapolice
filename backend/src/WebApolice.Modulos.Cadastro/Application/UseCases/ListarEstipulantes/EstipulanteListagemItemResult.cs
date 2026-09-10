using System;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.ListarEstipulantes;

public class EstipulanteListagemItemResult
{
    public long Id { get; set; }
    public Guid PublicId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Cnpj { get; set; }
    public bool Ativo { get; set; }
}
