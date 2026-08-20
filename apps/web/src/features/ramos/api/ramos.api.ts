import { httpClient } from '../../../services/http/httpClient';

export interface RamoDto {
  publicId: string;
  codigo: string;
  nome: string;
  descricao?: string;
  ativo: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface ListarRamosResponse {
  items: RamoDto[];
  totalCount: number;
}

export interface CriarRamoRequest {
  codigo: string;
  nome: string;
  descricao?: string;
}

export interface AlterarRamoRequest {
  nome: string;
  descricao?: string;
}

export const ramosApi = {
  listar: async (params: { pagina: number; tamanhoPagina: number; busca?: string; ativo?: boolean }) => {
    const res = await httpClient.get<ListarRamosResponse>('/api/ramos', { params });
    return res.data;
  },
  
  obter: async (publicId: string) => {
    const res = await httpClient.get<RamoDto>(`/api/ramos/${publicId}`);
    return res.data;
  },

  criar: async (data: CriarRamoRequest) => {
    const res = await httpClient.post<RamoDto>('/api/ramos', data);
    return res.data;
  },

  alterar: async (publicId: string, data: AlterarRamoRequest) => {
    await httpClient.put(`/api/ramos/${publicId}`, data);
  },

  inativar: async (publicId: string) => {
    await httpClient.patch(`/api/ramos/${publicId}/inativar`);
  },

  reativar: async (publicId: string) => {
    await httpClient.patch(`/api/ramos/${publicId}/reativar`);
  }
};
