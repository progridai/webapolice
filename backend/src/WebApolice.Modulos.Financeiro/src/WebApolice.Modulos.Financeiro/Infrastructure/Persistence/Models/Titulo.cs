using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Financeiro.src.WebApolice.Modulos.Financeiro.Infrastructure.Persistence.Models;

public partial class Titulo
{
    public long Id { get; set; }

    public long? PropostaMovimentoId { get; set; }

    public long? PropostaId { get; set; }

    public long? PessoaId { get; set; }

    public long? ClienteId { get; set; }

    public long? ClienteVinculoId { get; set; }

    public long? EstipulanteId { get; set; }

    public long? ConvenioCobrancaId { get; set; }

    public long? ContaCobrancaId { get; set; }

    public short StatusId { get; set; }

    public int? CompetenciaAno { get; set; }

    public int? CompetenciaMes { get; set; }

    public int? CompetenciaInt { get; set; }

    public DateOnly? DataVencimento { get; set; }

    public DateOnly? DataLancamento { get; set; }

    public int? Parcela { get; set; }

    public int? Sequencia { get; set; }

    public decimal? PremioAnterior { get; set; }

    public decimal? PremioAtual { get; set; }

    public decimal? PremioLiquido { get; set; }

    public decimal? PremioDiferenca { get; set; }

    public decimal? PremioTotal { get; set; }

    public decimal? PremioTotalOriginal { get; set; }

    public decimal? PremioFatura { get; set; }

    public decimal? Iof { get; set; }

    public decimal? ValorOriginal { get; set; }

    public decimal? ValorAtual { get; set; }

    public decimal? ValorPago { get; set; }

    public DateOnly? DataPagamento { get; set; }

    public DateOnly? DataVencimentoFatura { get; set; }

    public DateOnly? DataRecebimentoFatura { get; set; }

    public string? IdFaturaCartao { get; set; }

    public bool? CobrarNaFatura { get; set; }

    public string? Observacao { get; set; }

    public int? LegadoMovimentoPropostaId { get; set; }

    public int? LegadoPropostaId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ContaCobranca? ContaCobranca { get; set; }

    public virtual ConvenioCobranca? ConvenioCobranca { get; set; }

    public virtual ICollection<MovimentoCobrancaLog> MovimentoCobrancaLogs { get; set; } = new List<MovimentoCobrancaLog>();

    public virtual TituloStatus Status { get; set; } = null!;

    public virtual ICollection<TituloPagamento> TituloPagamentos { get; set; } = new List<TituloPagamento>();

    public virtual ICollection<TituloRetornoBancario> TituloRetornoBancarios { get; set; } = new List<TituloRetornoBancario>();
}
