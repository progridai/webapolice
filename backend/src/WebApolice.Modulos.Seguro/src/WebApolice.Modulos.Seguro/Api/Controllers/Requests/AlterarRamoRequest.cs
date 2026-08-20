namespace WebApolice.Modulos.Seguro.Api.Controllers.Requests;

public class AlterarRamoRequest
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
}
