/**
 * ForbiddenPage.tsx
 *
 * Página /forbidden — Acesso negado.
 * Exibida quando o usuário está autenticado mas não possui a role necessária.
 */
import React, { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../auth/useAuth';
import { ROUTES } from '../../app/routes/routePaths';
import { EmptyState } from '../../components/ui/EmptyState';
import { Button } from '../../components/ui/Button';
import { ErrorIcon } from '../../components/ui/Icons';
import '../ErrorPages.css';

export const ForbiddenPage: React.FC = () => {
  const { logout } = useAuth();
  const navigate = useNavigate();

  useEffect(() => {
    document.title = 'Acesso negado | WebApólice';
  }, []);

  return (
    <div className="error-page" role="main">
      <EmptyState
        title="Acesso negado"
        description="Você não tem permissão para acessar esta área. Se acredita que isso é um erro, entre em contato com o administrador do sistema."
        icon={<ErrorIcon size={48} className="error-page-icon" aria-hidden="true" />}
        action={
          <div className="error-page-actions">
            <Button
              id="btn-home-forbidden"
              variant="primary"
              onClick={() => navigate(ROUTES.APP)}
            >
              Voltar ao início
            </Button>
            <Button variant="secondary" onClick={() => logout()}>
              Sair da conta
            </Button>
          </div>
        }
      />
    </div>
  );
};
