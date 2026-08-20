namespace WebApolice.Modulos.Cadastro.Application.UseCases.CriarSubestipulante;

public sealed class CriarSubestipulanteCommand
{
    public string Nome { get; set; } = null!;
    public string? Cnpj { get; set; }
    public string? Codigo { get; set; }
    public string? Observacao { get; set; }
}
