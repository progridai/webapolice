using System;
using System.Collections.Generic;

namespace WebApolice.Shared.Infrastructure.Persistence.Models;

public partial class Cidade
{
    public long Id { get; set; }

    public long? EstadoId { get; set; }

    public string Nome { get; set; } = null!;

    public string? NomeNormalizado { get; set; }

    public string? Uf { get; set; }

    public int? LegadoId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Estado? Estado { get; set; }

    public virtual ICollection<PessoaEndereco> PessoaEnderecos { get; set; } = new List<PessoaEndereco>();
}
