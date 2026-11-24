namespace WebApolice.Modulos.Seguro.Domain.Modelos;

public sealed class PropostaFinanceiro
{
    public long Id { get; set; }
    public Guid PublicId { get; set; } = Guid.NewGuid();
    public long PropostaId { get; set; }
    public long ApoliceRamoId { get; set; }
    public DateOnly InicioConferido { get; set; }
    public int CompetenciaInicial { get; set; }
    public DateOnly VencimentoInicial { get; set; }
    public int CompetenciaReferencia { get; set; }
    public DateOnly VencimentoReferencia { get; set; }
    public decimal PremioLiquido { get; set; }
    public decimal IofPercentual { get; set; }
    public decimal IofValor { get; set; }
    public decimal TotalMensal { get; set; }
    public string CondicoesHash { get; set; } = null!;
    public DateTime ConferidaEm { get; set; }
    public Propostum Proposta { get; set; } = null!;
    public ApoliceRamoModel ApoliceRamo { get; set; } = null!;
}

public sealed class ParcelaCompetencia
{
    public long Id { get; set; }
    public Guid PublicId { get; set; } = Guid.NewGuid();
    public long ApoliceId { get; set; }
    public int Competencia { get; set; }
    public DateTime? GeradaEm { get; set; }
    public DateTime? FechadaEm { get; set; }
    public int Revisao { get; set; }
    public ApoliceModel Apolice { get; set; } = null!;
}

public partial class PropostaMovimento
{
    public long? ApoliceVidaId { get; set; }
    public long? PropostaFinanceiroId { get; set; }
    public long? ParcelaCompetenciaId { get; set; }
    public int RevisaoBaixa { get; set; }
    public ApoliceVidaModel? ApoliceVida { get; set; }
    public PropostaFinanceiro? PropostaFinanceiro { get; set; }
    public ParcelaCompetencia? ParcelaCompetencia { get; set; }
}
