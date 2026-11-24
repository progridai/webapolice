import { httpClient } from '../../../services/http';
import type { PagedResult } from '../../../shared/types/pagination';
import type { ParcelaHistorico } from '../types/parcelaHistorico.types';

export async function listarParcelas(
  id: string,
  pagina: number,
  signal: AbortSignal,
) {
  const params = new URLSearchParams({
    pagina: String(pagina),
    tamanhoPagina: '20',
  });
  return (
    await httpClient.get<PagedResult<ParcelaHistorico>>(
      `/api/propostas/${id}/parcelas?${params}`,
      { signal },
    )
  ).data;
}
