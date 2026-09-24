import { httpClient } from '../../../services/http/httpClient';
import type { ModuloDto, ModuloListDto, CriarModuloDto, AtualizarModuloDto } from '../types/modulo.types';

export interface ListarModulosParams {
  pagina: number;
  tamanhoPagina: number;
  busca?: string;
  ativo?: boolean;
}

export interface ListarModulosResponse {
  items: ModuloListDto[];
  totalCount: number;
}

export const modulosApi = {
  listar: async (params: ListarModulosParams): Promise<ListarModulosResponse> => {
    const { data } = await httpClient.get<ListarModulosResponse>('/api/modulos', { params });
    return data;
  },

  obter: async (publicId: string): Promise<ModuloDto> => {
    const { data } = await httpClient.get<ModuloDto>(`/api/modulos/${publicId}`);
    return data;
  },

  criar: async (dto: CriarModuloDto): Promise<ModuloDto> => {
    const { data } = await httpClient.post<ModuloDto>('/api/modulos', dto);
    return data;
  },

  alterar: async (publicId: string, dto: AtualizarModuloDto): Promise<ModuloDto> => {
    const { data } = await httpClient.put<ModuloDto>(`/api/modulos/${publicId}`, dto);
    return data;
  },

  inativar: async (publicId: string): Promise<void> => {
    await httpClient.patch(`/api/modulos/${publicId}/inativar`);
  },
};
