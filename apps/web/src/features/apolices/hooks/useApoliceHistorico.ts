import { useMemo } from 'react';
import { listarApoliceHistorico } from '../api/apolices.api';
import { useConsultaApolice } from './useConsultaApolice';

export function useApoliceHistorico(
  publicId: string | undefined,
  page: number = 1,
  pageSize: number = 20,
) {
  const load = useMemo(() => {
    if (!publicId) return null;
    return (signal: AbortSignal) =>
      listarApoliceHistorico(publicId, page, pageSize, signal);
  }, [publicId, page, pageSize]);
  const { data, isLoading, error } = useConsultaApolice(
    load,
    'Erro ao carregar histórico da apólice',
  );
  return { data, isLoading, error };
}
