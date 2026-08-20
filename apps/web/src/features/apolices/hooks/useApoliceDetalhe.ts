import { useState, useEffect, useCallback } from 'react';
import { obterApolice } from '../api/apolices.api';
import type { ApoliceDetalheResponse } from '../types/apolice.types';

export function useApoliceDetalhe(publicId: string | undefined) {
  const [data, setData] = useState<ApoliceDetalheResponse | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<Error | null>(null);

  const fetchData = useCallback(async () => {
    if (!publicId) return;
    
    setIsLoading(true);
    setError(null);
    try {
      const result = await obterApolice(publicId);
      setData(result);
    } catch (err: any) {
      setError(err instanceof Error ? err : new Error(err.message || 'Erro ao carregar detalhes da apólice'));
    } finally {
      setIsLoading(false);
    }
  }, [publicId]);

  useEffect(() => {
    // eslint-disable-next-line react-hooks/set-state-in-effect
    fetchData();
  }, [fetchData]);

  return { data, isLoading, error, retry: fetchData };
}
