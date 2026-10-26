using WebApolice.Modulos.Seguranca.Api.Authorization;
using WebApolice.Modulos.Cadastro.Application.Ports;
using WebApolice.Modulos.Cadastro.Application.UseCases.ConsultarCliente;
using WebApolice.Modulos.Cadastro.Application.UseCases.ListarClientes;
using WebApolice.Modulos.Seguranca.Application.DTOs;
using WebApolice.Modulos.Seguranca.Application.Ports;
using WebApolice.Modulos.Seguranca.Contracts;

namespace WebApolice.Api.Tests;

// Testes HTTP de JWT/authorization não conectam a um banco conhecido do host.
// As regras reais de JWT, middleware de autorização e PermissaoAuthorizationHandler são preservados.
internal sealed class ProvisionamentoIsolado : IProvisionamentoUsuarioService
{
    public Task ProvisionarAsync(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }
}

internal sealed class PermissoesIsoladas : IPermissoesEfetivasService
{
    public Task<PermissoesEfetivasUsuario> CalcularPermissoesAsync(string sub, CancellationToken cancellationToken = default) =>
        Task.FromResult(new PermissoesEfetivasUsuario(sub != "erp-ausente", sub != "erp-inativo",
            sub is "erp-total" or "erp-inativo", false, ["CLIENTES", "APOLICES"], [],
            sub == "erp-leitor" ? ["clientes.visualizar"] : []));
}

internal sealed class CatalogoIsolado : IDisponibilidadeCatalogo
{
    private static readonly HashSet<string> Permissoes = ["clientes.visualizar", "clientes.inserir", "clientes.alterar",
        "clientes.inativar", "clientes.reativar", "propostas.visualizar", "propostas.inserir", "propostas.alterar"];
    public Task<bool> PermissaoDisponivelAsync(string permissao, CancellationToken cancellationToken) => Task.FromResult(Permissoes.Contains(permissao));
    public Task<bool> RecursoDisponivelAsync(string modulo, string recurso, CancellationToken cancellationToken) => Task.FromResult(false);
}

internal sealed class ClientesQueriesIsoladas : IClientesQueries
{
    public Task<ConsultarClienteResult?> ObterDetalheAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult<ConsultarClienteResult?>(null);
    public Task<(ClienteListagemItemResult[] Itens, int TotalItens, int TotalPaginas)> ListarPaginadoAsync(int pagina,
        int tamanhoPagina, string? nome, string? documento, int? statusId, string? ordenarPor, string? direcao, CancellationToken cancellationToken) =>
        Task.FromResult<(ClienteListagemItemResult[], int, int)>(([], 0, 0));
}
