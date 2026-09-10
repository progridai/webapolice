using System;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.AlterarApoliceVida;

/// <summary>
/// Campos permitidos para edição de uma Vida na Apólice.
/// Não é permitido alterar o Cliente (ClientePublicId). O contexto (Subestipulante e Módulo) pode ser alterado.
/// </summary>
public sealed class AlterarApoliceVidaCommand
{
    public Guid ApolicePublicId { get; set; }
    public Guid ApoliceVidaPublicId { get; set; }
    public DateOnly? DataInicioVigencia { get; set; }
    public DateOnly? DataFimVigencia { get; set; }
    public string? Observacao { get; set; }
    public string? Contexto { get; set; }
    public Guid? SubestipulantePublicId { get; set; }
    public Guid? ModuloPublicId { get; set; }
    public Guid UsuarioPublicId { get; set; }
}
