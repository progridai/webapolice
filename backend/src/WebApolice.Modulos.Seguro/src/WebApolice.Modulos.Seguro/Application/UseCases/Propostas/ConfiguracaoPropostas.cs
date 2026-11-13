namespace WebApolice.Modulos.Seguro.Application.UseCases.Propostas;

public sealed class ConfiguracaoPropostas
{
    public string StatusDescartadaCodigo { get; set; } = string.Empty;
    public string StatusCanceladaCodigo { get; set; } = string.Empty;
    public bool ProcessarAgendamentos { get; set; }
    public string StatusInicialCodigo { get; set; } = string.Empty;
    public string StatusConfirmadaCodigo { get; set; } = string.Empty;
}
