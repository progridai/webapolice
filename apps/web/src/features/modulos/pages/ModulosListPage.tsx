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
import { modulosApi } from '../api/modulos.api';
import type { ModuloListDto } from '../types/modulo.types';

export const ModulosListPage: React.FC = () => {
  const navigate = useNavigate();
  const { possuiPermissao } = useAuthorization();

  const [modulos, setModulos] = useState<ModuloListDto[]>([]);
  const [total, setTotal] = useState(0);
  const [isLoading, setIsLoading] = useState(true);
  
  const [busca, setBusca] = useState('');
  const [ativo, setAtivo] = useState<string>('');
  const [pagina, setPagina] = useState(1);
  const [tamanhoPagina] = useState(10);

  const carregarModulos = useCallback(async () => {
    try {
      setIsLoading(true);
      const res = await modulosApi.listar({
        pagina,
        tamanhoPagina,
        busca: busca || undefined,
        ativo: ativo === 'true' ? true : ativo === 'false' ? false : undefined,
      });
      setModulos(res.items);
      setTotal(res.totalCount);
    } catch (err) {
      console.error('Erro ao listar módulos:', err);
    } finally {
      setIsLoading(false);
    }
  }, [pagina, tamanhoPagina, busca, ativo]);

  useEffect(() => {
    carregarModulos();
  }, [carregarModulos]);

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
    if (!window.confirm('Tem certeza que deseja inativar este Módulo?')) return;
    try {
      await modulosApi.inativar(publicId);
      carregarModulos();
    } catch (err) {
      console.error('Erro ao inativar módulo:', err);
      alert('Não foi possível inativar o módulo.');
    }
  };

  const columns: Column<ModuloListDto>[] = [
    {
      key: 'nome',
      label: 'Nome',
    },
    {
      key: 'ativo',
      label: 'Status',
      render: (modulo) => (
        <StatusBadge 
          status={modulo.ativo ? 'ativo' : 'inativo'} 
        />
      ),
    },
    {
      key: 'createdAt',
      label: 'Data de Cadastro',
      render: (modulo) => new Date(modulo.createdAt).toLocaleDateString('pt-BR'),
    },
    {
      key: 'acoes',
      label: 'Ações',
      align: 'right',
      render: (modulo) => {
        const rowActions = [];
        
        if (possuiPermissao('modulos.alterar')) {
          rowActions.push({
            label: 'Editar',
            icon: <EditIcon size={16} />,
            onClick: () => navigate(createPath(ROUTES.MODULOS_EDITAR, { publicId: modulo.publicId }))
          });
        }
        if (possuiPermissao('modulos.inativar') && modulo.ativo) {
          rowActions.push({
            label: 'Inativar',
            icon: <XCircleIcon size={16} />,
            variant: 'danger' as const,
            onClick: () => handleInativar(modulo.publicId)
          });
        }

        return (
          <RowActions
            primaryAction={{
              label: 'Visualizar',
              icon: <EyeIcon size={16} />,
              onClick: () => navigate(createPath(ROUTES.MODULOS_EDITAR, { publicId: modulo.publicId }))
            }}
            actions={rowActions}
            ariaLabel={`Ações para o módulo ${modulo.nome}`}
          />
        );
      }
    },
  ];

  return (
    <div className="flex flex-col gap-6">
      <PageHeader
        title="Módulos Globais"
        description="Gerencie os módulos disponíveis no sistema"
        breadcrumbs={
          <Breadcrumbs
            items={[
              { label: 'Início', href: ROUTES.APP },
              { label: 'Módulos' },
            ]}
          />
        }
        actions={
          possuiPermissao('modulos.inserir') ? (
            <Button onClick={() => navigate(ROUTES.MODULOS_NOVO)}>
              <PlusIcon size={20} className="mr-2" />
              Novo Módulo
            </Button>
          ) : undefined
        }
      />

      <div className="flex flex-col gap-4">
        <FilterBar>
          <div className="flex-1 flex flex-col gap-1">
            <label htmlFor="busca-modulos" className="text-sm font-medium text-texto-secundario">
              Buscar módulo
            </label>
            <SearchField 
              id="busca-modulos" 
              placeholder="Buscar por nome..." 
              value={busca} 
              onChange={handleBuscaChange} 
            />
          </div>
          <div className="w-full md:w-48 flex flex-col gap-1">
            <label htmlFor="status-modulos" className="text-sm font-medium text-texto-secundario">
              Status
            </label>
            <Select id="status-modulos" value={ativo} onChange={(e) => { setAtivo(e.target.value); setPagina(1); }}>
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
          data={modulos}
          keyExtractor={(row) => row.publicId}
          isLoading={isLoading}
          emptyTitle="Nenhum módulo encontrado"
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
