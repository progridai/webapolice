using Microsoft.EntityFrameworkCore.Storage;
using WebApolice.Auditoria.Contracts;
using WebApolice.Auditoria.Domain;
using WebApolice.Modulos.Cadastro.Application.Ports;

namespace WebApolice.Integration.Tests.Setup;

// Apenas para testes de orquestração com repositórios substituídos. Não comprova atomicidade física.
internal sealed class OperacaoCadastroSimulada(IRegistradorAuditoria registrador, WebApolice.SharedKernel.Application.Persistence.ITransacaoPersistencia transacao) : IOperacaoCadastroAuditada
{
    public async Task<T> ExecutarAsync<T>(string modulo, string recurso, string acao,
        Func<CancellationToken, Task<(T Resultado, string PublicId, object? Antes, object? Depois)>> executar,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await executar(cancellationToken);
            await registrador.RegistrarAsync(new RegistroAuditoria
            {
                Modulo = modulo, Recurso = recurso, Acao = acao, RecursoId = resultado.PublicId,
                Resultado = ResultadoAuditoria.Sucesso,
                DadosPosteriores = resultado.Depois is null ? null : System.Text.Json.JsonSerializer.SerializeToDocument(resultado.Depois)
            }, cancellationToken);
            await transacao.CommitAsync(cancellationToken);
            return resultado.Resultado;
        }
        catch
        {
            await transacao.RollbackAsync(CancellationToken.None);
            throw;
        }
    }
}
