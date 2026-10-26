using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Seguro.Domain.Modelos;

public partial class PropostaStatus
{
    public short Id { get; set; }

    public string Codigo { get; set; } = null!;

    public string Nome { get; set; } = null!;

    public bool PermiteMovimentacao { get; set; }

    public bool VisivelOperacional { get; set; }

    public bool Finalizador { get; set; }

    public virtual ICollection<Propostum> Proposta { get; set; } = new List<Propostum>();
}
