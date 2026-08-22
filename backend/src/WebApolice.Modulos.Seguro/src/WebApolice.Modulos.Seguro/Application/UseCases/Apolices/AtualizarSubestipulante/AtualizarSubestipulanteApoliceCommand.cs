using System;

using MediatR;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.AtualizarSubestipulante;

public class AtualizarSubestipulanteApoliceCommand : IRequest
{
    public Guid ApolicePublicId { get; set; }
    public Guid SubestipulantePublicId { get; set; }
    public DateOnly? DataInicio { get; set; }
    public DateOnly? DataFim { get; set; }
    public Guid UsuarioPublicId { get; set; }
}
