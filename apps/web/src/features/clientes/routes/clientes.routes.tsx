/* eslint-disable react-refresh/only-export-components */
import React, { Suspense, lazy } from 'react';
import { Route } from 'react-router-dom';
import { ROUTES } from '../../../app/routes/routePaths';
import { RoleProtectedRoute } from '../../../app/routes/RoleProtectedRoute';
import { APP_ROLES } from '../../../auth/roles';
import { AuthenticatedLayout } from '../../../layouts/AuthenticatedLayout';
import { PageLoading } from '../../../components/application/PageLoading';

const ClientesListPage = lazy(() =>
  import('../pages/ClientesListPage').then((m) => ({ default: m.ClientesListPage }))
);

 
export const ClientesRoutes = (
  <Route
    element={
      <RoleProtectedRoute allowedRoles={[APP_ROLES.ADMIN, APP_ROLES.GESTOR, APP_ROLES.OPERADOR]}>
        <AuthenticatedLayout />
      </RoleProtectedRoute>
    }
  >
    <Route
      path={ROUTES.CLIENTES}
      element={
        <Suspense fallback={<PageLoading />}>
          <ClientesListPage />
        </Suspense>
      }
    />
    <Route
      path={ROUTES.CLIENTE_DETALHES}
      element={
        <Suspense fallback={<PageLoading />}>
          <div style={{ padding: '2rem' }}>
            <h1>Detalhes em desenvolvimento</h1>
            <p>A edição/visualização de detalhes de clientes não faz parte desta etapa.</p>
          </div>
        </Suspense>
      }
    />
  </Route>
);
