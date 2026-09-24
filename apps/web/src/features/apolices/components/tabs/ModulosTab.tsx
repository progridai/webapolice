import React, { useState } from 'react';
import { useApoliceModulos } from '../../hooks/useApoliceModulos';
import { inativarApoliceModulo } from '../../api/apolices.api';
import type { ApoliceModuloResult } from '../../types/apolice.types';
import {
  Button,
  Alert,
  EmptyState,
  DataTable,
  StatusBadge,
  ConfirmDialog,
} from '../../../../components/ui';
import type { Column } from '../../../../components/ui/DataTable/DataTable';
import { ModuloApoliceModal } from '../modals/ModuloApoliceModal';
import { useAuthorization } from '../../../../auth/AuthorizationProvider';

interface ModulosTabProps {
  publicId: string;
}

export const ModulosTab: React.FC<ModulosTabProps> = ({ publicId }) => {
  const { data, isLoading, error, refetch } = useApoliceModulos(publicId);
  const { possuiPermissao } = useAuthorization();

  const podeInserir = possuiPermissao('apolices.modulos.inserir');
  const podeAlterar = possuiPermissao('apolices.modulos.alterar');
  const podeInativar = possuiPermissao('apolices.modulos.inativar');

  const [modalAberto, setModalAberto] = useState(false);
  // publicId do vínculo (apoliceModuloPublicId) — NÃO é o moduloPublicId do cadastro global
  const [moduloEdicao, setModuloEdicao] = useState<ApoliceModuloResult | undefined>();

  const [moduloInativacao, setModuloInativacao] = useState<ApoliceModuloResult | null>(null);
  const [inativando, setInativando] = useState(false);
  const [inativarErro, setInativarErro] = useState<string | null>(null);

  const handleNovo = () => {
    setModuloEdicao(undefined);
    setModalAberto(true);
  };

  const handleEditar = (modulo: ApoliceModuloResult) => {
    setModuloEdicao(modulo);
    setModalAberto(true);
  };

  // Inativação usa publicId do vínculo (apoliceModuloPublicId = seguro.apolice_modulo)
  const handleConfirmaInativacao = async () => {
    if (!moduloInativacao) return;
    try {
      setInativando(true);
      setInativarErro(null);
      await inativarApoliceModulo(publicId, moduloInativacao.publicId);
      setModuloInativacao(null);
      refetch();
    } catch (err: any) {
      setInativarErro(
        err.response?.data?.detail ||
          err.response?.data?.message ||
          err.message ||
          'Erro ao inativar o módulo.'
      );
    } finally {
      setInativando(false);
    }
  };

  const formatarVigencia = (inicio?: string, fim?: string): string => {
    if (!inicio && !fim) return '—';
    const i = inicio ? new Date(inicio).toLocaleDateString('pt-BR') : '…';
    const f = fim ? new Date(fim).toLocaleDateString('pt-BR') : '…';
    return `${i} → ${f}`;
  };

  const colunas: Column<ApoliceModuloResult>[] = [
    {
      key: 'nome',
      label: 'Módulo',
      render: (row) => (
        <div>
          <span className="font-medium text-texto-principal">{row.nome}</span>
          {row.descricao && (
            <p className="text-xs text-texto-terciario mt-0.5">{row.descricao}</p>
          )}
        </div>
      ),
    },
    {
      key: 'vigencia',
      label: 'Vigência',
      render: (row) => (
        <span className="text-texto-secundario text-sm">
          {formatarVigencia(row.dataInicio, row.dataFim)}
        </span>
      ),
    },
    {
      key: 'observacao',
      label: 'Observação',
      render: (row) => {
        if (!row.observacao) return <span className="text-texto-secundario">—</span>;
        return (
          <span className="text-texto-secundario" title={row.observacao}>
            {row.observacao.length > 50
              ? `${row.observacao.substring(0, 50)}...`
              : row.observacao}
          </span>
        );
      },
    },
    {
      key: 'status',
      label: 'Status',
      render: (row) => (
        <StatusBadge status={row.ativo ? 'ativo' : 'inativo'} label={row.ativo ? 'Ativo' : 'Inativo'} />
      ),
    },
    {
      key: 'acoes',
      label: 'Ações',
      align: 'right',
      render: (row) => (
        <div className="flex justify-end gap-2">
          {podeAlterar && row.ativo && (
            <Button variant="secondary" size="small" onClick={() => handleEditar(row)}>
              Editar
            </Button>
          )}
          {podeInativar && row.ativo && (
            <Button variant="danger" size="small" onClick={() => setModuloInativacao(row)}>
              Inativar
            </Button>
          )}
        </div>
      ),
    },
  ];

  if (error) {
    return (
      <div className="flex flex-col gap-4 items-start">
        <Alert variant="error" title="Erro ao carregar Módulos">
          {error.message || 'Não foi possível carregar os módulos desta apólice.'}
        </Alert>
        <Button onClick={refetch} variant="primary" size="small" loading={isLoading}>
          Tentar novamente
        </Button>
      </div>
    );
  }

  return (
    <div className="flex flex-col gap-4">
      <div className="flex justify-between items-center mb-2">
        <div>
          <h2 className="text-xl font-semibold text-texto-principal">Módulos da Apólice</h2>
          <p className="text-sm text-texto-terciario mt-1">
            Gestão dos módulos contratados vinculados diretamente à Apólice.
          </p>
        </div>
        {podeInserir && (
          <Button onClick={handleNovo} variant="primary">
            Vincular Módulo
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
          title="Nenhum módulo vinculado"
          description="Nenhum módulo vinculado a esta apólice."
          action={
            podeInserir ? (
              <Button onClick={handleNovo} variant="primary">
                Vincular Módulo
              </Button>
            ) : undefined
          }
        />
      ) : (
        <DataTable
          data={data || []}
          columns={colunas}
          keyExtractor={(row) => row.publicId}
          isLoading={isLoading}
          aria-label="Lista de Módulos da Apólice"
        />
      )}

      {modalAberto && (
        <ModuloApoliceModal
          aberto={modalAberto}
          onClose={() => setModalAberto(false)}
          apolicePublicId={publicId}
          modulosVinculados={data || []}
          moduloEdicao={moduloEdicao}
          onSucesso={refetch}
        />
      )}

      <ConfirmDialog
        aberto={!!moduloInativacao}
        onClose={() => !inativando && setModuloInativacao(null)}
        onConfirm={handleConfirmaInativacao}
        title="Inativar Módulo"
        description="Deseja inativar este vínculo de Módulo? O registro permanecerá disponível para histórico."
        confirmText="Inativar"
        variant="danger"
        loading={inativando}
      />
    </div>
  );
};
