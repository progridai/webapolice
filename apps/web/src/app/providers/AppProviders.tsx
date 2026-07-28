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
import { IdentidadeVisualProvider } from '../../shared/identidade';
import { AuthProvider } from '../../auth/AuthProvider';
import { AuthorizationProvider } from '../../auth/AuthorizationProvider';
import { ErrorBoundary } from '../../components/application/ErrorBoundary';
import { AppRoutes } from '../routes/AppRoutes';

export const AppProviders: React.FC = () => {
  return (
    <ThemeProvider>
      <IdentidadeVisualProvider>
        <AuthProvider>
          <AuthorizationProvider>
            <HashRouter>
              <ErrorBoundary>
                <AppRoutes />
              </ErrorBoundary>
            </HashRouter>
          </AuthorizationProvider>
        </AuthProvider>
      </IdentidadeVisualProvider>
    </ThemeProvider>
  );
};
