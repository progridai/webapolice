using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Seguranca.Application.DTOs;
using WebApolice.Modulos.Seguranca.Application.Ports;

namespace WebApolice.Modulos.Seguranca.Application.UseCases.Me;

public class ObterUsuarioAutenticadoUseCase
{
    private readonly IContextoUsuarioAutenticado _contexto;
    private readonly IPermissoesEfetivasService _permissoesService;

    public ObterUsuarioAutenticadoUseCase(
        IContextoUsuarioAutenticado contexto,
        IPermissoesEfetivasService permissoesService)
    {
        _contexto = contexto;
        _permissoesService = permissoesService;
    }

    public async Task<UsuarioAutenticadoDto> ExecuteAsync(CancellationToken cancellationToken)
    {
        if (!_contexto.EstaAutenticado || string.IsNullOrEmpty(_contexto.KeycloakSub))
        {
            return new UsuarioAutenticadoDto(false, false, false, false, Array.Empty<string>(), Array.Empty<string>());
        }

        var permissoesEfetivas = await _permissoesService.CalcularPermissoesAsync(_contexto.KeycloakSub, cancellationToken);

        return new UsuarioAutenticadoDto(
            permissoesEfetivas.UsuarioEncontrado,
            permissoesEfetivas.UsuarioAtivo,
            permissoesEfetivas.AcessoTotal,
            permissoesEfetivas.OperadorSistema,
            permissoesEfetivas.ModulosHabilitados.ToList(),
            permissoesEfetivas.Permissoes.ToList()
        );
    }
}
