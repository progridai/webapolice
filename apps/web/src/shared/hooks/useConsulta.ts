import { useCallback, useEffect, useMemo, useRef, useState } from 'react';

type Loader<T> = ((signal: AbortSignal) => Promise<T>) | null;

// Compartilha o ciclo de requisição de consultas, com cancelamento por contexto.
// O carregamento inicial é derivado da consulta; o efeito só grava resultados de I/O.
export function useConsulta<T>(
  load: Loader<T>,
  errorMessage: string,
  refreshTrigger = 0,
) {
  const request = useMemo(
    () => ({ load, errorMessage, refreshTrigger }),
    [load, errorMessage, refreshTrigger],
  );
  const [state, setState] = useState<{
    request: typeof request;
    data: T | null;
    error: Error | null;
    isLoading: boolean;
  } | null>(null);
  const activeRequest = useRef<{
    request: typeof request;
    controller: AbortController;
  } | null>(null);

  const execute = useCallback(
    async (signal: AbortSignal) => {
      if (!request.load) return;
      try {
        const data = await request.load(signal);
        if (!signal.aborted)
          setState({ request, data, error: null, isLoading: false });
      } catch (error: unknown) {
        if (signal.aborted) return;
        const canceled =
          (error instanceof Error || error instanceof DOMException) &&
          (error.name === 'AbortError' || error.name === 'CanceledError');
        setState((previous) => ({
          request,
          data: previous?.request.load === request.load ? previous.data : null,
          error: canceled
            ? null
            : error instanceof Error
              ? error
              : new Error(request.errorMessage),
          isLoading: false,
        }));
      }
    },
    [request],
  );

  useEffect(() => {
    if (!request.load) return;
    const controller = new AbortController();
    activeRequest.current = { request, controller };
    void execute(controller.signal);
    return () => {
      controller.abort();
      // Inclui uma eventual nova tentativa iniciada depois da consulta inicial.
      if (activeRequest.current?.request === request) {
        activeRequest.current.controller.abort();
        activeRequest.current = null;
      }
    };
  }, [request, execute]);

  const refetch = useCallback(async () => {
    const active = activeRequest.current;
    // Callbacks guardados de outra consulta ou de uma tela desmontada não iniciam I/O.
    if (!request.load || active?.request !== request) return;
    active.controller.abort();
    const controller = new AbortController();
    activeRequest.current = { request, controller };
    setState((previous) => ({
      request,
      data: previous?.request.load === request.load ? previous.data : null,
      error: null,
      isLoading: true,
    }));
    await execute(controller.signal);
  }, [request, execute]);

  const current = state?.request === request ? state : null;
  return {
    data: state?.request.load === request.load ? state.data : null,
    error: current?.error ?? null,
    isLoading: request.load !== null && (current?.isLoading ?? true),
    refetch,
  };
}
