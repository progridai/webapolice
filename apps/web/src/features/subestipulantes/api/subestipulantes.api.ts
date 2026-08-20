/**
 * subestipulantes.api.ts
 *
 * Cliente de API para comunicação com os endpoints de Subestipulantes.
 */
import { httpClient } from '../../../services/http/httpClient';
import type {
  SubestipulanteListItem,
  SubestipulanteDetalhe,
  ListarSubestipulantesQuery,
  PagedResult,
  CriarSubestipulanteRequest,
  AlterarSubestipulanteRequest,
} from '../types/subestipulante.types';

export const subestipulantesApi = {
  /**
   * Lista subestipulantes de forma paginada com filtros opcionais.
   */
  listar: async (query?: ListarSubestipulantesQuery, signal?: AbortSignal): Promise<PagedResult<SubestipulanteListItem>> => {
    const params: Record<string, string | number | boolean> = {};
    if (query?.pagina) params['pagina'] = query.pagina;
    if (query?.tamanhoPagina) params['tamanhoPagina'] = query.tamanhoPagina;
    if (query?.busca) params['busca'] = query.busca;
    if (query?.ativo !== undefined) params['ativo'] = query.ativo;

    const res = await httpClient.get<PagedResult<SubestipulanteListItem>>('/api/subestipulantes', {
      params,
      signal,
    });
    return res.data;
  },

  /**
   * Obtém detalhes de um subestipulante pelo seu identificador público (UUID).
   */
  obter: async (publicId: string, signal?: AbortSignal): Promise<SubestipulanteDetalhe> => {
    const res = await httpClient.get<SubestipulanteDetalhe>(`/api/subestipulantes/${publicId}`, {
      signal,
    });
    return res.data;
  },

  /**
   * Cria um novo subestipulante.
   */
  criar: async (data: CriarSubestipulanteRequest): Promise<{ publicId: string }> => {
    const res = await httpClient.post<{ publicId: string }>('/api/subestipulantes', data);
    return res.data;
  },

  /**
   * Altera um subestipulante existente.
   */
  alterar: async (publicId: string, data: AlterarSubestipulanteRequest): Promise<void> => {
    await httpClient.put(`/api/subestipulantes/${publicId}`, data);
  },

  /**
   * Inativa um subestipulante logicamente.
   */
  inativar: async (publicId: string): Promise<void> => {
    await httpClient.post(`/api/subestipulantes/${publicId}/inativar`);
  },

  /**
   * Reativa um subestipulante inativo.
   */
  reativar: async (publicId: string): Promise<void> => {
    await httpClient.post(`/api/subestipulantes/${publicId}/reativar`);
  },
};
