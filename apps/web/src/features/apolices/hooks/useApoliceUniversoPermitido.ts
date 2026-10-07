import { useState, useEffect, useCallback } from 'react';
import { obterApoliceUniversoPermitido } from '../api/apolices.api';
import type { ApoliceUniversoPermitidoResult } from '../types/apolice.types';

export function useApoliceUniversoPermitido(publicId: string | undefined) {
  const [data, setData] = useState<ApoliceUniversoPermitidoResult | null>(null);
  const [isLoading, setIsLoading] = useState(!!publicId);
  const [error, setError] = useState<Error | null>(null);
  const carregar = useCallback(
    (signal?: AbortSignal) => {
      if (!publicId) return Promise.resolve();
      return obterApoliceUniversoPermitido(publicId, signal)
        .then((result) => {
          if (!signal?.aborted) {
            setData(result);
            setError(null);
          }
        })
        .catch((err) => {
          if (!signal?.aborted)
            setError(
              err instanceof Error
                ? err
                : new Error('Não foi possível carregar o Universo Permitido.'),
            );
        })
        .finally(() => {
          if (!signal?.aborted) setIsLoading(false);
        });
    },
    [publicId],
  );
  useEffect(() => {
    const controller = new AbortController();
    void carregar(controller.signal);
    return () => controller.abort();
  }, [carregar]);
  const refetch = useCallback(() => {
    setIsLoading(true);
    setError(null);
    return carregar();
  }, [carregar]);
  return { data, isLoading, error, refetch };
}
