using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Atendimento.src.WebApolice.Modulos.Atendimento.Infrastructure.Persistence.Models;

public partial class ProtocoloAcompanhamento
{
    public long Id { get; set; }

    public long? ProtocoloLoteId { get; set; }

    public DateOnly? DataAcompanhamento { get; set; }

    public string? HoraOriginal { get; set; }

    public string? Contato { get; set; }

    public string? Descricao { get; set; }

    public int? UsuarioLegadoId { get; set; }

    public int LegadoId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ProtocoloLote? ProtocoloLote { get; set; }
}
