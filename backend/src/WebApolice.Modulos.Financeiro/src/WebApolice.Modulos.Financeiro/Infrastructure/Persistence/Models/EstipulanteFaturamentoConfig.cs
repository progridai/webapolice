using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Financeiro.src.WebApolice.Modulos.Financeiro.Infrastructure.Persistence.Models;

public partial class EstipulanteFaturamentoConfig
{
    public long Id { get; set; }

    public long EstipulanteId { get; set; }

    public long? FormaPagamentoId { get; set; }

    public long? ConvenioCobrancaId { get; set; }

    public short? RegraAgrupamentoFaturaId { get; set; }

    public int? DiaDebito { get; set; }

    public decimal? IofVg { get; set; }

    public decimal? IofInc { get; set; }

    public decimal? IofAp { get; set; }

    public string? NumeroPropostaVg { get; set; }

    public string? NumeroPropostaInc { get; set; }

    public string? NumeroPropostaAp { get; set; }

    public decimal? SorteioValor { get; set; }

    public string? Saf { get; set; }

    public int? Campanha { get; set; }

    public long? ParametroSiapeId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ConvenioCobranca? ConvenioCobranca { get; set; }

    public virtual FormaPagamentoEstipulante? FormaPagamento { get; set; }

    public virtual RegraAgrupamentoFatura? RegraAgrupamentoFatura { get; set; }
}
