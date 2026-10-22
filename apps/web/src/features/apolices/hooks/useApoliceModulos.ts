import { useState, useEffect, useCallback } from 'react';
import { listarApoliceModulos } from '../api/apolices.api';
import type { ApoliceModuloResult } from '../types/apolice.types';

export function useApoliceModulos(apolicePublicId: string | undefined) {
  const [data, setData] = useState<ApoliceModuloResult[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<Error | null>(null);

  const fetchModulos = useCallback(async () => {
    if (!apolicePublicId) return;
    try {
      setIsLoading(true);
      setError(null);
      const result = await listarApoliceModulos(apolicePublicId);
      setData(result);
    } catch (err: unknown) {
      if (
        (err instanceof Error || err instanceof DOMException) &&
        err.name === 'AbortError'
      )
        return;
      setError(
        err instanceof Error
          ? err
          : new Error('Erro ao carregar módulos da apólice'),
      );
    } finally {
      setIsLoading(false);
    }
  }, [apolicePublicId]);

  useEffect(() => {
    fetchModulos();
  }, [fetchModulos]);

  return { data, isLoading, error, refetch: fetchModulos };
}
