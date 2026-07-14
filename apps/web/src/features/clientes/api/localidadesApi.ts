import { httpClient } from '../../../services/http/httpClient';

export interface CidadeResponse {
  id: number;
  nome: string;
}

/**
 * Busca as cidades de um estado específico
 */
export async function buscarCidadesPorUf(uf: string): Promise<CidadeResponse[]> {
  const response = await httpClient.get<CidadeResponse[]>(`/api/localidades/cidades?uf=${uf}`);
  return response.data;
}
