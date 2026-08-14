using System;
using System.Collections.Generic;

namespace WebApolice.Shared.Infrastructure.Persistence.Models;

public partial class Pessoa
{
    public long Id { get; set; }

    public Guid PublicId { get; set; }

    public short TipoPessoa { get; set; }

    public string Nome { get; set; } = null!;

    public string? NomeNormalizado { get; set; }

    public string? DocumentoPrincipal { get; set; }

    public string? DocumentoPrincipalLimpo { get; set; }

    public bool DocumentoValido { get; set; }

    public DateOnly? DataNascimento { get; set; }

    public short? Sexo { get; set; }

    public string? Observacao { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<PessoaContatoInstitucional> PessoaContatoInstitucionals { get; set; } = new List<PessoaContatoInstitucional>();

    public virtual ICollection<PessoaContato> PessoaContatos { get; set; } = new List<PessoaContato>();

    public virtual ICollection<PessoaDocumento> PessoaDocumentos { get; set; } = new List<PessoaDocumento>();

    public virtual ICollection<PessoaEndereco> PessoaEnderecos { get; set; } = new List<PessoaEndereco>();
}
