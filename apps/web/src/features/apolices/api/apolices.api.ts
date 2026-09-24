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
  query: import('../types/apolice.types').ApoliceVidaQuery,
  signal?: AbortSignal
): Promise<PagedResult<ApoliceVidaListItem>> {
  const params = new URLSearchParams();

  if (query.page) params.append('pagina', query.page.toString());
  if (query.pageSize) params.append('tamanhoPagina', query.pageSize.toString());
  if (query.busca) params.append('busca', query.busca);
  if (query.status) params.append('status', query.status.toString());
  if (query.subestipulantePublicId) params.append('subestipulantePublicId', query.subestipulantePublicId);
  if (query.moduloPublicId) params.append('moduloPublicId', query.moduloPublicId);
  if (query.vigenciaDataReferencia) params.append('vigenciaDataReferencia', query.vigenciaDataReferencia);

  const queryString = params.toString();
  const url = queryString ? `/api/apolices/${publicId}/vidas?${queryString}` : `/api/apolices/${publicId}/vidas`;
  
  const response = await httpClient.get<PagedResult<ApoliceVidaListItem>>(url, { signal });
  return response.data;
}

export async function obterApoliceVida(
  apolicePublicId: string,
  vidaPublicId: string,
  signal?: AbortSignal
): Promise<ApoliceVidaListItem> {
  const response = await httpClient.get<ApoliceVidaListItem>(`/api/apolices/${apolicePublicId}/vidas/${vidaPublicId}`, { signal });
  return response.data;
}

export async function criarApoliceVida(
  apolicePublicId: string,
  payload: import('../types/apolice.types').CriarApoliceVidaRequest
): Promise<{ publicId: string }> {
  const response = await httpClient.post<{ publicId: string }>(`/api/apolices/${apolicePublicId}/vidas`, payload);
  return response.data;
}

export async function atualizarApoliceVida(
  apolicePublicId: string,
  vidaPublicId: string,
  payload: import('../types/apolice.types').AlterarApoliceVidaRequest
): Promise<void> {
  await httpClient.put(`/api/apolices/${apolicePublicId}/vidas/${vidaPublicId}`, payload);
}

export async function inativarApoliceVida(
  apolicePublicId: string,
  vidaPublicId: string
): Promise<void> {
  await httpClient.patch(`/api/apolices/${apolicePublicId}/vidas/${vidaPublicId}/inativar`);
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

export interface VincularSubestipulanteApoliceRequest {
  subestipulantePublicId: string;
  dataInicio?: string;
  dataFim?: string;
}

export async function vincularSubestipulanteApolice(
  apolicePublicId: string,
  payload: VincularSubestipulanteApoliceRequest
): Promise<void> {
  await httpClient.post(`/api/apolices/${apolicePublicId}/subestipulantes`, payload);
}

export interface AtualizarSubestipulanteApoliceRequest {
  dataInicio?: string;
  dataFim?: string;
}

export async function atualizarSubestipulanteApolice(
  apolicePublicId: string,
  subestipulantePublicId: string,
  payload: AtualizarSubestipulanteApoliceRequest
): Promise<void> {
  await httpClient.put(`/api/apolices/${apolicePublicId}/subestipulantes/${subestipulantePublicId}`, payload);
}

export async function inativarSubestipulanteApolice(
  apolicePublicId: string,
  subestipulantePublicId: string
): Promise<void> {
  await httpClient.patch(`/api/apolices/${apolicePublicId}/subestipulantes/${subestipulantePublicId}/inativar`);
}

// ── Subgrupos da Apólice ──────────────────────────────────────────────────

export async function listarApoliceSubgrupos(
  publicId: string,
  signal?: AbortSignal
): Promise<import('../types/apolice.types').ApoliceSubgrupoResult[]> {
  const response = await httpClient.get<import('../types/apolice.types').ApoliceSubgrupoResult[]>(
    `/api/apolices/${publicId}/subgrupos`,
    { signal }
  );
  return response.data;
}

export async function obterApoliceSubgrupo(
  apolicePublicId: string,
  subgrupoPublicId: string,
  signal?: AbortSignal
): Promise<import('../types/apolice.types').ApoliceSubgrupoResult> {
  const response = await httpClient.get<import('../types/apolice.types').ApoliceSubgrupoResult>(
    `/api/apolices/${apolicePublicId}/subgrupos/${subgrupoPublicId}`,
    { signal }
  );
  return response.data;
}

export async function criarApoliceSubgrupo(
  apolicePublicId: string,
  payload: import('../types/apolice.types').CriarSubgrupoApoliceRequest
): Promise<{ subgrupoPublicId: string }> {
  const response = await httpClient.post<{ subgrupoPublicId: string }>(
    `/api/apolices/${apolicePublicId}/subgrupos`,
    payload
  );
  return response.data;
}

export async function alterarApoliceSubgrupo(
  apolicePublicId: string,
  subgrupoPublicId: string,
  payload: import('../types/apolice.types').AlterarSubgrupoApoliceRequest
): Promise<void> {
  await httpClient.put(
    `/api/apolices/${apolicePublicId}/subgrupos/${subgrupoPublicId}`,
    payload
  );
}

export async function inativarApoliceSubgrupo(
  apolicePublicId: string,
  subgrupoPublicId: string
): Promise<void> {
  await httpClient.patch(
    `/api/apolices/${apolicePublicId}/subgrupos/${subgrupoPublicId}/inativar`
  );
}

// ── Módulos da Apólice (nova relação direta Apólice → Módulo) ────────────────

export async function listarApoliceModulos(
  apolicePublicId: string,
  signal?: AbortSignal
): Promise<import('../types/apolice.types').ApoliceModuloResult[]> {
  const response = await httpClient.get<import('../types/apolice.types').ApoliceModuloResult[]>(
    `/api/apolices/${apolicePublicId}/modulos`,
    { signal }
  );
  return response.data;
}

export async function criarApoliceModulo(
  apolicePublicId: string,
  payload: import('../types/apolice.types').CriarModuloApoliceRequest
): Promise<{ publicId: string }> {
  const response = await httpClient.post<{ publicId: string }>(
    `/api/apolices/${apolicePublicId}/modulos`,
    payload
  );
  return response.data;
}

// PUT usa apoliceModuloPublicId (publicId do vínculo seguro.apolice_modulo)
export async function alterarApoliceModulo(
  apolicePublicId: string,
  apoliceModuloPublicId: string,
  payload: import('../types/apolice.types').AlterarModuloApoliceRequest
): Promise<void> {
  await httpClient.put(
    `/api/apolices/${apolicePublicId}/modulos/${apoliceModuloPublicId}`,
    payload
  );
}

// PATCH usa apoliceModuloPublicId (publicId do vínculo seguro.apolice_modulo)
export async function inativarApoliceModulo(
  apolicePublicId: string,
  apoliceModuloPublicId: string
): Promise<void> {
  await httpClient.patch(
    `/api/apolices/${apolicePublicId}/modulos/${apoliceModuloPublicId}/inativar`
  );
}
