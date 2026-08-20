using System;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.ConsultarSeguradora;

public class ObterSeguradoraPorIdQuery
{
    public Guid PublicId { get; set; }

    public ObterSeguradoraPorIdQuery(Guid publicId)
    {
        PublicId = publicId;
    }
}
