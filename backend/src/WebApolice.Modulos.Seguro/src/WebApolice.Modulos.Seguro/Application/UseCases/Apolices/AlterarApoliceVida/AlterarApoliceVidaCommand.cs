using System;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.AlterarApoliceVida;

/// <summary>
/// Campos permitidos para edição de uma Vida na Apólice.
/// Não é permitido alterar: ClientePublicId, ApolicePublicId, SubestipulantePublicId, ModuloPublicId.
/// Mudanças de contexto (ex: mover para outro Subestipulante) exigem encerrar a participação atual e criar nova.
/// </summary>
public sealed class AlterarApoliceVidaCommand
{
    public Guid ApolicePublicId { get; set; }
    public Guid ApoliceVidaPublicId { get; set; }
    public DateOnly? DataInicioVigencia { get; set; }
    public DateOnly? DataFimVigencia { get; set; }
    public string? Observacao { get; set; }
    public Guid UsuarioPublicId { get; set; }
}
