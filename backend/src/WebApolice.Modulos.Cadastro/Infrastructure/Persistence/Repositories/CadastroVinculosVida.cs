using System.Data.Common;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WebApolice.Auditoria.Contracts;
using WebApolice.Auditoria.Domain;
using WebApolice.Modulos.Cadastro.Application.Ports;
using WebApolice.Modulos.Cadastro.Contracts;
using WebApolice.Modulos.Cadastro.Domain.Exceptions;
using WebApolice.Modulos.Cadastro.Domain.Modelos;

namespace WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Repositories;

public sealed class CadastroVinculosVida(CadastroDbContext db, IClienteVinculosRepository repository,
    IRegistradorAuditoria auditoria, IContextoAuditoria contexto, IClienteVinculosUso uso) : ICadastroVinculosVida
{
    public async Task UsarTransacaoAsync(DbTransaction? transacao, CancellationToken ct)
    {
        if (transacao is not null && !ReferenceEquals(transacao.Connection, db.Database.GetDbConnection()))
            throw new InvalidOperationException("Cadastro e Vida devem compartilhar a conexão.");
        await db.Database.UseTransactionAsync(transacao, ct);
    }

    public void DescartarAlteracoes() => db.ChangeTracker.Clear();

    public async Task<VinculoClienteSeguro> CriarAsync(long clienteId, long estipulanteId, string matricula, CancellationToken ct)
    {
        if (db.Database.CurrentTransaction is null)
            throw new InvalidOperationException("A criação do vínculo exige a transação da Vida.");
        if (string.IsNullOrWhiteSpace(matricula) || matricula.Trim().Length > 50)
            throw new ClienteInvalidoException("Informe a matrícula com até 50 caracteres.");

        // Mesmo lock utilizado na manutenção do Cliente: serializa a checagem de duplicidade.
        var clientePublicId = await db.Clientes.Where(c => c.Id == clienteId && c.DeletedAt == null)
            .Select(c => c.PublicId).SingleOrDefaultAsync(ct);
        var cliente = await repository.ObterClienteAsync(clientePublicId, true, ct)
            ?? throw new ClienteInvalidoException("Cliente não encontrado.");
        if (!await db.Pessoas.AnyAsync(p => p.Id == cliente.PessoaId && p.DeletedAt == null, ct))
            throw new ClienteInvalidoException("Cliente não encontrado.");
        var estipulante = await db.Estipulantes.AsNoTracking().SingleOrDefaultAsync(e => e.Id == estipulanteId, ct);
        if (estipulante is null || !estipulante.Ativo || estipulante.DeletedAt is not null)
            throw new ClienteInvalidoException("O estipulante da apólice precisa estar ativo para cadastrar uma matrícula.");
        if (await repository.DuplicadoAsync(cliente.Id, estipulanteId, matricula.Trim().ToUpperInvariant(), null, ct))
            throw new ClienteInvalidoException("Já existe um vínculo ativo com esta matrícula no estipulante da apólice. Selecione o vínculo existente.");

        var vinculo = new ClienteVinculoModel(cliente.Id, cliente.PessoaId, estipulanteId, matricula);
        repository.Adicionar(vinculo);
        await repository.SalvarAsync(ct);
        await AuditarAsync(vinculo, "VINCULO_CRIADO", null, ct);
        return Resultado(vinculo, estipulante.Nome);
    }

    public async Task<VinculoClienteSeguro> CompletarAsync(long clienteId, Guid vinculoPublicId, long estipulanteId, CancellationToken ct)
    {
        if (db.Database.CurrentTransaction is null)
            throw new InvalidOperationException("Completar o vínculo exige a transação da Vida.");
        var clientePublicId = await db.Clientes.Where(c => c.Id == clienteId && c.DeletedAt == null)
            .Select(c => c.PublicId).SingleOrDefaultAsync(ct);
        var cliente = await repository.ObterClienteAsync(clientePublicId, true, ct)
            ?? throw new ClienteInvalidoException("Cliente não encontrado.");
        var vinculo = await repository.ObterAsync(cliente.PublicId, vinculoPublicId, ct)
            ?? throw new ClienteInvalidoException("Vínculo não encontrado neste cliente.");
        if (!vinculo.Ativo || vinculo.PessoaId != cliente.PessoaId || string.IsNullOrWhiteSpace(vinculo.Matricula)
            || vinculo.Matricula.Length > 50 || !await db.Pessoas.AnyAsync(p => p.Id == cliente.PessoaId && p.DeletedAt == null, ct))
            throw new ClienteInvalidoException("Selecione uma matrícula ativa e preenchida do cliente.");
        var estipulante = await db.Estipulantes.AsNoTracking().SingleOrDefaultAsync(e => e.Id == estipulanteId, ct);
        if (estipulante is null || !estipulante.Ativo || estipulante.DeletedAt is not null)
            throw new ClienteInvalidoException("O estipulante da apólice precisa estar ativo para completar o vínculo.");
        if (vinculo.EstipulanteId == estipulanteId) return Resultado(vinculo, estipulante.Nome);
        if (vinculo.EstipulanteId.HasValue)
            throw new ClienteInvalidoException("Este vínculo já pertence a outro estipulante. Selecione uma matrícula compatível.");
        if (await uso.EstaEmUsoAsync(vinculo.Id, ct))
            throw new ClienteInvalidoException("Este vínculo já possui participação ou proposta e não pode receber outro estipulante.");
        if (await repository.DuplicadoAsync(cliente.Id, estipulanteId, vinculo.Matricula.Trim().ToUpperInvariant(), vinculo.Id, ct))
            throw new ClienteInvalidoException("Já existe um vínculo ativo com esta matrícula no estipulante da apólice. Selecione o vínculo existente.");

        var antes = new { vinculo.PublicId, vinculo.EstipulanteId };
        vinculo.CompletarEstipulante(estipulanteId);
        await repository.SalvarAsync(ct);
        await AuditarAsync(vinculo, "VINCULO_COMPLETADO", antes, ct);
        return Resultado(vinculo, estipulante.Nome);
    }

    private Task AuditarAsync(ClienteVinculoModel vinculo, string acao, object? antes, CancellationToken ct) =>
        auditoria.RegistrarAsync(new RegistroAuditoria
        {
            DataHoraUtc = DateTime.UtcNow,
            UsuarioIdExterno = contexto.ObterUsuarioIdExterno(),
            UsuarioNome = contexto.ObterUsuarioNome(),
            TraceId = contexto.ObterTraceId(),
            CorrelationId = contexto.ObterCorrelationId(),
            EnderecoIp = contexto.ObterEnderecoIp(),
            Origem = contexto.ObterOrigem(),
            Modulo = "Clientes",
            Recurso = "cliente_vinculo",
            Acao = acao,
            RecursoId = vinculo.PublicId.ToString(),
            Resultado = ResultadoAuditoria.Sucesso,
            DadosAnteriores = antes is null ? null : JsonSerializer.SerializeToDocument(antes),
            DadosPosteriores = JsonSerializer.SerializeToDocument(new { vinculo.PublicId, vinculo.Ativo, vinculo.EstipulanteId })
        }, ct);

    private static VinculoClienteSeguro Resultado(ClienteVinculoModel vinculo, string estipulanteNome) =>
        new()
        {
            Id = vinculo.Id, PublicId = vinculo.PublicId, ClienteId = vinculo.ClienteId, PessoaId = vinculo.PessoaId,
            EstipulanteId = vinculo.EstipulanteId, Matricula = vinculo.Matricula!, Ativo = true,
            Estipulante = estipulanteNome, EstipulanteAtivo = true
        };
}
