using System;

namespace WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Models;

/// <summary>
/// Modelo de persistÃªncia para leitura da tabela core.pessoa_endereco.
/// Mapeamento parcial fiel aos tipos da base oficial.
/// </summary>
public sealed class PessoaEnderecoModel
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
    public DateTimeOffset CreatedAt { get; set; }

    public PessoaEnderecoModel() { }

    public PessoaEnderecoModel(long pessoaId, long? cidadeId, string tipoEndereco, string? cep, string? logradouro, string? numero, string? complemento, string? bairro, string? uf, bool principal)
    {
        PessoaId = pessoaId;
        CidadeId = cidadeId;
        TipoEndereco = tipoEndereco;
        Cep = cep;
        Logradouro = logradouro;
        Numero = numero;
        Complemento = complemento;
        Bairro = bairro;
        Uf = uf;
        Principal = principal;
        Ativo = true;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void AtualizarEndereco(long? cidadeId, string? cep, string? logradouro, string? numero, string? complemento, string? bairro, string? uf, bool principal)
    {
        CidadeId = cidadeId;
        Cep = cep;
        Logradouro = logradouro;
        Numero = numero;
        Complemento = complemento;
        Bairro = bairro;
        Uf = uf;
        Principal = principal;
    }

    public void Inativar()
    {
        Ativo = false;
    }
}
