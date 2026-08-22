using System;

using MediatR;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.InativarSubestipulante;

public class InativarSubestipulanteApoliceCommand : IRequest
{
    public Guid ApolicePublicId { get; set; }
    public Guid SubestipulantePublicId { get; set; }
    public Guid UsuarioPublicId { get; set; }
}
