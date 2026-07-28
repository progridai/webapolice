/**
 * perfisApi.ts — Chamadas HTTP para o módulo de Perfis de Segurança.
 * Baseado estritamente no contrato real do backend (Parte 8.1).
 */
import { httpClient } from '../../../services/http/httpClient';
import type {
  PerfilDto,
  PerfilDetalheDto,
  PerfisQuery,
  CriarPerfilRequest,
  AtualizarPerfilRequest,
  PagedResult,
} from '../types/seguranca.types';

export async function listarPerfis(
  query: PerfisQuery,
  signal?: AbortSignal
): Promise<PagedResult<PerfilDto>> {
  const params = new URLSearchParams();
  if (query.page) params.append('pagina', query.page.toString());
  if (query.pageSize) params.append('tamanho_pagina', query.pageSize.toString());
  if (query.busca) params.append('busca', query.busca);
  if (query.ativo !== '' && query.ativo !== undefined)
    params.append('ativo', String(query.ativo));

  const qs = params.toString();
  const url = qs ? `/api/seguranca/perfis?${qs}` : '/api/seguranca/perfis';
  const response = await httpClient.get<PagedResult<PerfilDto>>(url, { signal });
  return response.data;
}

export async function obterPerfilDetalhe(publicId: string): Promise<PerfilDetalheDto> {
  const response = await httpClient.get<PerfilDetalheDto>(`/api/seguranca/perfis/${publicId}`);
  return response.data;
}

export async function criarPerfil(data: CriarPerfilRequest): Promise<{ id: string }> {
  const response = await httpClient.post<{ id: string }>('/api/seguranca/perfis', data);
  return response.data;
}

export async function atualizarPerfil(
  publicId: string,
  data: AtualizarPerfilRequest
): Promise<PerfilDetalheDto> {
  const response = await httpClient.put<PerfilDetalheDto>(
    `/api/seguranca/perfis/${publicId}`,
    data
  );
  return response.data;
}
