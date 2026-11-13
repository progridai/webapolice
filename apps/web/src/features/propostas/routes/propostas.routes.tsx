import { Route } from 'react-router-dom';
import { ROUTES } from '../../../app/routes/routePaths';
import { PermissionProtectedRoute } from '../../../app/routes/PermissionProtectedRoute';
import { AuthenticatedLayout } from '../../../layouts/AuthenticatedLayout';
import { PropostasRoutePage } from './PropostasRoutePage';

export const PropostasRoutes = (
  <Route
    element={
      <PermissionProtectedRoute
        moduloCodigo="APOLICES"
        permissaoCodigo="propostas.visualizar"
        recursoCodigo="PROPOSTAS"
      >
        <AuthenticatedLayout />
      </PermissionProtectedRoute>
    }
  >
    <Route path={ROUTES.PROPOSTAS} element={<PropostasRoutePage />} />
  </Route>
);
