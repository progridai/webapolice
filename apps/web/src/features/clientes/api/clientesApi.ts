/**
 * clientesApi.ts
 *
 * Comunicação direta com o backend para o módulo de Clientes.
 */
import { httpClient } from '../../../services/http/httpClient';
import type { ClienteListItem, ClientesQuery, PagedResult } from '../types/cliente.types';

/**
 * Lista clientes paginados utilizando os filtros informados.
 */
export async function listarClientes(
  query: ClientesQuery,
  signal?: AbortSignal
): Promise<PagedResult<ClienteListItem>> {
  // Converte a query do frontend para a query string da API
  const params = new URLSearchParams();

  if (query.page) params.append('pagina', query.page.toString());
  if (query.pageSize) params.append('tamanho_pagina', query.pageSize.toString());
  if (query.nome) params.append('nome', query.nome);
  if (query.cpf) params.append('cpf', query.cpf);
  if (query.status) params.append('status', query.status.toString());
  if (query.sortBy) params.append('ordenar_por', query.sortBy);
  if (query.direction) params.append('direcao', query.direction);

  const queryString = params.toString();
  const url = queryString ? `/api/clientes?${queryString}` : '/api/clientes';

  const response = await httpClient.get<PagedResult<ClienteListItem>>(url, { signal });
  return response.data;
}
