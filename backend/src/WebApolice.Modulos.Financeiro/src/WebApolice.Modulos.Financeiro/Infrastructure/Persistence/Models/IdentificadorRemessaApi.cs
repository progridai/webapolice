using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Financeiro.src.WebApolice.Modulos.Financeiro.Infrastructure.Persistence.Models;

public partial class IdentificadorRemessaApi
{
    public long Id { get; set; }

    public int? UsuarioLegadoId { get; set; }

    public DateTime? Datahora { get; set; }

    public int LegadoId { get; set; }

    public DateTime CreatedAt { get; set; }
}
