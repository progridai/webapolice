/**
 * AppRoutes.tsx
 *
 * Configuração oficial de roteamento da aplicação.
 *
 * Decisão: HashRouter
 * URLs no formato: /#/app, /#/login, /#/design-system
 * Razão: ambiente de hospedagem ainda não possui SPA fallback configurado.
 * Migração futura: substituir HashRouter por BrowserRouter quando o servidor
 * suportar SPA fallback (ex: nginx try_files). Ver docs/14-fundacao-frontend.md.
 *
 * Todas as políticas de acesso de rotas estão declaradas aqui.
 * REGRA: Toda rota funcional deve declarar sua política de acesso.
 */
import React, { Suspense, lazy } from 'react';
import { Navigate, Route, Routes } from 'react-router-dom';
import { ROUTES } from './routePaths';
import { ProtectedRoute } from './ProtectedRoute';
import { RoleProtectedRoute } from './RoleProtectedRoute';
import { APP_ROLES } from '../../auth/roles';
import { ENV } from '../config/env';

// Layouts
import { PublicLayout } from '../../layouts/PublicLayout';
import { AuthenticatedLayout } from '../../layouts/AuthenticatedLayout';

// Páginas públicas (sem lazy — carregamento crítico)
import { LoginPage } from '../../pages/Login';
import { UnauthorizedPage } from '../../pages/Unauthorized';
import { ForbiddenPage } from '../../pages/Forbidden';
import { NotFoundPage } from '../../pages/NotFound';
import { UnexpectedErrorPage } from '../../pages/UnexpectedError';

// Páginas autenticadas (com lazy loading)
const HomePage = lazy(() =>
  import('../../pages/Home').then((m) => ({ default: m.HomePage }))
);
const DesignSystemPage = lazy(() =>
  import('../../pages/DesignSystem').then((m) => ({ default: m.DesignSystemPage }))
);

// Carregamento de rota
import { PageLoading } from '../../components/application/PageLoading';

// Módulos
import { ClientesRoutes } from '../../features/clientes/routes/clientes.routes';

export const AppRoutes: React.FC = () => {
  return (
    <Routes>
      {/* Raiz: redireciona para /app (autenticado) ou /login (não-autenticado via ProtectedRoute) */}
      <Route path={ROUTES.ROOT} element={<Navigate to={ROUTES.APP} replace />} />

      {/* ── Rotas Públicas ── */}
      <Route element={<PublicLayout />}>
        <Route path={ROUTES.LOGIN} element={<LoginPage />} />
        <Route path={ROUTES.UNAUTHORIZED} element={<UnauthorizedPage />} />
        <Route path={ROUTES.FORBIDDEN} element={<ForbiddenPage />} />
        <Route path={ROUTES.ERROR} element={<UnexpectedErrorPage />} />
        <Route path={ROUTES.NOT_FOUND} element={<NotFoundPage />} />
      </Route>

      {/* ── Rotas Autenticadas (Gerais) ── */}
      <Route element={<ProtectedRoute />}>
        <Route element={<AuthenticatedLayout />}>
          <Route
            path={ROUTES.APP}
            element={
              <Suspense fallback={<PageLoading />}>
                <HomePage />
              </Suspense>
            }
          />
        </Route>
      </Route>

      {/* ── Módulos de Funcionalidade ── */}
      {ClientesRoutes}

      {/* ── Design System (autenticado + role admin) ── */}
      {ENV.ENABLE_DESIGN_SYSTEM && (
        <Route
          element={
            <RoleProtectedRoute allowedRoles={[APP_ROLES.ADMIN]}>
              <AuthenticatedLayout />
            </RoleProtectedRoute>
          }
        >
          <Route
            path={ROUTES.DESIGN_SYSTEM}
            element={
              <Suspense fallback={<PageLoading />}>
                <DesignSystemPage />
              </Suspense>
            }
          />
        </Route>
      )}
    </Routes>
  );
};
