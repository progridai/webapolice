using System;

namespace WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Models.Vinculos;

public sealed class SubestipulanteModel
{
    public long Id { get; private set; }
    public Guid PublicId { get; private set; }
    public long? PessoaId { get; private set; }
    public string Nome { get; private set; } = null!;
    public DateTimeOffset? DeletedAt { get; private set; }
}

public sealed class CorretoraModel
{
    public long Id { get; private set; }
    public Guid PublicId { get; private set; }
    public long? PessoaId { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
}

public sealed class SeguradoraModel
{
    public long Id { get; private set; }
    public Guid PublicId { get; private set; }
    public long? PessoaId { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
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

public class PessoaContatoInstitucionalModel
{
    public long Id { get; set; }
    public long PessoaId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Departamento { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public string? Ramal { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class CidadeModel
{
    public long Id { get; set; }
    public string Nome { get; set; } = string.Empty;
}

// ClienteModel foi removido — use a entidade de domínio Cliente diretamente (Domain/Cliente.cs).
