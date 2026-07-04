import React from 'react';
import './Spinner.css';

export type SpinnerSize = 'small' | 'medium' | 'large';

export interface SpinnerProps extends React.HTMLAttributes<HTMLDivElement> {
  size?: SpinnerSize;
  'aria-label'?: string;
}

export const Spinner: React.FC<SpinnerProps> = ({
  size = 'medium',
  className = '',
  'aria-label': ariaLabel = 'Carregando...',
  ...props
}) => {
  return (
    <div
      className={`spinner spinner-${size} ${className}`}
      role="status"
      aria-label={ariaLabel}
      {...props}
    >
      <span className="sr-only">{ariaLabel}</span>
    </div>
  );
};
