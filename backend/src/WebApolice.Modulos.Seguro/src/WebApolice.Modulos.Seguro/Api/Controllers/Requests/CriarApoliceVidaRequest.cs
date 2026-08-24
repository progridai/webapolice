using System;

namespace WebApolice.Modulos.Seguro.Api.Controllers.Requests;

/// <summary>
/// Payload para inclusão de um Cliente (Vida) em uma Apólice.
/// SubestipulantePublicId e ModuloPublicId determinam o contexto de participação:
///   - Ambos null         → Contexto A: Direto (apolice + cliente)
///   - Apenas Sub         → Contexto B: Subestipulante
///   - Sub + Modulo       → Contexto C: Subestipulante + Módulo
/// </summary>
public class CriarApoliceVidaRequest
{
    public Guid ClientePublicId { get; set; }
    public Guid? SubestipulantePublicId { get; set; }
    public Guid? ModuloPublicId { get; set; }
    public DateOnly? DataInicioVigencia { get; set; }
    public DateOnly? DataFimVigencia { get; set; }
    public string? Observacao { get; set; }
}
