import React, { useState } from 'react';
import { DataTable, StatusBadge, RowActions, Button, ConfirmDialog, XCircleIcon, CheckCircleIcon, EditIcon, EyeIcon } from '../../../components/ui';
import type { Column } from '../../../components/ui/DataTable/DataTable';
import type { EstipulanteListItem } from '../types/estipulante.types';

interface EstipulantesTableProps {
  estipulantes: EstipulanteListItem[];
  isLoading: boolean;
  sortBy?: string;
  direction?: 'asc' | 'desc';
  onSort: (column: string) => void;
  hasActiveFilters?: boolean;
  onClearFilters?: () => void;
  podeInativar?: boolean;
  podeReativar?: boolean;
  podeAlterar?: boolean;
  onInativar?: (publicId: string) => void;
  onReativar?: (publicId: string) => void;
  onEditar?: (publicId: string) => void;
  onDetalhar?: (publicId: string) => void;
}

export const EstipulantesTable: React.FC<EstipulantesTableProps> = ({
  estipulantes,
  isLoading,
  sortBy,
  direction,
  onSort,
  hasActiveFilters,
  onClearFilters,
  podeInativar = false,
  podeReativar = false,
  podeAlterar = false,
  onInativar,
  onReativar,
  onEditar,
  onDetalhar,
}) => {
  const [confirmDialog, setConfirmDialog] = useState<{
    isOpen: boolean;
    action: 'inativar' | 'reativar';
    estipulante: EstipulanteListItem | null;
  }>({ isOpen: false, action: 'inativar', estipulante: null });

  const formatCnpj = (cnpj: string) => {
    if (!cnpj) return '';
    const unmasked = cnpj.replace(/\D/g, '');
    if (unmasked.length === 14) {
      return unmasked.replace(/^(\d{2})(\d{3})(\d{3})(\d{4})(\d{2})$/, '$1.$2.$3/$4-$5');
    }
    return cnpj;
  };

  const handleInativarClick = (estipulante: EstipulanteListItem) => {
    setConfirmDialog({ isOpen: true, action: 'inativar', estipulante });
  };

  const handleReativarClick = (estipulante: EstipulanteListItem) => {
    setConfirmDialog({ isOpen: true, action: 'reativar', estipulante });
  };

  const confirmAction = () => {
    if (!confirmDialog.estipulante) return;
    
    if (confirmDialog.action === 'inativar' && onInativar) {
      onInativar(confirmDialog.estipulante.publicId);
    } else if (confirmDialog.action === 'reativar' && onReativar) {
      onReativar(confirmDialog.estipulante.publicId);
    }
    
    setConfirmDialog({ isOpen: false, action: 'inativar', estipulante: null });
  };

  const columns: Column<EstipulanteListItem>[] = [
    {
      key: 'nome',
      label: 'Estipulante',
      sortable: true,
      render: (est) => (
        <div style={{ display: 'flex', flexDirection: 'column' }}>
          <span style={{ fontWeight: 500, color: 'var(--color-text-primary)' }}>{est.razaoSocial}</span>
          {est.nomeFantasia && (
            <span style={{ fontSize: '0.875rem', color: 'var(--color-text-tertiary)' }}>{est.nomeFantasia}</span>
          )}
        </div>
      ),
    },
    {
      key: 'cnpj',
      label: 'CNPJ',
      render: (est) => <span style={{ color: 'var(--color-text-secondary)' }}>{formatCnpj(est.cnpj)}</span>,
    },
    {
      key: 'codigo',
      label: 'Código',
      sortable: true,
      render: (est) => est.codigo ? <span style={{ color: 'var(--color-text-tertiary)' }}>{est.codigo}</span> : <span>&mdash;</span>,
    },
    {
      key: 'grupo',
      label: 'Grupo',
      render: (est) => est.grupo ? <span>{est.grupo}</span> : <span>&mdash;</span>,
    },
    {
      key: 'status',
      label: 'Status',
      sortable: true,
      render: (est) => <StatusBadge status={est.ativo ? 'ativo' : 'inativo'} />,
    },
    {
      key: 'acoes',
      label: 'Ações',
      align: 'right',
      render: (est) => {
        const actions = [];
        if (podeAlterar && onEditar) {
          actions.push({
            label: 'Editar',
            icon: <EditIcon />,
            onClick: () => onEditar(est.publicId),
          });
        }
        if (est.ativo && podeInativar) {
          actions.push({
            label: 'Inativar',
            icon: <XCircleIcon />,
            onClick: () => handleInativarClick(est),
            danger: true,
          });
        }
        if (!est.ativo && podeReativar) {
          actions.push({
            label: 'Reativar',
            icon: <CheckCircleIcon />,
            onClick: () => handleReativarClick(est),
          });
        }
        
        return (
          <RowActions
            primaryAction={onDetalhar ? {
              label: 'Detalhes',
              icon: <EyeIcon />,
              onClick: () => onDetalhar(est.publicId),
            } : undefined}
            actions={actions}
            ariaLabel={`Ações para ${est.razaoSocial}`}
          />
        );
      },
    },
  ];

  return (
    <>
      <DataTable
        data={estipulantes}
        columns={columns}
        keyExtractor={(item) => item.publicId}
        isLoading={isLoading}
        sortBy={sortBy}
        direction={direction}
        onSort={onSort}
        aria-label="Lista de estipulantes"
        emptyTitle={hasActiveFilters ? 'Nenhum estipulante encontrado' : 'Nenhum estipulante cadastrado'}
        emptyDescription={
          hasActiveFilters
            ? 'Não encontramos nenhum estipulante com os filtros informados.'
            : 'Ainda não existem estipulantes cadastrados na plataforma.'
        }
        emptyAction={
          hasActiveFilters && onClearFilters ? (
            <Button onClick={onClearFilters}>
              Limpar filtros
            </Button>
          ) : undefined
        }
      />

      <ConfirmDialog
        isOpen={confirmDialog.isOpen}
        title={confirmDialog.action === 'inativar' ? 'Inativar Estipulante?' : 'Reativar Estipulante?'}
        description={
          confirmDialog.action === 'inativar'
            ? `O estipulante ${confirmDialog.estipulante?.razaoSocial} deixará de ficar disponível para uso operacional, mas seus dados serão preservados.`
            : `O estipulante ${confirmDialog.estipulante?.razaoSocial} voltará a ficar disponível para uso operacional.`
        }
        confirmLabel={confirmDialog.action === 'inativar' ? 'Inativar' : 'Reativar'}
        cancelLabel="Cancelar"
        variant={confirmDialog.action === 'inativar' ? 'danger' : 'primary'}
        onConfirm={confirmAction}
        onCancel={() => setConfirmDialog({ isOpen: false, action: 'inativar', estipulante: null })}
      />
    </>
  );
};
