using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;

public partial class MovimentoTipo
{
    public long Id { get; set; }

    public string Nome { get; set; } = null!;

    public bool GeraTitulo { get; set; }

    public string Classificacao { get; set; } = null!;

    public bool Ativo { get; set; }

    public bool AlteraProposta { get; set; }

    public bool Financeiro { get; set; }

    public bool Cancelamento { get; set; }

    public int? LegadoId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<PropostaMovimento> PropostaMovimentos { get; set; } = new List<PropostaMovimento>();
}
