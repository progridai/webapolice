using System;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Ramos.AlterarRamo;

public class AlterarRamoCommand
{
    public Guid PublicId { get; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }

    public AlterarRamoCommand(Guid publicId, string nome, string? descricao)
    {
        PublicId = publicId;
        Nome = nome;
        Descricao = descricao;
    }
}
