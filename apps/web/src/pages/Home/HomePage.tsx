import React, { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../auth/useAuth';
import { useAuthorization } from '../../auth/AuthorizationProvider';
import { APP_ROLES } from '../../auth/roles';
import { ENV } from '../../app/config/env';
import { ROUTES } from '../../app/routes/routePaths';
import {
  Card,
  CardHeader,
  CardTitle,
  CardDescription,
  CardContent,
  CardFooter,
} from '../../components/ui/Card';
import { Button } from '../../components/ui/Button';
import { BriefcaseIcon } from '../../components/ui/Icons';
import './HomePage.css';

export const HomePage: React.FC = () => {
  const { user, hasRole, hasAnyRole } = useAuth();
  const navigate = useNavigate();
  const canAccessDesignSystem = ENV.ENABLE_DESIGN_SYSTEM && hasRole(APP_ROLES.ADMIN);
  const canAccessClientes = hasAnyRole([
    APP_ROLES.ADMIN,
    APP_ROLES.GESTOR,
    APP_ROLES.OPERADOR,
  ]);
  const { possuiPermissao } = useAuthorization();
  const canAccessApolices = possuiPermissao('apolices.visualizar');
  const canAccessRamos = possuiPermissao('ramos.visualizar');
  const canAccessSeguradoras = possuiPermissao('seguradoras.visualizar');
  const canAccessCorretoras = possuiPermissao('corretoras.visualizar');
  const canAccessCooperados = possuiPermissao('cooperados.visualizar');

  useEffect(() => {
    document.title = 'Início | WebApólice';
  }, []);

  const firstName = user?.name?.split(' ')[0] || user?.username || 'usuário';

  return (
    <div className="home-page">
      <div className="home-header">
        <h1 className="home-title">Olá, {firstName}!</h1>
        <p className="home-subtitle">Você está autenticado e com acesso ao sistema.</p>
      </div>

      <div className="home-modules">
        <h2 className="home-section-title">Módulos disponíveis</h2>
        <div className="home-cards">
          {canAccessClientes && (
            <Card>
              <CardHeader>
                <CardTitle>Clientes</CardTitle>
                <CardDescription>
                  Consulte e gerencie os clientes cadastrados na plataforma.
                </CardDescription>
              </CardHeader>
              <CardContent>
                <p>Acesse a listagem de clientes, aplique filtros e consulte os registros disponíveis.</p>
              </CardContent>
              <CardFooter>
                <Button variant="primary" onClick={() => navigate(ROUTES.CLIENTES)}>
                  Acessar clientes
                </Button>
              </CardFooter>
            </Card>
          )}

          {canAccessSeguradoras && (
            <Card>
              <CardHeader className="flex flex-row items-center gap-3">
                <BriefcaseIcon size={24} className="text-brand-primaria" />
                <CardTitle>Seguradoras</CardTitle>
              </CardHeader>
              <CardContent>
                <p>Acesse o catálogo mestre de seguradoras, realize novos cadastros e consulte os registros.</p>
              </CardContent>
              <CardFooter>
                <Button variant="primary" onClick={() => navigate(ROUTES.SEGURADORAS)}>
                  Acessar seguradoras
                </Button>
              </CardFooter>
            </Card>
          )}

          {canAccessCorretoras && (
            <Card>
              <CardHeader className="flex flex-row items-center gap-3">
                <BriefcaseIcon size={24} className="text-brand-primaria" />
                <CardTitle>Corretoras</CardTitle>
              </CardHeader>
              <CardContent>
                <p>Consulte e gerencie as corretoras parceiras cadastradas no sistema.</p>
              </CardContent>
              <CardFooter>
                <Button variant="primary" onClick={() => navigate(ROUTES.CORRETORAS)}>
                  Acessar corretoras
                </Button>
              </CardFooter>
            </Card>
          )}

          {canAccessCooperados && (
            <Card>
              <CardHeader>
                <CardTitle>Cooperados</CardTitle>
                <CardDescription>
                  Consulte e gerencie os cooperados e coordenadores do sistema.
                </CardDescription>
              </CardHeader>
              <CardContent>
                <p>Acesse a listagem de cooperados, gerencie cadastros e visualize informações de coordenadores.</p>
              </CardContent>
              <CardFooter>
                <Button variant="primary" onClick={() => navigate(ROUTES.COOPERADOS)}>
                  Acessar cooperados
                </Button>
              </CardFooter>
            </Card>
          )}

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
        </div>
      </div>
    </div>
  );
};
