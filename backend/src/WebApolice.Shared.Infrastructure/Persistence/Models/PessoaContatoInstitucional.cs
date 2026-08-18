using System;
using System.Collections.Generic;

namespace WebApolice.Shared.Infrastructure.Persistence.Models;

public partial class PessoaContatoInstitucional
{
    public long Id { get; set; }

    public long PessoaId { get; set; }

    public string Nome { get; set; } = null!;

    public string Departamento { get; set; } = null!;

    public string? Email { get; set; }

    public string? Telefone { get; set; }

    public string? Ramal { get; set; }

    public bool Ativo { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Pessoa Pessoa { get; set; } = null!;
}
