using System;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.AlterarApoliceVida;

/// <summary>
/// Campos permitidos para edição de uma Vida na Apólice.
/// Subgrupo e Módulo são independentes e determinam as características do vínculo.
/// </summary>
public sealed class AlterarApoliceVidaCommand
{
    public Guid ApolicePublicId { get; set; }
    public Guid ApoliceVidaPublicId { get; set; }
    public Guid? ApoliceSubgrupoPublicId { get; set; }
    public Guid? ApoliceModuloPublicId { get; set; }
    public DateOnly? DataInicioVigencia { get; set; }
    public DateOnly? DataFimVigencia { get; set; }
    public string? Observacao { get; set; }
    public Guid UsuarioPublicId { get; set; }
}
