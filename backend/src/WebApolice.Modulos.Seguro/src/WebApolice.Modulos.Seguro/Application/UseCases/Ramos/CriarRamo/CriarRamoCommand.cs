namespace WebApolice.Modulos.Seguro.Application.UseCases.Ramos.CriarRamo;

public class CriarRamoCommand
{
    public string Codigo { get; }
    public string Nome { get; }
    public string? Descricao { get; }

    public CriarRamoCommand(string codigo, string nome, string? descricao)
    {
        Codigo = codigo;
        Nome = nome;
        Descricao = descricao;
    }
}
