import React from 'react';
import { DataTable, StatusBadge, RowActions, EyeIcon, EditIcon, Button, Badge } from '../../../components/ui';
import type { Column } from '../../../components/ui/DataTable/DataTable';
import { ROUTES, createPath } from '../../../app/routes/routePaths';
import { useNavigate, useLocation } from 'react-router-dom';
import type { CooperadoListDto } from '../types/cooperados.types';

interface CooperadosTableProps {
  cooperados: CooperadoListDto[];
  isLoading: boolean;
  sortBy?: string;
  direction?: 'asc' | 'desc';
  onSort: (column: string) => void;
  hasActiveFilters?: boolean;
  onClearFilters?: () => void;
  podeAlterar?: boolean;
}

export const CooperadosTable: React.FC<CooperadosTableProps> = ({
  cooperados,
  isLoading,
  sortBy,
  direction,
  onSort,
  hasActiveFilters,
  onClearFilters,
  podeAlterar = false,
}) => {
  const navigate = useNavigate();
  const location = useLocation();

  const handleVerDetalhes = (id: string) => {
    navigate(createPath(ROUTES.COOPERADOS_DETALHES, { id }), {
      state: { fromListagem: true, search: location.search },
    });
  };

  const handleEditar = (id: string) => {
    // Para edição pode ser mesma rota de detalhe ou uma especifica, deixo pronta:
    // navigate(createPath(ROUTES.COOPERADOS_EDITAR, { id }));
    navigate(`${createPath(ROUTES.COOPERADOS_DETALHES, { id })}/editar`);
  };

  const columns: Column<CooperadoListDto>[] = [
    {
      key: 'codigo',
      label: 'Código',
      sortable: true,
      render: (item) => <span className="text-texto-secundario text-sm font-medium">{item.codigo || '-'}</span>,
    },
    {
      key: 'nome',
      label: 'Nome',
      sortable: true,
      render: (item) => <span className="font-semibold text-texto-primario">{item.nome}</span>,
    },
    {
      key: 'cpfMascarado',
      label: 'CPF',
      render: (item) => <span className="text-texto-secundario text-sm">{item.cpfMascarado}</span>,
    },
    {
      key: 'contato',
      label: 'Contato',
      render: (item) => (
        <div className="flex flex-col text-sm text-texto-secundario">
          {item.telefone && <span>{item.telefone}</span>}
          {item.email && <span>{item.email}</span>}
          {!item.telefone && !item.email && <span>-</span>}
        </div>
      ),
    },
    {
      key: 'tipo',
      label: 'Tipo',
      sortable: true,
      render: (item) => (
        <Badge variant={item.tipo === 1 ? 'info' : 'warning'}>
          {item.tipo === 1 ? 'Cooperado' : 'Coordenador'}
        </Badge>
      ),
    },
    {
      key: 'status',
      label: 'Status',
      sortable: true,
      render: (item) => <StatusBadge status={item.desativado ? 'inativo' : 'ativo'} />,
    },
    {
      key: 'acoes',
      label: 'Ações',
      align: 'right',
      render: (item) => (
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
      ),
    },
  ];

  return (
    <DataTable
      data={cooperados}
      columns={columns}
      keyExtractor={(item) => item.publicId}
      isLoading={isLoading}
      sortBy={sortBy}
      direction={direction}
      onSort={onSort}
      aria-label="Lista de cooperados"
      emptyTitle={hasActiveFilters ? 'Nenhum cooperado encontrado' : 'Nenhum cooperado cadastrado'}
      emptyDescription={
        hasActiveFilters
          ? 'Não encontramos nenhum cooperado com os filtros informados.'
          : 'Ainda não existem cooperados ou coordenadores cadastrados na plataforma.'
      }
      emptyAction={
        hasActiveFilters && onClearFilters ? (
          <Button onClick={onClearFilters}>
            Limpar filtros
          </Button>
        ) : undefined
      }
    />
  );
};
