using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Seguranca.Domain;

public sealed class Modulo
{
    public long Id { get; private set; }
    public Guid PublicId { get; private set; }
    public string Codigo { get; private set; } = null!;
    public string Nome { get; private set; } = null!;
    public string? Descricao { get; private set; }
    public string? Icone { get; private set; }
    public int Ordem { get; private set; }
    public bool Ativo { get; private set; }
    public bool Habilitado { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public ICollection<Recurso> Recursos { get; private set; } = new List<Recurso>();

    private Modulo() { }

    public void Habilitar() => Habilitado = true;
    public void Desabilitar() => Habilitado = false;
}
