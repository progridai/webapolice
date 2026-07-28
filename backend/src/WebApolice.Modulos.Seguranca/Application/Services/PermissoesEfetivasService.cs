using System;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Seguranca.Application.DTOs;
using WebApolice.Modulos.Seguranca.Application.Ports;

namespace WebApolice.Modulos.Seguranca.Application.Services;

public class PermissoesEfetivasService : IPermissoesEfetivasService
{
    private readonly IUsuarioRepository _usuarioRepository;

    public PermissoesEfetivasService(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
    }

    public async Task<PermissoesEfetivasUsuario> CalcularPermissoesAsync(string keycloakSub, CancellationToken cancellationToken = default)
    {
        var dados = await _usuarioRepository.ObterDadosPermissoesPorKeycloakSubAsync(keycloakSub, cancellationToken);

        if (dados == null)
        {
            return new PermissoesEfetivasUsuario(
                UsuarioEncontrado: false,
                UsuarioAtivo: false,
                AcessoTotal: false,
                OperadorSistema: false,
                ModulosHabilitados: Array.Empty<string>(),
                Permissoes: Array.Empty<string>()
            );
        }

        if (!dados.Ativo)
        {
            return new PermissoesEfetivasUsuario(
                UsuarioEncontrado: true,
                UsuarioAtivo: false,
                AcessoTotal: false,
                OperadorSistema: false,
                ModulosHabilitados: Array.Empty<string>(),
                Permissoes: Array.Empty<string>()
            );
        }

        return new PermissoesEfetivasUsuario(
            UsuarioEncontrado: true,
            UsuarioAtivo: true,
            AcessoTotal: dados.AcessoTotal,
            OperadorSistema: dados.OperadorSistema,
            ModulosHabilitados: dados.ModulosHabilitados,
            Permissoes: dados.Permissoes
        );
    }
}
