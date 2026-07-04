/**
 * AppProviders.tsx
 *
 * Wrapper central que compõe todos os providers da aplicação.
 * Ordem: ThemeProvider → AuthProvider → HashRouter → ErrorBoundary
 *
 * REGRA: Adicione novos providers globais aqui, na ordem correta de dependência.
 */
import React from 'react';
import { HashRouter } from 'react-router-dom';
import { ThemeProvider } from '../../shared/theme/ThemeContext';
import { AuthProvider } from '../../auth/AuthProvider';
import { ErrorBoundary } from '../../components/application/ErrorBoundary';
import { AppRoutes } from '../routes/AppRoutes';

export const AppProviders: React.FC = () => {
  return (
    <ThemeProvider>
      <HashRouter>
        <AuthProvider>
          <ErrorBoundary>
            <AppRoutes />
          </ErrorBoundary>
        </AuthProvider>
      </HashRouter>
    </ThemeProvider>
  );
};
