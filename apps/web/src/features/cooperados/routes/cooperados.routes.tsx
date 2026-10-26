import { Route } from 'react-router-dom';
import { ROUTES } from '../../../app/routes/routePaths';
import { PermissionProtectedRoute } from '../../../app/routes/PermissionProtectedRoute';
import { AuthenticatedLayout } from '../../../layouts/AuthenticatedLayout';

import {
  CooperadosListRoute,
  NovoCooperadoRoute,
  EditarCooperadoRoute,
  CooperadoDetalheRoute,
} from './CooperadosRoutePages';

export const CooperadosRoutes = (
  <Route path={ROUTES.COOPERADOS} element={<AuthenticatedLayout />}>
    <Route
      index
      element={
        <PermissionProtectedRoute permissaoCodigo="cooperados.visualizar">
          <CooperadosListRoute />
        </PermissionProtectedRoute>
      }
    />
    <Route
      path="novo"
      element={
        <PermissionProtectedRoute permissaoCodigo="cooperados.inserir">
          <NovoCooperadoRoute />
        </PermissionProtectedRoute>
      }
    />
    <Route
      path=":id"
      element={
        <PermissionProtectedRoute permissaoCodigo="cooperados.visualizar">
          <CooperadoDetalheRoute />
        </PermissionProtectedRoute>
      }
    />
    <Route
      path=":id/editar"
      element={
        <PermissionProtectedRoute permissaoCodigo="cooperados.alterar">
          <EditarCooperadoRoute />
        </PermissionProtectedRoute>
      }
    />
  </Route>
);
