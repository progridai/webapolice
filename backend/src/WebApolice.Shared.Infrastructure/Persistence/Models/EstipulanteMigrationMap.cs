using System;
using System.Collections.Generic;

namespace WebApolice.Shared.Infrastructure.Persistence.Models;

public partial class EstipulanteMigrationMap
{
    public long Id { get; set; }

    public int LegadoEstipulanteId { get; set; }

    public long? PessoaId { get; set; }

    public long EstipulanteId { get; set; }

    public string? CnpjOriginal { get; set; }

    public string? CnpjLimpo { get; set; }

    public bool CnpjValido { get; set; }

    public string? NomeOriginal { get; set; }

    public string? CriterioUnificacaoPessoa { get; set; }

    public string? Observacao { get; set; }

    public DateTime CreatedAt { get; set; }
}
