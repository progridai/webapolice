using System;
using System.Collections.Generic;

namespace WebApolice.Shared.Infrastructure.Persistence.Models;

public partial class PessoaDocumento
{
    public long Id { get; set; }

    public long PessoaId { get; set; }

    public string TipoDocumento { get; set; } = null!;

    public string? Numero { get; set; }

    public string? NumeroLimpo { get; set; }

    public string? OrgaoEmissor { get; set; }

    public DateOnly? DataEmissao { get; set; }

    public bool Principal { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Pessoa Pessoa { get; set; } = null!;
}
