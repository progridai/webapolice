using System;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;

/// <summary>
/// Representa uma divisão contextual de uma Apólice (Subgrupo).
/// Subgrupo pertence exclusivamente a uma Apólice — não é cadastro global.
/// </summary>
public class ApoliceSubgrupoModel
{
    public long Id { get; set; }
    public Guid PublicId { get; set; }

    public long ApoliceId { get; set; }

    /// <summary>
    /// Nome do Subgrupo. Identificação funcional definida pelo negócio.
    /// O sistema não interpreta o nome; ele é apenas informativo.
    /// Exemplos: "Matriz", "Filial Centro", "Funcionários", "Débito em Conta".
    /// </summary>
    public string Nome { get; set; } = null!;

    public string? Observacao { get; set; }

    public bool Ativo { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    // Propriedade de Navegação (EF)
    public ApoliceModel? Apolice { get; set; }
}
