/**
 * CorretorasListPage.tsx
 *
 * Página de listagem oficial de Corretoras.
 */
import React, { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  PageHeader,
  Button,
  DataTable,
  Select,
  PlusIcon,
  EditIcon,
  XCircleIcon,
  CheckCircleIcon,
  Breadcrumbs,
  FilterBar,
  SearchField,
  ResultsSummary,
  RowActions,
  StatusBadge,
  Pagination,
  Alert,
  ConfirmDialog,
} from '../../../components/ui';
import type { Column } from '../../../components/ui/DataTable/DataTable';
import { ROUTES, createPath } from '../../../app/routes/routePaths';
import { useAuthorization } from '../../../auth/AuthorizationProvider';
import { corretorasApi } from '../api/corretoras.api';
import type { CorretoraListItem } from '../types/corretora.types';

export const CorretorasListPage: React.FC = () => {
  const navigate = useNavigate();
  const { possuiPermissao } = useAuthorization();

  const [corretoras, setCorretoras] = useState<CorretoraListItem[]>([]);
  const [total, setTotal] = useState(0);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [busca, setBusca] = useState('');
  const [ativo, setAtivo] = useState<string>('');
  const [pagina, setPagina] = useState(1);
  const [tamanhoPagina] = useState(10);

  const [dialogConfig, setDialogConfig] = useState<{
    aberto: boolean;
    tipo: 'inativar' | 'reativar';
    item: CorretoraListItem | null;
    loading: boolean;
  }>({
    aberto: false,
    tipo: 'inativar',
    item: null,
    loading: false,
  });

  const carregarCorretoras = useCallback(async () => {
    try {
      setIsLoading(true);
      setError(null);
      const res = await corretorasApi.listar({
        pagina,
        tamanhoPagina,
        busca: busca.trim() || undefined,
        ativo: ativo === 'true' ? true : ativo === 'false' ? false : undefined,
      });
      setCorretoras(res.itens || []);
      setTotal(res.totalItens || 0);
    } catch (err: unknown) {
      console.error('Erro ao listar corretoras:', err);
      setError('Não foi possível carregar a lista de corretoras.');
    } finally {
      setIsLoading(false);
    }
  }, [pagina, tamanhoPagina, busca, ativo]);

  useEffect(() => {
    carregarCorretoras();
  }, [carregarCorretoras]);

  const handleBuscaChange = (novaBusca: string) => {
    setBusca(novaBusca);
    setPagina(1);
  };

  const handleLimparFiltros = () => {
    setBusca('');
    setAtivo('');
    setPagina(1);
  };

  const hasFilters = busca !== '' || ativo !== '';

  const formatCnpj = (cnpj?: string) => {
    if (!cnpj) return '-';
    const clean = cnpj.replace(/\D/g, '');
    if (clean.length === 14) {
      return clean.replace(/^(\d{2})(\d{3})(\d{3})(\d{4})(\d{2})$/, '$1.$2.$3/$4-$5');
    }
    return cnpj;
  };

  const abrirConfirmacao = (item: CorretoraListItem, tipo: 'inativar' | 'reativar') => {
    setDialogConfig({ aberto: true, tipo, item, loading: false });
  };

  const fecharConfirmacao = () => {
    setDialogConfig((prev) => ({ ...prev, aberto: false, item: null, loading: false }));
  };

  const handleExecutarAcaoStatus = async () => {
    if (!dialogConfig.item) return;
    try {
      setDialogConfig((prev) => ({ ...prev, loading: true }));
      if (dialogConfig.tipo === 'inativar') {
        await corretorasApi.inativar(dialogConfig.item.publicId);
      } else {
        await corretorasApi.reativar(dialogConfig.item.publicId);
      }
      fecharConfirmacao();
      await carregarCorretoras();
    } catch (err: unknown) {
      console.error(`Erro ao ${dialogConfig.tipo} corretora:`, err);
      setError(`Ocorreu um erro ao ${dialogConfig.tipo} a corretora.`);
      setDialogConfig((prev) => ({ ...prev, loading: false }));
    }
  };

  const columns: Column<CorretoraListItem>[] = [
    {
      key: 'codigo',
      label: 'Código',
      render: (row) => row.codigo || '-',
    },
    {
      key: 'nome',
      label: 'Nome / Razão Social',
    },
    {
      key: 'cnpj',
      label: 'CNPJ',
      render: (row) => formatCnpj(row.cnpj),
    },
    {
      key: 'codigoProtheus',
      label: 'Cód. Protheus',
      render: (row) => row.codigoProtheus || '-',
    },
    {
      key: 'ativo',
      label: 'Status',
      render: (row) => <StatusBadge status={row.ativo ? 'ativo' : 'inativo'} />,
    },
    {
      key: 'acoes',
      label: 'Ações',
      align: 'right',
      render: (row) => {
        const rowActions = [];

        if (possuiPermissao('corretoras.alterar')) {
          rowActions.push({
            label: 'Editar',
            icon: <EditIcon size={16} />,
            onClick: () => navigate(createPath(ROUTES.CORRETORA_EDITAR, { publicId: row.publicId })),
          });
        }

        if (possuiPermissao('corretoras.inativar') && row.ativo) {
          rowActions.push({
            label: 'Inativar',
            icon: <XCircleIcon size={16} />,
            variant: 'danger' as const,
            onClick: () => abrirConfirmacao(row, 'inativar'),
          });
        }

        if (possuiPermissao('corretoras.reativar') && !row.ativo) {
          rowActions.push({
            label: 'Reativar',
            icon: <CheckCircleIcon size={16} />,
            onClick: () => abrirConfirmacao(row, 'reativar'),
          });
        }

        return (
          <RowActions
            actions={rowActions}
            ariaLabel={`Ações para a corretora ${row.nome}`}
          />
        );
      },
    },
  ];

  return (
    <div className="flex flex-col gap-6">
      <PageHeader
        title="Cadastro de Corretoras"
        description="Gerenciamento global de corretoras parceiras"
        breadcrumbs={
          <Breadcrumbs
            items={[
              { label: 'Início', href: ROUTES.APP },
              { label: 'Corretoras' },
            ]}
          />
        }
        actions={
          possuiPermissao('corretoras.inserir') ? (
            <Button onClick={() => navigate(ROUTES.CORRETORA_NOVA)}>
              <PlusIcon size={20} className="mr-2" />
              Nova Corretora
            </Button>
          ) : undefined
        }
      />

      {error && (
        <Alert variant="error" title="Erro de carregamento" onClose={() => setError(null)}>
          <div className="flex flex-col gap-2">
            <span>{error}</span>
            <div>
              <Button variant="secondary" size="sm" onClick={carregarCorretoras}>
                Tentar novamente
              </Button>
            </div>
          </div>
        </Alert>
      )}

      <div className="flex flex-col gap-4">
        <FilterBar>
          <div className="flex-1 flex flex-col gap-1">
            <label htmlFor="busca-corretoras" className="text-sm font-medium text-texto-secundario">
              Buscar Corretora
            </label>
            <SearchField
              id="busca-corretoras"
              placeholder="Buscar por nome, CNPJ ou código..."
              value={busca}
              onChange={handleBuscaChange}
            />
          </div>
          <div className="w-full md:w-48 flex flex-col gap-1">
            <label htmlFor="status-corretoras" className="text-sm font-medium text-texto-secundario">
              Status
            </label>
            <Select
              id="status-corretoras"
              value={ativo}
              onChange={(e) => {
                setAtivo(e.target.value);
                setPagina(1);
              }}
            >
              <option value="">Todos os status</option>
              <option value="true">Ativos</option>
              <option value="false">Inativos</option>
            </Select>
          </div>
          <div className="flex items-end pb-0">
            <Button
              variant="secondary"
              disabled={!hasFilters}
              onClick={handleLimparFiltros}
              className="h-10"
            >
              Limpar
            </Button>
          </div>
        </FilterBar>

        <ResultsSummary
          currentPage={pagina}
          pageSize={tamanhoPagina}
          totalItems={total}
        />

        <DataTable
          columns={columns}
          data={corretoras}
          keyExtractor={(row) => row.publicId}
          isLoading={isLoading}
          emptyTitle="Nenhuma corretora encontrada"
          emptyDescription={
            hasFilters
              ? 'Tente ajustar os filtros ou termos da pesquisa.'
              : 'Clique em "Nova Corretora" para começar a cadastrar.'
          }
        />

        {total > 0 && (
          <Pagination
            currentPage={pagina}
            totalPages={Math.ceil(total / tamanhoPagina)}
            totalItems={total}
            pageSize={tamanhoPagina}
            onPageChange={setPagina}
          />
        )}
      </div>

      <ConfirmDialog
        aberto={dialogConfig.aberto}
        onClose={fecharConfirmacao}
        onConfirm={handleExecutarAcaoStatus}
        loading={dialogConfig.loading}
        title={dialogConfig.tipo === 'inativar' ? 'Inativar Corretora' : 'Reativar Corretora'}
        description={
          dialogConfig.tipo === 'inativar' ? (
            <p>
              Tem certeza que deseja inativar a corretora{' '}
              <strong>{dialogConfig.item?.nome}</strong>?
            </p>
          ) : (
            <p>
              Deseja reativar a corretora <strong>{dialogConfig.item?.nome}</strong>?
            </p>
          )
        }
        confirmText={dialogConfig.tipo === 'inativar' ? 'Sim, inativar' : 'Sim, reativar'}
        variant={dialogConfig.tipo === 'inativar' ? 'danger' : 'primary'}
      />
    </div>
  );
};
