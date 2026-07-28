/**
 * useModulos.ts — Hook para gerenciar módulos do sistema.
 */
import { useState, useCallback, useEffect } from 'react';
import { listarModulos, alterarHabilitacaoModulo } from '../api/modulosApi';
import type { ModuloDto } from '../types/seguranca.types';

export function useModulos() {
  const [modulos, setModulos] = useState<ModuloDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [toggleError, setToggleError] = useState<string | null>(null);

  const carregarModulos = useCallback(async () => {
    setIsLoading(true);
    setError(null);
    try {
      const data = await listarModulos();
      setModulos(data);
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Erro ao carregar módulos';
      setError(message);
      console.error('Erro ao carregar módulos', err);
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    // eslint-disable-next-line react-hooks/set-state-in-effect
    carregarModulos();
  }, [carregarModulos]);

  const handleToggleModulo = async (publicId: string, currentHabilitado: boolean) => {
    setToggleError(null);
    try {
      const novoHabilitado = !currentHabilitado;
      await alterarHabilitacaoModulo(publicId, novoHabilitado);
      setModulos(current =>
        current.map(m => m.publicId === publicId ? { ...m, habilitado: novoHabilitado } : m)
      );
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Erro ao alterar status do módulo.';
      console.error('Erro ao alterar status do módulo', err);
      setToggleError(message);
    }
  };

  return { modulos, isLoading, error, toggleError, handleToggleModulo, recarregar: carregarModulos };
}
