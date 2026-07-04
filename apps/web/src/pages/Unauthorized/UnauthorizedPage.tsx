/**
 * UnauthorizedPage.tsx
 *
 * Página /unauthorized — Sessão necessária.
 * Exibida quando o usuário tenta acessar rota privada sem autenticação.
 */
import React, { useEffect } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../../auth/useAuth';
import { EmptyState } from '../../components/ui/EmptyState';
import { Button } from '../../components/ui/Button';
import { AlertIcon } from '../../components/ui/Icons';
import '../ErrorPages.css';

export const UnauthorizedPage: React.FC = () => {
  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const from = (location.state as { from?: { pathname: string } })?.from?.pathname;

  useEffect(() => {
    document.title = 'Sessão necessária | WebApólice';
  }, []);

  const handleLogin = async () => {
    const redirectUri = from
      ? window.location.origin + '/#' + from
      : window.location.origin + '/#/app';
    await login(redirectUri);
  };

  return (
    <div className="error-page" role="main">
      <EmptyState
        title="Sessão necessária"
        description="Você precisa fazer login para acessar esta página. Por favor, entre com sua conta corporativa."
        icon={<AlertIcon size={48} className="error-page-icon" aria-hidden="true" />}
        action={
          <div className="error-page-actions">
            <Button id="btn-login-unauthorized" variant="primary" onClick={handleLogin}>
              Entrar
            </Button>
            <Button variant="secondary" onClick={() => navigate(-1)}>
              Voltar
            </Button>
          </div>
        }
      />
    </div>
  );
};
