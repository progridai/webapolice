import React, { useState } from 'react';
import { Card, CardHeader, CardContent, RowActions, StatusBadge, XCircleIcon, CheckCircleIcon, ConfirmDialog, EyeIcon, EditIcon } from '../../../components/ui';
import type { EstipulanteListItem } from '../types/estipulante.types';

interface EstipulantesMobileListProps {
  estipulantes: EstipulanteListItem[];
  podeInativar?: boolean;
  podeReativar?: boolean;
  podeAlterar?: boolean;
  onInativar?: (publicId: string) => void;
  onReativar?: (publicId: string) => void;
  onEditar?: (publicId: string) => void;
  onDetalhar?: (publicId: string) => void;
}

export const EstipulantesMobileList: React.FC<EstipulantesMobileListProps> = ({ 
  estipulantes, 
  podeInativar = false,
  podeReativar = false,
  podeAlterar = false,
  onInativar,
  onReativar,
  onEditar,
  onDetalhar
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

  return (
    <>
      <div className="flex flex-col gap-4">
        {estipulantes.map((est) => (
          <Card key={est.publicId} className="w-full">
            <CardHeader className="pb-2 flex-row justify-between items-start">
              <div>
                <h3 className="font-semibold text-lg" style={{ color: 'var(--color-text-primary)' }}>{est.razaoSocial}</h3>
                {est.nomeFantasia && (
                  <p className="text-sm mt-1" style={{ color: 'var(--color-text-secondary)' }}>
                    {est.nomeFantasia}
                  </p>
                )}
                <p className="text-sm mt-1" style={{ color: 'var(--color-text-secondary)' }}>
                  {formatCnpj(est.cnpj)}
                  {est.codigo && ` • ${est.codigo}`}
                </p>
                {est.grupo && (
                  <p className="text-sm mt-1" style={{ color: 'var(--color-text-tertiary)' }}>
                    Grupo: {est.grupo}
                  </p>
                )}
              </div>
              <StatusBadge status={est.ativo ? 'ativo' : 'inativo'} />
            </CardHeader>
            <CardContent className="pt-0">
              <div className="flex justify-between items-end mt-4">
                <span className="text-xs" style={{ color: 'var(--color-text-tertiary)' }}>
                  Cadastrado em {new Date(est.dataCadastro).toLocaleDateString('pt-BR')}
                </span>
                
                {(onDetalhar || (podeAlterar && onEditar) || ((est.ativo && podeInativar) || (!est.ativo && podeReativar))) && (
                  <RowActions
                    primaryAction={onDetalhar ? {
                      label: 'Detalhes',
                      icon: <EyeIcon />,
                      onClick: () => onDetalhar(est.publicId),
                    } : undefined}
                    actions={[
                      ...(podeAlterar && onEditar ? [{
                        label: 'Editar',
                        icon: <EditIcon />,
                        onClick: () => onEditar(est.publicId),
                      }] : []),
                      ...(est.ativo && podeInativar ? [{
                        label: 'Inativar',
                        icon: <XCircleIcon />,
                        onClick: () => handleInativarClick(est),
                        danger: true,
                      }] : []),
                      ...(!est.ativo && podeReativar ? [{
                        label: 'Reativar',
                        icon: <CheckCircleIcon />,
                        onClick: () => handleReativarClick(est),
                      }] : []),
                    ]}
                    ariaLabel={`Ações para ${est.razaoSocial}`}
                  />
                )}
              </div>
            </CardContent>
          </Card>
        ))}
      </div>

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
