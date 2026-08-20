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
  EyeIcon,
  Pagination
} from '../../../components/ui';
import type { Column } from '../../../components/ui/DataTable/DataTable';
import { ROUTES, createPath } from '../../../app/routes/routePaths';
import { useAuthorization } from '../../../auth/AuthorizationProvider';
import { ramosApi, type RamoDto } from '../api/ramos.api';

export const RamosListPage: React.FC = () => {
  const navigate = useNavigate();
  const { possuiPermissao } = useAuthorization();

  const [ramos, setRamos] = useState<RamoDto[]>([]);
  const [total, setTotal] = useState(0);
  const [isLoading, setIsLoading] = useState(true);
  
  // Filtros e paginação
  const [busca, setBusca] = useState('');
  const [ativo, setAtivo] = useState<string>('');
  const [pagina, setPagina] = useState(1);
  const [tamanhoPagina] = useState(10);

  const carregarRamos = useCallback(async () => {
    try {
      setIsLoading(true);
      const res = await ramosApi.listar({
        pagina,
        tamanhoPagina,
        busca: busca || undefined,
        ativo: ativo === 'true' ? true : ativo === 'false' ? false : undefined,
      });
      setRamos(res.items);
      setTotal(res.totalCount);
    } catch (err) {
      console.error('Erro ao listar ramos:', err);
    } finally {
      setIsLoading(false);
    }
  }, [pagina, tamanhoPagina, busca, ativo]);

  useEffect(() => {
    carregarRamos();
  }, [carregarRamos]);

  // Disparado sempre que o SearchField com debounce mudar
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

  const handleInativar = async (publicId: string) => {
    if (!window.confirm('Tem certeza que deseja inativar este Ramo?')) return;
    try {
      await ramosApi.inativar(publicId);
      carregarRamos();
    } catch (err) {
      console.error('Erro ao inativar', err);
      alert('Erro ao inativar o ramo.');
    }
  };

  const handleReativar = async (publicId: string) => {
    if (!window.confirm('Tem certeza que deseja reativar este Ramo?')) return;
    try {
      await ramosApi.reativar(publicId);
      carregarRamos();
    } catch (err) {
      console.error('Erro ao reativar', err);
      alert('Erro ao reativar o ramo.');
    }
  };

  const columns: Column<RamoDto>[] = [
    {
      key: 'codigo',
      label: 'Código',
    },
    {
      key: 'nome',
      label: 'Nome',
    },
    {
      key: 'descricao',
      label: 'Descrição',
    },
    {
      key: 'ativo',
      label: 'Status',
      render: (row) => <StatusBadge status={row.ativo ? 'ativo' : 'inativo'} />
    },
    {
      key: 'acoes',
      label: 'Ações',
      align: 'right',
      render: (row) => {
        const rowActions = [];
        
        if (possuiPermissao('ramos.alterar')) {
          rowActions.push({
            label: 'Editar',
            icon: <EditIcon size={16} />,
            onClick: () => navigate(createPath(ROUTES.RAMOS_EDITAR, { publicId: row.publicId }))
          });
        }
        if (possuiPermissao('ramos.inativar') && row.ativo) {
          rowActions.push({
            label: 'Inativar',
            icon: <XCircleIcon size={16} />,
            variant: 'danger' as const,
            onClick: () => handleInativar(row.publicId)
          });
        }
        if (possuiPermissao('ramos.reativar') && !row.ativo) {
          rowActions.push({
            label: 'Reativar',
            icon: <CheckCircleIcon size={16} />,
            onClick: () => handleReativar(row.publicId)
          });
        }

        return (
          <RowActions
            primaryAction={{
              label: 'Visualizar',
              icon: <EyeIcon size={16} />,
              // Temporário enquanto não há tela de visualização
              onClick: () => alert('Visualizar ramo não implementado.')
            }}
            actions={rowActions}
            ariaLabel={`Ações para o ramo ${row.nome}`}
          />
        );
      }
    }
  ];

  return (
    <div className="flex flex-col gap-6">
      <PageHeader
        title="Catálogo de Ramos"
        description="Gerencie os ramos de seguros utilizados no sistema"
        breadcrumbs={
          <Breadcrumbs
            items={[
              { label: 'Início', href: ROUTES.APP },
              { label: 'Ramos' }
            ]}
          />
        }
        actions={
          possuiPermissao('ramos.inserir') ? (
            <Button onClick={() => navigate(ROUTES.RAMOS_NOVO)}>
              <PlusIcon size={20} className="mr-2" />
              Novo Ramo
            </Button>
          ) : undefined
        }
      />

      <div className="flex flex-col gap-4">
        <FilterBar>
          <div className="flex-1 flex flex-col gap-1">
            <label htmlFor="busca-ramos" className="text-sm font-medium text-texto-secundario">
              Buscar ramo
            </label>
            <SearchField 
              id="busca-ramos" 
              placeholder="Buscar por código ou nome..." 
              value={busca} 
              onChange={handleBuscaChange} 
            />
          </div>
          <div className="w-full md:w-48 flex flex-col gap-1">
            <label htmlFor="status-ramos" className="text-sm font-medium text-texto-secundario">
              Status
            </label>
            <Select id="status-ramos" value={ativo} onChange={(e) => { setAtivo(e.target.value); setPagina(1); }}>
              <option value="">Todos os status</option>
              <option value="true">Ativos</option>
              <option value="false">Inativos</option>
            </Select>
          </div>
          <div className="flex items-end pb-0">
            <Button variant="secondary" disabled={!hasFilters} onClick={handleLimparFiltros} className="h-10">
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
          data={ramos}
          keyExtractor={(row) => row.publicId}
          isLoading={isLoading}
          emptyTitle="Nenhum ramo encontrado"
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
    </div>
  );
};
