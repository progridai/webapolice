using System;
using MediatR;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.InativarModuloApolice;

public class InativarModuloApoliceCommand : IRequest<Unit>
{
    public Guid ApolicePublicId { get; set; }
    public Guid ApoliceModuloPublicId { get; set; }
    
    public Guid UsuarioPublicId { get; set; }
}
