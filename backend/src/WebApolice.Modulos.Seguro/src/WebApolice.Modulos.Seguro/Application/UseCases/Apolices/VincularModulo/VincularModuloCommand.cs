using System;
using MediatR;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.VincularModulo;

/// <summary>
/// Command para vincular um Módulo Global a um Subestipulante no contexto de uma Apólice.
/// Identificação externa via PublicIds — nenhum ID interno (bigint) é exposto.
/// </summary>
public class VincularModuloApoliceCommand : IRequest<long>
{
    public Guid ApolicePublicId { get; set; }
    public Guid SubestipulantePublicId { get; set; }
    public Guid ModuloPublicId { get; set; }
    public DateOnly? DataInicio { get; set; }
    public DateOnly? DataFim { get; set; }
    public Guid UsuarioPublicId { get; set; }
}
