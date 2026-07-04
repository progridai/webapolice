import React from 'react';
import { useNavigate } from 'react-router-dom';
import { Card, CardHeader, CardContent, Button } from '../../../components/ui';
import { EyeIcon } from '../../../components/ui/Icons';
import { ROUTES, createPath } from '../../../app/routes/routePaths';
import type { ClienteListItem } from '../types/cliente.types';
import { ClienteStatusBadge } from './ClienteStatusBadge';

interface ClientesMobileListProps {
  clientes: ClienteListItem[];
}

export const ClientesMobileList: React.FC<ClientesMobileListProps> = ({ clientes }) => {
  const navigate = useNavigate();

  return (
    <div className="flex flex-col gap-4">
      {clientes.map((cliente) => (
        <Card key={cliente.id} className="w-full">
          <CardHeader className="pb-2 flex-row justify-between items-start">
            <div>
              <h3 className="font-semibold text-lg">{cliente.nome}</h3>
              <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">
                {cliente.cpfMascarado}
              </p>
            </div>
            <ClienteStatusBadge status={cliente.status} />
          </CardHeader>
          <CardContent className="pt-0">
            <div className="flex justify-between items-end mt-4">
              <span className="text-xs text-gray-400">
                Cadastrado em {new Date(cliente.dataCadastroUtc).toLocaleDateString('pt-BR')}
              </span>
              <Button
                variant="outline"
                size="sm"
                onClick={() => navigate(createPath(ROUTES.CLIENTE_DETALHES, { id: String(cliente.id) }))}
                aria-label={`Visualizar detalhes de ${cliente.nome}`}
                icon={<EyeIcon />}
              >
                Detalhes
              </Button>
            </div>
          </CardContent>
        </Card>
      ))}
    </div>
  );
};
