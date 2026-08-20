using System;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Ramos.ObterRamo;

public class ObterRamoQuery
{
    public Guid PublicId { get; }

    public ObterRamoQuery(Guid publicId)
    {
        PublicId = publicId;
    }
}
