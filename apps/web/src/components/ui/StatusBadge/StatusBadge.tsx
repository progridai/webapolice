import React from 'react';
import { Badge } from '../Badge';
import type { BadgeVariant } from '../Badge';

export type StatusValue = 'ativo' | 'inativo' | 'pendente' | 'bloqueado' | 'falecido' | 'extinto' | string;

export interface StatusBadgeProps {
  status: StatusValue;
  className?: string;
}

export const StatusBadge: React.FC<StatusBadgeProps> = ({ status, className = '' }) => {
  const normalizedStatus = status.toLowerCase();

  let variant: BadgeVariant;

  switch (normalizedStatus) {
    case 'ativo':
      variant = 'success';
      break;
    case 'inativo':
    case 'bloqueado':
    case 'falecido':
    case 'extinto':
      variant = 'error';
      break;
    case 'pendente':
      variant = 'warning';
      break;
    default:
      variant = 'neutral';
      break;
  }

  // Capitalize the first letter for display
  const displayStatus = status.charAt(0).toUpperCase() + status.slice(1).toLowerCase();

  return (
    <Badge variant={variant} className={className}>
      {displayStatus}
    </Badge>
  );
};
