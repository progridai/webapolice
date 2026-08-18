using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Financeiro.src.WebApolice.Modulos.Financeiro.Infrastructure.Persistence.Models;

public partial class RegraAgrupamentoFatura
{
    public short Id { get; set; }

    public string Codigo { get; set; } = null!;

    public string Nome { get; set; } = null!;

    public string? Descricao { get; set; }

    public virtual ICollection<ContaCobranca> ContaCobrancas { get; set; } = new List<ContaCobranca>();

    public virtual ICollection<EstipulanteFaturamentoConfig> EstipulanteFaturamentoConfigs { get; set; } = new List<EstipulanteFaturamentoConfig>();
}
