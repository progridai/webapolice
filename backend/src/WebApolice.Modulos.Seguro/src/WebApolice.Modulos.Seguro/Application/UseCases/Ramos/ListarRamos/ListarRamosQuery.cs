namespace WebApolice.Modulos.Seguro.Application.UseCases.Ramos.ListarRamos;

public class ListarRamosQuery
{
    public int Pagina { get; }
    public int TamanhoPagina { get; }
    public string? Busca { get; }
    public bool? Ativo { get; }

    public ListarRamosQuery(int pagina = 1, int tamanhoPagina = 10, string? busca = null, bool? ativo = null)
    {
        Pagina = pagina < 1 ? 1 : pagina;
        TamanhoPagina = tamanhoPagina < 1 ? 10 : tamanhoPagina;
        Busca = busca;
        Ativo = ativo;
    }
}
