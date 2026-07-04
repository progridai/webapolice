/**
 * HomePage.tsx
 *
 * Página inicial autenticada (/app).
 * Exibe saudação ao usuário, módulos disponíveis e acesso ao Design System.
 * Não contém dados fictícios nem métricas simuladas.
 */
import React, { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../auth/useAuth';
import { APP_ROLES } from '../../auth/roles';
import { ENV } from '../../app/config/env';
import { ROUTES } from '../../app/routes/routePaths';
import { Card, CardHeader, CardTitle, CardDescription, CardContent, CardFooter } from '../../components/ui/Card';
import { Button } from '../../components/ui/Button';
import './HomePage.css';

export const HomePage: React.FC = () => {
  const { user, hasRole } = useAuth();
  const navigate = useNavigate();
  const canAccessDesignSystem = ENV.ENABLE_DESIGN_SYSTEM && hasRole(APP_ROLES.ADMIN);

  useEffect(() => {
    document.title = `Início | WebApólice`;
  }, []);

  const firstName = user?.name?.split(' ')[0] || user?.username || 'usuário';

  return (
    <div className="home-page">
      <div className="home-header">
        <h1 className="home-title">
          Olá, {firstName}!
        </h1>
        <p className="home-subtitle">
          Você está autenticado e com acesso ao sistema.
        </p>
      </div>

      <div className="home-modules">
        <h2 className="home-section-title">Módulos disponíveis</h2>
        <div className="home-cards">
          {canAccessDesignSystem && (
            <Card>
              <CardHeader>
                <CardTitle>Design System</CardTitle>
                <CardDescription>
                  Catálogo de componentes visuais e tokens de design da aplicação.
                </CardDescription>
              </CardHeader>
              <CardContent>
                <p>Acesse o catálogo completo de componentes, tokens e diretrizes da identidade visual WebApólice.</p>
              </CardContent>
              <CardFooter>
                <Button variant="primary" onClick={() => navigate(ROUTES.DESIGN_SYSTEM)}>
                  Acessar catálogo
                </Button>
              </CardFooter>
            </Card>
          )}

          {/* Slot para módulos futuros */}
          <Card>
            <CardHeader>
              <CardTitle>Clientes</CardTitle>
              <CardDescription>
                Gestão de clientes da plataforma WebApólice.
              </CardDescription>
            </CardHeader>
            <CardContent>
              <p>O módulo de clientes estará disponível em breve.</p>
            </CardContent>
            <CardFooter>
              <Button variant="secondary" disabled>
                Em breve
              </Button>
            </CardFooter>
          </Card>
        </div>
      </div>
    </div>
  );
};
