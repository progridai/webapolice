import React, { lazy } from 'react';
import { Route } from 'react-router-dom';
import { ROUTES } from '../../../app/routes/routePaths';
import { PermissionProtectedRoute } from '../../../app/routes/PermissionProtectedRoute';

const CooperadosListPage = lazy(() =>
  import('../pages/CooperadosListPage').then((m) => ({ default: m.CooperadosListPage }))
);
const NovoCooperadoPage = lazy(() =>
  import('../pages/NovoCooperadoPage').then((m) => ({ default: m.NovoCooperadoPage }))
);
const EditarCooperadoPage = lazy(() =>
  import('../pages/EditarCooperadoPage').then((m) => ({ default: m.EditarCooperadoPage }))
);
const CooperadoDetalhePage = lazy(() =>
  import('../pages/CooperadoDetalhePage').then((m) => ({ default: m.CooperadoDetalhePage }))
);

export const CooperadosRoutes = (
  <Route path={ROUTES.COOPERADOS}>
    <Route
      index
      element={
        <PermissionProtectedRoute permissao="cooperados.visualizar">
          <CooperadosListPage />
        </PermissionProtectedRoute>
      }
    />
    <Route
      path="novo"
      element={
        <PermissionProtectedRoute permissao="cooperados.inserir">
          <NovoCooperadoPage />
        </PermissionProtectedRoute>
      }
    />
    <Route
      path=":id"
      element={
        <PermissionProtectedRoute permissao="cooperados.visualizar">
          <CooperadoDetalhePage />
        </PermissionProtectedRoute>
      }
    />
    <Route
      path=":id/editar"
      element={
        <PermissionProtectedRoute permissao="cooperados.alterar">
          <EditarCooperadoPage />
        </PermissionProtectedRoute>
      }
    />
  </Route>
);
