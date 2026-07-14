import { useState, useEffect, useCallback } from 'react';
import { obterClienteDetalhe } from '../api/obterClienteDetalhe';
import type { ClienteDetalheResponse } from '../types/clienteDetalhe.types';
import { HttpApiError } from '../../../services/http/httpError';

interface UseClienteDetalheResult {
  data: ClienteDetalheResponse | null;
  isLoading: boolean;
  error: Error | HttpApiError | null;
  retry: () => void;
}

export function useClienteDetalhe(id: string | undefined): UseClienteDetalheResult {
  const [data, setData] = useState<ClienteDetalheResponse | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(!!id);
  const [error, setError] = useState<Error | HttpApiError | null>(null);
  const [retryCount, setRetryCount] = useState<number>(0);

  const retry = useCallback(() => {
    setRetryCount((prev) => prev + 1);
  }, []);

  useEffect(() => {
    if (!id) {
      return;
    }

    const abortController = new AbortController();
    let isMounted = true;

    const fetchData = async () => {
      setIsLoading(true);
      setError(null);
      
      try {
        const result = await obterClienteDetalhe(id, abortController.signal);
        if (isMounted) {
          setData(result);
          setIsLoading(false);
        }
      } catch (err) {
        if (isMounted && err instanceof Error && err.name !== 'AbortError') {
          setError(err);
          setIsLoading(false);
        }
      }
    };

    fetchData();

    return () => {
      isMounted = false;
      abortController.abort();
    };
  }, [id, retryCount]);

  return { data, isLoading, error, retry };
}
