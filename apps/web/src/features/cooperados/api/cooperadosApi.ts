import { httpClient } from '../../../services/http/httpClient';
import type { CooperadoListDto, CooperadosFiltersState, ListagemPaginadaResult, CooperadoDetalheDto, CooperadoFormData } from '../types/cooperados.types';

export async function listarCooperados(
  query: CooperadosFiltersState,
  signal?: AbortSignal
): Promise<ListagemPaginadaResult<CooperadoListDto>> {
  const params = new URLSearchParams();

  if (query.page) params.append('pagina', query.page.toString());
  if (query.limit) params.append('tamanho_pagina', query.limit.toString());
  if (query.nome) params.append('nome', query.nome);
  if (query.cpf) params.append('cpf', query.cpf);
  if (query.status) params.append('status', query.status.toString());
  if (query.sortBy) params.append('ordenar_por', query.sortBy);
  if (query.direction) params.append('direcao', query.direction);

  const queryString = params.toString();
  const url = queryString ? `/api/cooperados?${queryString}` : '/api/cooperados';

  const response = await httpClient.get<ListagemPaginadaResult<CooperadoListDto>>(url, { signal });
  return response.data;
}

export async function obterCooperadoDetalhe(id: string, signal?: AbortSignal): Promise<CooperadoDetalheDto> {
  const response = await httpClient.get<CooperadoDetalheDto>(`/api/cooperados/${id}`, { signal });
  return response.data;
}

export async function cadastrarCooperado(data: CooperadoFormData): Promise<{ publicId: string }> {
  const response = await httpClient.post<{ publicId: string }>('/api/cooperados', data);
  return response.data;
}

export async function alterarCooperado(id: string, data: CooperadoFormData): Promise<void> {
  await httpClient.put(`/api/cooperados/${id}`, data);
}

export async function inativarCooperado(id: string): Promise<void> {
  await httpClient.patch(`/api/cooperados/${id}/inativar`);
}

export async function ativarCooperado(id: string): Promise<void> {
  await httpClient.patch(`/api/cooperados/${id}/ativar`);
}

export async function listarCoordenadoresAtivos(): Promise<CooperadoListDto[]> {
  const response = await httpClient.get<ListagemPaginadaResult<CooperadoListDto>>('/api/cooperados?tipo=2&status=1&tamanho_pagina=100');
  return response.data.itens;
}
