import { useState, useEffect, useCallback } from 'react';
import { obterEstipulante, obterConfiguracao } from '../api/estipulantes.api';
import type {
  EstipulanteDetalheResponse,
  EstipulanteConfiguracaoResponse,
} from '../types/estipulante.types';
import { HttpApiError } from '../../../services/http/httpError';

export interface EstipulanteDetalheCompleto {
  estipulante: EstipulanteDetalheResponse;
  configuracao: EstipulanteConfiguracaoResponse | null;
}

interface UseEstipulanteDetalheResult {
  data: EstipulanteDetalheCompleto | null;
  isLoading: boolean;
  error: Error | HttpApiError | null;
  retry: () => void;
}

export function useEstipulanteDetalhe(
  publicId: string | undefined,
): UseEstipulanteDetalheResult {
  const [data, setData] = useState<EstipulanteDetalheCompleto | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(!!publicId);
  const [error, setError] = useState<Error | HttpApiError | null>(null);
  const [retryCount, setRetryCount] = useState<number>(0);

  const retry = useCallback(() => {
    setRetryCount((prev) => prev + 1);
  }, []);

  useEffect(() => {
    if (!publicId) return;

    let isMounted = true;

    const fetchData = async () => {
      setIsLoading(true);
      setError(null);

      try {
        const [estipulanteResult, configResult] = await Promise.all([
          obterEstipulante(publicId),
          obterConfiguracao(publicId).catch((err: unknown) => {
            if (err instanceof HttpApiError && err.isNotFound()) return null;
            throw err;
          }),
        ]);

        if (isMounted) {
          setData({
            estipulante: estipulanteResult,
            configuracao: configResult,
          });
          setIsLoading(false);
        }
      } catch (err: unknown) {
        if (isMounted) {
          if (!(
            (err instanceof Error || err instanceof DOMException) &&
            err.name === 'AbortError'
          )) {
            setError(
              err instanceof Error
                ? err
                : new Error(
                    'Não foi possível carregar os dados do estipulante.',
                  ),
            );
          }
          setIsLoading(false);
        }
      }
    };

    fetchData();

    return () => {
      isMounted = false;
    };
  }, [publicId, retryCount]);

  return { data, isLoading, error, retry };
}
