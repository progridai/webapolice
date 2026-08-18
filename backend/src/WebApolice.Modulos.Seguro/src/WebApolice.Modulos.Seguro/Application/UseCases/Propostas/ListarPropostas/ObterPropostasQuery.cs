using System;
using MediatR;
using WebApolice.SharedKernel.Application.Models;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Propostas.ListarPropostas;

public class ObterPropostasQuery : IRequest<PagedResult<PropostaDto>>
{
    public int Pagina { get; set; } = 1;
    public int TamanhoPagina { get; set; } = 20;
    public string? TermoBusca { get; set; }
}
