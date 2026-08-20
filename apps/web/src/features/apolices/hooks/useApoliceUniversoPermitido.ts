import { useState, useEffect } from 'react';
import { obterApoliceUniversoPermitido } from '../api/apolices.api';
import type { ApoliceUniversoPermitidoResult } from '../types/apolice.types';

export function useApoliceUniversoPermitido(publicId: string | undefined) {
  const [data, setData] = useState<ApoliceUniversoPermitidoResult | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<Error | null>(null);

  useEffect(() => {
    if (!publicId) return;

    const controller = new AbortController();

    async function fetchUniversoPermitido() {
      try {
        setIsLoading(true);
        setError(null);
        const result = await obterApoliceUniversoPermitido(publicId!, controller.signal);
        // eslint-disable-next-line react-hooks/set-state-in-effect
        setData(result);
      } catch (err: any) {
        if (err.name === 'AbortError') return;
        // eslint-disable-next-line react-hooks/set-state-in-effect
        setError(err);
      } finally {
        // eslint-disable-next-line react-hooks/set-state-in-effect
        setIsLoading(false);
      }
    }

    fetchUniversoPermitido();

    return () => controller.abort();
  }, [publicId]);

  return { data, isLoading, error };
}
