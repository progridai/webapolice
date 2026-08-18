using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Convenio.src.WebApolice.Modulos.Convenio.Infrastructure.Persistence.Models;

public partial class SiapeOrgao
{
    public long Id { get; set; }

    public string? Codigo { get; set; }

    public string? Nome { get; set; }

    public int? LegadoId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<SiapeCliente> SiapeClientes { get; set; } = new List<SiapeCliente>();
}
