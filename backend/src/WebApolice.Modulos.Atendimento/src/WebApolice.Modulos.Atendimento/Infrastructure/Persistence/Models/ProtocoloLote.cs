using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Atendimento.src.WebApolice.Modulos.Atendimento.Infrastructure.Persistence.Models;

public partial class ProtocoloLote
{
    public long Id { get; set; }

    public Guid PublicId { get; set; }

    public int? NumeroProtocolo { get; set; }

    public DateTime? DataProtocolo { get; set; }

    public int? ConsultorLegadoId { get; set; }

    public int? UsuarioLegadoId { get; set; }

    public bool? AnexoConsultor { get; set; }

    public bool? AnexoSeguradora { get; set; }

    public string Status { get; set; } = null!;

    public string? Observacao { get; set; }

    public int LegadoId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<ProtocoloAcompanhamento> ProtocoloAcompanhamentos { get; set; } = new List<ProtocoloAcompanhamento>();

    public virtual ICollection<ProtocoloItem> ProtocoloItems { get; set; } = new List<ProtocoloItem>();

    public virtual ICollection<ProtocoloRelatorioSeguradoraItem> ProtocoloRelatorioSeguradoraItems { get; set; } = new List<ProtocoloRelatorioSeguradoraItem>();
}
