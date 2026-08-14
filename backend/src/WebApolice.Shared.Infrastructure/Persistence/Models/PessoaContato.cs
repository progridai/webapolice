using System;
using System.Collections.Generic;

namespace WebApolice.Shared.Infrastructure.Persistence.Models;

public partial class PessoaContato
{
    public long Id { get; set; }

    public long PessoaId { get; set; }

    public string TipoContato { get; set; } = null!;

    public string Valor { get; set; } = null!;

    public string? ValorNormalizado { get; set; }

    public bool Principal { get; set; }

    public bool Ativo { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Pessoa Pessoa { get; set; } = null!;
}
