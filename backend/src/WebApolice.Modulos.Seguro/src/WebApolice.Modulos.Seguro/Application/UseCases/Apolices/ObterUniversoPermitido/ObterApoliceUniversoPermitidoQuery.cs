using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ObterUniversoPermitido;

public sealed record ObterApoliceUniversoPermitidoQuery(
    Guid ApolicePublicId
);

public sealed record ApoliceUniversoPermitidoResult(
    List<ApoliceProdutoResult> Produtos
);

public sealed record ApoliceProdutoResult(
    long ProdutoIdInternal,
    bool Ativo,
    List<ApolicePlanoResult> Planos
);

public sealed record ApolicePlanoResult(
    long PlanoIdInternal,
    long? TabelaPrecoIdInternal,
    bool Ativo,
    List<ApoliceCoberturaResult> Coberturas
);

public sealed record ApoliceCoberturaResult(
    long CoberturaIdInternal,
    bool Ativo,
    decimal? ImportanciaSeguradaOverride,
    decimal? PremioOverride
);
