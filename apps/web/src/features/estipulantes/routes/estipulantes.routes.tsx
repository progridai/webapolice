/* eslint-disable react-refresh/only-export-components */
import React, { Suspense, lazy } from 'react';
import { Route } from 'react-router-dom';
import { ROUTES } from '../../../app/routes/routePaths';
import { PermissionProtectedRoute } from '../../../app/routes/PermissionProtectedRoute';
import { AuthenticatedLayout } from '../../../layouts/AuthenticatedLayout';
import { PageLoading } from '../../../components/application/PageLoading';

const EstipulantesListPage = lazy(() =>
  import('../pages/EstipulantesListPage').then((m) => ({ default: m.EstipulantesListPage }))
);

const CadastrarEstipulantePage = lazy(() =>
  import('../pages/CadastrarEstipulantePage').then((m) => ({ default: m.CadastrarEstipulantePage }))
);

const EstipulanteDetalhePage = lazy(() =>
  import('../pages/EstipulanteDetalhePage').then((m) => ({ default: m.EstipulanteDetalhePage }))
);

const EditarEstipulantePage = lazy(() =>
  import('../pages/EditarEstipulantePage').then((m) => ({ default: m.EditarEstipulantePage }))
);

export const EstipulantesRoutes = (
  <Route
    element={
      <PermissionProtectedRoute moduloCodigo="ESTIPULANTES" permissaoCodigo="estipulantes.visualizar">
        <AuthenticatedLayout />
      </PermissionProtectedRoute>
    }
  >
    <Route
      path={ROUTES.ESTIPULANTES}
      element={
        <Suspense fallback={<PageLoading />}>
          <EstipulantesListPage />
        </Suspense>
      }
    />
    <Route
      path={ROUTES.ESTIPULANTE_NOVO}
      element={
        <PermissionProtectedRoute moduloCodigo="ESTIPULANTES" permissaoCodigo="estipulantes.inserir">
          <Suspense fallback={<PageLoading />}>
            <CadastrarEstipulantePage />
          </Suspense>
        </PermissionProtectedRoute>
      }
    />
    <Route
      path={ROUTES.ESTIPULANTE_DETALHES}
      element={
        <PermissionProtectedRoute moduloCodigo="ESTIPULANTES" permissaoCodigo="estipulantes.visualizar">
          <Suspense fallback={<PageLoading />}>
            <EstipulanteDetalhePage />
          </Suspense>
        </PermissionProtectedRoute>
      }
    />
    <Route
      path={ROUTES.ESTIPULANTE_EDITAR}
      element={
        <PermissionProtectedRoute moduloCodigo="ESTIPULANTES" permissaoCodigo="estipulantes.alterar">
          <Suspense fallback={<PageLoading />}>
            <EditarEstipulantePage />
          </Suspense>
        </PermissionProtectedRoute>
      }
    />
  </Route>
);
