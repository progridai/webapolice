import React from 'react';
import { CheckIcon, ErrorIcon, AlertIcon, InfoIcon } from '../Icons';
import './Alert.css';

export type AlertVariant = 'success' | 'error' | 'warning' | 'info';

export interface AlertProps extends Omit<React.HTMLAttributes<HTMLDivElement>, 'title'> {
  variant?: AlertVariant;
  title?: React.ReactNode;
  onClose?: () => void;
}

const variantIcons = {
  success: CheckIcon,
  error: ErrorIcon,
  warning: AlertIcon,
  info: InfoIcon,
};

export const Alert: React.FC<AlertProps> = ({
  variant = 'info',
  title,
  children,
  onClose,
  className = '',
  role,
  ...props
}) => {
  const IconComponent = variantIcons[variant];
  // Por padrão, erro e warning têm role="alert" para leitores de tela
  const resolvedRole = role || (variant === 'error' || variant === 'warning' ? 'alert' : 'status');

  return (
    <div
      className={`alert alert-${variant} ${className}`}
      role={resolvedRole}
      {...props}
    >
      <div className="alert-icon-container">
        <IconComponent className="alert-icon" />
      </div>
      <div className="alert-body">
        {title && <h4 className="alert-title">{title}</h4>}
        <div className="alert-content">{children}</div>
      </div>
      {onClose && (
        <button
          type="button"
          className="alert-close-btn"
          onClick={onClose}
          aria-label="Fechar alerta"
        >
          ×
        </button>
      )}
    </div>
  );
};
export default Alert;
