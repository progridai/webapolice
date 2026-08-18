using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Financeiro.src.WebApolice.Modulos.Financeiro.Infrastructure.Persistence.Models;

public partial class FormaPagamentoEstipulante
{
    public long Id { get; set; }

    public string Nome { get; set; } = null!;

    public string? Codigo { get; set; }

    public int? LegadoId { get; set; }

    public bool Ativo { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<EstipulanteFaturamentoConfig> EstipulanteFaturamentoConfigs { get; set; } = new List<EstipulanteFaturamentoConfig>();
}
