using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Documentos.src.WebApolice.Modulos.Documentos.Infrastructure.Persistence.Models;

public partial class TipoAnexo
{
    public long Id { get; set; }

    public string? Codigo { get; set; }

    public string Nome { get; set; } = null!;

    public string? Categoria { get; set; }

    public string? Descricao { get; set; }

    public bool ExigeValidade { get; set; }

    public bool ExigeAssinatura { get; set; }

    public bool Sensivel { get; set; }

    public bool Ativo { get; set; }

    public string? LegadoValorOriginal { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<ArquivoVinculo> ArquivoVinculos { get; set; } = new List<ArquivoVinculo>();
}
