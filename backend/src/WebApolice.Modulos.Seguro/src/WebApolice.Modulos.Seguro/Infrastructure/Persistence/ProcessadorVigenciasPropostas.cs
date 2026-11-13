using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebApolice.Auditoria.Contracts;
using WebApolice.Auditoria.Infrastructure;
using WebApolice.Modulos.Cadastro.Contracts;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas.EfetivarVigencia;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas.EncerrarProposta;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Modulos.Seguro.Infrastructure.Persistence;

public sealed class ProcessadorVigenciasPropostas(IServiceScopeFactory scopes, IOptions<ConfiguracaoPropostas> configuracao,
    TimeProvider relogio, ILogger<ProcessadorVigenciasPropostas> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!configuracao.Value.ProcessarAgendamentos) return;
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1), relogio);
        do
        {
            try { await ProcessarAsync(stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception ex) { logger.LogError(ex, "Não foi possível processar a fila de vigências de propostas. Uma nova tentativa ocorrerá no próximo ciclo."); }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    public async Task ProcessarAsync(CancellationToken ct)
    {
        var codigo = configuracao.Value.StatusConfirmadaCodigo;
        if (string.IsNullOrWhiteSpace(codigo)) return;
        await ProcessarCancelamentosAsync(codigo, ct);
        long cursor = 0;
        var hoje = VigenciaPropostas.Hoje(relogio);
        while (!ct.IsCancellationRequested)
        {
            IReadOnlyList<AgendamentoVigenciaProposta> pagina;
            await using (var leitura = scopes.CreateAsyncScope())
                pagina = await leitura.ServiceProvider.GetRequiredService<IPropostasRepository>().ListarAgendadasAsync(hoje, codigo, cursor, ct);
            if (pagina.Count == 0) return;
            foreach (var registro in pagina)
            {
                ct.ThrowIfCancellationRequested(); cursor = registro.Posicao;
                await using var escopo = scopes.CreateAsyncScope();
                var sp = escopo.ServiceProvider;
                var operacao = new OperacaoPropostas(sp.GetRequiredService<SeguroDbContext>(), new OperacaoAuditada(
                    sp.GetRequiredService<AuditoriaDbContext>(), sp.GetRequiredService<IRegistradorAuditoria>(), new ContextoAgendamento()));
                var handler = new EfetivarVigenciaPropostaHandler(sp.GetRequiredService<IPropostasRepository>(), operacao,
                    sp.GetRequiredService<IClientesPropostas>(), sp.GetRequiredService<ICadastroSeguroQueries>(), configuracao, relogio);
                try { await handler.Handle(new(registro.PublicId, registro.RevisaoPreenchimento), ct); }
                catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
                catch (ValidacaoException) { logger.LogInformation("A proposta {PublicId} mudou durante o processamento da vigência; será reavaliada no próximo ciclo.", registro.PublicId); }
                catch (Exception ex) { logger.LogError(ex, "Falha ao efetivar a vigência da proposta {PublicId}. A transação foi revertida e haverá nova tentativa.", registro.PublicId); }
            }
        }
    }

    private async Task ProcessarCancelamentosAsync(string codigo, CancellationToken ct)
    {
        long cursor = 0; var hoje = VigenciaPropostas.Hoje(relogio);
        while (!ct.IsCancellationRequested)
        {
            IReadOnlyList<AgendamentoVigenciaProposta> pagina;
            await using (var leitura = scopes.CreateAsyncScope())
                pagina = await leitura.ServiceProvider.GetRequiredService<IPropostasRepository>().ListarCancelamentosAgendadosAsync(hoje, codigo, cursor, ct);
            if (pagina.Count == 0) return;
            foreach (var registro in pagina)
            {
                ct.ThrowIfCancellationRequested(); cursor = registro.Posicao;
                await using var escopo = scopes.CreateAsyncScope(); var sp = escopo.ServiceProvider;
                var operacao = new OperacaoPropostas(sp.GetRequiredService<SeguroDbContext>(), new OperacaoAuditada(
                    sp.GetRequiredService<AuditoriaDbContext>(), sp.GetRequiredService<IRegistradorAuditoria>(), new ContextoAgendamento()));
                var handler = new EfetivarCancelamentoVigenciaHandler(sp.GetRequiredService<IPropostasRepository>(), operacao, configuracao, relogio);
                try { await handler.Handle(new(registro.PublicId, registro.RevisaoPreenchimento), ct); }
                catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
                catch (ValidacaoException) { logger.LogInformation("O cancelamento da proposta {PublicId} mudou durante o processamento; será reavaliado no próximo ciclo.", registro.PublicId); }
                catch (Exception ex) { logger.LogError(ex, "Falha ao cancelar a vigência da proposta {PublicId}. A transação foi revertida e haverá nova tentativa.", registro.PublicId); }
            }
        }
    }

    private sealed class ContextoAgendamento : IContextoAuditoria
    {
        public string? ObterUsuarioIdExterno() => "sistema:vigencias-propostas";
        public string? ObterUsuarioNome() => "Processamento de Vigências";
        public string? ObterTraceId() => Activity.Current?.Id;
        public string? ObterCorrelationId() => null;
        public string? ObterEnderecoIp() => null;
        public string? ObterOrigem() => "Agendamento de Vigências";
    }
}
