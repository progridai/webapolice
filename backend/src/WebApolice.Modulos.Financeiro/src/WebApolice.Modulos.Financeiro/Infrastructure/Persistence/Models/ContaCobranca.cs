using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Financeiro.src.WebApolice.Modulos.Financeiro.Infrastructure.Persistence.Models;

public partial class ContaCobranca
{
    public long Id { get; set; }

    public long PessoaId { get; set; }

    public long ClienteId { get; set; }

    public long ClienteVinculoId { get; set; }

    public long? EstipulanteId { get; set; }

    public long? SubestipulanteId { get; set; }

    public long? ConvenioCobrancaId { get; set; }

    public short RegraAgrupamentoId { get; set; }

    public string IdentificadorAgrupamento { get; set; } = null!;

    public bool Ativo { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ConvenioCobranca? ConvenioCobranca { get; set; }

    public virtual RegraAgrupamentoFatura RegraAgrupamento { get; set; } = null!;

    public virtual ICollection<Titulo> Titulos { get; set; } = new List<Titulo>();
}
