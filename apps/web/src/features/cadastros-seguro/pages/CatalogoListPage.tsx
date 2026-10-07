import { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  PageHeader,
  Button,
  DataTable,
  FormField,
  Select,
  FilterBar,
  SearchField,
  Pagination,
  StatusBadge,
  ConfirmDialog,
  Alert,
} from '../../../components/ui';
import type { Column } from '../../../components/ui/DataTable/DataTable';
import { useAuthorization } from '../../../auth/AuthorizationProvider';
import { catalogosApi, type RegistroCatalogo } from '../api/catalogos.api';
import type { ConfigCatalogo } from '../catalogos.config';
export function CatalogoListPage({ config }: { config: ConfigCatalogo }) {
  const navigate = useNavigate();
  const { possuiPermissao } = useAuthorization();
  const [items, setItems] = useState<RegistroCatalogo[]>([]);
  const [total, setTotal] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [busca, setBusca] = useState('');
  const [status, setStatus] = useState('');
  const [pagina, setPagina] = useState(1);
  const [confirmar, setConfirmar] = useState<RegistroCatalogo | null>(null);
  const [salvando, setSalvando] = useState(false);
  const carregar = useCallback(
    async (signal?: AbortSignal) => {
      return catalogosApi
        .listar(
          config.recurso,
          {
            pagina,
            tamanhoPagina: 20,
            busca,
            ...(status ? { ativo: status === 'true' } : {}),
          },
          signal,
        )
        .then((res) => {
          if (!signal?.aborted) {
            setItems(res.items);
            setTotal(res.totalCount);
            setError('');
          }
        })
        .catch((err) => {
          if (!signal?.aborted)
            setError(
              err instanceof Error
                ? err.message
                : 'Não foi possível carregar o cadastro.',
            );
        })
        .finally(() => {
          if (!signal?.aborted) setLoading(false);
        });
    },
    [config.recurso, pagina, busca, status],
  );
  useEffect(() => {
    const controller = new AbortController();
    void carregar(controller.signal);
    return () => controller.abort();
  }, [carregar]);
  const alterarStatus = async () => {
    if (!confirmar) return;
    setSalvando(true);
    try {
      await catalogosApi.status(
        config.recurso,
        confirmar.publicId,
        !confirmar.ativo,
      );
      setConfirmar(null);
      await carregar();
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : 'Não foi possível alterar o status.',
      );
    } finally {
      setSalvando(false);
    }
  };
  const columns: Column<RegistroCatalogo>[] = [
    { key: 'nome', label: 'Nome' },
    {
      key: 'ativo',
      label: 'Status',
      render: (row) => <StatusBadge status={row.ativo ? 'ativo' : 'inativo'} />,
    },
    {
      key: 'acoes',
      label: 'Ações',
      render: (row) => (
        <div className="flex gap-2 justify-end">
          <Button
            variant="secondary"
            size="small"
            onClick={() => navigate(`${config.rota}/${row.publicId}`)}
          >
            Visualizar
          </Button>
          {possuiPermissao(`${config.permissao}.alterar`) && (
            <Button
              variant="secondary"
              size="small"
              onClick={() => navigate(`${config.rota}/${row.publicId}/editar`)}
            >
              Editar
            </Button>
          )}
          {possuiPermissao(
            `${config.permissao}.${row.ativo ? 'inativar' : 'reativar'}`,
          ) && (
            <Button
              variant="secondary"
              size="small"
              onClick={() => setConfirmar(row)}
            >
              {row.ativo ? 'Inativar' : 'Reativar'}
            </Button>
          )}
        </div>
      ),
    },
  ];
  return (
    <main className="flex flex-col gap-6 p-6" tabIndex={-1}>
      <PageHeader
        title={config.titulo}
        actions={
          possuiPermissao(`${config.permissao}.inserir`) ? (
            <Button onClick={() => navigate(`${config.rota}/novo`)}>
              Novo cadastro
            </Button>
          ) : undefined
        }
      />
      {error && (
        <Alert variant="error" title="Não foi possível concluir">
          {error}
          <Button variant="secondary" onClick={() => void carregar()}>
            Tentar novamente
          </Button>
        </Alert>
      )}
      <FilterBar>
        <SearchField
          value={busca}
          onChange={(value) => {
            setLoading(true);
            setError('');
            setBusca(value);
            setPagina(1);
          }}
          placeholder="Buscar por nome"
          aria-label="Buscar por nome"
        />
        <FormField label="Status">
          <Select
            value={status}
            onChange={(e) => {
              setLoading(true);
              setError('');
              setStatus(e.target.value);
              setPagina(1);
            }}
            options={[
              { value: '', label: 'Todos' },
              { value: 'true', label: 'Ativos' },
              { value: 'false', label: 'Inativos' },
            ]}
          />
        </FormField>
      </FilterBar>
      <DataTable
        data={items}
        columns={columns}
        keyExtractor={(row) => row.publicId}
        isLoading={loading}
        emptyTitle="Nenhum registro encontrado"
      />
      {total > 0 && (
        <Pagination
          currentPage={pagina}
          totalPages={Math.ceil(total / 20)}
          totalItems={total}
          pageSize={20}
          onPageChange={(page) => {
            setLoading(true);
            setError('');
            setPagina(page);
          }}
        />
      )}
      <ConfirmDialog
        aberto={!!confirmar}
        onClose={() => !salvando && setConfirmar(null)}
        onConfirm={() => void alterarStatus()}
        title={confirmar?.ativo ? 'Inativar cadastro' : 'Reativar cadastro'}
        description={
          confirmar ? `Deseja alterar o status de ${confirmar.nome}?` : ''
        }
        confirmText="Confirmar"
        loading={salvando}
      />
    </main>
  );
}
