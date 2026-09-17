namespace WebApolice.Modulos.Seguro.Api.Controllers.Requests;

/// <summary>
/// Payload para alteração de um Subgrupo da Apólice.
/// Status (ativo/inativo) é gerenciado via endpoint dedicado /inativar.
/// </summary>
public class AlterarSubgrupoApoliceRequest
{
    public string Nome { get; set; } = null!;

    public string? Observacao { get; set; }
}
