using System;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.AlterarSubestipulante;

public sealed class AlterarSubestipulanteCommand
{
    public Guid PublicId { get; set; }
    public string Nome { get; set; } = null!;
    public string? Cnpj { get; set; }
    public string? Codigo { get; set; }
    public string? Observacao { get; set; }
}
