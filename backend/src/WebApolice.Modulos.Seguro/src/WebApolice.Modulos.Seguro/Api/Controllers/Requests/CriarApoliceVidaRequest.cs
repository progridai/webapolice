using System;

namespace WebApolice.Modulos.Seguro.Api.Controllers.Requests;

/// <summary>
/// Payload para inclusão de um Cliente (Vida) em uma Apólice.
/// Subgrupo e Módulo são independentes e determinam as características do vínculo:
///   - Podem ser informados em qualquer combinação (nenhum, apenas um, ou ambos).
/// </summary>
public class CriarApoliceVidaRequest
{
    public Guid ClientePublicId { get; set; }
    public Guid? ApoliceSubgrupoPublicId { get; set; }
    public Guid? ApoliceModuloPublicId { get; set; }
    public DateOnly? DataInicioVigencia { get; set; }
    public DateOnly? DataFimVigencia { get; set; }
    public string? Observacao { get; set; }
}
