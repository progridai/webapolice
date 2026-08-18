using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;

public class ApoliceModel
{
    public long Id { get; set; }
    public Guid PublicId { get; set; }

    public long EstipulanteId { get; set; }
    public long SeguradoraId { get; set; }
    public long? CorretoraId { get; set; }

    public string? Nome { get; set; }
    public DateOnly DataInicioVigencia { get; set; }
    public DateOnly? DataFimVigencia { get; set; }
    public DateOnly? DataAniversario { get; set; }

    public long? ApoliceOrigemId { get; set; }
    public int Versao { get; set; }

    public string Status { get; set; } = "ativa";
    public bool Ativo { get; set; }

    public int? LegadoId { get; set; }
    public string? Observacao { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    // Propriedades de Navegação (EF)
    public ApoliceModel? ApoliceOrigem { get; set; }
    public ICollection<ApoliceModel> Renovacoes { get; set; } = new List<ApoliceModel>();
    public ApoliceConfiguracaoModel? Configuracao { get; set; }
    public ICollection<ApoliceRamoModel> Ramos { get; set; } = new List<ApoliceRamoModel>();
    public ICollection<ApoliceSubestipulanteModel> Subestipulantes { get; set; } = new List<ApoliceSubestipulanteModel>();
    public ICollection<ApoliceVidaModel> Vidas { get; set; } = new List<ApoliceVidaModel>();
    public ICollection<ApoliceHistoricoModel> Historicos { get; set; } = new List<ApoliceHistoricoModel>();
    public ICollection<ApoliceProdutoModel> ApoliceProdutos { get; set; } = new List<ApoliceProdutoModel>();
    public ICollection<Propostum> Propostas { get; set; } = new List<Propostum>();
}
