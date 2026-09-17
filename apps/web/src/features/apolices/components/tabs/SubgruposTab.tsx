import React, { useState } from 'react';
import { useApoliceSubgrupos } from '../../hooks/useApoliceSubgrupos';
import { inativarApoliceSubgrupo } from '../../api/apolices.api';
import type { ApoliceSubgrupoResult } from '../../types/apolice.types';
import { 
  Button, 
  Alert, 
  EmptyState, 
  DataTable, 
  StatusBadge,
  ConfirmDialog
} from '../../../../components/ui';
import type { Column } from '../../../../components/ui/DataTable/DataTable';
import { SubgrupoApoliceModal } from '../modals/SubgrupoApoliceModal';
import { useAuthorization } from '../../../../auth/AuthorizationProvider';

interface SubgruposTabProps {
  publicId: string;
}

export const SubgruposTab: React.FC<SubgruposTabProps> = ({ publicId }) => {
  const { data, isLoading, error, refetch } = useApoliceSubgrupos(publicId);
  const { possuiPermissao } = useAuthorization();
  
  const podeInserir = possuiPermissao('apolices.subgrupos.inserir');
  const podeAlterar = possuiPermissao('apolices.subgrupos.alterar');
  const podeInativar = possuiPermissao('apolices.subgrupos.inativar');

  const [modalAberto, setModalAberto] = useState(false);
  const [subgrupoEdicao, setSubgrupoEdicao] = useState<ApoliceSubgrupoResult | undefined>();
  
  const [subgrupoInativacao, setSubgrupoInativacao] = useState<ApoliceSubgrupoResult | null>(null);
  const [inativando, setInativando] = useState(false);
  const [inativarErro, setInativarErro] = useState<string | null>(null);

  const handleNovo = () => {
    setSubgrupoEdicao(undefined);
    setModalAberto(true);
  };

  const handleEditar = (subgrupo: ApoliceSubgrupoResult) => {
    setSubgrupoEdicao(subgrupo);
    setModalAberto(true);
  };

  const handleConfirmaInativacao = async () => {
    if (!subgrupoInativacao) return;
    try {
      setInativando(true);
      setInativarErro(null);
      await inativarApoliceSubgrupo(publicId, subgrupoInativacao.subgrupoPublicId);
      setSubgrupoInativacao(null);
      refetch();
    } catch (err: any) {
      if (err.response?.data?.detail) {
        setInativarErro(err.response.data.detail);
      } else {
        setInativarErro(err.message || 'Erro ao inativar o subgrupo.');
      }
    } finally {
      setInativando(false);
    }
  };

  const colunas: Column<ApoliceSubgrupoResult>[] = [
    {
      key: 'nome',
      label: 'Nome',
      render: (row) => <span className="font-medium text-texto-principal">{row.nome}</span>,
    },
    {
      key: 'observacao',
      label: 'Observação',
      render: (row) => {
        if (!row.observacao) return <span className="text-texto-secundario">—</span>;
        return (
          <span className="text-texto-secundario" title={row.observacao}>
            {row.observacao.length > 50 ? `${row.observacao.substring(0, 50)}...` : row.observacao}
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
            <Button variant="danger" size="small" onClick={() => setSubgrupoInativacao(row)}>
              Inativar
            </Button>
          )}
        </div>
      ),
    }
  ];

  if (error) {
    return (
      <div className="flex flex-col gap-4 items-start">
        <Alert variant="error" title="Erro ao carregar Subgrupos">
          {error.message || 'Não foi possível carregar os subgrupos desta apólice.'}
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
          <h2 className="text-xl font-semibold text-texto-principal">Subgrupos da Apólice</h2>
          <p className="text-sm text-texto-terciario mt-1">Gestão de divisões contextuais da Apólice.</p>
        </div>
        {podeInserir && (
          <Button onClick={handleNovo} variant="primary">
            Adicionar Subgrupo
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
          title="Nenhum Subgrupo cadastrado"
          description="Nenhum Subgrupo cadastrado nesta Apólice."
        />
      ) : (
        <DataTable
          data={data || []}
          columns={colunas}
          keyExtractor={(row) => row.subgrupoPublicId}
          isLoading={isLoading}
          aria-label="Lista de Subgrupos da Apólice"
        />
      )}

      {modalAberto && (
        <SubgrupoApoliceModal
          aberto={modalAberto}
          onClose={() => setModalAberto(false)}
          apolicePublicId={publicId}
          subgrupoEdicao={subgrupoEdicao}
          onSucesso={refetch}
        />
      )}

      <ConfirmDialog
        aberto={!!subgrupoInativacao}
        onClose={() => !inativando && setSubgrupoInativacao(null)}
        onConfirm={handleConfirmaInativacao}
        title="Inativar Subgrupo"
        description="Deseja inativar este Subgrupo da Apólice? O registro permanecerá disponível para histórico."
        confirmText="Inativar"
        variant="danger"
        loading={inativando}
      />
    </div>
  );
};
