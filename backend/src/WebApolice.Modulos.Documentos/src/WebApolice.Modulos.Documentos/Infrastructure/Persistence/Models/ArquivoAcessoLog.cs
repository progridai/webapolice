using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Documentos.src.WebApolice.Modulos.Documentos.Infrastructure.Persistence.Models;

public partial class ArquivoAcessoLog
{
    public long Id { get; set; }

    public long ArquivoId { get; set; }

    public long? UsuarioId { get; set; }

    public int? UsuarioLegadoId { get; set; }

    public string Acao { get; set; } = null!;

    public string? IpOrigem { get; set; }

    public string? UserAgent { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Arquivo Arquivo { get; set; } = null!;
}
