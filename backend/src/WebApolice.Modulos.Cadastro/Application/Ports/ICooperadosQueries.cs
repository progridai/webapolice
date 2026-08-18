using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.SharedKernel.Application;

namespace WebApolice.Modulos.Cadastro.Application.Ports;

public sealed record CooperadoListDto(
    Guid PublicId,
    string Nome,
    string CpfMascarado,
    short Tipo,
    string? Codigo,
    bool Desativado,
    DateTimeOffset DataCadastroUtc
);

public interface ICooperadosQueries
{
    Task<WebApolice.Modulos.Cadastro.Application.UseCases.ListarClientes.ListagemPaginadaResult<CooperadoListDto>> ListarAsync(int pagina, int tamanhoPagina, string? termoBusca, short? tipo, CancellationToken cancellationToken);
}
