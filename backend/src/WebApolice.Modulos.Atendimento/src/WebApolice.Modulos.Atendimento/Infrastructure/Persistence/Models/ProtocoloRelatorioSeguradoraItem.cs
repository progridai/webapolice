using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Atendimento.src.WebApolice.Modulos.Atendimento.Infrastructure.Persistence.Models;

public partial class ProtocoloRelatorioSeguradoraItem
{
    public long Id { get; set; }

    public long RelatorioId { get; set; }

    public long? ProtocoloLoteId { get; set; }

    public long? PessoaId { get; set; }

    public long? ClienteId { get; set; }

    public long? ClienteVinculoId { get; set; }

    public int? LegadoClienteId { get; set; }

    public int? LegadoProtocoloId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ProtocoloLote? ProtocoloLote { get; set; }

    public virtual ProtocoloRelatorioSeguradora Relatorio { get; set; } = null!;
}
