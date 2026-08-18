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
}
