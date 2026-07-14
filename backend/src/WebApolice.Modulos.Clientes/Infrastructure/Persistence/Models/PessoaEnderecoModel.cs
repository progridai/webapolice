using System;

namespace WebApolice.Modulos.Clientes.Infrastructure.Persistence.Models;

/// <summary>
/// Modelo de persistência para leitura da tabela core.pessoa_endereco.
/// Mapeamento parcial fiel aos tipos da base oficial.
/// </summary>
public sealed class PessoaEnderecoModel
{
    public long Id { get; private set; }
    public long PessoaId { get; private set; }
    public long? CidadeId { get; private set; }
    public string TipoEndereco { get; private set; } = null!;
    public string? Cep { get; private set; }
    public string? Logradouro { get; private set; }
    public string? Numero { get; private set; }
    public string? Complemento { get; private set; }
    public string? Bairro { get; private set; }
    public string? Uf { get; private set; }
    public bool Principal { get; private set; }
    public bool Ativo { get; private set; }
    public int? LegadoSituacaoEndereco { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private PessoaEnderecoModel() { }

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
        CreatedAt = DateTime.UtcNow;
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
}
