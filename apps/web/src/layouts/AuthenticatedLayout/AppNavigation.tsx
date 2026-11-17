import React from 'react';
import { NavLink, useLocation } from 'react-router-dom';
import { useAppNavigation } from '../../app/navigation/useAppNavigation';

interface AppNavigationProps {
  onNavigate?: () => void;
}

export const AppNavigation: React.FC<AppNavigationProps> = ({ onNavigate }) => {
  const location = useLocation();
  const visibleGroups = useAppNavigation();

  return (
    <nav aria-label="Navegação principal">
      <div className="app-nav-groups">
        {visibleGroups.map((group) => (
          <div className="app-nav-group" key={group.label ?? 'inicio'}>
            {group.label && (
              <h2 className="app-nav-group-title">{group.label}</h2>
            )}
            <ul
              className="app-nav-list"
              role="list"
              aria-label={group.label ?? 'Início'}
            >
              {group.items.map((item) => {
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
          </div>
        ))}
      </div>
    </nav>
  );
};
