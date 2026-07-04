/**
 * AuthenticatedLayout.tsx
 *
 * Layout principal da aplicação autenticada.
 * Estrutura: cabeçalho | sidebar (desktop) + conteúdo | (mobile: overlay nav)
 *
 * Acessibilidade:
 * - skip link para conteúdo principal
 * - aria-label em header, nav, main, aside
 * - foco gerenciado no menu mobile
 * - document.title atualizado via hook de rota (feito nas páginas)
 */
import React, { useCallback, useState } from 'react';
import { Outlet } from 'react-router-dom';
import { AppHeader } from './AppHeader';
import { AppSidebar } from './AppSidebar';
import { MobileNavigation } from './MobileNavigation';
import './AuthenticatedLayout.css';

export const AuthenticatedLayout: React.FC = () => {
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);

  const handleMenuToggle = useCallback(() => {
    setIsMobileMenuOpen((prev) => !prev);
  }, []);

  const handleMenuClose = useCallback(() => {
    setIsMobileMenuOpen(false);
  }, []);

  return (
    <div className="authenticated-layout">
      {/* Skip link acessível */}
      <a href="#conteudo-principal" className="skip-link">
        Pular para o conteúdo principal
      </a>

      {/* Cabeçalho */}
      <AppHeader
        onMenuToggle={handleMenuToggle}
        isMobileMenuOpen={isMobileMenuOpen}
      />

      <div className="authenticated-layout-body">
        {/* Sidebar — visível em desktop/tablet */}
        <AppSidebar />

        {/* Área de conteúdo */}
        <main
          className="authenticated-layout-main"
          id="conteudo-principal"
          tabIndex={-1}
        >
          <Outlet />
        </main>
      </div>

      {/* Navegação mobile */}
      <MobileNavigation
        isOpen={isMobileMenuOpen}
        onClose={handleMenuClose}
      />
    </div>
  );
};
