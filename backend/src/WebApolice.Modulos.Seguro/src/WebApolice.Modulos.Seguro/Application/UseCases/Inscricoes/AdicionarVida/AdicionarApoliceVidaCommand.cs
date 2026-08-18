using System;
using MediatR;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Inscricoes.AdicionarVida;

public class AdicionarApoliceVidaCommand : IRequest<long>
{
    public long ApoliceId { get; set; }
    public long? ApoliceSubestipulanteId { get; set; }
    public long? ApoliceSubestipulanteModuloId { get; set; }
    
    public long ClienteId { get; set; }
    
    // Configurações e Vigências
    public DateTime DataInclusao { get; set; }
    
    public Guid UsuarioPublicId { get; set; }
}
