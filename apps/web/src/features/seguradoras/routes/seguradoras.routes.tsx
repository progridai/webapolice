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
      <PermissionProtectedRoute permissaoCodigo="seguradoras.visualizar">
        <AuthenticatedLayout />
      </PermissionProtectedRoute>
    }
  >
    <Route path={ROUTES.SEGURADORAS} element={<SeguradorasListPage />} />
    <Route
      path={ROUTES.SEGURADORA_NOVA}
      element={
        <PermissionProtectedRoute permissaoCodigo="seguradoras.inserir">
          <SeguradoraFormPage />
        </PermissionProtectedRoute>
      }
    />
    <Route
      path={ROUTES.SEGURADORA_EDITAR}
      element={
        <PermissionProtectedRoute permissaoCodigo="seguradoras.alterar">
          <SeguradoraFormPage />
        </PermissionProtectedRoute>
      }
    />
  </Route>
);
