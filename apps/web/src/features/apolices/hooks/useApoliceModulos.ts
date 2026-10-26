import { useMemo } from 'react';
import { listarApoliceModulos } from '../api/apolices.api';
import { useConsultaApolice } from './useConsultaApolice';

export function useApoliceModulos(publicId: string | undefined) {
  const load = useMemo(() => {
    if (!publicId) return null;
    return (signal: AbortSignal) => listarApoliceModulos(publicId, signal);
  }, [publicId]);
  const result = useConsultaApolice(
    load,
    'Erro ao carregar módulos da apólice',
  );
  return { ...result, data: result.data ?? [] };
}
