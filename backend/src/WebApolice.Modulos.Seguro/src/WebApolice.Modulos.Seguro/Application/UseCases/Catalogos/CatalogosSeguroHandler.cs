using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.SharedKernel.Application.Exceptions;
namespace WebApolice.Modulos.Seguro.Application.UseCases.Catalogos;
public sealed class CatalogosSeguroHandler(ICatalogosSeguro cadastro)
{
    public Task<CatalogoPagina<CoberturaDto>> ListarCoberturasAsync(int pagina,int tamanho,string? busca,bool? ativo,CancellationToken ct)=>cadastro.ListarCoberturasAsync(Math.Max(1,pagina),Math.Clamp(tamanho,1,100),busca?.Trim(),ativo,ct);
    public Task<CatalogoPagina<PlanoDto>> ListarPlanosAsync(int pagina,int tamanho,string? busca,bool? ativo,CancellationToken ct)=>cadastro.ListarPlanosAsync(Math.Max(1,pagina),Math.Clamp(tamanho,1,100),busca?.Trim(),ativo,ct);
    public Task<CoberturaDto?> ObterCoberturaAsync(Guid id,CancellationToken ct)=>cadastro.ObterCoberturaAsync(id,ct);
    public Task<PlanoDto?> ObterPlanoAsync(Guid id,CancellationToken ct)=>cadastro.ObterPlanoAsync(id,ct);
    public Task StatusAsync(Guid id,bool ativo,bool cobertura,CancellationToken ct)=>cadastro.StatusAsync(id,ativo,cobertura,ct);
    public Task<IReadOnlyList<PlanoCoberturaDto>> CoberturasPlanoAsync(Guid id,CancellationToken ct)=>cadastro.CoberturasPlanoAsync(id,ct);
    public Task VincularAsync(Guid plano,Guid cobertura,bool ativo,CancellationToken ct)=>cadastro.VincularAsync(plano,cobertura,ativo,ct);
    public Task SalvarVinculoAsync(Guid plano, Guid cobertura, PlanoCoberturaDados dados, CancellationToken ct)
    {
        ValidarPremios(dados, obrigatorios: true);
        return cadastro.SalvarVinculoAsync(plano, cobertura, dados, ct);
    }
    public static void ValidarPremios(PremiosCoberturaDados dados, bool obrigatorios)
    {
        foreach (var campo in new[] { ("titular", dados.PremioTitular), ("cônjuge", dados.PremioConjuge) })
        {
            if (obrigatorios && campo.Item2 is null)
                throw new ValidacaoException($"Informe o prêmio do {campo.Item1}. Use zero quando não houver cobrança.");
            if (campo.Item2 is decimal valor && (valor < 0 || valor > 9999999999999999.99m || decimal.Round(valor, 2) != valor))
                throw new ValidacaoException($"O prêmio do {campo.Item1} deve ser um valor não negativo, com até duas casas decimais e dentro do limite monetário.");
        }
    }
    public Task<CoberturaDto> SalvarCoberturaAsync(Guid? id,CoberturaDados dados,CancellationToken ct)
    {
        ValidarNome(dados.Nome);
        if(dados.NomeReduzido?.Trim().Length>30 || dados.Basica?.Trim().Length>50) throw new ValidacaoException("Nome reduzido ou classificação básica excede o limite permitido.");
        return cadastro.SalvarCoberturaAsync(id,dados,ct);
    }
    public Task<PlanoDto> SalvarPlanoAsync(Guid? id,PlanoDados dados,CancellationToken ct)
    {
        ValidarNome(dados.Nome);
        if(dados.Ramo?.Trim().Length>80) throw new ValidacaoException("Ramo excede 80 caracteres.");
        return cadastro.SalvarPlanoAsync(id,dados,ct);
    }
    public static void ValidarNome(string nome)
    {
        if(string.IsNullOrWhiteSpace(nome) || nome.Trim().Length>150) throw new ValidacaoException("Nome é obrigatório e deve ter até 150 caracteres.");
    }
}
