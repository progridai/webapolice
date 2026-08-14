using System;
using System.Collections.Generic;

namespace WebApolice.Shared.Infrastructure.Persistence.Models;

public partial class ClienteMigrationMap
{
    public long Id { get; set; }

    public int LegadoClienteId { get; set; }

    public long PessoaId { get; set; }

    public long ClienteId { get; set; }

    public long? ClienteVinculoId { get; set; }

    public string? CpfOriginal { get; set; }

    public string? CpfLimpo { get; set; }

    public bool CpfValido { get; set; }

    public string? NomeOriginal { get; set; }

    public string? MatriculaOriginal { get; set; }

    public string CriterioUnificacaoPessoa { get; set; } = null!;

    public string CriterioCriacaoVinculo { get; set; } = null!;

    public string? Observacao { get; set; }

    public DateTime CreatedAt { get; set; }
}
