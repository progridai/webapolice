import { httpClient } from '../../../services/http/httpClient';

export interface PlanoModuloDados {
  nome: string;
  ramo: string | null;
  paga: boolean | null;
  reajuste: boolean | null;
  ativo: boolean;
}
export interface CoberturaModuloDados {
  coberturaPublicId: string;
  premioTitular: number;
  premioConjuge: number;
  ativo: boolean;
}
export interface CoberturaModulo extends CoberturaModuloDados {
  publicId: string;
  coberturaAtiva: boolean;
  nome: string;
  nomeReduzido: string | null;
  basica: string | null;
  reajuste: boolean | null;
}
export interface PlanoModulo extends PlanoModuloDados {
  publicId: string;
  coberturas: CoberturaModulo[];
}
export interface ConfiguracaoPlanoModulo {
  apoliceModuloPublicId: string;
  podeAlterar: boolean;
  plano: PlanoModulo | null;
}
const caminho = (apolice: string, modulo: string) =>
  `/api/apolices/${apolice}/modulos/${modulo}/plano`;
export const planoModuloApi = {
  obter: async (apolice: string, modulo: string, signal?: AbortSignal) =>
    (
      await httpClient.get<ConfiguracaoPlanoModulo>(caminho(apolice, modulo), {
        signal,
      })
    ).data,
  opcoesCoberturas: async (
    apolice: string,
    modulo: string,
    pagina: number,
    signal?: AbortSignal,
  ) =>
    (
      await httpClient.get<{
        items: { publicId: string; nome: string }[];
        totalCount: number;
      }>(`${caminho(apolice, modulo)}/coberturas/opcoes`, {
        params: { pagina, tamanho: 100 },
        signal,
      })
    ).data,
  salvarPlano: async (
    apolice: string,
    modulo: string,
    dados: PlanoModuloDados,
  ) =>
    (await httpClient.put<PlanoModulo>(caminho(apolice, modulo), dados)).data,
  salvarCobertura: async (
    apolice: string,
    modulo: string,
    id: string | undefined,
    dados: CoberturaModuloDados,
  ) =>
    (id
      ? await httpClient.put<CoberturaModulo>(
          `${caminho(apolice, modulo)}/coberturas/${id}`,
          dados,
        )
      : await httpClient.post<CoberturaModulo>(
          `${caminho(apolice, modulo)}/coberturas`,
          dados,
        )
    ).data,
};
