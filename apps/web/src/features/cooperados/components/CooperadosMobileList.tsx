import React from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { Card, CardHeader, CardContent, RowActions, EyeIcon, EditIcon, StatusBadge, Badge } from '../../../components/ui';
import { ROUTES, createPath } from '../../../app/routes/routePaths';
import type { CooperadoListDto } from '../types/cooperados.types';

interface CooperadosMobileListProps {
  cooperados: CooperadoListDto[];
  podeAlterar?: boolean;
}

export const CooperadosMobileList: React.FC<CooperadosMobileListProps> = ({ cooperados, podeAlterar = false }) => {
  const navigate = useNavigate();
  const location = useLocation();

  const handleVerDetalhes = (id: string) => {
    navigate(createPath(ROUTES.COOPERADOS_DETALHES, { id }), {
      state: { fromListagem: true, search: location.search },
    });
  };

  const handleEditar = (id: string) => {
    navigate(`${createPath(ROUTES.COOPERADOS_DETALHES, { id })}/editar`);
  };

  return (
    <div className="flex flex-col gap-4">
      {cooperados.map((item) => (
        <Card key={item.publicId} className="w-full">
          <CardHeader className="pb-2 flex-row justify-between items-start">
            <div>
              <div className="flex items-center gap-2 mb-1">
                <Badge variant={item.tipo === 1 ? 'info' : 'warning'}>
                  {item.tipo === 1 ? 'Cooperado' : 'Coordenador'}
                </Badge>
                {item.codigo && (
                  <span className="text-xs font-semibold text-texto-secundario">
                    #{item.codigo}
                  </span>
                )}
              </div>
              <h3 className="font-semibold text-texto-primario text-lg">{item.nome}</h3>
              <p className="text-sm text-texto-secundario mt-1">
                {item.cpfMascarado}
              </p>
            </div>
            <StatusBadge status={item.statusId} />
          </CardHeader>
          <CardContent className="pt-0">
            <div className="flex flex-col mt-2 mb-4 text-sm text-texto-secundario gap-1">
              {item.telefone && <span>📞 {item.telefone}</span>}
              {item.email && <span>📧 {item.email}</span>}
            </div>

            <div className="flex justify-between items-end border-t border-borda pt-4">
              <span className="text-xs text-texto-secundario">
                Cadastro: {new Date(item.dataCadastroUtc).toLocaleDateString('pt-BR')}
              </span>
              <RowActions
                primaryAction={{
                  label: 'Detalhes',
                  icon: <EyeIcon />,
                  onClick: () => handleVerDetalhes(item.publicId),
                }}
                actions={[
                  ...(podeAlterar ? [{
                    label: 'Editar',
                    icon: <EditIcon />,
                    onClick: () => handleEditar(item.publicId),
                  }] : []),
                ]}
                ariaLabel={`Ações para ${item.nome}`}
              />
            </div>
          </CardContent>
        </Card>
      ))}
    </div>
  );
};
