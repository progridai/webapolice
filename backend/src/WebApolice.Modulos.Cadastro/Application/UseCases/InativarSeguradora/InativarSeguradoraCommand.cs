using System;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.InativarSeguradora;

public class InativarSeguradoraCommand
{
    public Guid PublicId { get; set; }

    public InativarSeguradoraCommand(Guid publicId)
    {
        PublicId = publicId;
    }
}
