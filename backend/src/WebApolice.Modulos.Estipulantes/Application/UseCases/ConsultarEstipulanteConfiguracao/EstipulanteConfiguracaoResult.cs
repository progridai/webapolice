using System;

namespace WebApolice.Modulos.Estipulantes.Application.UseCases.ConsultarEstipulanteConfiguracao;

public class EstipulanteConfiguracaoResult
{
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
}
