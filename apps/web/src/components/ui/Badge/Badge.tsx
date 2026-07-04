import React from 'react';
import './Badge.css';

export type BadgeVariant = 'neutral' | 'success' | 'warning' | 'error' | 'info' | 'brand';

export interface BadgeProps extends React.HTMLAttributes<HTMLSpanElement> {
  variant?: BadgeVariant;
  dot?: boolean;
}

export const Badge: React.FC<BadgeProps> = ({
  variant = 'neutral',
  dot = false,
  children,
  className = '',
  ...props
}) => {
  return (
    <span className={`badge badge-${variant} ${className}`} {...props}>
      {dot && <span className="badge-dot" aria-hidden="true" />}
      {children}
    </span>
  );
};
export default Badge;
