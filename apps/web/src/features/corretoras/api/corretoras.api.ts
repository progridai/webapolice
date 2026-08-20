/**
 * corretoras.api.ts
 *
 * Cliente de API para o domínio de Corretoras.
 */
import { httpClient } from '../../../services/http/httpClient';
import type {
  CorretoraListItem,
  CorretoraDetalhe,
  ListarCorretorasQuery,
  CriarCorretoraRequest,
  AlterarCorretoraRequest,
  PagedResult,
} from './types/corretora.types';

const BASE_URL = '/api/corretoras';

export const corretorasApi = {
  /**
   * Lista corretoras de forma paginada.
   */
  listar: async (query?: ListarCorretorasQuery): Promise<PagedResult<CorretoraListItem>> => {
    const res = await httpClient.get<PagedResult<CorretoraListItem>>(BASE_URL, {
      params: query,
    });
    return res.data;
  },

  /**
   * Obtém detalhes de uma corretora específica.
   */
  obterPorId: async (publicId: string): Promise<CorretoraDetalhe> => {
    const res = await httpClient.get<CorretoraDetalhe>(`${BASE_URL}/${publicId}`);
    return res.data;
  },

  /**
   * Cria uma nova corretora.
   */
  criar: async (data: CriarCorretoraRequest): Promise<{ publicId: string }> => {
    const res = await httpClient.post<{ publicId: string }>(BASE_URL, data);
    return res.data;
  },

  /**
   * Altera uma corretora existente.
   */
  alterar: async (publicId: string, data: Omit<AlterarCorretoraRequest, 'publicId'>): Promise<void> => {
    await httpClient.put<void>(`${BASE_URL}/${publicId}`, {
      publicId,
      ...data,
    });
  },

  /**
   * Inativa uma corretora.
   */
  inativar: async (publicId: string): Promise<void> => {
    await httpClient.patch<void>(`${BASE_URL}/${publicId}/inativar`);
  },

  /**
   * Reativa uma corretora.
   */
  reativar: async (publicId: string): Promise<void> => {
    await httpClient.patch<void>(`${BASE_URL}/${publicId}/reativar`);
  },
};
