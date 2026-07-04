/**
 * PageLoading.tsx
 *
 * Indicador de carregamento para transições de rota (lazy loading).
 * Mais leve que AppLoading — usado dentro de layouts existentes.
 */
import React from 'react';
import { Spinner } from '../ui/Spinner';
import './PageLoading.css';

export const PageLoading: React.FC = () => {
  return (
    <div
      className="page-loading"
      role="status"
      aria-live="polite"
      aria-label="Carregando página"
    >
      <Spinner size="medium" aria-label="Carregando..." />
    </div>
  );
};
