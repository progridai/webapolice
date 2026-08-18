using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;

public class ApoliceSubestipulanteModel
{
    public long Id { get; set; }
    public long ApoliceId { get; set; }
    public long SubestipulanteId { get; set; }
    
    public DateOnly? DataInicio { get; set; }
    public DateOnly? DataFim { get; set; }
    public bool Ativo { get; set; }
    public int? LegadoId { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    // Propriedades de Navegação (EF)
    public ApoliceModel? Apolice { get; set; }
    public ICollection<ApoliceVidaModel> Vidas { get; set; } = new List<ApoliceVidaModel>();
    public ICollection<ApoliceSubestipulanteModuloModel> Modulos { get; set; } = new List<ApoliceSubestipulanteModuloModel>();
}
