using System;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Propostas.ListarPropostas;

public class PropostaDto
{
    public Guid PublicId { get; set; }
    public string? Numero { get; set; }
    
    // Indicador de Origem (Para o Frontend diferenciar se pode editar no modelo novo)
    public bool IntegradaApolice { get; set; }
    
    public decimal? PremioLiquido { get; set; }
    public DateOnly? DataInclusao { get; set; }
    public string Status { get; set; } = string.Empty;
    public string StatusCodigo { get; set; } = string.Empty;
    public Guid? ApolicePublicId { get; set; }
    public Guid? ApoliceVidaPublicId { get; set; }
    public string? ApoliceNome { get; set; }
    public Guid? ClientePublicId { get; set; }
    public string? ClienteNome { get; set; }
    public Guid? ClienteVinculoPublicId { get; set; }
    public string? VinculoMatricula { get; set; }
    public bool Vigente { get; set; }
    public int Versao { get; set; }
    public DateTime CreatedAt { get; set; }
}
