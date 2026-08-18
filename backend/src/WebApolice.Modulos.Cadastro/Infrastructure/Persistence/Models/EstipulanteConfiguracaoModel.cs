using System;

namespace WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Models;

public class EstipulanteConfiguracaoModel
{
    public long Id { get; set; }
    public long EstipulanteId { get; set; }
    public bool? PermitePropostas { get; set; }
    public bool? ControlaComissao { get; set; }
    public DateOnly? DataInicioVigencia { get; set; }
    public DateOnly? DataFimVigencia { get; set; }
    public DateOnly? DataAniversario { get; set; }
    public DateOnly? DataUltimoReajuste { get; set; }
    public int? DataBaseReajuste { get; set; }
    public DateOnly? DataLimiteReajuste { get; set; }
    public int? DiasAvisoReajuste { get; set; }
    public int? Carencia { get; set; }
    public string? AdesaoPor { get; set; }
    public string? Custeio { get; set; }
    public string? Adesao { get; set; }
    public int? FaixaEtariaInicio { get; set; }
    public int? FaixaEtariaFim { get; set; }
    public long? CancelaEstipulanteId { get; set; }
    public bool DesconsiderarPropostaAtiva { get; set; }
    public bool PermitirProtocoloDuplicado { get; set; }
    
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public EstipulanteModel Estipulante { get; set; } = null!;
}
