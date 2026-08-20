/**
 * SubestipulantesListPage.tsx
 *
 * Página de listagem oficial de Subestipulantes.
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
import { subestipulantesApi } from '../api/subestipulantes.api';
import type { SubestipulanteListItem } from '../types/subestipulante.types';

export const SubestipulantesListPage: React.FC = () => {
  const navigate = useNavigate();
  const { possuiPermissao } = useAuthorization();

  const [subestipulantes, setSubestipulantes] = useState<SubestipulanteListItem[]>([]);
  const [total, setTotal] = useState(0);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Filtros e Paginação
  const [busca, setBusca] = useState('');
  const [ativo, setAtivo] = useState<string>('');
  const [pagina, setPagina] = useState(1);
  const [tamanhoPagina] = useState(10);

  // Estados de confirmação para Inativação / Reativação
  const [dialogConfig, setDialogConfig] = useState<{
    aberto: boolean;
    tipo: 'inativar' | 'reativar';
    item: SubestipulanteListItem | null;
    loading: boolean;
  }>({
    aberto: false,
    tipo: 'inativar',
    item: null,
    loading: false,
  });

  const carregarSubestipulantes = useCallback(async () => {
    try {
      setIsLoading(true);
      setError(null);
      const res = await subestipulantesApi.listar({
        pagina,
        tamanhoPagina,
        busca: busca.trim() || undefined,
        ativo: ativo === 'true' ? true : ativo === 'false' ? false : undefined,
      });
      setSubestipulantes(res.itens || []);
      setTotal(res.totalItens || 0);
    } catch (err: unknown) {
      console.error('Erro ao listar subestipulantes:', err);
      setError('Não foi possível carregar a lista de subestipulantes.');
    } finally {
      setIsLoading(false);
    }
  }, [pagina, tamanhoPagina, busca, ativo]);

  useEffect(() => {
    // eslint-disable-next-line react-hooks/set-state-in-effect
    carregarSubestipulantes();
  }, [carregarSubestipulantes]);

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

  const abrirConfirmacao = (item: SubestipulanteListItem, tipo: 'inativar' | 'reativar') => {
    setDialogConfig({
      aberto: true,
      tipo,
      item,
      loading: false,
    });
  };

  const fecharConfirmacao = () => {
    setDialogConfig((prev) => ({ ...prev, aberto: false, item: null, loading: false }));
  };

  const handleExecutarAcaoStatus = async () => {
    if (!dialogConfig.item) return;

    try {
      setDialogConfig((prev) => ({ ...prev, loading: true }));
      if (dialogConfig.tipo === 'inativar') {
        await subestipulantesApi.inativar(dialogConfig.item.publicId);
      } else {
        await subestipulantesApi.reativar(dialogConfig.item.publicId);
      }
      fecharConfirmacao();
      await carregarSubestipulantes();
    } catch (err: unknown) {
      console.error(`Erro ao ${dialogConfig.tipo} subestipulante:`, err);
      setError(`Ocorreu um erro ao ${dialogConfig.tipo} o subestipulante.`);
      setDialogConfig((prev) => ({ ...prev, loading: false }));
    }
  };

  const columns: Column<SubestipulanteListItem>[] = [
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

        if (possuiPermissao('subestipulantes.alterar')) {
          rowActions.push({
            label: 'Editar',
            icon: <EditIcon size={16} />,
            onClick: () => navigate(createPath(ROUTES.SUBESTIPULANTE_EDITAR, { publicId: row.publicId })),
          });
        }

        if (possuiPermissao('subestipulantes.inativar') && row.ativo) {
          rowActions.push({
            label: 'Inativar',
            icon: <XCircleIcon size={16} />,
            variant: 'danger' as const,
            onClick: () => abrirConfirmacao(row, 'inativar'),
          });
        }

        if (possuiPermissao('subestipulantes.reativar') && !row.ativo) {
          rowActions.push({
            label: 'Reativar',
            icon: <CheckCircleIcon size={16} />,
            onClick: () => abrirConfirmacao(row, 'reativar'),
          });
        }

        return (
          <RowActions
            actions={rowActions}
            ariaLabel={`Ações para o subestipulante ${row.nome}`}
          />
        );
      },
    },
  ];

  return (
    <div className="flex flex-col gap-6">
      <PageHeader
        title="Cadastro de Subestipulantes"
        description="Gerenciamento global de subestipulantes"
        breadcrumbs={
          <Breadcrumbs
            items={[
              { label: 'Início', href: ROUTES.APP },
              { label: 'Subestipulantes' },
            ]}
          />
        }
        actions={
          possuiPermissao('subestipulantes.inserir') ? (
            <Button onClick={() => navigate(ROUTES.SUBESTIPULANTE_NOVO)}>
              <PlusIcon size={20} className="mr-2" />
              Novo Subestipulante
            </Button>
          ) : undefined
        }
      />

      {error && (
        <Alert
          variant="error"
          title="Erro de carregamento"
          onClose={() => setError(null)}
        >
          <div className="flex flex-col gap-2">
            <span>{error}</span>
            <div>
              <Button variant="secondary" size="sm" onClick={carregarSubestipulantes}>
                Tentar novamente
              </Button>
            </div>
          </div>
        </Alert>
      )}

      <div className="flex flex-col gap-4">
        <FilterBar>
          <div className="flex-1 flex flex-col gap-1">
            <label htmlFor="busca-subestipulantes" className="text-sm font-medium text-texto-secundario">
              Buscar subestipulante
            </label>
            <SearchField
              id="busca-subestipulantes"
              placeholder="Buscar por nome, CNPJ ou código..."
              value={busca}
              onChange={handleBuscaChange}
            />
          </div>
          <div className="w-full md:w-48 flex flex-col gap-1">
            <label htmlFor="status-subestipulantes" className="text-sm font-medium text-texto-secundario">
              Status
            </label>
            <Select
              id="status-subestipulantes"
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
          data={subestipulantes}
          keyExtractor={(row) => row.publicId}
          isLoading={isLoading}
          emptyTitle="Nenhum subestipulante encontrado"
          emptyDescription={
            hasFilters
              ? 'Tente ajustar os filtros ou termos da pesquisa.'
              : 'Clique em "Novo Subestipulante" para começar a cadastrar.'
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
        title={dialogConfig.tipo === 'inativar' ? 'Inativar Subestipulante' : 'Reativar Subestipulante'}
        description={
          dialogConfig.tipo === 'inativar' ? (
            <p>
              Tem certeza que deseja inativar o subestipulante{' '}
              <strong>{dialogConfig.item?.nome}</strong>?
              <br />
              <span className="text-sm text-texto-secundario">
                O cadastro ficará inativo no sistema, mas os vínculos e apólices existentes não serão apagados.
              </span>
            </p>
          ) : (
            <p>
              Deseja reativar o subestipulante <strong>{dialogConfig.item?.nome}</strong> para novas operações?
            </p>
          )
        }
        confirmText={dialogConfig.tipo === 'inativar' ? 'Sim, inativar' : 'Sim, reativar'}
        variant={dialogConfig.tipo === 'inativar' ? 'danger' : 'primary'}
      />
    </div>
  );
};
