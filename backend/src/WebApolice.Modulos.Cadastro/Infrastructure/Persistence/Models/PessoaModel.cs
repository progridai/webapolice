using System;
using System.Collections.Generic;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Models.Vinculos;

namespace WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Models;

public class PessoaModel
{
    public long Id { get; set; }
    public Guid PublicId { get; set; }
    public short TipoPessoa { get; set; }
    public string Nome { get; set; } = null!;
    public string? NomeNormalizado { get; set; }
    public string? DocumentoPrincipal { get; set; }
    public string? DocumentoPrincipalLimpo { get; set; }
    public bool DocumentoValido { get; set; }
    public DateTime? DataNascimento { get; set; }
    public short? Sexo { get; set; }
    public string? Observacao { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    
    public ICollection<EstipulanteModel> Estipulantes { get; set; } = new List<EstipulanteModel>();

    public PessoaModel() { }

    public PessoaModel(short tipoPessoa, string nome, string? documentoPrincipal, string? documentoPrincipalLimpo, bool documentoValido, DateTime? dataNascimento, short? sexo, string? observacao)
    {
        PublicId = Guid.NewGuid();
        TipoPessoa = tipoPessoa;
        Nome = nome;
        NomeNormalizado = nome.ToUpperInvariant();
        DocumentoPrincipal = documentoPrincipal;
        DocumentoPrincipalLimpo = documentoPrincipalLimpo;
        DocumentoValido = documentoValido;
        DataNascimento = dataNascimento;
        Sexo = sexo;
        Observacao = observacao;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void AtualizarDadosPessoais(string nome, DateTime? dataNascimento, short? sexo, string? observacao)
    {
        Nome = nome;
        NomeNormalizado = nome.ToUpperInvariant();
        DataNascimento = dataNascimento;
        Sexo = sexo;
        Observacao = observacao;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void AtualizarDocumento(string? documentoPrincipal, string? documentoPrincipalLimpo)
    {
        DocumentoPrincipal = documentoPrincipal;
        DocumentoPrincipalLimpo = documentoPrincipalLimpo;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
