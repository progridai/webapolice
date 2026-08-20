import { Route } from 'react-router-dom';
import { ROUTES } from '../../../app/routes/routePaths';
import { PermissionProtectedRoute } from '../../../app/routes/PermissionProtectedRoute';
import { AuthenticatedLayout } from '../../../layouts/AuthenticatedLayout';

import { RamosListPage } from '../pages/RamosListPage';
import { RamoFormPage } from '../pages/RamoFormPage';

export const RamosRoutes = (
  <Route
    element={
      <PermissionProtectedRoute permissoes={['ramos.visualizar']}>
        <AuthenticatedLayout />
      </PermissionProtectedRoute>
    }
  >
    <Route path={ROUTES.RAMOS} element={<RamosListPage />} />
    <Route path={ROUTES.RAMOS_NOVO} element={<RamoFormPage />} />
    <Route path={ROUTES.RAMOS_EDITAR} element={<RamoFormPage />} />
  </Route>
);
