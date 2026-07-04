/**
 * AppNavigation.tsx
 *
 * Links de navegação principal da aplicação.
 * Usa aria-current="page" para indicar a rota ativa.
 * Itens desabilitados ficam visualmente evidentes mas não são links.
 */
import React from 'react';
import { NavLink, useLocation } from 'react-router-dom';
import { ROUTES } from '../../app/routes/routePaths';
import { ENV } from '../../app/config/env';
import { useAuth } from '../../auth/useAuth';
import { APP_ROLES } from '../../auth/roles';

interface NavItem {
  label: string;
  path: string;
  icon: string;
  requiresRoles?: string[];
  requiresEnvFlag?: boolean;
}

const NAV_ITEMS: NavItem[] = [
  {
    label: 'Início',
    path: ROUTES.APP,
    icon: '🏠',
  },
  {
    label: 'Clientes',
    path: ROUTES.CLIENTES,
    icon: '👥',
    requiresRoles: [APP_ROLES.ADMIN, APP_ROLES.GESTOR, APP_ROLES.OPERADOR],
  },
  {
    label: 'Design System',
    path: ROUTES.DESIGN_SYSTEM,
    icon: '🎨',
    requiresRoles: [APP_ROLES.ADMIN],
    requiresEnvFlag: true,
  },
];

interface AppNavigationProps {
  onNavigate?: () => void;
}

export const AppNavigation: React.FC<AppNavigationProps> = ({ onNavigate }) => {
  const { hasAnyRole } = useAuth();
  const location = useLocation();

  const visibleItems = NAV_ITEMS.filter((item) => {
    if (item.requiresEnvFlag && !ENV.ENABLE_DESIGN_SYSTEM) return false;
    if (item.requiresRoles && !hasAnyRole(item.requiresRoles)) return false;
    return true;
  });

  return (
    <nav aria-label="Navegação principal">
      <ul className="app-nav-list" role="list">
        {visibleItems.map((item) => {
          const isActive = location.pathname === item.path;
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
                  {item.icon}
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
