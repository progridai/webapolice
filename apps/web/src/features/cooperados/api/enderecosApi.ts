import { httpClient } from '../../../services/http/httpClient';

export interface EnderecoConsultaResult {
  cep: string;
  logradouro: string | null;
  bairro: string | null;
  cidade: string;
  uf: string;
  cidadeId: number | null;
  estadoId: number | null;
}

export async function consultarCep(cep: string, signal?: AbortSignal): Promise<EnderecoConsultaResult> {
  const response = await httpClient.get<EnderecoConsultaResult>(`/api/enderecos/cep/${cep}`, { signal });
  return response.data;
}
