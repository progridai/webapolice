using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebApolice.Modulos.Seguranca.Application.Ports;
using WebApolice.Modulos.Seguranca.Domain;

namespace WebApolice.Modulos.Seguranca.Application.Services;

public class ProvisionamentoUsuarioService : IProvisionamentoUsuarioService
{
    private readonly IContextoUsuarioAutenticado _contextoUsuarioAutenticado;
    private readonly IUsuarioProvisionamentoRepository _repository;
    private readonly ILogger<ProvisionamentoUsuarioService> _logger;

    public ProvisionamentoUsuarioService(
        IContextoUsuarioAutenticado contextoUsuarioAutenticado,
        IUsuarioProvisionamentoRepository repository,
        ILogger<ProvisionamentoUsuarioService> logger)
    {
        _contextoUsuarioAutenticado = contextoUsuarioAutenticado;
        _repository = repository;
        _logger = logger;
    }

    public async Task ProvisionarAsync(CancellationToken cancellationToken)
    {
        if (!_contextoUsuarioAutenticado.EstaAutenticado)
        {
            return;
        }

        var keycloakSub = _contextoUsuarioAutenticado.KeycloakSub;

        if (string.IsNullOrWhiteSpace(keycloakSub))
        {
            _logger.LogWarning("Usuário autenticado sem claim sub. Provisionamento ignorado.");
            return;
        }

        try
        {
            await ProcessarProvisionamentoAsync(keycloakSub, cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException is Npgsql.PostgresException pgEx &&
                pgEx.SqlState == "23505" &&
                pgEx.ConstraintName == "ix_usuario_keycloak_sub")
            {
                _logger.LogInformation("Concorrência no provisionamento detectada para sub. Reavaliando atualização.");
                
                // Limpamos o Change Tracker para descartar a entidade que falhou a inserção
                _repository.LimparRastreamento();
                
                // O usuário foi criado simultaneamente por outra requisição.
                // Buscamos o usuário recém-criado para atualização normal.
                var usuarioConcorrente = await _repository.ObterPorKeycloakSubParaAtualizacaoAsync(keycloakSub, cancellationToken);
                
                if (usuarioConcorrente != null)
                {
                    bool atualizado = usuarioConcorrente.AtualizarDadosIdentidade(
                        _contextoUsuarioAutenticado.Username,
                        _contextoUsuarioAutenticado.Nome,
                        _contextoUsuarioAutenticado.Email);
                        
                    if (atualizado)
                    {
                        await _repository.SalvarAlteracoesAsync(cancellationToken);
                        _logger.LogInformation("Dados cadastrais atualizados para o usuário concorrente: {KeycloakSub}", keycloakSub);
                    }
                }
                else
                {
                    // Se violou unique constraint mas o usuário não foi retornado (ex: deletado logo após?), relançamos.
                    throw;
                }
            }
            else
            {
                throw;
            }
        }
    }

    private async Task ProcessarProvisionamentoAsync(string keycloakSub, CancellationToken cancellationToken)
    {
        var username = _contextoUsuarioAutenticado.Username;
        var nome = _contextoUsuarioAutenticado.Nome;
        var email = _contextoUsuarioAutenticado.Email;

        var usuario = await _repository.ObterPorKeycloakSubParaAtualizacaoAsync(keycloakSub, cancellationToken);

        if (usuario == null)
        {
            usuario = Usuario.Criar(keycloakSub, username, nome, email);
            await _repository.AdicionarAsync(usuario, cancellationToken);
            await _repository.SalvarAlteracoesAsync(cancellationToken);
            _logger.LogInformation("Novo usuário provisionado no sistema via JIT: {KeycloakSub}", keycloakSub);
        }
        else
        {
            bool atualizado = usuario.AtualizarDadosIdentidade(username, nome, email);
            if (atualizado)
            {
                await _repository.SalvarAlteracoesAsync(cancellationToken);
                _logger.LogInformation("Dados cadastrais atualizados para o usuário: {KeycloakSub}", keycloakSub);
            }
        }
    }
}
