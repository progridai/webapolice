using System;

namespace WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Models;

/// <summary>
/// Modelo de persistÃªncia para leitura da tabela core.pessoa_contato.
/// Mapeamento parcial fiel aos tipos da base oficial.
/// </summary>
public sealed class PessoaContatoModel
{
    public long Id { get; set; }
    public long PessoaId { get; set; }
    public string TipoContato { get; set; } = null!;
    public string Valor { get; set; } = null!;
    public string? ValorNormalizado { get; set; }
    public bool Principal { get; set; }
    public bool Ativo { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public PessoaContatoModel() { }

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
