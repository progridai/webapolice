import { useEffect, useMemo, useRef, useState } from 'react';
import { useConsulta } from '../../shared/hooks/useConsulta';
import {
  obterAtalhosHome,
  salvarAtalhosHome,
  type PreferenciasHome,
} from './atalhosHome.api';

export function useAtalhosHome(userId: string) {
  const load = useMemo(
    () => (userId ? (signal: AbortSignal) => obterAtalhosHome(signal) : null),
    [userId],
  );
  const consultation = useConsulta(
    load,
    'Não foi possível carregar seus atalhos.',
  );
  const activeSave = useRef<AbortController | null>(null);
  const [saved, setSaved] = useState<{
    load: typeof load;
    data: PreferenciasHome;
  } | null>(null);
  const [mutation, setMutation] = useState<{
    load: typeof load;
    saving: boolean;
    error: string | null;
  } | null>(null);

  useEffect(
    () => () => {
      activeSave.current?.abort();
      activeSave.current = null;
    },
    [load],
  );

  async function save(paths: string[]) {
    if (!load || activeSave.current) return false;
    const controller = new AbortController();
    activeSave.current = controller;
    setMutation({ load, saving: true, error: null });
    try {
      const data = await salvarAtalhosHome(paths, controller.signal);
      if (controller.signal.aborted) return false;
      setSaved({ load, data });
      setMutation({ load, saving: false, error: null });
      return true;
    } catch (cause: unknown) {
      if (!controller.signal.aborted) {
        setMutation({
          load,
          saving: false,
          error:
            cause instanceof Error
              ? cause.message
              : 'Não foi possível salvar seus atalhos.',
        });
      }
      return false;
    } finally {
      if (activeSave.current === controller) activeSave.current = null;
    }
  }

  return {
    ...consultation,
    data: saved?.load === load ? saved.data : consultation.data,
    saving: mutation?.load === load && mutation.saving,
    saveError: mutation?.load === load ? mutation.error : null,
    clearSaveError: () => setMutation(null),
    save,
  };
}
