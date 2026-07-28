/**
 * useAuditoria.ts — Hooks para carregar auditoria (listagem e detalhe).
 */
import { useState, useEffect, useCallback } from 'react';
import { listarAuditoria, obterAuditoria } from '../api/auditoriaApi';
import type { AuditoriaDetalheDto, AuditoriaListDto, AuditoriaQuery, PagedResult } from '../types/seguranca.types';

// ── Listagem ───────────────────────────────────────────────────────────────────

interface UseAuditoriaListState {
  data: PagedResult<AuditoriaListDto> | null;
  isLoading: boolean;
  error: Error | null;
}

export function useAuditoriaList(query: AuditoriaQuery) {
  const [state, setState] = useState<UseAuditoriaListState>({
    data: null,
    isLoading: true,
    error: null,
  });

  const fetch = async () => {
    setState((prev) => ({ ...prev, isLoading: true, error: null }));
    try {
      const data = await listarAuditoria(query);
      setState({ data, isLoading: false, error: null });
    } catch {
      setState((prev) => ({
        ...prev,
        isLoading: false,
        error: new Error('Não foi possível carregar os registros de auditoria.'),
      }));
    }
  };

  useEffect(() => {
    fetch();
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [query.page, query.pageSize, query.acao, query.entidade, query.dataInicial, query.dataFinal]);

  return { ...state, retry: fetch };
}

// ── Detalhe ────────────────────────────────────────────────────────────────────

interface UseAuditoriaDetalheState {
  data: AuditoriaDetalheDto | null;
  isLoading: boolean;
  error: Error | null;
}

export function useAuditoriaDetalhe(publicId: string) {
  const [state, setState] = useState<UseAuditoriaDetalheState>({
    data: null,
    isLoading: true,
    error: null,
  });

  const fetch = useCallback(async () => {
    setState((prev) => ({ ...prev, isLoading: true, error: null }));
    try {
      const data = await obterAuditoria(publicId);
      setState({ data, isLoading: false, error: null });
    } catch {
      setState((prev) => ({
        ...prev,
        isLoading: false,
        error: new Error('Não foi possível carregar o detalhe do registro de auditoria.'),
      }));
    }
  }, [publicId]);

  useEffect(() => {
    // eslint-disable-next-line react-hooks/set-state-in-effect
    fetch();
  }, [fetch]);

  return { ...state, retry: fetch };
}
