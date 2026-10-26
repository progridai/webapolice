import { useMemo } from 'react';
import { listarApoliceVidas } from '../api/apolices.api';
import type { ApoliceVidaQuery } from '../types/apolice.types';
import { useConsultaApolice } from './useConsultaApolice';

export function useApoliceVidas(
  publicId: string | undefined,
  query: ApoliceVidaQuery,
) {
  const {
    page,
    pageSize,
    busca,
    status,
    apoliceSubgrupoPublicId,
    apoliceModuloPublicId,
    vigenciaDataReferencia,
  } = query;
  const load = useMemo(() => {
    if (!publicId) return null;
    return (signal: AbortSignal) =>
      listarApoliceVidas(
        publicId,
        {
          page,
          pageSize,
          busca,
          status,
          apoliceSubgrupoPublicId,
          apoliceModuloPublicId,
          vigenciaDataReferencia,
        },
        signal,
      );
  }, [
    publicId,
    page,
    pageSize,
    busca,
    status,
    apoliceSubgrupoPublicId,
    apoliceModuloPublicId,
    vigenciaDataReferencia,
  ]);
  const { refetch, ...state } = useConsultaApolice(
    load,
    'Erro ao carregar vidas da apólice',
  );
  return { ...state, retry: refetch };
}
