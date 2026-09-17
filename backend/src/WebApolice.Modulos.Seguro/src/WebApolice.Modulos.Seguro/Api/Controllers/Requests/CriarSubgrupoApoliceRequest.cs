namespace WebApolice.Modulos.Seguro.Api.Controllers.Requests;

/// <summary>
/// Payload para criação de um Subgrupo da Apólice.
/// Nome é o único campo obrigatório.
/// </summary>
public class CriarSubgrupoApoliceRequest
{
    /// <summary>
    /// Nome do Subgrupo. O sistema não interpreta o valor — é apenas uma identificação funcional.
    /// Exemplos: "Matriz", "Filial Centro", "Funcionários", "Débito em Conta".
    /// </summary>
    public string Nome { get; set; } = null!;

    public string? Observacao { get; set; }
}
