using System;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;

public class ApoliceHistoricoModel
{
    public long Id { get; set; }
    public long ApoliceId { get; set; }
    
    public string Acao { get; set; } = null!; // Criacao, Endosso, Renovacao, Alteracao, Cancelamento
    public string? Descricao { get; set; }
    
    // Id global do Usuário (se armazenar no banco ou em JWT/Claims)
    public Guid? UsuarioPublicId { get; set; } 
    
    public DateTimeOffset DataAcao { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    // Navegação
    public ApoliceModel? Apolice { get; set; }
}
