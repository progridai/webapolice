namespace WebApolice.Modulos.Seguranca.Application.Ports;

public interface IContextoUsuarioAutenticado
{
    bool EstaAutenticado { get; }
    string? KeycloakSub { get; }
    string? Username { get; }
    string? Nome { get; }
    string? Email { get; }
}
