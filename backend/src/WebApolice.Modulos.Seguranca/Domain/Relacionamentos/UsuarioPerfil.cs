using System;

namespace WebApolice.Modulos.Seguranca.Domain.Relacionamentos;

public sealed class UsuarioPerfil
{
    public long UsuarioId { get; private set; }
    public long PerfilId { get; private set; }
    public long? AtribuidoPorUsuarioId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Usuario Usuario { get; private set; } = null!;
    public Perfil Perfil { get; private set; } = null!;
    public Usuario? AtribuidoPorUsuario { get; private set; }

    public UsuarioPerfil(long usuarioId, long perfilId, long? atribuidoPorUsuarioId)
    {
        UsuarioId = usuarioId;
        PerfilId = perfilId;
        AtribuidoPorUsuarioId = atribuidoPorUsuarioId;
        CreatedAt = DateTime.UtcNow;
    }

    private UsuarioPerfil() { }
}
