import { useMemo } from 'react';
import { obterApoliceUniversoPermitido } from '../api/apolices.api';
import { useConsultaApolice } from './useConsultaApolice';

export function useApoliceUniversoPermitido(publicId: string | undefined) {
  const load = useMemo(() => {
    if (!publicId) return null;
    return (signal: AbortSignal) =>
      obterApoliceUniversoPermitido(publicId, signal);
  }, [publicId]);
  return useConsultaApolice(
    load,
    'Não foi possível carregar o Universo Permitido.',
  );
}
