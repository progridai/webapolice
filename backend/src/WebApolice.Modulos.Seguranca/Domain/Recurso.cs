using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Seguranca.Domain;

public sealed class Recurso
{
    public long Id { get; private set; }
    public Guid PublicId { get; private set; }
    public long ModuloId { get; private set; }
    public string Codigo { get; private set; } = null!;
    public string Nome { get; private set; } = null!;
    public string? Descricao { get; private set; }
    public string? RotaFrontend { get; private set; }
    public int Ordem { get; private set; }
    public bool Ativo { get; private set; }
    public bool Habilitado { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public Modulo Modulo { get; private set; } = null!;
    public ICollection<Permissao> Permissoes { get; private set; } = new List<Permissao>();

    private Recurso() { }

    public void Habilitar() => Habilitado = true;
    public void Desabilitar() => Habilitado = false;
}
