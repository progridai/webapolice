using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Sinistros.src.WebApolice.Modulos.Sinistros.Infrastructure.Persistence.Models;

public partial class Acompanhamento
{
    public long Id { get; set; }

    public long? SinistroId { get; set; }

    public DateOnly? DataAcompanhamento { get; set; }

    public string? HoraOriginal { get; set; }

    public string? Contato { get; set; }

    public string? Descricao { get; set; }

    public int? UsuarioLegadoId { get; set; }

    public int LegadoId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Sinistro? Sinistro { get; set; }
}
