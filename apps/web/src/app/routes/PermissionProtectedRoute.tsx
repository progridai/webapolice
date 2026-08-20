import React from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useAuthorization } from '../../auth/AuthorizationProvider';

interface PermissionProtectedRouteProps {
  children: React.ReactNode;
  moduloCodigo?: string;
  permissaoCodigo?: string;
  somenteOperador?: boolean;
}

export const PermissionProtectedRoute: React.FC<PermissionProtectedRouteProps> = ({
  children,
  moduloCodigo,
  permissaoCodigo,
  somenteOperador,
}) => {
  const location = useLocation();
  const { isLoading, error, usuarioEncontrado, usuarioAtivo, ehOperadorSistema, possuiModulo, possuiPermissao, possuiAcessoTotal } = useAuthorization();

  if (isLoading) {
    return <div>Carregando...</div>;
  }

  if (error || !usuarioEncontrado || !usuarioAtivo) {
    return <Navigate to="/unauthorized" state={{ from: location }} replace />;
  }

  if (somenteOperador && !ehOperadorSistema()) {
    return <Navigate to="/forbidden" state={{ from: location }} replace />;
  }

  if (moduloCodigo && !possuiModulo(moduloCodigo)) {
    return <Navigate to="/forbidden" state={{ from: location }} replace />;
  }

  if (permissaoCodigo && !possuiAcessoTotal() && !possuiPermissao(permissaoCodigo)) {
    return <Navigate to="/forbidden" state={{ from: location }} replace />;
  }

  return <>{children}</>;
};
