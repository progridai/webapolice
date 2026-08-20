using System;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.ConsultarSeguradora;

public class SeguradoraDetalheResult
{
    public Guid PublicId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Codigo { get; set; }
    public string? Susep { get; set; }
    public string? Cnpj { get; set; }
    public string? CnpjLimpo { get; set; }
    public bool Ativo { get; set; }
    public string? Observacao { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
