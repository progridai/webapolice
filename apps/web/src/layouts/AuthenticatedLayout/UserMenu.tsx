/**
 * UserMenu.tsx
 *
 * Menu do usuário autenticado no cabeçalho.
 * Exibe nome, avatar com iniciais e ação de logout.
 */
import React, { useCallback, useRef, useState } from 'react';
import { useAuth } from '../../auth/useAuth';

/** Extrai iniciais do nome para o avatar */
function getInitials(name: string): string {
  const parts = name.trim().split(/\s+/);
  if (parts.length === 1) return parts[0].slice(0, 2).toUpperCase();
  return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
}

export const UserMenu: React.FC = () => {
  const { user, logout } = useAuth();
  const [isOpen, setIsOpen] = useState(false);
  const menuRef = useRef<HTMLDivElement>(null);
  const buttonRef = useRef<HTMLButtonElement>(null);

  const handleToggle = useCallback(() => {
    setIsOpen((prev) => !prev);
  }, []);

  const handleKeyDown = useCallback(
    (e: React.KeyboardEvent) => {
      if (e.key === 'Escape') {
        setIsOpen(false);
        buttonRef.current?.focus();
      }
    },
    []
  );

  const handleLogout = useCallback(async () => {
    setIsOpen(false);
    await logout();
  }, [logout]);

  if (!user) return null;

  const initials = getInitials(user.name || user.username);

  return (
    <div className="user-menu" ref={menuRef} onKeyDown={handleKeyDown}>
      <button
        ref={buttonRef}
        id="user-menu-button"
        className="user-menu-trigger"
        aria-haspopup="menu"
        aria-expanded={isOpen}
        aria-label={`Menu do usuário: ${user.name || user.username}`}
        onClick={handleToggle}
        type="button"
      >
        <span className="user-avatar" aria-hidden="true">
          {initials}
        </span>
        <span className="user-name">{user.name || user.username}</span>
        <span className="user-menu-chevron" aria-hidden="true">
          {isOpen ? '▲' : '▼'}
        </span>
      </button>

      {isOpen && (
        <div
          className="user-menu-dropdown"
          role="menu"
          aria-labelledby="user-menu-button"
        >
          <div className="user-menu-info" role="none">
            <span className="user-menu-display-name">{user.name}</span>
            <span className="user-menu-email">{user.email}</span>
          </div>
          <hr className="user-menu-divider" />
          <button
            className="user-menu-item user-menu-logout"
            role="menuitem"
            type="button"
            onClick={handleLogout}
          >
            Sair da conta
          </button>
        </div>
      )}
    </div>
  );
};
