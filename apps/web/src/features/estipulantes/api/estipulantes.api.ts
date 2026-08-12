/**
 * estipulantes.api.ts
 *
 * Comunicação direta com o backend para o módulo de Estipulantes.
 */
import { httpClient } from '../../../services/http/httpClient';
import type { 
  EstipulanteListItem, EstipulantesQuery, PagedResult, 
  CriarEstipulanteRequest, EstipulanteResponse,
  AtualizarEstipulanteRequest, EstipulanteDetalheResponse,
  EstipulanteConfiguracaoRequest, EstipulanteConfiguracaoResponse 
} from '../types/estipulante.types';

/**
 * Cadastra um novo estipulante.
 */
export async function cadastrarEstipulante(data: CriarEstipulanteRequest): Promise<EstipulanteResponse> {
  const response = await httpClient.post<EstipulanteResponse>('/api/estipulantes', data);
  return response.data;
}

/**
 * Obtém os detalhes de um estipulante.
 */
export async function obterEstipulante(publicId: string): Promise<EstipulanteDetalheResponse> {
  const response = await httpClient.get<EstipulanteDetalheResponse>(`/api/estipulantes/${publicId}`);
  return response.data;
}

/**
 * Obtém a configuração de um estipulante.
 */
export async function obterConfiguracao(publicId: string): Promise<EstipulanteConfiguracaoResponse> {
  const response = await httpClient.get<EstipulanteConfiguracaoResponse>(`/api/estipulantes/${publicId}/configuracao`);
  return response.data;
}

/**
 * Altera os dados básicos de um estipulante.
 */
export async function alterarEstipulante(publicId: string, data: AtualizarEstipulanteRequest): Promise<void> {
  await httpClient.put(`/api/estipulantes/${publicId}`, data);
}



/**
 * Lista estipulantes paginados utilizando os filtros informados.
 */
export async function listarEstipulantes(
  query: EstipulantesQuery,
  signal?: AbortSignal
): Promise<PagedResult<EstipulanteListItem>> {
  const params = new URLSearchParams();

  if (query.page) params.append('pagina', query.page.toString());
  if (query.pageSize) params.append('tamanho_pagina', query.pageSize.toString());
  if (query.busca) params.append('busca', query.busca);
  if (query.status) params.append('status', query.status.toString());
  if (query.sortBy) params.append('ordenar_por', query.sortBy);
  if (query.direction) params.append('direcao', query.direction);

  const queryString = params.toString();
  const url = queryString ? `/api/estipulantes?${queryString}` : '/api/estipulantes';

  const response = await httpClient.get<PagedResult<EstipulanteListItem>>(url, { signal });
  return response.data;
}

/**
 * Inativa um estipulante.
 */
export async function inativarEstipulante(publicId: string): Promise<void> {
  await httpClient.post(`/api/estipulantes/${publicId}/inativar`);
}

/**
 * Reativa um estipulante.
 */
export async function reativarEstipulante(publicId: string): Promise<void> {
  await httpClient.post(`/api/estipulantes/${publicId}/reativar`);
}

/**
 * Exclui um estipulante logicamente.
 */
export async function excluirEstipulante(publicId: string): Promise<void> {
  await httpClient.delete(`/api/estipulantes/${publicId}`);
}
