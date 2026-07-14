import { httpClient } from '../../../services/http/httpClient';
import type { ClienteDetalheResponse } from '../types/clienteDetalhe.types';

/**
 * Obtém os detalhes completos de um cliente pelo ID.
 */
export async function obterClienteDetalhe(id: string, signal?: AbortSignal): Promise<ClienteDetalheResponse> {
  const response = await httpClient.get<ClienteDetalheResponse>(`/api/clientes/${id}`, { signal });
  return response.data;
}
