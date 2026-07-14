using System;

namespace WebApolice.Modulos.Clientes.Infrastructure.Persistence.Models.Vinculos;

public sealed class EstipulanteModel
{
    public long Id { get; private set; }
    public Guid PublicId { get; private set; }
    public long? PessoaId { get; private set; }
    public string Nome { get; private set; } = null!;
    public DateTime? DeletedAt { get; private set; }
}

public sealed class SubestipulanteModel
{
    public long Id { get; private set; }
    public Guid PublicId { get; private set; }
    public long? PessoaId { get; private set; }
    public string Nome { get; private set; } = null!;
    public DateTime? DeletedAt { get; private set; }
}

public sealed class CorretoraModel
{
    public long Id { get; private set; }
    public long? PessoaId { get; private set; }
    public DateTime? DeletedAt { get; private set; }
}

public sealed class SeguradoraModel
{
    public long Id { get; private set; }
    public long? PessoaId { get; private set; }
    public DateTime? DeletedAt { get; private set; }
}

public sealed class AgenciadorModel
{
    public long Id { get; private set; }
    public long? PessoaId { get; private set; }
    public DateTime? DeletedAt { get; private set; }
}

public sealed class GrupoModel
{
    public long Id { get; private set; }
    public string Nome { get; private set; } = null!;
}

public sealed class SubgrupoModel
{
    public long Id { get; private set; }
    public string Nome { get; private set; } = null!;
}

public sealed class LotacaoModel
{
    public long Id { get; private set; }
    public string Nome { get; private set; } = null!;
}

public sealed class BancoModel
{
    public long Id { get; private set; }
    public string? Codigo { get; private set; }
    public string Nome { get; private set; } = null!;
}
