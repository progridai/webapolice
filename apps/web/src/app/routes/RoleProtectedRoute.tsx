/**
 * RoleProtectedRoute.tsx
 *
 * Componente de rota protegida por autenticação E autorização por role.
 *
 * Comportamento:
 * - Aguarda resolução do estado de autenticação
 * - Exige que o usuário esteja autenticado
 * - Valida uma ou mais roles antes de permitir acesso
 * - Redireciona para /forbidden quando a role não é satisfeita
 * - Não apenas oculta conteúdo — protege a rota de fato
 *
 * IMPORTANTE: A autorização real acontece no backend.
 * Este componente serve para navegação e experiência do usuário.
 *
 * @example
 * ```tsx
 * <RoleProtectedRoute allowedRoles={[APP_ROLES.ADMIN, APP_ROLES.GESTOR]}>
 *   <MinhaPagina />
 * </RoleProtectedRoute>
 * ```
 */
import React from 'react';
import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useAuth } from '../../auth/useAuth';
import { ROUTES } from './routePaths';
import { PageLoading } from '../../components/application/PageLoading';

interface RoleProtectedRouteProps {
  /** Roles aceitas — usuário deve ter ao menos uma */
  allowedRoles: string[];
  /** Elemento a renderizar se autorizado (padrão: <Outlet /> para rotas aninhadas) */
  children?: React.ReactNode;
}

export const RoleProtectedRoute: React.FC<RoleProtectedRouteProps> = ({
  allowedRoles,
  children,
}) => {
  const { isLoading, isAuthenticated, hasAnyRole } = useAuth();
  const location = useLocation();

  // Aguarda resolução da autenticação
  if (isLoading) {
    return <PageLoading />;
  }

  // Usuário não autenticado — redireciona preservando rota
  if (!isAuthenticated) {
    return <Navigate to={ROUTES.UNAUTHORIZED} state={{ from: location }} replace />;
  }

  // Usuário autenticado sem role suficiente
  if (!hasAnyRole(allowedRoles)) {
    return <Navigate to={ROUTES.FORBIDDEN} replace />;
  }

  // Acesso autorizado
  return children ? <>{children}</> : <Outlet />;
};
