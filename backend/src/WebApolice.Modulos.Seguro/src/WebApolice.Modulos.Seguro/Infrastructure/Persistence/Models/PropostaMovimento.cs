using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;

public partial class PropostaMovimento
{
    public long Id { get; set; }

    public long? PropostaId { get; set; }

    public long? PessoaId { get; set; }

    public long? ClienteId { get; set; }

    public long? ClienteVinculoId { get; set; }

    public long? EstipulanteId { get; set; }

    public long? ConvenioCobrancaId { get; set; }

    public long? MovimentoTipoId { get; set; }

    public string Classificacao { get; set; } = null!;

    public DateOnly? DataVencimento { get; set; }

    public DateOnly? DataLancamento { get; set; }

    public DateOnly? DataPagamento { get; set; }

    public int? Dia { get; set; }

    public int? Mes { get; set; }

    public int? Ano { get; set; }

    public int? CompetenciaInt { get; set; }

    public decimal? PremioAnterior { get; set; }

    public decimal? PremioAtual { get; set; }

    public decimal? PremioLiquido { get; set; }

    public decimal? PremioDiferenca { get; set; }

    public decimal? PremioTotal { get; set; }

    public decimal? PremioTotalOriginal { get; set; }

    public decimal? PremioFatura { get; set; }

    public decimal? ValorPago { get; set; }

    public decimal? Iof { get; set; }

    public decimal? ComissaoBase { get; set; }

    public decimal? ComissaoLiquida { get; set; }

    public decimal? ComissaoBruta { get; set; }

    public int? SituacaoCodigo { get; set; }

    public string? SituacaoDescricao { get; set; }

    public char? Gerado { get; set; }

    public char? ComissaoGerado { get; set; }

    public char? TituloGerado { get; set; }

    public int? Parcela { get; set; }

    public int? Sequencia { get; set; }

    public DateOnly? DataVencimentoFatura { get; set; }

    public DateOnly? DataRecebimentoFatura { get; set; }

    public string? IdFaturaCartao { get; set; }

    public bool? CobrarNaFatura { get; set; }

    public int? UsuarioCobradorLegadoId { get; set; }

    public string? Observacao { get; set; }

    public int LegadoId { get; set; }

    public int? LegadoMovAnt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual MovimentoTipo? MovimentoTipo { get; set; }

    public virtual Propostum? Proposta { get; set; }
}
