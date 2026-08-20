namespace WebApolice.Modulos.Seguro.Api.Controllers.Requests;

public class CriarRamoRequest
{
    public string Codigo { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
}
