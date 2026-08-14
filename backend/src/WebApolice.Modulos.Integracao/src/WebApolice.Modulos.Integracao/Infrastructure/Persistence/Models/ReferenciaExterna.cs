using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Integracao.src.WebApolice.Modulos.Integracao.Infrastructure.Persistence.Models;

public partial class ReferenciaExterna
{
    public long Id { get; set; }

    public string Sistema { get; set; } = null!;

    public string EntidadeTipo { get; set; } = null!;

    public long EntidadeId { get; set; }

    public string ChaveExterna { get; set; } = null!;

    public string? Dados { get; set; }

    public bool Ativo { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
