using System;
using MediatR;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.AlterarModuloApolice;

public class AlterarModuloApoliceCommand : IRequest<Unit>
{
    public Guid ApolicePublicId { get; set; }
    public Guid ApoliceModuloPublicId { get; set; }
    
    public DateOnly? DataInicio { get; set; }
    public DateOnly? DataFim { get; set; }
    public string? Observacao { get; set; }
    
    public Guid UsuarioPublicId { get; set; }
}
