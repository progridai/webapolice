/**
 * MobileNavigation.tsx
 *
 * Navegação mobile com menu hambúrguer.
 * Acessibilidade:
 * - aria-expanded no botão
 * - aria-controls aponta para o menu
 * - Escape fecha o menu
 * - Overlay bloqueia interação do conteúdo atrás
 * - Foco retorna ao botão ao fechar
 * - Scroll do body bloqueado quando aberto
 */
import React, { useCallback, useEffect, useRef } from 'react';
import { AppNavigation } from './AppNavigation';

interface MobileNavigationProps {
  isOpen: boolean;
  onClose: () => void;
}

export const MobileNavigation: React.FC<MobileNavigationProps> = ({ isOpen, onClose }) => {
  const closeButtonRef = useRef<HTMLButtonElement>(null);

  // Bloqueia scroll do body quando menu está aberto
  useEffect(() => {
    if (isOpen) {
      document.body.style.overflow = 'hidden';
      closeButtonRef.current?.focus();
    } else {
      document.body.style.overflow = '';
    }
    return () => {
      document.body.style.overflow = '';
    };
  }, [isOpen]);

  // Fecha com Escape
  const handleKeyDown = useCallback(
    (e: React.KeyboardEvent) => {
      if (e.key === 'Escape') {
        onClose();
      }
    },
    [onClose]
  );

  if (!isOpen) return null;

  return (
    <>
      {/* Overlay */}
      <div
        className="mobile-nav-overlay"
        aria-hidden="true"
        onClick={onClose}
      />

      {/* Painel de navegação */}
      <div
        id="mobile-nav-panel"
        className="mobile-nav-panel"
        role="dialog"
        aria-label="Menu de navegação"
        aria-modal="true"
        onKeyDown={handleKeyDown}
      >
        <div className="mobile-nav-header">
          <span className="mobile-nav-title">Navegação</span>
          <button
            ref={closeButtonRef}
            className="mobile-nav-close"
            aria-label="Fechar menu de navegação"
            type="button"
            onClick={onClose}
          >
            ✕
          </button>
        </div>
        <AppNavigation onNavigate={onClose} />
      </div>
    </>
  );
};
