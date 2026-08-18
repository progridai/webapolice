using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Sinistros.src.WebApolice.Modulos.Sinistros.Infrastructure.Persistence.Models;

public partial class SinistroStatus
{
    public short Id { get; set; }

    public string Codigo { get; set; } = null!;

    public string Nome { get; set; } = null!;

    public bool Finalizador { get; set; }

    public bool Ativo { get; set; }

    public virtual ICollection<Sinistro> Sinistros { get; set; } = new List<Sinistro>();
}
