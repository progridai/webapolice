using System;

namespace WebApolice.Modulos.Seguranca.Domain.Relacionamentos;

public sealed class PerfilPermissao
{
    public long PerfilId { get; private set; }
    public long PermissaoId { get; private set; }
    public long? AtribuidoPorUsuarioId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Perfil Perfil { get; private set; } = null!;
    public Permissao Permissao { get; private set; } = null!;
    public Usuario? AtribuidoPorUsuario { get; private set; }

    public PerfilPermissao(long perfilId, long permissaoId, long? atribuidoPorUsuarioId)
    {
        PerfilId = perfilId;
        PermissaoId = permissaoId;
        AtribuidoPorUsuarioId = atribuidoPorUsuarioId;
        CreatedAt = DateTime.UtcNow;
    }

    private PerfilPermissao() { }
}
