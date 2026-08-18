using System;
using MediatR;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Propostas.AdicionarItem;

public class AdicionarPropostaItemCommand : IRequest<Guid>
{
    public Guid PropostaId { get; set; }
    public long ProdutoId { get; set; }
    public long PlanoId { get; set; }
    public decimal? Valor { get; set; }
}
