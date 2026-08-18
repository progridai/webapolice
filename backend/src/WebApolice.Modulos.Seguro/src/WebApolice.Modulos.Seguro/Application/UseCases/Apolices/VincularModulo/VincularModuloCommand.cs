using System;
using MediatR;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.VincularModulo;

public class VincularModuloCommand : IRequest<long>
{
    public long ApoliceId { get; set; }
    public long ApoliceSubestipulanteId { get; set; }
    public long ModuloId { get; set; } // O ID real do Módulo global
    public Guid UsuarioPublicId { get; set; }
}
