using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;

public partial class Propostum
{
    public long Id { get; set; }

    public Guid PublicId { get; set; }

    public long PessoaId { get; set; }

    public long ClienteId { get; set; }

    public long ClienteVinculoId { get; set; }

    public long EstipulanteId { get; set; }

    public long? SubestipulanteId { get; set; }

    public long? SeguradoraId { get; set; }

    public long? CorretoraId { get; set; }

    public long? ConvenioCobrancaId { get; set; }

    public long? ContaCobrancaId { get; set; }

    public short StatusId { get; set; }

    public long? MovimentoTipoId { get; set; }

    public string? Numero { get; set; }

    public DateOnly? DataInclusao { get; set; }

    public DateOnly? DataMovimento { get; set; }

    public DateOnly? DataPrimeiroVencimento { get; set; }

    public DateOnly? DataProximoVencimento { get; set; }

    public string? BancoAgencia { get; set; }

    public string? BancoContaCorrente { get; set; }

    public DateOnly? BancoDataDebito { get; set; }

    public string? BancoDiaDebito { get; set; }

    public decimal? PremioLiquido { get; set; }

    public decimal? IofPercentual { get; set; }

    public decimal? IofValor { get; set; }

    public decimal? ValorParcela { get; set; }

    public int? MovimentoFaturaMes { get; set; }

    public int? MovimentoFaturaAno { get; set; }

    public long? SubgrupoId { get; set; }

    public long? LotacaoId { get; set; }

    public DateOnly? DataUltimoAjusteIndice { get; set; }

    public bool? ComissaoEstornada { get; set; }

    public DateOnly? DataEstornoComissao { get; set; }

    public int? ProtocoloClienteLegadoId { get; set; }

    public int? ProtocoloStatus { get; set; }

    public int? CompetenciaInclusaoInt { get; set; }

    public int? SituacaoProposta { get; set; }

    public DateTime? DataAlteracaoSituacao { get; set; }

    public DateTime? DataProcessamentoFunpresp { get; set; }

    public bool? PossuiBonusFunpresp { get; set; }

    public string? Observacao { get; set; }

    public int LegadoId { get; set; }

    public int? LegadoPropostaAnt { get; set; }

    public string? LegadoMovimentoIni { get; set; }

    public string? LegadoMovimentoFim { get; set; }

    public bool Vigente { get; set; }

    public bool VisivelOperacional { get; set; }

    public long? PropostaOrigemId { get; set; }

    public int Versao { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<Propostum> InversePropostaOrigem { get; set; } = new List<Propostum>();

    public virtual ICollection<PropostaBeneficiario> PropostaBeneficiarios { get; set; } = new List<PropostaBeneficiario>();

    public virtual ICollection<PropostaCobertura> PropostaCoberturas { get; set; } = new List<PropostaCobertura>();

    public virtual ICollection<PropostaHistorico> PropostaHistoricoPropostaAnteriors { get; set; } = new List<PropostaHistorico>();

    public virtual ICollection<PropostaHistorico> PropostaHistoricoPropostaNovas { get; set; } = new List<PropostaHistorico>();

    public virtual ICollection<PropostaItem> PropostaItems { get; set; } = new List<PropostaItem>();

    public virtual ICollection<PropostaMovimento> PropostaMovimentos { get; set; } = new List<PropostaMovimento>();

    public virtual Propostum? PropostaOrigem { get; set; }

    public virtual PropostaStatus Status { get; set; } = null!;
}
