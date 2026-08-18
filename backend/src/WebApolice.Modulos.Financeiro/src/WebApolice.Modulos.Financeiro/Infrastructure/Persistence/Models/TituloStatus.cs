using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Financeiro.src.WebApolice.Modulos.Financeiro.Infrastructure.Persistence.Models;

public partial class TituloStatus
{
    public short Id { get; set; }

    public string Codigo { get; set; } = null!;

    public string Nome { get; set; } = null!;

    public bool Finalizador { get; set; }

    public bool PermiteCobranca { get; set; }

    public bool Inadimplente { get; set; }

    public bool Ativo { get; set; }

    public virtual ICollection<Titulo> Titulos { get; set; } = new List<Titulo>();
}
