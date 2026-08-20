import { httpClient } from '../../../services/http/httpClient';
import type { 
  ApoliceListItem, ApolicesQuery, PagedResult, 
  ApoliceDetalheResponse, ApoliceVidaListItem
} from '../types/apolice.types';

export async function listarApolices(
  query: ApolicesQuery,
  signal?: AbortSignal
): Promise<PagedResult<ApoliceListItem>> {
  const params = new URLSearchParams();

  if (query.page) params.append('pagina', query.page.toString());
  if (query.pageSize) params.append('tamanhoPagina', query.pageSize.toString());
  if (query.busca) params.append('busca', query.busca);
  if (query.status) params.append('status', query.status.toString());
  if (query.ativo !== undefined) params.append('ativo', query.ativo.toString());
  if (query.estipulanteId) params.append('estipulanteId', query.estipulanteId);
  if (query.seguradoraId) params.append('seguradoraId', query.seguradoraId);
  if (query.tipoRamo) params.append('tipoRamo', query.tipoRamo);

  const queryString = params.toString();
  const url = queryString ? `/api/apolices?${queryString}` : '/api/apolices';

  const response = await httpClient.get<PagedResult<ApoliceListItem>>(url, { signal });
  return response.data;
}

export async function obterApolice(publicId: string): Promise<ApoliceDetalheResponse> {
  const response = await httpClient.get<ApoliceDetalheResponse>(`/api/apolices/${publicId}`);
  return response.data;
}

export async function criarApolice(data: import('../schemas/apoliceForm.schema').ApoliceFormValues): Promise<{ publicId: string }> {
  const payload = {
    ...data,
    estipulanteId: Number(data.estipulanteId),
    seguradoraId: Number(data.seguradoraId),
    corretoraId: data.corretoraId ? Number(data.corretoraId) : null,
    dataFimVigencia: data.dataFimVigencia || null,
    dataAniversario: data.dataAniversario || null,
    ramos: data.ramos?.map(r => ({
      tipoRamo: r.tipoRamo,
      numeroApolice: r.numeroApolice || null,
      iofPercentual: r.iofPercentual ?? null,
    }))
  };
  const response = await httpClient.post<{ publicId: string }>('/api/apolices', payload);
  return response.data;
}

export async function alterarApolice(publicId: string, data: import('../schemas/apoliceForm.schema').ApoliceFormValues): Promise<void> {
  const payload = {
    ...data,
    estipulanteId: Number(data.estipulanteId),
    seguradoraId: Number(data.seguradoraId),
    corretoraId: data.corretoraId ? Number(data.corretoraId) : null,
    dataFimVigencia: data.dataFimVigencia || null,
    dataAniversario: data.dataAniversario || null,
    ramos: data.ramos?.map(r => ({
      tipoRamo: r.tipoRamo,
      numeroApolice: r.numeroApolice || null,
      iofPercentual: r.iofPercentual ?? null,
    }))
  };
  await httpClient.put(`/api/apolices/${publicId}`, payload);
}

export async function listarApoliceVidas(
  publicId: string,
  page: number = 1,
  pageSize: number = 20,
  signal?: AbortSignal
): Promise<PagedResult<ApoliceVidaListItem>> {
  const params = new URLSearchParams({
    pagina: page.toString(),
    tamanhoPagina: pageSize.toString()
  });
  
  const response = await httpClient.get<PagedResult<ApoliceVidaListItem>>(`/api/apolices/${publicId}/vidas?${params.toString()}`, { signal });
  return response.data;
}

export async function listarApoliceSubestipulantes(
  publicId: string,
  signal?: AbortSignal
): Promise<import('../types/apolice.types').ApoliceSubestipulanteResult[]> {
  const response = await httpClient.get<import('../types/apolice.types').ApoliceSubestipulanteResult[]>(
    `/api/apolices/${publicId}/subestipulantes`,
    { signal }
  );
  return response.data;
}

export async function obterApoliceUniversoPermitido(
  publicId: string,
  signal?: AbortSignal
): Promise<import('../types/apolice.types').ApoliceUniversoPermitidoResult> {
  const response = await httpClient.get<import('../types/apolice.types').ApoliceUniversoPermitidoResult>(
    `/api/apolices/${publicId}/universo-permitido`,
    { signal }
  );
  return response.data;
}

export async function listarApoliceHistorico(
  publicId: string,
  page: number = 1,
  pageSize: number = 20,
  signal?: AbortSignal
): Promise<PagedResult<import('../types/apolice.types').ApoliceHistoricoResult>> {
  const params = new URLSearchParams({
    pagina: page.toString(),
    tamanhoPagina: pageSize.toString()
  });
  
  const response = await httpClient.get<PagedResult<import('../types/apolice.types').ApoliceHistoricoResult>>(
    `/api/apolices/${publicId}/historico?${params.toString()}`, 
    { signal }
  );
  return response.data;
}

export interface VincularRamoApoliceRequest {
  ramoPublicId: string;
  numeroApolice?: string;
  iofPercentual?: number;
}

export async function vincularRamoApolice(
  apolicePublicId: string, 
  payload: VincularRamoApoliceRequest
): Promise<void> {
  await httpClient.post(`/api/apolices/${apolicePublicId}/ramos`, payload);
}

export interface AtualizarRamoApoliceRequest {
  numeroApolice?: string;
  iofPercentual?: number;
}

export async function atualizarRamoApolice(
  apolicePublicId: string, 
  ramoPublicId: string, 
  payload: AtualizarRamoApoliceRequest
): Promise<void> {
  await httpClient.put(`/api/apolices/${apolicePublicId}/ramos/${ramoPublicId}`, payload);
}

export async function inativarRamoApolice(
  apolicePublicId: string, 
  ramoPublicId: string
): Promise<void> {
  await httpClient.patch(`/api/apolices/${apolicePublicId}/ramos/${ramoPublicId}/inativar`);
}
