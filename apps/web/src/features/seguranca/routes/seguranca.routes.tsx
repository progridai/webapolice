/* eslint-disable react-refresh/only-export-components */
import React, { Suspense, lazy } from 'react';
import { Route } from 'react-router-dom';
import { ROUTES } from '../../../app/routes/routePaths';
import { PermissionProtectedRoute } from '../../../app/routes/PermissionProtectedRoute';
import { AuthenticatedLayout } from '../../../layouts/AuthenticatedLayout';
import { PageLoading } from '../../../components/application/PageLoading';

const PerfisListPage = lazy(() =>
  import('../pages/PerfisListPage').then((m) => ({ default: m.PerfisListPage }))
);
const CadastrarPerfilPage = lazy(() =>
  import('../pages/CadastrarPerfilPage').then((m) => ({ default: m.CadastrarPerfilPage }))
);
const EditarPerfilPage = lazy(() =>
  import('../pages/EditarPerfilPage').then((m) => ({ default: m.EditarPerfilPage }))
);
const DetalhesPerfilPage = lazy(() =>
  import('../pages/DetalhesPerfilPage').then((m) => ({ default: m.DetalhesPerfilPage }))
);

const UsuariosListPage = lazy(() =>
  import('../pages/UsuariosListPage').then((m) => ({ default: m.UsuariosListPage }))
);
const CadastrarUsuarioPage = lazy(() =>
  import('../pages/CadastrarUsuarioPage').then((m) => ({ default: m.CadastrarUsuarioPage }))
);
const EditarUsuarioPage = lazy(() =>
  import('../pages/EditarUsuarioPage').then((m) => ({ default: m.EditarUsuarioPage }))
);
const DetalhesUsuarioPage = lazy(() =>
  import('../pages/DetalhesUsuarioPage').then((m) => ({ default: m.DetalhesUsuarioPage }))
);

const ModulosPage = lazy(() =>
  import('../pages/ModulosPage').then((m) => ({ default: m.ModulosPage }))
);

const CatalogoPage = lazy(() =>
  import('../pages/CatalogoPage').then((m) => ({ default: m.CatalogoPage }))
);

const AuditoriaListPage = lazy(() =>
  import('../pages/AuditoriaListPage').then((m) => ({ default: m.AuditoriaListPage }))
);

const AuditoriaDetalhesPage = lazy(() =>
  import('../pages/AuditoriaDetalhesPage').then((m) => ({ default: m.AuditoriaDetalhesPage }))
);

export const SegurancaRoutes = (
  <Route
    element={
      <PermissionProtectedRoute moduloCodigo="SEGURANCA">
        <AuthenticatedLayout />
      </PermissionProtectedRoute>
    }
  >
    {/* Perfis */}
    <Route
      path={ROUTES.SEGURANCA_PERFIS}
      element={
        <PermissionProtectedRoute permissaoCodigo="seguranca.perfis.visualizar">
          <Suspense fallback={<PageLoading />}>
            <PerfisListPage />
          </Suspense>
        </PermissionProtectedRoute>
      }
    />
    <Route
      path={ROUTES.SEGURANCA_PERFIL_NOVO}
      element={
        <PermissionProtectedRoute permissaoCodigo="seguranca.perfis.inserir">
          <Suspense fallback={<PageLoading />}>
            <CadastrarPerfilPage />
          </Suspense>
        </PermissionProtectedRoute>
      }
    />
    <Route
      path={ROUTES.SEGURANCA_PERFIL_EDITAR}
      element={
        <PermissionProtectedRoute permissaoCodigo="seguranca.perfis.alterar">
          <Suspense fallback={<PageLoading />}>
            <EditarPerfilPage />
          </Suspense>
        </PermissionProtectedRoute>
      }
    />
    <Route
      path={ROUTES.SEGURANCA_PERFIL_DETALHES}
      element={
        <Suspense fallback={<PageLoading />}>
          <DetalhesPerfilPage />
        </Suspense>
      }
    />

    {/* Usuários */}
    <Route
      path={ROUTES.SEGURANCA_USUARIOS}
      element={
        <PermissionProtectedRoute permissaoCodigo="seguranca.usuarios.visualizar">
          <Suspense fallback={<PageLoading />}>
            <UsuariosListPage />
          </Suspense>
        </PermissionProtectedRoute>
      }
    />
    <Route
      path={ROUTES.SEGURANCA_USUARIO_NOVO}
      element={
        <PermissionProtectedRoute permissaoCodigo="seguranca.usuarios.inserir">
          <Suspense fallback={<PageLoading />}>
            <CadastrarUsuarioPage />
          </Suspense>
        </PermissionProtectedRoute>
      }
    />
    <Route
      path={ROUTES.SEGURANCA_USUARIO_EDITAR}
      element={
        <PermissionProtectedRoute permissaoCodigo="seguranca.usuarios.alterar">
          <Suspense fallback={<PageLoading />}>
            <EditarUsuarioPage />
          </Suspense>
        </PermissionProtectedRoute>
      }
    />
    <Route
      path={ROUTES.SEGURANCA_USUARIO_DETALHES}
      element={
        <Suspense fallback={<PageLoading />}>
          <DetalhesUsuarioPage />
        </Suspense>
      }
    />

    {/* Módulos */}
    <Route
      path={ROUTES.SEGURANCA_MODULOS}
      element={
        <PermissionProtectedRoute somenteOperador>
          <Suspense fallback={<PageLoading />}>
            <ModulosPage />
          </Suspense>
        </PermissionProtectedRoute>
      }
    />

    {/* Catálogo — somente leitura */}
    <Route
      path={ROUTES.SEGURANCA_CATALOGO}
      element={
        <PermissionProtectedRoute permissaoCodigo="seguranca.catalogo.visualizar">
          <Suspense fallback={<PageLoading />}>
            <CatalogoPage />
          </Suspense>
        </PermissionProtectedRoute>
      }
    />

    {/* Auditoria */}
    <Route
      path={ROUTES.SEGURANCA_AUDITORIA}
      element={
        <PermissionProtectedRoute permissaoCodigo="seguranca.auditoria.visualizar">
          <Suspense fallback={<PageLoading />}>
            <AuditoriaListPage />
          </Suspense>
        </PermissionProtectedRoute>
      }
    />
    <Route
      path={ROUTES.SEGURANCA_AUDITORIA_DETALHES}
      element={
        <PermissionProtectedRoute permissaoCodigo="seguranca.auditoria.visualizar">
          <Suspense fallback={<PageLoading />}>
            <AuditoriaDetalhesPage />
          </Suspense>
        </PermissionProtectedRoute>
      }
    />
  </Route>
);
