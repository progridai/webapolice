using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Convenio.src.WebApolice.Modulos.Convenio.Infrastructure.Persistence.Models;

public partial class SiapeCliente
{
    public long Id { get; set; }

    public long ClienteId { get; set; }

    public long? ClienteVinculoId { get; set; }

    public long PessoaId { get; set; }

    public string? Siape { get; set; }

    public long? OrgaoId { get; set; }

    public string? Categoria { get; set; }

    public string? Setor { get; set; }

    public string? Instituto { get; set; }

    public string? Agencia { get; set; }

    public string? Funcao { get; set; }

    public string? Contrato { get; set; }

    public string? DigitoVerificador { get; set; }

    public string? Instituidor { get; set; }

    public bool Ativo { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual SiapeOrgao? Orgao { get; set; }
}
