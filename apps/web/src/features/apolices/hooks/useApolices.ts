import { useCallback } from 'react';
import { listarApolices } from '../api/apolices.api';
import type { ApolicesQuery } from '../types/apolice.types';
import { useConsultaApolice } from './useConsultaApolice';

export function useApolices(query: ApolicesQuery, refreshTrigger = 0) {
  const {
    page,
    pageSize,
    busca,
    status,
    ativo,
    estipulanteId,
    seguradoraId,
    tipoRamo,
    vigenciaDataReferencia,
  } = query;
  const load = useCallback(
    (signal: AbortSignal) =>
      listarApolices(
        {
          page,
          pageSize,
          busca,
          status,
          ativo,
          estipulanteId,
          seguradoraId,
          tipoRamo,
          vigenciaDataReferencia,
        },
        signal,
      ),
    [
      page,
      pageSize,
      busca,
      status,
      ativo,
      estipulanteId,
      seguradoraId,
      tipoRamo,
      vigenciaDataReferencia,
    ],
  );
  const { refetch, ...state } = useConsultaApolice(
    load,
    'Erro ao carregar apólices',
    refreshTrigger,
  );
  return { ...state, retry: refetch };
}
