import React from 'react';
import { NavLink, useLocation } from 'react-router-dom';
import { ROUTES } from '../../app/routes/routePaths';
import { ENV } from '../../app/config/env';
import { useAuthorization } from '../../auth/AuthorizationProvider';
import {
  BookOpenIcon,
  ClipboardListIcon,
  HomeIcon,
  PaletteIcon,
  SettingsIcon,
  ShieldIcon,
  UserIcon,
  UsersIcon,
  BriefcaseIcon,
} from '../../components/ui/Icons';

interface NavItem {
  label: string;
  path: string;
  icon: React.ComponentType<{ size?: number; className?: string }>;
  moduloCodigo?: string;
  permissaoCodigo?: string;
  somenteOperador?: boolean;
  requiresEnvFlag?: boolean;
}

const NAV_ITEMS: NavItem[] = [
  {
    label: 'Início',
    path: ROUTES.APP,
    icon: HomeIcon,
  },
  {
    label: 'Clientes',
    path: ROUTES.CLIENTES,
    icon: UsersIcon,
    moduloCodigo: 'CLIENTES',
  },
  {
    label: 'Estipulantes',
    path: ROUTES.ESTIPULANTES,
    icon: BriefcaseIcon,
    moduloCodigo: 'ESTIPULANTES',
    permissaoCodigo: 'estipulantes.visualizar',
  },
  {
    label: 'Cooperados',
    path: ROUTES.COOPERADOS,
    icon: UsersIcon,
    moduloCodigo: 'COOPERADOS',
    permissaoCodigo: 'cooperados.visualizar',
  },
  {
    label: 'Usuários',
    path: ROUTES.SEGURANCA_USUARIOS,
    icon: UserIcon,
    moduloCodigo: 'SEGURANCA',
    permissaoCodigo: 'seguranca.usuarios.visualizar',
  },
  {
    label: 'Perfis',
    path: ROUTES.SEGURANCA_PERFIS,
    icon: ShieldIcon,
    moduloCodigo: 'SEGURANCA',
    permissaoCodigo: 'seguranca.perfis.visualizar',
  },
  {
    label: 'Catálogo',
    path: ROUTES.SEGURANCA_CATALOGO,
    icon: BookOpenIcon,
    moduloCodigo: 'SEGURANCA',
    permissaoCodigo: 'seguranca.catalogo.visualizar',
  },
  {
    label: 'Auditoria',
    path: ROUTES.SEGURANCA_AUDITORIA,
    icon: ClipboardListIcon,
    moduloCodigo: 'SEGURANCA',
    permissaoCodigo: 'seguranca.auditoria.visualizar',
  },
  {
    label: 'Módulos',
    path: ROUTES.SEGURANCA_MODULOS,
    icon: SettingsIcon,
    somenteOperador: true,
  },
  {
    label: 'Design System',
    path: ROUTES.DESIGN_SYSTEM,
    icon: PaletteIcon,
    somenteOperador: true,
    requiresEnvFlag: true,
  },
];

interface AppNavigationProps {
  onNavigate?: () => void;
}

export const AppNavigation: React.FC<AppNavigationProps> = ({ onNavigate }) => {
  const { ehOperadorSistema, possuiModulo, possuiPermissao, possuiAcessoTotal } = useAuthorization();
  const location = useLocation();

  const visibleItems = NAV_ITEMS.filter((item) => {
    if (item.requiresEnvFlag && !ENV.ENABLE_DESIGN_SYSTEM) return false;
    if (item.somenteOperador && !ehOperadorSistema()) return false;
    if (item.moduloCodigo && !possuiModulo(item.moduloCodigo)) return false;
    if (item.permissaoCodigo && !possuiAcessoTotal() && !possuiPermissao(item.permissaoCodigo)) return false;
    return true;
  });

  return (
    <nav aria-label="Navegação principal">
      <ul className="app-nav-list" role="list">
        {visibleItems.map((item) => {
          const isActive = location.pathname === item.path;
          const Icon = item.icon;

          return (
            <li key={item.path}>
              <NavLink
                to={item.path}
                className={({ isActive: active }) =>
                  `app-nav-item${active ? ' app-nav-item--active' : ''}`
                }
                aria-current={isActive ? 'page' : undefined}
                onClick={onNavigate}
              >
                <span className="app-nav-icon" aria-hidden="true">
                  <Icon size={18} />
                </span>
                <span className="app-nav-label">{item.label}</span>
              </NavLink>
            </li>
          );
        })}
      </ul>
    </nav>
  );
};
