using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Documentos.src.WebApolice.Modulos.Documentos.Infrastructure.Persistence.Models;

public partial class ArquivoVinculo
{
    public long Id { get; set; }

    public long ArquivoId { get; set; }

    public long? TipoAnexoId { get; set; }

    public string EntidadeTipo { get; set; } = null!;

    public long EntidadeId { get; set; }

    public int? EntidadeLegadoId { get; set; }

    public bool Principal { get; set; }

    public bool Obrigatorio { get; set; }

    public string? Observacao { get; set; }

    public string? LegadoOrigemColuna { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public string? EntidadeLegadoTipo { get; set; }

    public string? EntidadeLegadoChave1 { get; set; }

    public string? EntidadeLegadoChave2 { get; set; }

    public string? CriterioResolucao { get; set; }

    public bool VinculoResolvido { get; set; }

    public string? EntidadeLegadoChaveConcatenada { get; set; }

    public virtual Arquivo Arquivo { get; set; } = null!;

    public virtual TipoAnexo? TipoAnexo { get; set; }
}
