using System;

namespace WebApolice.Modulos.Clientes.Infrastructure.Persistence.Models;

/// <summary>
/// Modelo de persistência para leitura da tabela core.pessoa.
/// Mapeamento parcial fiel aos tipos da base oficial.
/// </summary>
public sealed class PessoaModel
{
    public long Id { get; private set; }
    public Guid PublicId { get; private set; }
    public short TipoPessoa { get; private set; }
    public string Nome { get; private set; } = null!;
    public string? NomeNormalizado { get; private set; }
    public string? DocumentoPrincipal { get; private set; }
    
    // Mapeado no private set para uso interno, nunca exportado.
    public string? DocumentoPrincipalLimpo { get; private set; }
    
    public bool DocumentoValido { get; private set; }
    public DateOnly? DataNascimento { get; private set; }
    public short? Sexo { get; private set; }
    public string? Observacao { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    // EF Core constructor
    private PessoaModel() { }

    public PessoaModel(short tipoPessoa, string nome, string? documentoPrincipal, string? documentoPrincipalLimpo, bool documentoValido, DateOnly? dataNascimento, short? sexo, string? observacao)
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
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AtualizarDadosPessoais(string nome, DateOnly? dataNascimento, short? sexo, string? observacao)
    {
        Nome = nome;
        NomeNormalizado = nome.ToUpperInvariant();
        DataNascimento = dataNascimento;
        Sexo = sexo;
        Observacao = observacao;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AtualizarDocumento(string? documentoPrincipal, string? documentoPrincipalLimpo)
    {
        DocumentoPrincipal = documentoPrincipal;
        DocumentoPrincipalLimpo = documentoPrincipalLimpo;
        UpdatedAt = DateTime.UtcNow;
    }
}
