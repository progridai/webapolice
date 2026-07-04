/**
 * AppHeader.tsx
 *
 * Cabeçalho principal da aplicação autenticada.
 * Contém: logo, botão de menu mobile, seletor de tema, menu do usuário.
 */
import React from 'react';
import { Link } from 'react-router-dom';
import { ROUTES } from '../../app/routes/routePaths';
import { useTema, type TemaPreferido } from '../../shared/theme/ThemeContext';
import { ThemeIcon } from '../../components/ui/Icons';
import { UserMenu } from './UserMenu';

interface AppHeaderProps {
  onMenuToggle: () => void;
  isMobileMenuOpen: boolean;
}

export const AppHeader: React.FC<AppHeaderProps> = ({ onMenuToggle, isMobileMenuOpen }) => {
  const { temaPreferido, alterarTema } = useTema();

  return (
    <header className="app-header" role="banner">
      <div className="app-header-start">
        {/* Botão hambúrguer — apenas mobile */}
        <button
          className="app-header-menu-btn"
          aria-label={isMobileMenuOpen ? 'Fechar menu de navegação' : 'Abrir menu de navegação'}
          aria-expanded={isMobileMenuOpen}
          aria-controls="mobile-nav-panel"
          type="button"
          onClick={onMenuToggle}
        >
          <span aria-hidden="true">{isMobileMenuOpen ? '✕' : '☰'}</span>
        </button>

        {/* Logo */}
        <Link
          to={ROUTES.APP}
          className="app-header-logo"
          aria-label="WebApólice — página inicial"
        >
          <span className="app-header-logo-circle" aria-hidden="true" />
          <span className="app-header-logo-text">WebApólice</span>
        </Link>
      </div>

      <div className="app-header-end">
        {/* Seletor de tema */}
        <label htmlFor="auth-seletor-tema" className="sr-only">
          Selecione o tema da aplicação
        </label>
        <ThemeIcon aria-hidden="true" />
        <select
          id="auth-seletor-tema"
          className="app-header-theme-select"
          value={temaPreferido}
          onChange={(e) => alterarTema(e.target.value as TemaPreferido)}
          aria-label="Tema do sistema"
        >
          <option value="claro">Claro</option>
          <option value="escuro">Escuro</option>
          <option value="sistema">Sistema</option>
        </select>

        {/* Menu do usuário */}
        <UserMenu />
      </div>
    </header>
  );
};
