/**
 * AppLoading.tsx
 *
 * Tela de carregamento inicial da aplicação.
 * Exibida enquanto o Keycloak está sendo inicializado.
 * Acessível: usa aria-live e aria-busy.
 */
import React from 'react';
import { Spinner } from '../ui/Spinner';
import './AppLoading.css';

export const AppLoading: React.FC = () => {
  return (
    <div
      className="app-loading"
      role="status"
      aria-live="polite"
      aria-label="Carregando a aplicação"
    >
      <div className="app-loading-inner">
        <div className="app-loading-logo" aria-hidden="true">
          <span className="app-loading-logo-circle" />
          <span className="app-loading-logo-text">WebApólice</span>
        </div>
        <Spinner size="large" aria-label="Inicializando..." />
        <p className="app-loading-message" aria-hidden="true">
          Verificando autenticação...
        </p>
      </div>
    </div>
  );
};
