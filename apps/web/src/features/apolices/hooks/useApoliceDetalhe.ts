import { useMemo } from 'react';
import { obterApolice } from '../api/apolices.api';
import { useConsultaApolice } from './useConsultaApolice';

export function useApoliceDetalhe(publicId: string | undefined) {
  const load = useMemo(() => {
    if (!publicId) return null;
    return (signal: AbortSignal) => obterApolice(publicId, signal);
  }, [publicId]);
  const { refetch, ...state } = useConsultaApolice(
    load,
    'Erro ao carregar detalhes da apólice',
  );
  return { ...state, retry: refetch };
}
