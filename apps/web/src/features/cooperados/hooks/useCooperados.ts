import { useCallback } from 'react';
import { listarCooperados } from '../api/cooperadosApi';
import type { CooperadosFiltersState } from '../types/cooperados.types';
import { HttpApiError } from '../../../services/http';
import { useConsulta } from '../../../shared/hooks/useConsulta';

export function useCooperados(query: CooperadosFiltersState) {
  const { page, limit, nome, cpf, status, sortBy, direction } = query;
  const load = useCallback(
    async (signal: AbortSignal) => {
      try {
        return await listarCooperados(
          { page, limit, nome, cpf, status, sortBy, direction },
          signal,
        );
      } catch (error: unknown) {
        // Preserva a mensagem e a conversão de erros HTTP já usadas pela listagem.
        if (error instanceof HttpApiError) {
          throw new Error(error.message || 'Erro ao comunicar com a API', {
            cause: error,
          });
        }
        throw error;
      }
    },
    [page, limit, nome, cpf, status, sortBy, direction],
  );
  const { refetch, ...state } = useConsulta(load, 'Erro desconhecido');
  return { ...state, retry: refetch };
}
