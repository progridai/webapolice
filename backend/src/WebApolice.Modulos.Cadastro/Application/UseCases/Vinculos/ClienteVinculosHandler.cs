using WebApolice.Modulos.Cadastro.Application.Ports;
using WebApolice.Modulos.Cadastro.Contracts;
using WebApolice.Modulos.Cadastro.Domain.Exceptions;
using WebApolice.Modulos.Cadastro.Domain.Modelos;
using WebApolice.SharedKernel.Application.Models;
namespace WebApolice.Modulos.Cadastro.Application.UseCases.Vinculos;

public sealed record ClienteVinculoResult(Guid PublicId, string Matricula, bool Ativo,
    Guid? EstipulantePublicId, string? Estipulante, string? Grupo, string? Subgrupo, string? Lotacao)
{
    public IReadOnlyList<ApoliceVinculoCliente> Apolices { get; init; } = [];
}
public sealed record EstipulanteVinculoOpcao(Guid PublicId, string Nome);
public sealed record SalvarClienteVinculo(Guid EstipulantePublicId, string Matricula, bool Ativo = true);

public sealed class ClienteVinculosHandler(IClienteVinculosRepository repository,
    IClienteVinculosUso uso, IOperacaoCadastroAuditada operacao)
{
    public async Task<PagedResult<ClienteVinculoResult>> ListarAsync(Guid clienteId, int pagina, int tamanho, CancellationToken ct)
    {
        if (pagina < 1 || tamanho < 1 || tamanho > 100 || (long)(pagina - 1) * tamanho > int.MaxValue)
            throw new ClienteInvalidoException("Paginação inválida.");
        var cliente = await repository.ObterClienteAsync(clienteId, false, ct)
            ?? throw new ClienteNaoEncontradoException("Cliente não encontrado.");
        var paginaVinculos = await repository.ListarAsync(clienteId, pagina, tamanho, ct);
        var vinculos = paginaVinculos.Items.ToArray();
        var apolices = await uso.ListarApolicesAsync(cliente.Id, vinculos.Select(v => v.Id).ToArray(), ct);
        return new()
        {
            Items = vinculos.Select(v => v.Dados with { Apolices = apolices.GetValueOrDefault(v.Id) ?? [] }).ToArray(),
            Page = paginaVinculos.Page,
            PageSize = paginaVinculos.PageSize,
            TotalCount = paginaVinculos.TotalCount
        };
    }

    public async Task<PagedResult<EstipulanteVinculoOpcao>> EstipulantesAsync(Guid clienteId, string? busca, int pagina, int tamanho, CancellationToken ct)
    {
        if (pagina < 1 || tamanho < 1 || tamanho > 100 || (long)(pagina - 1) * tamanho > int.MaxValue)
            throw new ClienteInvalidoException("Paginação inválida.");
        if (await repository.ObterClienteAsync(clienteId, false, ct) is null)
            throw new ClienteNaoEncontradoException("Cliente não encontrado.");
        return await repository.ListarEstipulantesAsync(busca, pagina, tamanho, ct);
    }

    public Task<Guid> SituacaoAsync(Guid clienteId, Guid vinculoId, bool ativo, CancellationToken ct) =>
        operacao.ExecutarAsync<Guid>("Clientes", "cliente_vinculo", ativo ? "VINCULO_REATIVADO" : "VINCULO_INATIVADO", async token =>
        {
            var cliente = await repository.ObterClienteAsync(clienteId, true, token)
                ?? throw new ClienteNaoEncontradoException("Cliente não encontrado.");
            var vinculo = await repository.ObterAsync(clienteId, vinculoId, token)
                ?? throw new ClienteNaoEncontradoException("Vínculo não encontrado neste cliente.");
            if (ativo && (!vinculo.EstipulanteId.HasValue || string.IsNullOrWhiteSpace(vinculo.Matricula)))
                throw new ClienteInvalidoException("Complete estipulante e matrícula antes de reativar o vínculo.");
            if (ativo && await repository.DuplicadoAsync(cliente.Id, vinculo.EstipulanteId!.Value,
                (vinculo.Matricula ?? "").Trim().ToUpperInvariant(), vinculo.Id, token))
                throw new ClienteInvalidoException("Já existe um vínculo ativo com este estipulante e matrícula.");
            var antes = new { vinculo.PublicId, vinculo.Ativo };
            vinculo.DefinirSituacao(ativo);
            await repository.SalvarAsync(token);
            return (vinculo.PublicId, vinculo.PublicId.ToString(), antes, new { vinculo.PublicId, vinculo.Ativo });
        }, ct);

    public Task<Guid> SalvarAsync(Guid clienteId, Guid? vinculoId, SalvarClienteVinculo dados, CancellationToken ct) =>
        operacao.ExecutarAsync<Guid>("Clientes", "cliente_vinculo", vinculoId.HasValue ? "VINCULO_ALTERADO" : "VINCULO_CRIADO", async token =>
        {
            if (dados.EstipulantePublicId == Guid.Empty || string.IsNullOrWhiteSpace(dados.Matricula) || dados.Matricula.Trim().Length > 50)
                throw new ClienteInvalidoException("Informe estipulante e matrícula com até 50 caracteres.");
            // Serializa inclusões/edições do mesmo cliente, incluindo a verificação de duplicidade.
            var cliente = await repository.ObterClienteAsync(clienteId, true, token)
                ?? throw new ClienteNaoEncontradoException("Cliente não encontrado.");
            var estipulante = await repository.ObterEstipulanteAsync(dados.EstipulantePublicId, token)
                ?? throw new ClienteInvalidoException("Estipulante não encontrado.");
            var vinculo = vinculoId.HasValue ? await repository.ObterAsync(clienteId, vinculoId.Value, token)
                ?? throw new ClienteNaoEncontradoException("Vínculo não encontrado neste cliente.") : null;
            var mudouIdentidade = vinculo is null || vinculo.EstipulanteId != estipulante.Id || vinculo.Matricula != dados.Matricula.Trim();
            if (mudouIdentidade && (!estipulante.Ativo || estipulante.DeletedAt.HasValue))
                throw new ClienteInvalidoException("Selecione um estipulante ativo.");
            if (vinculo is not null && mudouIdentidade && await uso.EstaEmUsoAsync(vinculo.Id, token))
                throw new ClienteInvalidoException("Este vínculo já possui participação ou proposta. Cadastre outro vínculo para mudar matrícula ou estipulante.");
            if (dados.Ativo && await repository.DuplicadoAsync(cliente.Id, estipulante.Id, dados.Matricula.Trim().ToUpperInvariant(), vinculo?.Id, token))
                throw new ClienteInvalidoException("Já existe um vínculo ativo com este estipulante e matrícula.");
            var antes = vinculo is null ? null : new { vinculo.PublicId, vinculo.Ativo };
            if (vinculo is null) { vinculo = new ClienteVinculoModel(cliente.Id, cliente.PessoaId, estipulante.Id, dados.Matricula); repository.Adicionar(vinculo); }
            vinculo.Alterar(estipulante.Id, dados.Matricula, dados.Ativo);
            await repository.SalvarAsync(token);
            return (vinculo.PublicId, vinculo.PublicId.ToString(), antes, new { vinculo.PublicId, vinculo.Ativo });
        }, ct);
}
