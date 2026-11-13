import { useMemo } from 'react';
import { httpClient } from '../../../services/http/httpClient';
import { useConsulta } from '../../../shared/hooks/useConsulta';
import type { PagedResult } from '../../../shared/types/pagination';
export interface VinculoVidaOpcao {
  publicId: string;
  matricula: string;
  estipulante: string | null;
  requerEstipulante?: boolean;
}
export function useVinculosVida(
  apoliceId: string,
  clienteId: string,
  pagina: number,
) {
  const load = useMemo(
    () =>
      !clienteId
        ? null
        : async (signal: AbortSignal) =>
            (
              await httpClient.get<PagedResult<VinculoVidaOpcao>>(
                `/api/apolices/${apoliceId}/vidas/vinculos-disponiveis?clientePublicId=${encodeURIComponent(clienteId)}&pagina=${pagina}&tamanhoPagina=20`,
                { signal },
              )
            ).data,
    [apoliceId, clienteId, pagina],
  );
  return useConsulta(load, 'Erro ao carregar vínculos do cliente.');
}
