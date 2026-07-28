using System;
using System.Collections.Generic;
using WebApolice.Modulos.Seguranca.Domain.Relacionamentos;

namespace WebApolice.Modulos.Seguranca.Domain;

public sealed class Usuario
{
    public long Id { get; private set; }
    public Guid PublicId { get; private set; }
    public string KeycloakSub { get; private set; } = null!;
    public string? Username { get; private set; }
    public string? Nome { get; private set; }
    public string? Email { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime? UltimoLoginEm { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public ICollection<UsuarioPerfil> Perfis { get; private set; } = new List<UsuarioPerfil>();

    private Usuario() { }

    public Usuario(string keycloakSub, string username, string nome, string email, bool ativo)
    {
        KeycloakSub = keycloakSub;
        Username = username;
        Nome = nome;
        Email = email;
        Ativo = ativo;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AtualizarAdmin(string nome, string email, bool ativo)
    {
        Nome = nome;
        Email = email;
        Ativo = ativo;
        UpdatedAt = DateTime.UtcNow;
    }

    public static Usuario Criar(string keycloakSub, string? username, string? nome, string? email)
    {
        return new Usuario(keycloakSub, username ?? string.Empty, nome ?? string.Empty, email ?? string.Empty, true);
    }

    public bool AtualizarDadosIdentidade(string? username, string? nome, string? email)
    {
        bool alterado = false;

        if (username != null && Username != username)
        {
            Username = username;
            alterado = true;
        }

        if (nome != null && Nome != nome)
        {
            Nome = nome;
            alterado = true;
        }

        if (email != null && Email != email)
        {
            Email = email;
            alterado = true;
        }

        if (alterado)
        {
            UpdatedAt = DateTime.UtcNow;
        }

        return alterado;
    }
}
