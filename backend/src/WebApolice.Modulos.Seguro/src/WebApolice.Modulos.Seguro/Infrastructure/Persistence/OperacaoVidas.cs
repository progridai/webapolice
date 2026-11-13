using WebApolice.Auditoria.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using WebApolice.Modulos.Cadastro.Contracts;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;
namespace WebApolice.Modulos.Seguro.Infrastructure.Persistence;

public sealed class OperacaoVidas(SeguroDbContext db, OperacaoAuditada operacao, ICadastroVinculosVida cadastro) : IOperacaoVidas
{
    public async Task<T> ExecutarAsync<T>(string acao,
        Func<CancellationToken, Task<(T Resultado, string PublicId, object? Antes, object? Depois)>> executar,
        CancellationToken ct)
    {
        try
        {
            return await operacao.ExecutarAsync(db, "Seguro", "apolice_vida", acao, async token =>
            {
                var transacao = db.Database.CurrentTransaction
                    ?? throw new InvalidOperationException("Transação da Vida ausente.");
                await cadastro.UsarTransacaoAsync(transacao.GetDbTransaction(), token);
                return await executar(token);
            }, ct);
        }
        catch
        {
            cadastro.DescartarAlteracoes();
            throw;
        }
        finally
        {
            await cadastro.UsarTransacaoAsync(null, CancellationToken.None);
        }
    }
}
