import { useMemo } from 'react';
import { listarApoliceSubestipulantes } from '../api/apolices.api';
import { useConsultaApolice } from './useConsultaApolice';

export function useApoliceSubestipulantes(publicId: string | undefined) {
  const load = useMemo(() => {
    if (!publicId) return null;
    return (signal: AbortSignal) =>
      listarApoliceSubestipulantes(publicId, signal);
  }, [publicId]);
  const result = useConsultaApolice(
    load,
    'Erro ao carregar subestipulantes da apólice',
  );
  return { ...result, data: result.data ?? [] };
}
