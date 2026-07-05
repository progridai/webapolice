/**
 * NotFoundPage.tsx
 *
 * Página 404 — Página não encontrada.
 */
import React, { useEffect } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { ROUTES } from '../../app/routes/routePaths';
import { EmptyState } from '../../components/ui/EmptyState';
import { Button } from '../../components/ui/Button';
import { InfoIcon } from '../../components/ui/Icons';
import '../ErrorPages.css';

export const NotFoundPage: React.FC = () => {
  const navigate = useNavigate();

  const location = useLocation();

  useEffect(() => {
    document.title = 'Página não encontrada | WebApólice';
  }, []);

  return (
    <div className="error-page" role="main">
      <EmptyState
        title="Página não encontrada"
        description={`A página que você procura (${location.pathname}) não existe ou foi movida. Verifique o endereço ou retorne ao início.`}
        icon={<InfoIcon size={48} className="error-page-icon" aria-hidden="true" />}
        action={
          <div className="error-page-actions">
            <Button
              id="btn-home-notfound"
              variant="primary"
              onClick={() => navigate(ROUTES.APP)}
            >
              Voltar ao início
            </Button>
            <Button variant="secondary" onClick={() => navigate(-1)}>
              Página anterior
            </Button>
          </div>
        }
      />
    </div>
  );
};
