using System;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.CriarApoliceVida;

public sealed class CriarApoliceVidaCommand
{
    public Guid ApolicePublicId { get; set; }
    public Guid ClientePublicId { get; set; }
    
    /// <summary>
    /// Contexto B e C: informe o PublicId do vínculo Apólice ↔ Subestipulante.
    /// Contexto A (direto): null.
    /// </summary>
    public Guid? SubestipulantePublicId { get; set; }

    /// <summary>
    /// Contexto C somente: informe o PublicId do vínculo Apólice ↔ Subestipulante ↔ Módulo.
    /// Contexto A e B: null.
    /// </summary>
    public Guid? ModuloPublicId { get; set; }
    
    public DateOnly? DataInicioVigencia { get; set; }
    public DateOnly? DataFimVigencia { get; set; }
    public string? Observacao { get; set; }
    public Guid UsuarioPublicId { get; set; }
}
