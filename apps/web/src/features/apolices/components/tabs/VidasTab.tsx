import React, { useState, useMemo } from 'react';
import { useApoliceVidas } from '../../hooks/useApoliceVidas';
import { DataTable, StatusBadge, Pagination, Alert, Button, EmptyState, ConfirmDialog, Badge } from '../../../../components/ui';
import type { Column } from '../../../../components/ui/DataTable/DataTable';
import type { ApoliceVidaListItem, ApoliceVidaQuery } from '../../types/apolice.types';
import { useAuthorization } from '../../../../auth/AuthorizationProvider';
import { ApoliceVidaFormModal } from './ApoliceVidaFormModal';
import { inativarApoliceVida, criarApoliceVida, atualizarApoliceVida } from '../../api/apolices.api';

interface VidasTabProps {
  publicId: string;
}

export const VidasTab: React.FC<VidasTabProps> = ({ publicId }) => {
  const [query, setQuery] = useState<ApoliceVidaQuery>({ page: 1, pageSize: 10 });
  const { data, isLoading, error, retry } = useApoliceVidas(publicId, query);
  const { possuiPermissao } = useAuthorization();

  const podeInserir = possuiPermissao('apolices.vidas.inserir');
  const podeAlterar = possuiPermissao('apolices.vidas.alterar');
  const podeInativar = possuiPermissao('apolices.vidas.inativar');

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [selectedVida, setSelectedVida] = useState<ApoliceVidaListItem | undefined>(undefined);

  const [confirmDialog, setConfirmDialog] = useState<{ isOpen: boolean; vidaId?: string; isLoading: boolean }>({
    isOpen: false,
    isLoading: false,
  });

  const handlePageChange = (newPage: number) => setQuery(prev => ({ ...prev, page: newPage }));

  const handleAdd = () => {
    setSelectedVida(undefined);
    setIsModalOpen(true);
  };

  const handleEdit = (vida: ApoliceVidaListItem) => {
    setSelectedVida(vida);
    setIsModalOpen(true);
  };

  const handleEncerrarClick = (vidaId: string) => {
    setConfirmDialog({ isOpen: true, vidaId, isLoading: false });
  };

  const confirmarEncerramento = async () => {
    if (!confirmDialog.vidaId) return;
    setConfirmDialog(prev => ({ ...prev, isLoading: true }));
    try {
      await inativarApoliceVida(publicId, confirmDialog.vidaId);
      setConfirmDialog({ isOpen: false, isLoading: false });
      retry(); // Recarrega a tabela
    } catch (err) {
      console.error(err);
      alert('Erro ao encerrar participação.');
      setConfirmDialog(prev => ({ ...prev, isLoading: false }));
    }
  };

  const handleFormSubmit = async (formData: import('../../schemas/apoliceVida.schema').ApoliceVidaFormValues) => {
    setIsSubmitting(true);
    try {
      if (selectedVida) {
        // Edit mode (enviar apenas o permitido no payload)
        await atualizarApoliceVida(publicId, selectedVida.apoliceVidaPublicId, {
          dataInicioVigencia: formData.dataInicioVigencia || null,
          dataFimVigencia: formData.dataFimVigencia || null,
          observacao: formData.observacao || null
        });
      } else {
        // Create mode
        await criarApoliceVida(publicId, {
          clientePublicId: formData.clientePublicId,
          subestipulantePublicId: formData.contexto === 'direto' ? null : formData.subestipulantePublicId,
          moduloPublicId: formData.contexto === 'modulo' ? formData.moduloPublicId : null,
          dataInicioVigencia: formData.dataInicioVigencia || null,
          dataFimVigencia: formData.dataFimVigencia || null,
          observacao: formData.observacao || null
        });
      }
      setIsModalOpen(false);
      retry();
    } catch (err) {
      console.error(err);
      alert('Erro ao salvar a participação de vida.');
    } finally {
      setIsSubmitting(false);
    }
  };

  const columns = useMemo<Column<ApoliceVidaListItem>[]>(() => {
    const cols: Column<ApoliceVidaListItem>[] = [
      {
        key: 'cliente',
        label: 'Cliente',
        render: (vida) => (
          <div className="flex flex-col">
            <span className="font-medium text-texto-principal">{vida.clienteNome}</span>
            {vida.clienteDocumentoMascarado && <span className="text-xs text-texto-terciario">{vida.clienteDocumentoMascarado}</span>}
          </div>
        ),
      },
      {
        key: 'contexto',
        label: 'Contexto',
        render: (vida) => {
          if (vida.contexto === 'direto') {
            return <Badge variant="neutral">Direto na Apólice</Badge>;
          }
          if (vida.contexto === 'subestipulante') {
            return (
              <div className="flex flex-col">
                <span className="text-xs font-medium text-texto-secundario uppercase">Subestipulante</span>
                <span className="text-sm text-texto-principal">{vida.subestipulanteNome}</span>
              </div>
            );
          }
          return (
            <div className="flex flex-col">
              <span className="text-xs font-medium text-texto-secundario uppercase">Subestipulante</span>
              <span className="text-sm text-texto-principal mb-1">{vida.subestipulanteNome}</span>
              <div className="flex items-center text-xs text-texto-terciario">
                <span className="mr-1">&rarr;</span> Módulo: <span className="font-medium ml-1 text-texto-secundario">{vida.moduloNome}</span>
              </div>
            </div>
          );
        },
      },
      {
        key: 'vigencia',
        label: 'Vigência',
        render: (vida) => (
          <span className="text-texto-secundario text-sm">
            {vida.dataInicioVigencia ? new Date(vida.dataInicioVigencia).toLocaleDateString('pt-BR') : '—'} 
            {' até '} 
            {vida.dataFimVigencia ? new Date(vida.dataFimVigencia).toLocaleDateString('pt-BR') : '—'}
          </span>
        ),
      },
      {
        key: 'status',
        label: 'Status',
        render: (vida) => <StatusBadge status={vida.ativo ? 'ativo' : 'inativo'} label={vida.status || (vida.ativo ? 'Ativa' : 'Encerrada')} />,
      },
    ];

    if (podeAlterar || podeInativar) {
      cols.push({
        key: 'acoes',
        label: '',
        render: (vida) => (
          <div className="flex items-center justify-end gap-2">
            {podeAlterar && (
              <Button variant="ghost" size="small" onClick={() => handleEdit(vida)}>Editar</Button>
            )}
            {podeInativar && vida.ativo && (
              <Button variant="ghost" size="small" className="text-error hover:bg-error/10" onClick={() => handleEncerrarClick(vida.apoliceVidaPublicId)}>
                Encerrar
              </Button>
            )}
          </div>
        ),
      });
    }

    return cols;
  }, [podeAlterar, podeInativar]);

  if (error) {
    return (
      <div className="flex flex-col gap-4 items-start">
        <Alert variant="error" title="Erro ao carregar as vidas">
          {error.message}
        </Alert>
        <Button onClick={retry} size="small" loading={isLoading}>
          Tentar novamente
        </Button>
      </div>
    );
  }

  const hasData = data && data.items && data.items.length > 0;

  return (
    <div className="flex flex-col gap-4">
      {podeInserir && (
        <div className="flex justify-end">
          <Button variant="primary" onClick={handleAdd}>
            Adicionar Vida
          </Button>
        </div>
      )}

      {(!isLoading && !hasData) ? (
        <EmptyState
          title="Nenhuma vida encontrada"
          description="Esta apólice não possui beneficiários ou vidas cadastradas."
        />
      ) : (
        <>
          <DataTable
            data={data?.items || []}
            columns={columns}
            keyExtractor={(item) => item.apoliceVidaPublicId}
            isLoading={isLoading}
            aria-label="Lista de Vidas da Apólice"
          />
          
          {data && Math.ceil(data.totalCount / data.pageSize) > 1 && (
            <div className="flex justify-center md:justify-end mt-4 pt-4 border-t border-borda">
              <Pagination
                currentPage={data.page}
                totalPages={Math.ceil(data.totalCount / data.pageSize)}
                onPageChange={handlePageChange}
                disabled={isLoading}
              />
            </div>
          )}
        </>
      )}

      <ApoliceVidaFormModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        onSubmit={handleFormSubmit}
        apolicePublicId={publicId}
        initialData={selectedVida}
        isSubmitting={isSubmitting}
      />

      <ConfirmDialog
        aberto={confirmDialog.isOpen}
        title="Encerrar Participação"
        description="Deseja encerrar esta participação na Apólice? O Cadastro Global do Cliente será preservado e a participação continuará disponível no histórico."
        confirmText="Encerrar"
        cancelText="Cancelar"
        variant="danger"
        loading={confirmDialog.isLoading}
        onConfirm={confirmarEncerramento}
        onClose={() => setConfirmDialog({ isOpen: false, isLoading: false })}
      />
    </div>
  );
};
