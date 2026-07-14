using System;

namespace WebApolice.Modulos.Clientes.Infrastructure.Persistence.Models;

/// <summary>
/// Modelo de persistência para leitura da tabela core.pessoa_contato.
/// Mapeamento parcial fiel aos tipos da base oficial.
/// </summary>
public sealed class PessoaContatoModel
{
    public long Id { get; private set; }
    public long PessoaId { get; private set; }
    public string TipoContato { get; private set; } = null!;
    public string Valor { get; private set; } = null!;
    public string? ValorNormalizado { get; private set; }
    public bool Principal { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private PessoaContatoModel() { }

    public PessoaContatoModel(long pessoaId, string tipoContato, string valor, string? valorNormalizado, bool principal)
    {
        PessoaId = pessoaId;
        TipoContato = tipoContato;
        Valor = valor;
        ValorNormalizado = valorNormalizado;
        Principal = principal;
        Ativo = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void AtualizarValor(string valor, string? valorNormalizado, bool principal)
    {
        Valor = valor;
        ValorNormalizado = valorNormalizado;
        Principal = principal;
    }

    public void Inativar()
    {
        Ativo = false;
    }
}
