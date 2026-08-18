using System;
using System.Collections.Generic;

namespace WebApolice.Shared.Infrastructure.Persistence.Models;

public partial class PessoaEndereco
{
    public long Id { get; set; }

    public long PessoaId { get; set; }

    public long? CidadeId { get; set; }

    public string TipoEndereco { get; set; } = null!;

    public string? Cep { get; set; }

    public string? Logradouro { get; set; }

    public string? Numero { get; set; }

    public string? Complemento { get; set; }

    public string? Bairro { get; set; }

    public string? Uf { get; set; }

    public bool Principal { get; set; }

    public bool Ativo { get; set; }

    public int? LegadoSituacaoEndereco { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Cidade? Cidade { get; set; }

    public virtual Pessoa Pessoa { get; set; } = null!;
}
