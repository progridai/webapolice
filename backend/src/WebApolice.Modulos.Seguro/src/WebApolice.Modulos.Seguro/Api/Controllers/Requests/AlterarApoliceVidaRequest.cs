using System;

namespace WebApolice.Modulos.Seguro.Api.Controllers.Requests;

/// <summary>
/// Payload para edição de uma Vida na Apólice.
/// Regra: O Cliente não pode ser alterado. Para mudar o Cliente, encerre a participação e crie uma nova.
/// O Contexto (Subestipulante / Módulo) pode ser alterado.
/// </summary>
public class AlterarApoliceVidaRequest
{
    public DateOnly? DataInicioVigencia { get; set; }
    public DateOnly? DataFimVigencia { get; set; }
    public string? Observacao { get; set; }
    public string? Contexto { get; set; }
    public Guid? SubestipulantePublicId { get; set; }
    public Guid? ModuloPublicId { get; set; }
}
