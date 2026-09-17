using System;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Apolices.AlterarSubgrupo;

/// <summary>
/// Command para alterar nome e/ou observação de um Subgrupo da Apólice.
/// Status (ativo/inativo) é gerenciado por fluxo separado (inativar).
/// </summary>
public class AlterarSubgrupoApoliceCommand
{
    public Guid ApolicePublicId { get; set; }
    public Guid SubgrupoPublicId { get; set; }

    /// <summary>
    /// Novo nome do Subgrupo. Obrigatório.
    /// </summary>
    public string Nome { get; set; } = null!;

    public string? Observacao { get; set; }
}
