import { httpClient } from '../../../services/http/httpClient';
export type ValorCampo = string | number | boolean | null;
export interface RegistroCatalogo {
  publicId: string;
  nome: string;
  ativo: boolean;
  createdAt: string;
  updatedAt: string;
  [campo: string]: ValorCampo;
}
export interface PaginaCatalogo {
  items: RegistroCatalogo[];
  totalCount: number;
}
export interface BancoOpcao {
  codigo: string;
  nome: string;
}
export interface CoberturaPlano {
  coberturaPublicId: string;
  nome: string;
  ativo: boolean;
  coberturaAtiva: boolean;
  premioTitular: number | null;
  premioConjuge: number | null;
}
export interface PremiosCobertura {
  premioTitular: number | null;
  premioConjuge: number | null;
}
export interface ConvenioOpcao {
  publicId: string;
  nome: string;
  ativo: boolean;
}
export const catalogosApi = {
  listar: async (
    recurso: string,
    params: Record<string, string | number | boolean>,
    signal?: AbortSignal,
  ) =>
    (
      await httpClient.get<PaginaCatalogo>(`/api/${recurso}`, {
        params,
        signal,
      })
    ).data,
  obter: async (recurso: string, id: string, signal?: AbortSignal) =>
    (
      await httpClient.get<RegistroCatalogo>(`/api/${recurso}/${id}`, {
        signal,
      })
    ).data,
  salvar: async (
    recurso: string,
    id: string | undefined,
    dados: Record<string, ValorCampo>,
  ) =>
    id
      ? (await httpClient.put<RegistroCatalogo>(`/api/${recurso}/${id}`, dados))
          .data
      : (await httpClient.post<RegistroCatalogo>(`/api/${recurso}`, dados))
          .data,
  status: async (recurso: string, id: string, ativo: boolean) => {
    await httpClient.patch(
      `/api/${recurso}/${id}/${ativo ? 'reativar' : 'inativar'}`,
    );
  },
  bancos: async (signal?: AbortSignal) =>
    (
      await httpClient.get<BancoOpcao[]>('/api/convenios-cobranca/bancos', {
        signal,
      })
    ).data,
  conveniosSubgrupo: async (signal?: AbortSignal) =>
    (
      await httpClient.get<ConvenioOpcao[]>(
        '/api/apolices/convenios-cobranca/opcoes',
        { signal },
      )
    ).data,
  coberturasPlano: async (id: string, signal?: AbortSignal) =>
    (
      await httpClient.get<CoberturaPlano[]>(`/api/planos/${id}/coberturas`, {
        signal,
      })
    ).data,
  vincular: async (plano: string, cobertura: string, ativo: boolean) => {
    await httpClient.patch(
      `/api/planos/${plano}/coberturas/${cobertura}/${ativo ? 'reativar' : 'inativar'}`,
    );
  },
  salvarPremiosPlano: async (
    plano: string,
    cobertura: string,
    premios: PremiosCobertura,
    ativo: boolean,
  ) => {
    await httpClient.put(`/api/planos/${plano}/coberturas/${cobertura}`, {
      ...premios,
      ativo,
    });
  },
};
