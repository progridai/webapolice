using System;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Ramos.AlterarStatusRamo;

public class AlterarStatusRamoCommand
{
    public Guid PublicId { get; }
    public bool Ativo { get; }

    public AlterarStatusRamoCommand(Guid publicId, bool ativo)
    {
        PublicId = publicId;
        Ativo = ativo;
    }
}
