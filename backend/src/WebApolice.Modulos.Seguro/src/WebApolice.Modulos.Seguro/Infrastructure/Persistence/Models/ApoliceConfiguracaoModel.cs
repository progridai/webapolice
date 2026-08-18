using System;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;

public class ApoliceConfiguracaoModel
{
    public long ApoliceId { get; set; }
    
    // Regras de Vigência e Movimentação
    public string? TipoAdesao { get; set; } // Compulsória, Facultativa
    public string? Custeio { get; set; } // Integral, Contributário, Não Contributário
    public int? CarenciaDias { get; set; }
    
    // Regras de Reajuste
    public int? MesBaseReajuste { get; set; }
    public string? IndiceReajuste { get; set; }
    
    // Coberturas e Formação de Preço Operacional
    public bool CobreConjuge { get; set; }
    public bool ControlaExcedente { get; set; }
    
    // Regras de Faturamento e Operação do Seguro
    public int? DiaCorteFaturamento { get; set; }
    public int? PrazoAvisoSinistroDias { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    
    // Navegação (1:1 com Apólice)
    public ApoliceModel? Apolice { get; set; }
}
