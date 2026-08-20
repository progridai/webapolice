using System;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.ReativarSeguradora;

public class ReativarSeguradoraCommand
{
    public Guid PublicId { get; set; }

    public ReativarSeguradoraCommand(Guid publicId)
    {
        PublicId = publicId;
    }
}
