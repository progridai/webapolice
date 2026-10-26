/**
 * subestipulantes.routes.tsx
 *
 * Mapeamento das rotas do módulo de Subestipulantes com controle de acesso por permissão.
 */
import { Route } from 'react-router-dom';
import { ROUTES } from '../../../app/routes/routePaths';
import { PermissionProtectedRoute } from '../../../app/routes/PermissionProtectedRoute';
import { AuthenticatedLayout } from '../../../layouts/AuthenticatedLayout';

import { SubestipulantesListPage } from '../pages/SubestipulantesListPage';
import { SubestipulanteFormPage } from '../pages/SubestipulanteFormPage';

export const SubestipulantesRoutes = (
  <Route
    element={
      <PermissionProtectedRoute permissaoCodigo="subestipulantes.visualizar">
        <AuthenticatedLayout />
      </PermissionProtectedRoute>
    }
  >
    <Route
      path={ROUTES.SUBESTIPULANTES}
      element={<SubestipulantesListPage />}
    />
    <Route
      path={ROUTES.SUBESTIPULANTE_NOVO}
      element={
        <PermissionProtectedRoute permissaoCodigo="subestipulantes.inserir">
          <SubestipulanteFormPage />
        </PermissionProtectedRoute>
      }
    />
    <Route
      path={ROUTES.SUBESTIPULANTE_EDITAR}
      element={
        <PermissionProtectedRoute permissaoCodigo="subestipulantes.alterar">
          <SubestipulanteFormPage />
        </PermissionProtectedRoute>
      }
    />
  </Route>
);
