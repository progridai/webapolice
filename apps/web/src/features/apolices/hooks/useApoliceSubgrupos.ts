import { useMemo } from 'react';
import { listarApoliceSubgrupos } from '../api/apolices.api';
import { useConsultaApolice } from './useConsultaApolice';

export function useApoliceSubgrupos(publicId: string | undefined) {
  const load = useMemo(() => {
    if (!publicId) return null;
    return (signal: AbortSignal) => listarApoliceSubgrupos(publicId, signal);
  }, [publicId]);
  const result = useConsultaApolice(
    load,
    'Erro ao carregar subgrupos da apólice',
  );
  return { ...result, data: result.data ?? [] };
}
