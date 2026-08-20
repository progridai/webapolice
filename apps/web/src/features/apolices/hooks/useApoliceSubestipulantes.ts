import { useState, useEffect } from 'react';
import { listarApoliceSubestipulantes } from '../api/apolices.api';
import type { ApoliceSubestipulanteResult } from '../types/apolice.types';

export function useApoliceSubestipulantes(publicId: string | undefined) {
  const [data, setData] = useState<ApoliceSubestipulanteResult[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<Error | null>(null);

  useEffect(() => {
    if (!publicId) return;

    const controller = new AbortController();

    async function fetchSubestipulantes() {
      try {
        setIsLoading(true);
        setError(null);
        const result = await listarApoliceSubestipulantes(publicId!, controller.signal);
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

    fetchSubestipulantes();

    return () => controller.abort();
  }, [publicId]);

  return { data, isLoading, error };
}
