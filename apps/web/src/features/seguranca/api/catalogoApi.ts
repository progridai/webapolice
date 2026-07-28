/**
 * catalogoApi.ts — Chamadas HTTP para o catálogo de módulos/recursos/permissões.
 */
import { httpClient } from '../../../services/http/httpClient';
import type { CatalogoModuloDto } from '../types/seguranca.types';

export async function obterCatalogo(): Promise<CatalogoModuloDto[]> {
  const response = await httpClient.get<CatalogoModuloDto[]>('/api/seguranca/catalogo');
  return response.data;
}
