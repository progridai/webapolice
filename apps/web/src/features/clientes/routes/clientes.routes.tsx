/* eslint-disable react-refresh/only-export-components */
import React, { Suspense, lazy } from 'react';
import { Route } from 'react-router-dom';
import { ROUTES } from '../../../app/routes/routePaths';
import { PermissionProtectedRoute } from '../../../app/routes/PermissionProtectedRoute';
import { AuthenticatedLayout } from '../../../layouts/AuthenticatedLayout';
import { PageLoading } from '../../../components/application/PageLoading';

const ClientesListPage = lazy(() =>
  import('../pages/ClientesListPage').then((m) => ({ default: m.ClientesListPage }))
);

const ClienteDetalhePage = lazy(() =>
  import('../pages/ClienteDetalhePage').then((m) => ({ default: m.ClienteDetalhePage }))
);

const CadastrarClientePage = lazy(() =>
  import('../pages/CadastrarClientePage').then((m) => ({ default: m.CadastrarClientePage }))
);

const EditarClientePage = lazy(() =>
  import('../pages/EditarClientePage').then((m) => ({ default: m.EditarClientePage }))
);

export const ClientesRoutes = (
  <Route
    element={
      <PermissionProtectedRoute moduloCodigo="CLIENTES" permissaoCodigo="clientes.visualizar">
        <AuthenticatedLayout />
      </PermissionProtectedRoute>
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
      path={ROUTES.CLIENTE_NOVO}
      element={
        <PermissionProtectedRoute moduloCodigo="CLIENTES" permissaoCodigo="clientes.inserir">
          <Suspense fallback={<PageLoading />}>
            <CadastrarClientePage />
          </Suspense>
        </PermissionProtectedRoute>
      }
    />
    <Route
      path={ROUTES.CLIENTE_EDITAR}
      element={
        <PermissionProtectedRoute moduloCodigo="CLIENTES" permissaoCodigo="clientes.alterar">
          <Suspense fallback={<PageLoading />}>
            <EditarClientePage />
          </Suspense>
        </PermissionProtectedRoute>
      }
    />
    <Route
      path={ROUTES.CLIENTE_DETALHES}
      element={
        <Suspense fallback={<PageLoading />}>
          <ClienteDetalhePage />
        </Suspense>
      }
    />
  </Route>
);
