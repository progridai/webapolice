using System;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ObterApoliceVida;

public sealed record ObterApoliceVidaQuery(
    Guid ApolicePublicId,
    Guid ApoliceVidaPublicId
);
