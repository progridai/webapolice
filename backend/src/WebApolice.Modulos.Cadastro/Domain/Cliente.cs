using System;
using WebApolice.Modulos.Cadastro.Domain.Exceptions;

namespace WebApolice.Modulos.Cadastro.Domain;

/// <summary>
/// Raiz de agregaÃ§Ã£o do mÃ³dulo de Clientes.
/// Mapeada para a tabela oficial 'cadastro.cliente'.
/// Dados pessoais residem em core.pessoa e nÃ£o pertencem a este agregado diretamente.
/// </summary>
public sealed class Cliente
{
    public long Id { get; private set; }
    public Guid PublicId { get; private set; }
    public long PessoaId { get; private set; }
    
    // Supondo StatusId = 1 (Ativo), 2 (Inativo) de acordo com cadastro.cliente_status
    public short StatusId { get; private set; } 
    
    public bool Falecido { get; private set; }
    public DateOnly? DataObito { get; private set; }
    public string? Observacao { get; private set; }
    public DateOnly? DataCadastroLegado { get; private set; }
    public int? LegadoId { get; private set; }
    public string? Re { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    // Construtor para o EF Core
    private Cliente() { }

    public Cliente(long pessoaId, short statusId)
    {
        PessoaId = pessoaId;
        PublicId = Guid.NewGuid();
        StatusId = statusId; 
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public void Ativar(short statusIdAtivo)
    {
        if (StatusId == statusIdAtivo) return;
        StatusId = statusIdAtivo;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Inativar(short statusIdInativo)
    {
        if (StatusId == statusIdInativo) return;
        StatusId = statusIdInativo;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RegistrarObito(DateOnly dataObito)
    {
        Falecido = true;
        DataObito = dataObito;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AtualizarDados(bool falecido, DateOnly? dataObito, string? observacao, string? re)
    {
        Falecido = falecido;
        DataObito = dataObito;
        Observacao = observacao;
        Re = re;
        UpdatedAt = DateTime.UtcNow;
    }
}
