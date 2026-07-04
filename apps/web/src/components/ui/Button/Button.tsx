import React, { forwardRef } from 'react';
import { Spinner } from '../Spinner/Spinner';
import './Button.css';

export type ButtonVariant = 'primary' | 'secondary' | 'text' | 'danger';
export type ButtonSize = 'small' | 'medium' | 'large';

export interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: ButtonVariant;
  size?: ButtonSize;
  loading?: boolean;
}

export const Button = forwardRef<HTMLButtonElement, ButtonProps>(
  (
    {
      variant = 'primary',
      size = 'medium',
      loading = false,
      disabled = false,
      children,
      className = '',
      type = 'button',
      ...props
    },
    ref
  ) => {
    const isButtonDisabled = disabled || loading;

    return (
      <button
        ref={ref}
        type={type}
        disabled={isButtonDisabled}
        className={`btn btn-${variant} btn-${size} ${loading ? 'btn-loading' : ''} ${className}`}
        aria-busy={loading ? 'true' : undefined}
        {...props}
      >
        {loading ? (
          <>
            <Spinner size="small" className="btn-spinner" aria-label="Processando..." />
            <span className="btn-loading-text sr-only">Carregando...</span>
            <span className="btn-content-hidden">{children}</span>
          </>
        ) : (
          children
        )}
      </button>
    );
  }
);

Button.displayName = 'Button';
