using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Financeiro.src.WebApolice.Modulos.Financeiro.Infrastructure.Persistence.Models;

public partial class RetornoBancarioCodigo
{
    public long Id { get; set; }

    public string? Codigo { get; set; }

    public string Descricao { get; set; } = null!;

    public string Tipo { get; set; } = null!;

    public bool GeraBaixa { get; set; }

    public bool GeraRejeicao { get; set; }

    public bool Ativo { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<TituloRetornoBancario> TituloRetornoBancarios { get; set; } = new List<TituloRetornoBancario>();
}
