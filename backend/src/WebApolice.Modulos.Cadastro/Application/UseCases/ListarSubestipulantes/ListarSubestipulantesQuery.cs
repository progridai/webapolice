namespace WebApolice.Modulos.Cadastro.Application.UseCases.ListarSubestipulantes;

public sealed class ListarSubestipulantesQuery
{
    public int Pagina { get; set; } = 1;
    public int TamanhoPagina { get; set; } = 10;
    public string? Busca { get; set; }
    public bool? Ativo { get; set; }
}

public sealed record ListagemPaginadaResult<T>(
    System.Collections.Generic.IEnumerable<T> Itens,
    int PaginaAtual,
    int TamanhoPagina,
    int TotalItens,
    int TotalPaginas);

