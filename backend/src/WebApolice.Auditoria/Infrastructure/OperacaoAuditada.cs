using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using WebApolice.Auditoria.Contracts;
using WebApolice.Auditoria.Domain;

namespace WebApolice.Auditoria.Infrastructure;

/// <summary>Coordena negócio e auditoria na mesma conexão/transação física.</summary>
public sealed class OperacaoAuditada(AuditoriaDbContext auditoria, IRegistradorAuditoria registrador, IContextoAuditoria contexto)
{
    public async Task<T> ExecutarAsync<T>(DbContext negocio, string modulo, string recurso, string acao,
        Func<CancellationToken, Task<(T Resultado, string PublicId, object? Antes, object? Depois)>> executar,
        CancellationToken cancellationToken)
    {
        if (!ReferenceEquals(negocio.Database.GetDbConnection(), auditoria.Database.GetDbConnection()))
            throw new InvalidOperationException("Negócio e auditoria devem compartilhar a conexão.");

        await using var transacao = await negocio.Database.BeginTransactionAsync(cancellationToken);
        await auditoria.Database.UseTransactionAsync(transacao.GetDbTransaction(), cancellationToken);
        try
        {
            var resultado = await executar(cancellationToken);
            await registrador.RegistrarAsync(new RegistroAuditoria
            {
                DataHoraUtc = DateTime.UtcNow,
                UsuarioIdExterno = contexto.ObterUsuarioIdExterno(),
                UsuarioNome = contexto.ObterUsuarioNome(),
                TraceId = contexto.ObterTraceId(),
                CorrelationId = contexto.ObterCorrelationId(),
                EnderecoIp = contexto.ObterEnderecoIp(),
                Origem = contexto.ObterOrigem(),
                Modulo = modulo,
                Recurso = recurso,
                Acao = acao,
                RecursoId = resultado.PublicId,
                Resultado = ResultadoAuditoria.Sucesso,
                DadosAnteriores = resultado.Antes is null ? null : JsonSerializer.SerializeToDocument(resultado.Antes),
                DadosPosteriores = resultado.Depois is null ? null : JsonSerializer.SerializeToDocument(resultado.Depois)
            }, cancellationToken);
            await transacao.CommitAsync(cancellationToken);
            return resultado.Resultado;
        }
        catch
        {
            await transacao.RollbackAsync(CancellationToken.None);
            negocio.ChangeTracker.Clear();
            auditoria.ChangeTracker.Clear();
            throw;
        }
        finally
        {
            await auditoria.Database.UseTransactionAsync(null, CancellationToken.None);
        }
    }
}
