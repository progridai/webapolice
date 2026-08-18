using System;
using MediatR;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.VincularSubestipulante;

public class VincularSubestipulanteCommand : IRequest<long>
{
    public long ApoliceId { get; set; }
    public long SubestipulanteId { get; set; }
    public Guid UsuarioPublicId { get; set; }
}
