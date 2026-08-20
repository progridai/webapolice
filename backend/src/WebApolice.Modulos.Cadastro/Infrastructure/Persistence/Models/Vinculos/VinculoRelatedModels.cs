using System;

namespace WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Models.Vinculos;

public sealed class SubestipulanteModel
{
    public long Id { get; set; }
    public Guid PublicId { get; set; }
    public long PessoaId { get; set; }

    public string? Codigo { get; set; }
    public bool Ativo { get; set; } = true;
    public string? Observacao { get; set; }
    public int? LegadoId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    // Navigation property (cross-module read, ExcludeFromMigrations em core.pessoa)
    public PessoaModel? Pessoa { get; set; }
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
