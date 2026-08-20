/**
 * seguradoras.api.ts
 *
 * Cliente de API para comunicação com os endpoints de Seguradoras.
 */
import { httpClient } from '../../../services/http/httpClient';
import type {
  SeguradoraListItem,
  SeguradoraDetalhe,
  ListarSeguradorasQuery,
  PagedResult,
  CriarSeguradoraRequest,
  AlterarSeguradoraRequest,
} from '../types/seguradora.types';

export const seguradorasApi = {
  /**
   * Lista seguradoras de forma paginada com filtros opcionais.
   */
  listar: async (query?: ListarSeguradorasQuery, signal?: AbortSignal): Promise<PagedResult<SeguradoraListItem>> => {
    const params: Record<string, string | number | boolean> = {};
    if (query?.pagina) params['pagina'] = query.pagina;
    if (query?.tamanhoPagina) params['tamanhoPagina'] = query.tamanhoPagina;
    if (query?.busca) params['busca'] = query.busca;
    if (query?.ativo !== undefined) params['ativo'] = query.ativo;

    const res = await httpClient.get<PagedResult<SeguradoraListItem>>('/api/seguradoras', {
      params,
      signal,
    });
    return res.data;
  },

  /**
   * Obtém detalhes de uma seguradora pelo seu identificador público (UUID).
   */
  obter: async (publicId: string, signal?: AbortSignal): Promise<SeguradoraDetalhe> => {
    const res = await httpClient.get<SeguradoraDetalhe>(`/api/seguradoras/${publicId}`, {
      signal,
    });
    return res.data;
  },

  /**
   * Cria uma nova seguradora.
   */
  criar: async (data: CriarSeguradoraRequest): Promise<{ publicId: string }> => {
    const res = await httpClient.post<{ publicId: string }>('/api/seguradoras', data);
    return res.data;
  },

  /**
   * Altera uma seguradora existente.
   */
  alterar: async (publicId: string, data: AlterarSeguradoraRequest): Promise<void> => {
    await httpClient.put(`/api/seguradoras/${publicId}`, data);
  },

  /**
   * Inativa uma seguradora logicamente.
   */
  inativar: async (publicId: string): Promise<void> => {
    await httpClient.post(`/api/seguradoras/${publicId}/inativar`);
  },

  /**
   * Reativa uma seguradora inativa.
   */
  reativar: async (publicId: string): Promise<void> => {
    await httpClient.post(`/api/seguradoras/${publicId}/reativar`);
  },
};
