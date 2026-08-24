using System;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.InativarApoliceVida;

public sealed class InativarApoliceVidaCommand
{
    public Guid ApolicePublicId { get; set; }
    public Guid ApoliceVidaPublicId { get; set; }
    public Guid UsuarioPublicId { get; set; }
}
