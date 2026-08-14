using System;
using System.Collections.Generic;

namespace WebApolice.Shared.Infrastructure.Persistence.Models;

public partial class Estado
{
    public long Id { get; set; }

    public string Uf { get; set; } = null!;

    public string? Nome { get; set; }

    public virtual ICollection<Cidade> Cidades { get; set; } = new List<Cidade>();
}
