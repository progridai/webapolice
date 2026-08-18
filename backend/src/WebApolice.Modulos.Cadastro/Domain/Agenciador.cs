using System;

namespace WebApolice.Modulos.Cadastro.Domain;

/// <summary>
/// Raiz de agregação para o contexto de Cooperados e Coordenadores.
/// Mapeada para a tabela 'cadastro.agenciador'.
/// Dados pessoais básicos residem em core.pessoa.
/// </summary>
public sealed class Agenciador
{
    public long Id { get; private set; }
    public Guid PublicId { get; private set; }
    public long? PessoaId { get; private set; }
    public long? CidadeId { get; private set; }
    public long? BancoId { get; private set; }
    public long? CoordenadorId { get; private set; }
    
    // Colunas históricas obrigatórias da tabela
    public string? Nome { get; private set; } 
    public bool CpfValido { get; private set; }

    public string? Codigo { get; private set; }
    public TipoAgenciador Tipo { get; private set; }
    public string? Susep { get; private set; }
    public string? Inss { get; private set; }
    public string? Issqn { get; private set; }
    
    public int? NumeroDependentes { get; private set; }
    public DateOnly? DataInscricao { get; private set; }
    public bool? Credenciado { get; private set; }
    
    public string? Agencia { get; private set; }
    public string? ContaCorrente { get; private set; }
    public string? Observacao { get; private set; }
    
    public int? LegadoId { get; private set; }
    
    public bool Desativado { get; private set; }
    public DateOnly? DataDesativado { get; private set; }
    
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

    private Agenciador() { }

    public Agenciador(long pessoaId, TipoAgenciador tipo, string codigo, string susep, string inss, string issqn, int? numeroDependentes, DateOnly? dataInscricao, bool? credenciado, long? coordenadorId, long? bancoId, string agencia, string contaCorrente, string observacao)
    {
        PessoaId = pessoaId;
        PublicId = Guid.NewGuid();
        Tipo = tipo;
        Codigo = codigo;
        Susep = susep;
        Inss = inss;
        Issqn = issqn;
        NumeroDependentes = numeroDependentes;
        DataInscricao = dataInscricao;
        Credenciado = credenciado;
        CoordenadorId = tipo == TipoAgenciador.Cooperado ? coordenadorId : null;
        BancoId = bancoId;
        Agencia = agencia;
        ContaCorrente = contaCorrente;
        Observacao = observacao;
        
        Desativado = false;
        CpfValido = true; // Assumimos que o validador de domínio já tratou
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public void AtualizarDados(string codigo, string susep, string inss, string issqn, int? numeroDependentes, DateOnly? dataInscricao, bool? credenciado, long? coordenadorId, long? bancoId, string agencia, string contaCorrente, string observacao)
    {
        Codigo = codigo;
        Susep = susep;
        Inss = inss;
        Issqn = issqn;
        NumeroDependentes = numeroDependentes;
        DataInscricao = dataInscricao;
        Credenciado = credenciado;
        CoordenadorId = Tipo == TipoAgenciador.Cooperado ? coordenadorId : null;
        BancoId = bancoId;
        Agencia = agencia;
        ContaCorrente = contaCorrente;
        Observacao = observacao;
        
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Inativar(DateOnly dataDesligamento)
    {
        if (Desativado) return;
        Desativado = true;
        DataDesativado = dataDesligamento;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Ativar()
    {
        if (!Desativado) return;
        Desativado = false;
        DataDesativado = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
