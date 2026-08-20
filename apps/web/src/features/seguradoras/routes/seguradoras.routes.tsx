/**
 * seguradoras.routes.tsx
 *
 * Mapeamento das rotas do módulo de Seguradoras com controle de acesso por permissão.
 */
import { Route } from 'react-router-dom';
import { ROUTES } from '../../../app/routes/routePaths';
import { PermissionProtectedRoute } from '../../../app/routes/PermissionProtectedRoute';
import { AuthenticatedLayout } from '../../../layouts/AuthenticatedLayout';

import { SeguradorasListPage } from '../pages/SeguradorasListPage';
import { SeguradoraFormPage } from '../pages/SeguradoraFormPage';

export const SeguradorasRoutes = (
  <Route
    element={
      <PermissionProtectedRoute permissoes={['seguradoras.visualizar']}>
        <AuthenticatedLayout />
      </PermissionProtectedRoute>
    }
  >
    <Route path={ROUTES.SEGURADORAS} element={<SeguradorasListPage />} />
    <Route
      path={ROUTES.SEGURADORA_NOVA}
      element={
        <PermissionProtectedRoute permissoes={['seguradoras.inserir']}>
          <SeguradoraFormPage />
        </PermissionProtectedRoute>
      }
    />
    <Route
      path={ROUTES.SEGURADORA_EDITAR}
      element={
        <PermissionProtectedRoute permissoes={['seguradoras.alterar']}>
          <SeguradoraFormPage />
        </PermissionProtectedRoute>
      }
    />
  </Route>
);
