import { httpClient } from '../../../services/http/httpClient';

export const permissoesExclusaoVinculos = {
  ramos: 'apolices.ramos.excluir',
  subgrupos: 'apolices.subgrupos.excluir',
  subestipulantes: 'apolices.subestipulantes.excluir',
  modulos: 'apolices.modulos.excluir',
  'planos-modulo': 'apolices.modulos.excluir',
  'coberturas-modulo': 'apolices.modulos.excluir',
  'produtos-anteriores': 'apolices.universo.excluir',
  'planos-anteriores': 'apolices.universo.excluir',
  'coberturas-anteriores': 'apolices.universo.excluir',
} as const;

export type TipoVinculoApolice = keyof typeof permissoesExclusaoVinculos;

export async function excluirVinculoApolice(
  apolicePublicId: string,
  tipo: TipoVinculoApolice,
  vinculoPublicId: string,
): Promise<void> {
  await httpClient.delete(
    `/api/apolices/${apolicePublicId}/vinculos/${tipo}/${vinculoPublicId}`,
  );
}
