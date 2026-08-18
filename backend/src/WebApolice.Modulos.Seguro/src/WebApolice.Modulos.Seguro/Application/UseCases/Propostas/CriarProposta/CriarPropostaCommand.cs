using System;
using MediatR;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Propostas.CriarProposta;

public class CriarPropostaCommand : IRequest<Guid>
{
    public Guid ApoliceId { get; set; }
    public Guid ApoliceVidaId { get; set; }
    
    // O ClienteId precisa ser enviado para provar que a requisiÃ§Ã£o nÃ£o estÃ¡
    // forjando a ApoliceVidaId de um terceiro.
    public long ClienteId { get; set; }
    
    // Opcional, sÃ³ usado se a ApÃ³lice nÃ£o forÃ§ar uma Corretora.
    public long? CorretoraId { get; set; }
    
    // Totalmente independente (nÃ£o-obrigatÃ³rio derivar da Apolice)
    public long? ConvenioCobrancaId { get; set; }
    public long? ContaCobrancaId { get; set; }
    
    public decimal? PremioLiquido { get; set; }
    public decimal? ValorParcela { get; set; }
    
    public DateOnly? DataPrimeiroVencimento { get; set; }
    public DateOnly? DataProximoVencimento { get; set; }
    
    public string? BancoAgencia { get; set; }
    public string? BancoContaCorrente { get; set; }
}
