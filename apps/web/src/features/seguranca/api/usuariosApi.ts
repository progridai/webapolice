/**
 * usuariosApi.ts — Chamadas HTTP para o módulo de Usuários de Segurança.
 * Baseado estritamente no contrato real do backend (Parte 8.1).
 */
import { httpClient } from '../../../services/http/httpClient';
import type {
  UsuarioListDto,
  UsuarioDetalheDto,
  UsuariosQuery,
  CriarUsuarioRequest,
  AtualizarUsuarioRequest,
  PagedResult,
} from '../types/seguranca.types';

export async function listarUsuarios(
  query: UsuariosQuery,
  signal?: AbortSignal
): Promise<PagedResult<UsuarioListDto>> {
  const params = new URLSearchParams();
  if (query.page) params.append('pagina', query.page.toString());
  if (query.pageSize) params.append('tamanho_pagina', query.pageSize.toString());
  if (query.busca) params.append('busca', query.busca);
  if (query.ativo !== '' && query.ativo !== undefined)
    params.append('ativo', String(query.ativo));

  const qs = params.toString();
  const url = qs ? `/api/seguranca/usuarios?${qs}` : '/api/seguranca/usuarios';
  const response = await httpClient.get<PagedResult<UsuarioListDto>>(url, { signal });
  return response.data;
}

export async function obterUsuarioDetalhe(publicId: string): Promise<UsuarioDetalheDto> {
  const response = await httpClient.get<UsuarioDetalheDto>(`/api/seguranca/usuarios/${publicId}`);
  return response.data;
}

export async function criarUsuario(data: CriarUsuarioRequest): Promise<{ id: string }> {
  const response = await httpClient.post<{ id: string }>('/api/seguranca/usuarios', data);
  return response.data;
}

export async function atualizarUsuario(
  publicId: string,
  data: AtualizarUsuarioRequest
): Promise<UsuarioDetalheDto> {
  const response = await httpClient.put<UsuarioDetalheDto>(
    `/api/seguranca/usuarios/${publicId}`,
    data
  );
  return response.data;
}
