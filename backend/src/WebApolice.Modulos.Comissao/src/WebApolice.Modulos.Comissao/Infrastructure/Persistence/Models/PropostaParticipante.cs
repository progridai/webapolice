using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Comissao.src.WebApolice.Modulos.Comissao.Infrastructure.Persistence.Models;

public partial class PropostaParticipante
{
    public long Id { get; set; }

    public long PropostaId { get; set; }

    public string ParticipanteTipo { get; set; } = null!;

    public long? ParticipanteId { get; set; }

    public string? CodigoAgenciamento { get; set; }

    public decimal? PercentualAgenciamento { get; set; }

    public int? AgenciamentoParcelaInicial { get; set; }

    public int? AgenciamentoParcelaFinal { get; set; }

    public decimal? Bonus { get; set; }

    public decimal? PercentualCarteira { get; set; }

    public int? CarteiraParcelaInicial { get; set; }

    public bool Ativo { get; set; }

    public string? LegadoCampoOrigem { get; set; }

    public DateTime CreatedAt { get; set; }

    public long? AgenciadorId { get; set; }

    public long? CorretoraId { get; set; }

    public int? CodigoLegadoParticipante { get; set; }
}
