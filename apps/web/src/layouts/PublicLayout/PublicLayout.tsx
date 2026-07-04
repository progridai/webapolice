/**
 * PublicLayout.tsx
 *
 * Layout para páginas públicas: login e páginas de erro.
 * Estrutura simples, centrada, sem navegação autenticada.
 */
import React from 'react';
import { Outlet } from 'react-router-dom';
import { useTema, type TemaPreferido } from '../../shared/theme/ThemeContext';
import { ThemeIcon } from '../../components/ui/Icons';
import './PublicLayout.css';

export const PublicLayout: React.FC = () => {
  const { temaPreferido, alterarTema } = useTema();

  return (
    <div className="public-layout">
      <header className="public-layout-header">
        <div className="public-layout-logo" aria-label="WebApólice">
          <span className="public-logo-circle" aria-hidden="true" />
          <span className="public-logo-text">WebApólice</span>
        </div>
        <div className="public-layout-actions">
          <label htmlFor="public-seletor-tema" className="sr-only">
            Selecione o tema da aplicação
          </label>
          <ThemeIcon aria-hidden="true" />
          <select
            id="public-seletor-tema"
            className="public-theme-select"
            value={temaPreferido}
            onChange={(e) => alterarTema(e.target.value as TemaPreferido)}
            aria-label="Tema do sistema"
          >
            <option value="claro">Claro</option>
            <option value="escuro">Escuro</option>
            <option value="sistema">Sistema</option>
          </select>
        </div>
      </header>

      <main className="public-layout-main" id="conteudo-principal">
        <a href="#conteudo-principal" className="skip-link">
          Pular para o conteúdo principal
        </a>
        <Outlet />
      </main>

      <footer className="public-layout-footer">
        <p>
          <strong>WebApólice</strong> &copy; {new Date().getFullYear()}
        </p>
      </footer>
    </div>
  );
};
