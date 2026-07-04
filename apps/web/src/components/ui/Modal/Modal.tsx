import React, { useEffect, useRef } from 'react';
import './Modal.css';

export type ModalSize = 'small' | 'medium' | 'large';

export interface ModalProps extends React.HTMLAttributes<HTMLDivElement> {
  aberto: boolean;
  onClose: () => void;
  title: string;
  size?: ModalSize;
  children: React.ReactNode;
  footer?: React.ReactNode;
}

export const Modal: React.FC<ModalProps> = ({
  aberto,
  onClose,
  title,
  size = 'medium',
  children,
  footer,
  className = '',
  ...props
}) => {
  const previousActiveElement = useRef<HTMLElement | null>(null);
  const modalRef = useRef<HTMLDivElement>(null);
  const closeButtonRef = useRef<HTMLButtonElement>(null);

  // ID do título do modal para acessibilidade aria-labelledby
  const titleId = React.useId();

  // Controlar foco e rolagem do corpo
  useEffect(() => {
    if (aberto) {
      // Salva o elemento com foco atual para retornar depois
      previousActiveElement.current = document.activeElement as HTMLElement;

      // Bloqueia rolagem do body
      const originalOverflow = document.body.style.overflow;
      document.body.style.overflow = 'hidden';

      // Coloca foco inicial no botão de fechar ou no próprio contêiner do modal
      if (closeButtonRef.current) {
        closeButtonRef.current.focus();
      } else if (modalRef.current) {
        modalRef.current.focus();
      }

      // Listener para tecla Escape e armadilha de foco básica
      const handleKeyDown = (e: KeyboardEvent) => {
        if (e.key === 'Escape') {
          onClose();
        }

        // Armadilha de foco (Tab loop)
        if (e.key === 'Tab' && modalRef.current) {
          const focusableElements = modalRef.current.querySelectorAll(
            'a[href], area[href], input:not([disabled]), select:not([disabled]), textarea:not([disabled]), button:not([disabled]), iframe, object, embed, [tabindex="0"], [contenteditable]'
          );
          
          if (focusableElements.length > 0) {
            const firstElement = focusableElements[0] as HTMLElement;
            const lastElement = focusableElements[focusableElements.length - 1] as HTMLElement;

            if (e.shiftKey) {
              if (document.activeElement === firstElement) {
                lastElement.focus();
                e.preventDefault();
              }
            } else {
              if (document.activeElement === lastElement) {
                firstElement.focus();
                e.preventDefault();
              }
            }
          }
        }
      };

      window.addEventListener('keydown', handleKeyDown);

      return () => {
        // Restaura rolagem
        document.body.style.overflow = originalOverflow;
        window.removeEventListener('keydown', handleKeyDown);
        
        // Retorna o foco ao elemento original
        if (previousActiveElement.current) {
          previousActiveElement.current.focus();
        }
      };
    }
  }, [aberto, onClose]);

  if (!aberto) return null;

  return (
    <div
      className="modal-backdrop-component"
      onClick={onClose}
      role="presentation"
    >
      <div
        ref={modalRef}
        className={`modal-container-component modal-size-${size} ${className}`}
        onClick={(e) => e.stopPropagation()}
        role="dialog"
        aria-modal="true"
        aria-labelledby={titleId}
        tabIndex={-1}
        {...props}
      >
        <div className="modal-header-component">
          <h2 id={titleId} className="modal-title-text">
            {title}
          </h2>
          <button
            ref={closeButtonRef}
            type="button"
            className="modal-close-btn-component"
            onClick={onClose}
            aria-label="Fechar Modal"
          >
            ×
          </button>
        </div>
        <div className="modal-body-component">{children}</div>
        {footer && <div className="modal-footer-component">{footer}</div>}
      </div>
    </div>
  );
};
export default Modal;
