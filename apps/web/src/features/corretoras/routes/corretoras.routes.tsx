/**
 * corretoras.routes.tsx
 *
 * Mapeamento das rotas do módulo de Corretoras com controle de acesso por permissão.
 */
import { Route } from 'react-router-dom';
import { ROUTES } from '../../../app/routes/routePaths';
import { PermissionProtectedRoute } from '../../../app/routes/PermissionProtectedRoute';
import { AuthenticatedLayout } from '../../../layouts/AuthenticatedLayout';

import { CorretorasListPage } from '../pages/CorretorasListPage';
import { CorretoraFormPage } from '../pages/CorretoraFormPage';

export const CorretorasRoutes = (
  <Route
    element={
      <PermissionProtectedRoute permissaoCodigo="corretoras.visualizar">
        <AuthenticatedLayout />
      </PermissionProtectedRoute>
    }
  >
    <Route path={ROUTES.CORRETORAS} element={<CorretorasListPage />} />
    <Route
      path={ROUTES.CORRETORA_NOVA}
      element={
        <PermissionProtectedRoute permissaoCodigo="corretoras.inserir">
          <CorretoraFormPage />
        </PermissionProtectedRoute>
      }
    />
    <Route
      path={ROUTES.CORRETORA_EDITAR}
      element={
        <PermissionProtectedRoute permissaoCodigo="corretoras.alterar">
          <CorretoraFormPage />
        </PermissionProtectedRoute>
      }
    />
  </Route>
);
