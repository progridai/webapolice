using System;
using MediatR;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.InativarModulo;

/// <summary>
/// Command para inativar o vínculo contextual de um Módulo com um Subestipulante em uma Apólice.
/// Não inativa o Módulo Global, o Subestipulante, a Apólice ou qualquer Vida associada.
/// </summary>
public class InativarModuloApoliceCommand : IRequest
{
    public Guid ApolicePublicId { get; set; }
    public Guid SubestipulantePublicId { get; set; }
    public Guid ModuloPublicId { get; set; }
    public Guid UsuarioPublicId { get; set; }
}
