import { httpClient } from '../../../services/http/httpClient';

export interface ModuloGlobalListItem {
  publicId: string;
  nome: string;
  descricao?: string;
  ativo: boolean;
}

export interface ListarModulosGlobaisQuery {
  pagina?: number;
  tamanhoPagina?: number;
  busca?: string;
  ativo?: boolean;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export const modulosGlobaisApi = {
  listar: async (query?: ListarModulosGlobaisQuery, signal?: AbortSignal): Promise<PagedResult<ModuloGlobalListItem>> => {
    const params = new URLSearchParams();
    
    if (query?.pagina) params.append('pagina', query.pagina.toString());
    if (query?.tamanhoPagina) params.append('tamanhoPagina', query.tamanhoPagina.toString());
    if (query?.busca) params.append('busca', query.busca);
    if (query?.ativo !== undefined) params.append('ativo', query.ativo.toString());
    
    const queryString = params.toString();
    const url = queryString ? `/api/modulos?${queryString}` : '/api/modulos';
    
    const response = await httpClient.get<PagedResult<ModuloGlobalListItem>>(url, { signal });
    return response.data;
  }
};
