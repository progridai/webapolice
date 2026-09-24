using System;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.CriarApoliceVida;

public sealed class CriarApoliceVidaCommand
{
    public Guid ApolicePublicId { get; set; }
    public Guid ClientePublicId { get; set; }
    public Guid? ApoliceSubgrupoPublicId { get; set; }
    public Guid? ApoliceModuloPublicId { get; set; }
    public DateOnly? DataInicioVigencia { get; set; }
    public DateOnly? DataFimVigencia { get; set; }
    public string? Observacao { get; set; }
    public Guid UsuarioPublicId { get; set; }
}
