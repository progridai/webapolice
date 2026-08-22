import React, { useState } from 'react';
import { useAuthorization } from '../../../../auth/AuthorizationProvider';
import { DataTable, StatusBadge, Alert, Button, ConfirmDialog } from '../../../../components/ui';
import type { Column } from '../../../../components/ui/DataTable/DataTable';
import type { ApoliceSubestipulanteModuloResult, ApoliceSubestipulanteResult } from '../../types/apolice.types';
import { ModuloSubestipulanteApoliceModal } from '../modals/ModuloSubestipulanteApoliceModal';
import { inativarModuloSubestipulanteApolice } from '../../api/apolices.api';

interface SubestipulanteModulosProps {
  apolicePublicId: string;
  subestipulante: ApoliceSubestipulanteResult;
  onRefresh: () => void;
}

export const SubestipulanteModulos: React.FC<SubestipulanteModulosProps> = ({
  apolicePublicId,
  subestipulante,
  onRefresh
}) => {
  const { possuiPermissao } = useAuthorization();
  
  const podeInserir = possuiPermissao('apolices.subestipulantes.modulos.inserir');
  const podeAlterar = possuiPermissao('apolices.subestipulantes.modulos.alterar');
  const podeInativar = possuiPermissao('apolices.subestipulantes.modulos.inativar');

  const [modalAberto, setModalAberto] = useState(false);
  const [moduloParaEditar, setModuloParaEditar] = useState<ApoliceSubestipulanteModuloResult | undefined>();
  
  const [inativarModuloId, setInativarModuloId] = useState<string | null>(null);
  const [inativando, setInativando] = useState(false);
  const [inativarErro, setInativarErro] = useState<string | null>(null);

  const modulos = subestipulante.modulos || [];

  const handleAdicionar = () => {
    setModuloParaEditar(undefined);
    setModalAberto(true);
  };

  const handleEditar = (item: ApoliceSubestipulanteModuloResult) => {
    setModuloParaEditar(item);
    setModalAberto(true);
  };

  const confirmarInativacao = async () => {
    if (!inativarModuloId) return;
    try {
      setInativando(true);
      setInativarErro(null);
      await inativarModuloSubestipulanteApolice(apolicePublicId, subestipulante.subestipulantePublicId, inativarModuloId);
      setInativarModuloId(null);
      onRefresh();
    } catch (err: any) {
      setInativarErro(err.response?.data?.message || 'Ocorreu um erro ao inativar o vínculo do módulo.');
    } finally {
      setInativando(false);
    }
  };

  const columns: Column<ApoliceSubestipulanteModuloResult>[] = [
    {
      key: 'modulo',
      label: 'Módulo',
      render: (item) => (
        <div className="flex flex-col">
          <span className="font-medium text-texto-principal">{item.moduloNome}</span>
          {item.moduloDescricao && (
            <span className="text-sm text-texto-secundario">{item.moduloDescricao}</span>
          )}
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
      key: 'status',
      label: 'Status do Vínculo',
      render: (item) => (
        <div className="flex flex-col gap-1 items-start">
          <StatusBadge status={item.vinculoAtivo ? 'ativo' : 'inativo'} label={item.vinculoAtivo ? 'Ativo' : 'Inativo'} />
          {!item.moduloAtivoGlobal && (
            <span className="text-xs text-amber-600 bg-amber-50 px-2 py-0.5 rounded-full border border-amber-200">Cadastro Global Inativo</span>
          )}
        </div>
      ),
    },
    {
      key: 'acoes',
      label: 'Ações',
      align: 'right',
      render: (item) => (
        <div className="flex gap-2 justify-end">
          {podeAlterar && item.vinculoAtivo && (
            <Button variant="secondary" size="small" onClick={() => handleEditar(item)}>
              Editar
            </Button>
          )}
          {podeInativar && item.vinculoAtivo && (
            <Button variant="danger" size="small" onClick={() => setInativarModuloId(item.moduloPublicId)}>
              Inativar
            </Button>
          )}
        </div>
      ),
    },
  ];

  return (
    <div className="flex flex-col gap-3 py-2 w-full">
      <div className="flex justify-between items-center mb-1">
        <h4 className="text-sm font-semibold text-slate-700">Módulos Vinculados</h4>
        {podeInserir && (
          <Button variant="secondary" size="small" onClick={handleAdicionar}>
            Adicionar Módulo
          </Button>
        )}
      </div>

      {inativarErro && (
        <Alert variant="error" title="Atenção" onClose={() => setInativarErro(null)}>
          {inativarErro}
        </Alert>
      )}

      <div className="border border-slate-200 rounded-md overflow-hidden shadow-sm bg-white">
        <DataTable
          data={modulos}
          columns={columns}
          keyExtractor={(item) => item.moduloPublicId}
          emptyTitle="Nenhum módulo"
          emptyDescription="Nenhum módulo vinculado a este subestipulante nesta apólice."
        />
      </div>

      {modalAberto && (
        <ModuloSubestipulanteApoliceModal
          aberto={modalAberto}
          onClose={() => setModalAberto(false)}
          apolicePublicId={apolicePublicId}
          subestipulantePublicId={subestipulante.subestipulantePublicId}
          moduloEdicao={moduloParaEditar}
          onSucesso={onRefresh}
        />
      )}

      <ConfirmDialog
        aberto={!!inativarModuloId}
        onClose={() => !inativando && setInativarModuloId(null)}
        onConfirm={confirmarInativacao}
        title="Inativar Vínculo do Módulo"
        description="Deseja inativar o vínculo deste Módulo com o Subestipulante nesta Apólice? O Cadastro Global do Módulo será preservado."
        confirmText="Inativar Vínculo"
        variant="danger"
        loading={inativando}
      />
    </div>
  );
};
