using System;
using System.Collections.Generic;

namespace WebApolice.Shared.Infrastructure.Persistence.Models;

public partial class MovimentoPropostaMigrationMap
{
    public long Id { get; set; }

    public int LegadoMovimentoPropostaId { get; set; }

    public long PropostaMovimentoId { get; set; }

    public long? TituloId { get; set; }

    public long? TituloPagamentoId { get; set; }

    public long? TituloRetornoBancarioId { get; set; }

    public long? LancamentoComissaoId { get; set; }

    public int? LegadoPropostaId { get; set; }

    public long? PropostaId { get; set; }

    public int? LegadoClienteId { get; set; }

    public long? ClienteId { get; set; }

    public long? ClienteVinculoId { get; set; }

    public long? PessoaId { get; set; }

    public int? LegadoEstipulanteId { get; set; }

    public long? EstipulanteId { get; set; }

    public int? LegadoMovimentoId { get; set; }

    public long? MovimentoTipoId { get; set; }

    public string? Classificacao { get; set; }

    public string? CriterioMigracao { get; set; }

    public string? Observacao { get; set; }

    public DateTime CreatedAt { get; set; }
}
