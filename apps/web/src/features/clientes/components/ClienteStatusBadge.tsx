import React from 'react';
import { Badge } from '../../../components/ui';

interface ClienteStatusBadgeProps {
  status: 'Ativo' | 'Inativo' | string;
}

export const ClienteStatusBadge: React.FC<ClienteStatusBadgeProps> = ({ status }) => {
  if (status === 'Ativo') {
    return <Badge variant="success">Ativo</Badge>;
  }
  
  if (status === 'Inativo') {
    return <Badge variant="error">Inativo</Badge>;
  }

  return <Badge variant="neutral">{status}</Badge>;
};
