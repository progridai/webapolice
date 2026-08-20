/* eslint-disable react-refresh/only-export-components */
import React, { Suspense, lazy } from 'react';
import { Route } from 'react-router-dom';
import { ROUTES } from '../../../app/routes/routePaths';
import { PermissionProtectedRoute } from '../../../app/routes/PermissionProtectedRoute';
import { AuthenticatedLayout } from '../../../layouts/AuthenticatedLayout';
import { PageLoading } from '../../../components/application/PageLoading';

const ApolicesListPage = lazy(() =>
  import('../pages/ApolicesListPage').then((m) => ({
    default: m.ApolicesListPage,
  }))
);

const ApoliceDetalhesPage = lazy(() =>
  import('../pages/ApoliceDetalhesPage').then((m) => ({
    default: m.ApoliceDetalhesPage,
  }))
);

const ApoliceFormPage = lazy(() =>
  import('../pages/ApoliceFormPage').then((m) => ({
    default: m.ApoliceFormPage,
  }))
);

export const ApolicesRoutes = (
  <Route
    element={
      <PermissionProtectedRoute
        moduloCodigo="APOLICES"
        permissaoCodigo="apolices.visualizar"
      >
        <AuthenticatedLayout />
      </PermissionProtectedRoute>
    }
  >
    <Route
      path={ROUTES.APOLICES}
      element={
        <Suspense fallback={<PageLoading />}>
          <ApolicesListPage />
        </Suspense>
      }
    />
    <Route
      path={ROUTES.APOLICE_NOVA}
      element={
        <PermissionProtectedRoute permissaoCodigo="apolices.inserir">
          <Suspense fallback={<PageLoading />}>
            <ApoliceFormPage />
          </Suspense>
        </PermissionProtectedRoute>
      }
    />
    <Route
      path={ROUTES.APOLICE_DETALHES}
      element={
        <Suspense fallback={<PageLoading />}>
          <ApoliceDetalhesPage />
        </Suspense>
      }
    />
    <Route
      path={ROUTES.APOLICE_EDITAR}
      element={
        <PermissionProtectedRoute permissaoCodigo="apolices.alterar">
          <Suspense fallback={<PageLoading />}>
            <ApoliceFormPage />
          </Suspense>
        </PermissionProtectedRoute>
      }
    />
  </Route>
);
