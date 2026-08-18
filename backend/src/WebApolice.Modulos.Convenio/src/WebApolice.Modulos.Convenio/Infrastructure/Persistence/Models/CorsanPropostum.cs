using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Convenio.src.WebApolice.Modulos.Convenio.Infrastructure.Persistence.Models;

public partial class CorsanPropostum
{
    public long Id { get; set; }

    public long PropostaId { get; set; }

    public long? ClienteId { get; set; }

    public long? ClienteVinculoId { get; set; }

    public long? PessoaId { get; set; }

    public string? Empresa { get; set; }

    public string? Rubrica { get; set; }

    public string? Grupo { get; set; }

    public bool Ativo { get; set; }

    public DateTime CreatedAt { get; set; }
}
