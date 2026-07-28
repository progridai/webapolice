import React from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { Card, CardHeader, CardContent, RowActions, EyeIcon, EditIcon, StatusBadge } from '../../../components/ui';
import { ROUTES, createPath } from '../../../app/routes/routePaths';
import type { ClienteListItem } from '../types/cliente.types';

interface ClientesMobileListProps {
  clientes: ClienteListItem[];
  podeAlterar?: boolean;
}

export const ClientesMobileList: React.FC<ClientesMobileListProps> = ({ clientes, podeAlterar = false }) => {
  const navigate = useNavigate();
  const location = useLocation();

  const handleVerDetalhes = (id: number) => {
    navigate(createPath(ROUTES.CLIENTE_DETALHES, { id: String(id) }), {
      state: { fromListagem: true, search: location.search },
    });
  };

  const handleEditar = (id: number) => {
    navigate(`/clientes/${id}/editar`);
  };

  return (
    <div className="flex flex-col gap-4">
      {clientes.map((cliente) => (
        <Card key={cliente.id} className="w-full">
          <CardHeader className="pb-2 flex-row justify-between items-start">
            <div>
              <h3 className="font-semibold text-lg">{cliente.nome}</h3>
              <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">
                {cliente.documentoMascarado}
              </p>
            </div>
            <StatusBadge status={cliente.status} />
          </CardHeader>
          <CardContent className="pt-0">
            <div className="flex justify-between items-end mt-4">
              <span className="text-xs text-gray-400">
                Cadastrado em {new Date(cliente.dataCadastroUtc).toLocaleDateString('pt-BR')}
              </span>
              <RowActions
                primaryAction={{
                  label: 'Detalhes',
                  icon: <EyeIcon />,
                  onClick: () => handleVerDetalhes(cliente.id),
                }}
                actions={[
                  ...(podeAlterar ? [{
                    label: 'Editar',
                    icon: <EditIcon />,
                    onClick: () => handleEditar(cliente.id),
                  }] : []),
                ]}
                ariaLabel={`Ações para ${cliente.nome}`}
              />
            </div>
          </CardContent>
        </Card>
      ))}
    </div>
  );
};
