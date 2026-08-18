using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;

public class ApoliceVidaModel
{
    public long Id { get; set; }
    public Guid PublicId { get; set; }

    public long ApoliceId { get; set; }
    public long ClienteId { get; set; }
    public long? ClienteVinculoId { get; set; }

    public long? ApoliceSubestipulanteId { get; set; }
    public long? ApoliceSubestipulanteModuloId { get; set; }

    public DateOnly? DataInicioVigencia { get; set; }
    public DateOnly? DataFimVigencia { get; set; }

    public string Status { get; set; } = "ativa";
    public bool Ativo { get; set; }
    public string? Origem { get; set; }

    public int? LegadoId { get; set; }
    public string? Observacao { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    // Propriedades de Navegação (EF)
    public ApoliceModel? Apolice { get; set; }
    public ApoliceSubestipulanteModel? ApoliceSubestipulante { get; set; }
    public ApoliceSubestipulanteModuloModel? ApoliceSubestipulanteModulo { get; set; }
    
    public ICollection<Propostum> Propostas { get; set; } = new List<Propostum>();
}
