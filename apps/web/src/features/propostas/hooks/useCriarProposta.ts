import { useRef, useState } from 'react';
import { criarPropostaDaVida } from '../api/propostas.api';

export function useCriarProposta() {
  const enviando = useRef(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  async function criar(
    apolice: string,
    vida: string,
    onCriada: (id: string) => void,
    origem?: string,
  ) {
    if (enviando.current) return;
    enviando.current = true;
    setIsSubmitting(true);
    setError(null);
    try {
      const id = await criarPropostaDaVida(apolice, vida, origem);
      onCriada(id);
    } catch (cause) {
      setError(
        cause instanceof Error
          ? cause.message
          : 'Não foi possível criar a pré proposta.',
      );
    } finally {
      enviando.current = false;
      setIsSubmitting(false);
    }
  }
  return { criar, isSubmitting, error };
}
