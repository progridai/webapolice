using System;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Apolices.CriarSubgrupo;

/// <summary>
/// Command para criar um novo Subgrupo da Apólice.
/// O payload é simples: apenas nome e observação (opcional).
/// O Subgrupo pertence exclusivamente à Apólice indicada.
/// </summary>
public class CriarSubgrupoApoliceCommand
{
    public Guid ApolicePublicId { get; set; }

    /// <summary>
    /// Nome do Subgrupo. Identificação funcional — o sistema não interpreta o valor.
    /// Exemplos: "Matriz", "Filial Centro", "Funcionários".
    /// </summary>
    public string Nome { get; set; } = null!;

    public string? Observacao { get; set; }
}
