/**
 * ProtectedRoute.tsx
 *
 * Componente de rota protegida por autenticação.
 *
 * Comportamento:
 * - Aguarda resolução do estado de autenticação (exibe carregamento)
 * - Permite acesso apenas a usuários autenticados
 * - Redireciona não-autenticados para /unauthorized, preservando a rota de origem
 * - Não renderiza conteúdo protegido enquanto o estado é indefinido
 */
import React from 'react';
import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useAuth } from '../../auth/useAuth';
import { ROUTES } from './routePaths';
import { PageLoading } from '../../components/application/PageLoading';

interface ProtectedRouteProps {
  /** Elemento a renderizar se autenticado (padrão: <Outlet /> para rotas aninhadas) */
  children?: React.ReactNode;
}

export const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ children }) => {
  const { isLoading, isAuthenticated } = useAuth();
  const location = useLocation();

  // Aguarda resolução da autenticação
  if (isLoading) {
    return <PageLoading />;
  }

  // Redireciona não-autenticado, preservando a rota de destino
  if (!isAuthenticated) {
    return (
      <Navigate
        to={ROUTES.UNAUTHORIZED}
        state={{ from: location }}
        replace
      />
    );
  }

  // Renderiza conteúdo protegido
  return children ? <>{children}</> : <Outlet />;
};
