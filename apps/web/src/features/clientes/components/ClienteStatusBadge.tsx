import React from 'react';
import { Badge } from '../../../components/ui';
import type { ClienteStatus } from '../types/cliente.types';

interface ClienteStatusBadgeProps {
  status: ClienteStatus | string;
}

export const ClienteStatusBadge: React.FC<ClienteStatusBadgeProps> = ({ status }) => {
  const normalizedStatus = status.toLowerCase();

  if (normalizedStatus === 'ativo') {
    return <Badge variant="success">Ativo</Badge>;
  }
  
  if (normalizedStatus === 'inativo') {
    return <Badge variant="error">Inativo</Badge>;
  }

  return <Badge variant="neutral">{status}</Badge>;
};
