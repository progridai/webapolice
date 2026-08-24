using System;

namespace WebApolice.Modulos.Seguro.Api.Controllers.Requests;

/// <summary>
/// Payload para edição de uma Vida na Apólice.
/// Somente os campos de vigência e observação são editáveis.
/// Para mudar de contexto (ex: novo Subestipulante), encerre a participação atual e crie uma nova.
/// </summary>
public class AlterarApoliceVidaRequest
{
    public DateOnly? DataInicioVigencia { get; set; }
    public DateOnly? DataFimVigencia { get; set; }
    public string? Observacao { get; set; }
}
