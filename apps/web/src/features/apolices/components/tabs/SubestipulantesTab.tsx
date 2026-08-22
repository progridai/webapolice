import React, { useState } from 'react';
import { useApoliceSubestipulantes } from '../../hooks/useApoliceSubestipulantes';
import { DataTable, StatusBadge, Alert, Button, EmptyState, ConfirmDialog } from '../../../../components/ui';
import type { Column } from '../../../../components/ui/DataTable/DataTable';
import type { ApoliceSubestipulanteResult } from '../../types/apolice.types';
import { useAuthorization } from '../../../../auth/AuthorizationProvider';
import { SubestipulanteApoliceModal } from '../modals/SubestipulanteApoliceModal';
import { SubestipulanteModulos } from '../modulos/SubestipulanteModulos';
import { inativarSubestipulanteApolice } from '../../api/apolices.api';

interface SubestipulantesTabProps {
  publicId: string;
}

export const SubestipulantesTab: React.FC<SubestipulantesTabProps> = ({ publicId }) => {
  const { data, isLoading, error, refetch } = useApoliceSubestipulantes(publicId);
  const { possuiPermissao } = useAuthorization();

  const podeInserir = possuiPermissao('apolices.subestipulantes.inserir');
  const podeAlterar = possuiPermissao('apolices.subestipulantes.alterar');
  const podeInativar = possuiPermissao('apolices.subestipulantes.inativar');

  const [modalAberto, setModalAberto] = useState(false);
  const [subParaEditar, setSubParaEditar] = useState<ApoliceSubestipulanteResult | undefined>();
  
  const [inativarSubId, setInativarSubId] = useState<string | null>(null);
  const [inativando, setInativando] = useState(false);
  const [inativarErro, setInativarErro] = useState<string | null>(null);

  const handleAdicionar = () => {
    setSubParaEditar(undefined);
    setModalAberto(true);
  };

  const handleEditar = (item: ApoliceSubestipulanteResult) => {
    setSubParaEditar(item);
    setModalAberto(true);
  };

  const confirmarInativacao = async () => {
    if (!inativarSubId) return;
    try {
      setInativando(true);
      setInativarErro(null);
      await inativarSubestipulanteApolice(publicId, inativarSubId);
      setInativarSubId(null);
      refetch();
    } catch (err: any) {
      setInativarErro(err.response?.data?.message || 'Ocorreu um erro ao inativar o vínculo.');
    } finally {
      setInativando(false);
    }
  };

  if (error) {
    return (
      <div className="flex flex-col gap-4 items-start">
        <Alert variant="error" title="Erro ao carregar subestipulantes">
          {error.message}
        </Alert>
        <Button onClick={refetch} size="small" loading={isLoading}>
          Tentar novamente
        </Button>
      </div>
    );
  }

  const columns: Column<ApoliceSubestipulanteResult>[] = [
    {
      key: 'subestipulante',
      label: 'Subestipulante',
      render: (item) => (
        <div className="flex flex-col">
          <span className="font-medium text-texto-principal">{item.nome}</span>
          <span className="text-sm text-texto-secundario">{item.documento || item.codigo || '—'}</span>
        </div>
      ),
    },
    {
      key: 'vigencia',
      label: 'Vigência do Vínculo',
      render: (item) => {
        const inicio = item.dataInicio ? new Date(item.dataInicio).toLocaleDateString('pt-BR', { timeZone: 'UTC' }) : '—';
        const fim = item.dataFim ? new Date(item.dataFim).toLocaleDateString('pt-BR', { timeZone: 'UTC' }) : '—';
        return <span className="text-texto-secundario">{inicio} até {fim}</span>;
      },
    },
    {
      key: 'modulos',
      label: 'Módulos',
      render: (item) => {
        const count = item.modulos?.length || 0;
        return (
          <span className="text-sm text-texto-secundario">
            {count === 0 ? 'Sem módulos' : `${count} módulo(s)`}
          </span>
        );
      },
    },
    {
      key: 'status',
      label: 'Status do Vínculo',
      render: (item) => <StatusBadge status={item.ativo ? 'ativo' : 'inativo'} label={item.ativo ? 'Ativo' : 'Inativo'} />,
    },
    {
      key: 'acoes',
      label: 'Ações',
      align: 'right',
      render: (item) => (
        <div className="flex gap-2 justify-end">
          {podeAlterar && item.ativo && (
            <Button variant="secondary" size="small" onClick={() => handleEditar(item)}>
              Editar
            </Button>
          )}
          {podeInativar && item.ativo && (
            <Button variant="danger" size="small" onClick={() => setInativarSubId(item.subestipulantePublicId)}>
              Inativar
            </Button>
          )}
        </div>
      ),
    },
  ];

  return (
    <div className="flex flex-col gap-4">
      <div className="flex justify-between items-center mb-2">
        <div>
          <h2 className="text-xl font-semibold text-texto-principal">Subestipulantes Vinculados</h2>
          <p className="text-sm text-texto-terciario mt-1">Gerencie os vínculos de subestipulantes da apólice.</p>
        </div>
        {podeInserir && (
          <Button onClick={handleAdicionar} variant="primary">
            Adicionar Subestipulante
          </Button>
        )}
      </div>

      {inativarErro && (
        <Alert variant="error" title="Atenção" onClose={() => setInativarErro(null)}>
          {inativarErro}
        </Alert>
      )}

      {(!data || data.length === 0) && !isLoading ? (
        <EmptyState
          title="Nenhum Subestipulante"
          description="Nenhum Subestipulante vinculado a esta Apólice."
        />
      ) : (
        <DataTable
          data={data || []}
          columns={columns}
          keyExtractor={(item) => item.subestipulantePublicId}
          isLoading={isLoading}
          renderExpandedRow={(item) => (
            <SubestipulanteModulos 
              apolicePublicId={publicId} 
              subestipulante={item} 
              onRefresh={refetch} 
            />
          )}
          aria-label="Lista de Subestipulantes da Apólice"
        />
      )}

      {modalAberto && (
        <SubestipulanteApoliceModal
          aberto={modalAberto}
          onClose={() => setModalAberto(false)}
          apolicePublicId={publicId}
          subestipulanteEdicao={subParaEditar}
          onSucesso={refetch}
        />
      )}

      <ConfirmDialog
        aberto={!!inativarSubId}
        onClose={() => !inativando && setInativarSubId(null)}
        onConfirm={confirmarInativacao}
        title="Inativar Vínculo"
        description="Deseja inativar o vínculo deste Subestipulante com a Apólice? O Cadastro Global do Subestipulante será preservado."
        confirmText="Inativar Vínculo"
        variant="danger"
        loading={inativando}
      />
    </div>
  );
};
