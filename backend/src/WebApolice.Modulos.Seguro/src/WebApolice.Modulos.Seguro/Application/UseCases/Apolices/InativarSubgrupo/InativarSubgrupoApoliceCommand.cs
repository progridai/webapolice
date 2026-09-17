using System;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Apolices.InativarSubgrupo;

public class InativarSubgrupoApoliceCommand
{
    public Guid ApolicePublicId { get; set; }
    public Guid SubgrupoPublicId { get; set; }
}
