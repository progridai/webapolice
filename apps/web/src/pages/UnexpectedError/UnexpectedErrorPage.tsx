/**
 * UnexpectedErrorPage.tsx
 *
 * Página /error — Erro inesperado.
 * Usada para erros de sistema não tratados.
 * Não exibe detalhes técnicos ao usuário.
 */
import React, { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { ROUTES } from '../../app/routes/routePaths';
import { EmptyState } from '../../components/ui/EmptyState';
import { Button } from '../../components/ui/Button';
import { ErrorIcon } from '../../components/ui/Icons';
import '../ErrorPages.css';

export const UnexpectedErrorPage: React.FC = () => {
  const navigate = useNavigate();

  useEffect(() => {
    document.title = 'Erro inesperado | WebApólice';
  }, []);

  const handleRetry = () => {
    window.location.reload();
  };

  return (
    <div className="error-page" role="main">
      <EmptyState
        title="Algo deu errado"
        description="Ocorreu um erro inesperado no sistema. Nossa equipe já foi notificada. Tente novamente ou retorne ao início."
        icon={<ErrorIcon size={48} className="error-page-icon" aria-hidden="true" />}
        action={
          <div className="error-page-actions">
            <Button
              id="btn-retry-error"
              variant="primary"
              onClick={handleRetry}
            >
              Tentar novamente
            </Button>
            <Button
              variant="secondary"
              onClick={() => navigate(ROUTES.APP)}
            >
              Voltar ao início
            </Button>
          </div>
        }
      />
    </div>
  );
};
