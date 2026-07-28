/**
 * auditoriaApi.ts — Chamadas HTTP para os endpoints de auditoria.
 */
import { httpClient } from '../../../services/http/httpClient';
import type { AuditoriaDetalheDto, AuditoriaListDto, AuditoriaQuery, PagedResult } from '../types/seguranca.types';

export async function listarAuditoria(query: AuditoriaQuery = {}): Promise<PagedResult<AuditoriaListDto>> {
  const params: Record<string, string> = {};
  if (query.page) params['pagina'] = String(query.page);
  if (query.pageSize) params['tamanhoPagina'] = String(query.pageSize);

  const response = await httpClient.get<PagedResult<AuditoriaListDto>>('/api/seguranca/auditoria', { params });
  return response.data;
}

export async function obterAuditoria(publicId: string): Promise<AuditoriaDetalheDto> {
  const response = await httpClient.get<AuditoriaDetalheDto>(`/api/seguranca/auditoria/${publicId}`);
  return response.data;
}
