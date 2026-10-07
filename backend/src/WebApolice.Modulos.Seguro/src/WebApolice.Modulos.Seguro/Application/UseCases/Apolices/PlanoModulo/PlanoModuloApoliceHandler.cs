using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.Application.UseCases.Catalogos;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Apolices.PlanoModulo;

public sealed class PlanoModuloApoliceHandler(IPlanoModuloApolice cadastro)
{
    public Task<CatalogoPagina<CoberturaModuloOpcao>> OpcoesCoberturasAsync(Guid apolice, Guid modulo, int pagina, int tamanho, CancellationToken ct) =>
        cadastro.OpcoesCoberturasAsync(apolice, modulo, Math.Max(1, pagina), Math.Clamp(tamanho, 1, 100), ct);

    public Task<ConfiguracaoPlanoModuloDto> ObterAsync(Guid apolice, Guid modulo, CancellationToken ct) =>
        cadastro.ObterAsync(apolice, modulo, ct);

    public Task<PlanoModuloDto> SalvarPlanoAsync(Guid apolice, Guid modulo, PlanoModuloDados dados, CancellationToken ct)
    {
        ValidarPlano(dados);
        return cadastro.SalvarPlanoAsync(apolice, modulo, dados, ct);
    }

    public static void ValidarPlano(PlanoModuloDados dados)
    {
        CatalogosSeguroHandler.ValidarNome(dados.Nome);
        if (dados.Ramo?.Trim().Length > 80) throw new ValidacaoException("Ramo excede 80 caracteres.");
    }

    public Task<CoberturaModuloDto> SalvarCoberturaAsync(Guid apolice, Guid modulo, Guid? cobertura,
        CoberturaModuloDados dados, CancellationToken ct)
    {
        ValidarCobertura(dados);
        return cadastro.SalvarCoberturaAsync(apolice, modulo, cobertura, dados, ct);
    }

    public static void ValidarCobertura(CoberturaModuloDados dados)
    {
        if (dados.CoberturaPublicId == Guid.Empty) throw new ValidacaoException("Selecione uma Cobertura do cadastro.");
        CatalogosSeguroHandler.ValidarPremios(new() { PremioTitular = dados.PremioTitular, PremioConjuge = dados.PremioConjuge }, true);
    }
}
