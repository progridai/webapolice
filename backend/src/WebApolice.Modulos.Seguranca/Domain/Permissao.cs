using System;

namespace WebApolice.Modulos.Seguranca.Domain;

public sealed class Permissao
{
    public long Id { get; private set; }
    public Guid PublicId { get; private set; }
    public long RecursoId { get; private set; }
    public string Codigo { get; private set; } = null!;
    public string Nome { get; private set; } = null!;
    public string? Descricao { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public Recurso Recurso { get; private set; } = null!;

    private Permissao() { }
}
