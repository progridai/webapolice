using System.Collections.Generic;
using WebApolice.Modulos.Clientes.Domain;

namespace WebApolice.Modulos.Clientes.Application.UseCases.ListarClientes;

public sealed record ListarClientesQuery(
    int Pagina,
    int TamanhoPagina,
    string? Nome,
    string? Cpf,
    StatusCliente? Status,
    string? OrdenarPor,
    string? Direcao
);

public sealed record ListagemPaginadaResult<T>(
    IReadOnlyList<T> Itens,
    int PaginaAtual,
    int TamanhoPagina,
    int TotalItens,
    int TotalPaginas
);

public sealed record ClienteListagemItemResult(
    long Id,
    string Nome,
    string CpfMascarado,
    string Status,
    System.DateTime DataCadastroUtc
);
