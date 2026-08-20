namespace WebApolice.Modulos.Cadastro.Application.UseCases.CriarCorretora;

public class CriarCorretoraCommand
{
    public string Nome { get; set; } = string.Empty;
    public string? Cnpj { get; set; }
    public string? Codigo { get; set; }
    public string? CodigoProtheus { get; set; }
    public string? Observacao { get; set; }
}
