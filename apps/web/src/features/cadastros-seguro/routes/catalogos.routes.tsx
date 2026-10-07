import { Route } from 'react-router-dom';
import { PermissionProtectedRoute } from '../../../app/routes/PermissionProtectedRoute';
import { AuthenticatedLayout } from '../../../layouts/AuthenticatedLayout';
import { CatalogoListPage } from '../pages/CatalogoListPage';
import { CatalogoFormPage } from '../pages/CatalogoFormPage';
import { CONVENIOS, COBERTURAS, PLANOS } from '../catalogos.config';
export const CatalogosRoutes = [CONVENIOS, COBERTURAS, PLANOS].map((config) => (
  <Route
    key={config.recurso}
    element={
      <PermissionProtectedRoute
        moduloCodigo="CADASTRO"
        permissaoCodigo={`${config.permissao}.visualizar`}
      >
        <AuthenticatedLayout />
      </PermissionProtectedRoute>
    }
  >
    <Route path={config.rota} element={<CatalogoListPage config={config} />} />
    <Route
      path={`${config.rota}/novo`}
      element={
        <PermissionProtectedRoute
          permissaoCodigo={`${config.permissao}.inserir`}
        >
          <CatalogoFormPage config={config} />
        </PermissionProtectedRoute>
      }
    />
    <Route
      path={`${config.rota}/:publicId`}
      element={<CatalogoFormPage config={config} leitura />}
    />
    <Route
      path={`${config.rota}/:publicId/editar`}
      element={
        <PermissionProtectedRoute
          permissaoCodigo={`${config.permissao}.alterar`}
        >
          <CatalogoFormPage config={config} />
        </PermissionProtectedRoute>
      }
    />
  </Route>
));
