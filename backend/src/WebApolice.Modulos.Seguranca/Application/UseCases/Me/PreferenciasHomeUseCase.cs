using WebApolice.Modulos.Seguranca.Application.Ports;
using WebApolice.Modulos.Seguranca.Domain;

namespace WebApolice.Modulos.Seguranca.Application.UseCases.Me;

public sealed record PreferenciasHomeDto(string[]? AtalhosHome);

public sealed class PreferenciasHomeUseCase(
    IContextoUsuarioAutenticado contexto,
    IPreferenciasHomeRepository repository)
{
    private Task<Usuario?> ObterUsuarioAsync(CancellationToken cancellationToken) =>
        contexto.EstaAutenticado && !string.IsNullOrWhiteSpace(contexto.KeycloakSub)
            ? repository.ObterUsuarioAtivoAsync(contexto.KeycloakSub, cancellationToken)
            : Task.FromResult<Usuario?>(null);

    public async Task<PreferenciasHomeDto?> ObterAsync(CancellationToken cancellationToken)
    {
        var usuario = await ObterUsuarioAsync(cancellationToken);
        return usuario is null ? null : new PreferenciasHomeDto(usuario.AtalhosHome?.ToArray());
    }

    public async Task<PreferenciasHomeDto?> SalvarAsync(string[]? atalhos, CancellationToken cancellationToken)
    {
        var usuario = await ObterUsuarioAsync(cancellationToken);
        if (usuario is null) return null;
        usuario.DefinirAtalhosHome(atalhos);
        await repository.SalvarAsync(cancellationToken);
        return new PreferenciasHomeDto(usuario.AtalhosHome?.ToArray());
    }
}
