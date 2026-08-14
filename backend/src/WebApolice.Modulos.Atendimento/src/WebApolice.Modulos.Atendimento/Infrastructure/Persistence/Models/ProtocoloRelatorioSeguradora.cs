using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Atendimento.src.WebApolice.Modulos.Atendimento.Infrastructure.Persistence.Models;

public partial class ProtocoloRelatorioSeguradora
{
    public long Id { get; set; }

    public DateTime? DataRelatorio { get; set; }

    public string? Observacao { get; set; }

    public int LegadoId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<ProtocoloRelatorioSeguradoraItem> ProtocoloRelatorioSeguradoraItems { get; set; } = new List<ProtocoloRelatorioSeguradoraItem>();
}
