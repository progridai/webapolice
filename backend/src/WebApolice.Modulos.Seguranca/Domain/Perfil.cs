using System;
using System.Collections.Generic;
using WebApolice.Modulos.Seguranca.Domain.Relacionamentos;
using WebApolice.Modulos.Seguranca.Domain.Auditoria;

namespace WebApolice.Modulos.Seguranca.Domain;

public sealed class Perfil
{
    public long Id { get; private set; }
    public Guid PublicId { get; private set; }
    public string Codigo { get; private set; } = null!;
    public string Nome { get; private set; } = null!;
    public string? Descricao { get; private set; }
    public bool PerfilSistema { get; private set; }
    public bool AcessoTotal { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public ICollection<PerfilPermissao> Permissoes { get; private set; } = new List<PerfilPermissao>();
    public ICollection<UsuarioPerfil> Usuarios { get; private set; } = new List<UsuarioPerfil>();

    public Perfil(string codigo, string nome, string descricao, bool ativo, bool perfilSistema, bool acessoTotal)
    {
        Codigo = codigo;
        Nome = nome;
        Descricao = descricao;
        Ativo = ativo;
        PerfilSistema = perfilSistema;
        AcessoTotal = acessoTotal;
        CreatedAt = DateTime.UtcNow;
    }

    private Perfil() { }

    public void Atualizar(string nome, string descricao, bool ativo)
    {
        Nome = nome;
        Descricao = descricao;
        Ativo = ativo;
        UpdatedAt = DateTime.UtcNow;
    }
}
