namespace WebApolice.Modulos.Cadastro.Application.UseCases.CriarSeguradora;

public class CriarSeguradoraCommand
{
    public string Nome { get; set; } = string.Empty;
    public string? Codigo { get; set; }
    public string? Susep { get; set; }
    public string? Cnpj { get; set; }
    public string? Observacao { get; set; }
}
