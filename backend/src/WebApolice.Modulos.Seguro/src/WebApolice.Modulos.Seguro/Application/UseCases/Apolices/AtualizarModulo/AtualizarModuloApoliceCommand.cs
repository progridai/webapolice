using System;
using MediatR;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.AtualizarModulo;

/// <summary>
/// Command para atualizar atributos contextuais do vínculo Módulo na Apólice.
/// Apenas DataInicio e DataFim são editáveis — ModuloPublicId é read-only após criação.
/// </summary>
public class AtualizarModuloApoliceCommand : IRequest
{
    public Guid ApolicePublicId { get; set; }
    public Guid SubestipulantePublicId { get; set; }
    public Guid ModuloPublicId { get; set; }
    public DateOnly? DataInicio { get; set; }
    public DateOnly? DataFim { get; set; }
    public Guid UsuarioPublicId { get; set; }
}
