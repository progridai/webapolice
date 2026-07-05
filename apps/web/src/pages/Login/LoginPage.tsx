/**
 * LoginPage.tsx
 *
 * Página pública de login (/login).
 * Redireciona para Keycloak se não autenticado.
 * Redireciona para /app se já autenticado.
 */
import React, { useEffect } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../../auth/useAuth';
import { ROUTES } from '../../app/routes/routePaths';
import { Button } from '../../components/ui/Button';
import { Spinner } from '../../components/ui/Spinner';
import { createPostLoginRedirectUri } from '../../auth/authRedirect';
import './LoginPage.css';

export const LoginPage: React.FC = () => {
  const { isAuthenticated, isLoading, login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const from = (location.state as { from?: Location })?.from?.pathname ?? ROUTES.APP;

  useEffect(() => {
    document.title = 'Login | WebApólice';
  }, []);

  // Já autenticado — redireciona para a rota de origem ou /app
  useEffect(() => {
    if (!isLoading && isAuthenticated) {
      navigate(from, { replace: true });
    }
  }, [isAuthenticated, isLoading, navigate, from]);

  if (isLoading) {
    return (
      <div className="login-page" role="status" aria-live="polite">
        <Spinner size="large" aria-label="Verificando autenticação..." />
      </div>
    );
  }

  const handleLogin = async () => {
    // Redireciona para a raiz sem hash para que o OIDC (Keycloak) consiga validar corretamente.
    // Após o login, o AppRoutes redirecionará da raiz (/) para /app automaticamente.
    await login(createPostLoginRedirectUri(from));
  };

  return (
    <div className="login-page">
      <div className="login-card">
        <div className="login-logo" aria-label="WebApólice">
          <span className="login-logo-circle" aria-hidden="true" />
          <span className="login-logo-text">WebApólice</span>
        </div>

        <h1 className="login-title">Bem-vindo</h1>
        <p className="login-description">
          Faça login com sua conta corporativa para acessar o sistema.
        </p>

        <Button
          id="btn-login"
          variant="primary"
          size="large"
          onClick={handleLogin}
          className="login-btn"
        >
          Entrar com SSO
        </Button>

        <p className="login-footer">
          Autenticação gerenciada pelo Keycloak via OIDC (PKCE S256).
        </p>
      </div>
    </div>
  );
};
