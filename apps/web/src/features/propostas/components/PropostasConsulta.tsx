import { useCallback, useState } from 'react';
import {
  Alert,
  Badge,
  Button,
  DataTable,
  FilterBar,
  Input,
  Pagination,
  ResultsSummary,
} from '../../../components/ui';
import type { Column } from '../../../components/ui/DataTable/DataTable';
import { useAuthorization } from '../../../auth/AuthorizationProvider';
import { useConsulta } from '../../../shared/hooks/useConsulta';
import { formatarDataOuVazio } from '../../../shared/utils/formatters';
import { listarPropostas } from '../api/propostas.api';
import type { Proposta } from '../types/proposta.types';
import { PropostaDetalhes } from './PropostaDetalhes';

export function PropostasConsulta({
  clientePublicId,
  clienteVinculoPublicId,
  apolicePublicId,
  apoliceVidaPublicId,
  initialPublicId,
  onBusy,
}: {
  clientePublicId?: string;
  clienteVinculoPublicId?: string;
  apolicePublicId?: string;
  apoliceVidaPublicId?: string;
  initialPublicId?: string;
  onBusy?: (busy: boolean) => void;
}) {
  const { possuiPermissao, possuiRecurso, possuiModulo } = useAuthorization();
  const permitido =
    possuiPermissao('propostas.visualizar') &&
    possuiRecurso('PROPOSTAS') &&
    possuiModulo('APOLICES');
  const [pagina, setPagina] = useState(1);
  const [busca, setBusca] = useState('');
  const [termoBusca, setTermoBusca] = useState('');
  const [selecionada, setSelecionada] = useState<string | null>(
    initialPublicId ?? null,
  );
  const load = useCallback(
    (signal: AbortSignal) =>
      listarPropostas(
        {
          pagina,
          tamanhoPagina: 20,
          termoBusca,
          clientePublicId,
          clienteVinculoPublicId,
          apolicePublicId,
          apoliceVidaPublicId,
        },
        signal,
      ),
    [
      pagina,
      termoBusca,
      apolicePublicId,
      apoliceVidaPublicId,
      clientePublicId,
      clienteVinculoPublicId,
    ],
  );
  const { data, error, isLoading, refetch } = useConsulta(
    permitido ? load : null,
    'Não foi possível carregar as propostas.',
  );
  const columns: Column<Proposta>[] = [
    {
      key: 'numero',
      label: 'Proposta',
      render: (p) => p.numero || 'Sem número',
    },
    {
      key: 'cliente',
      label: 'Cliente',
      render: (p) => p.clienteNome || 'Cadastro indisponível',
    },
    {
      key: 'matricula',
      label: 'Matrícula',
      render: (p) => p.vinculoMatricula || 'Não informada',
    },
    {
      key: 'apolice',
      label: 'Apólice',
      render: (p) => p.apoliceNome || 'Não vinculada',
    },
    {
      key: 'status',
      label: 'Situação',
      render: (p) => <Badge>{p.status}</Badge>,
    },
    { key: 'versao', label: 'Versão', render: (p) => p.versao },
    {
      key: 'vigencia',
      label: 'Vigência',
      render: (p) => (p.vigente ? 'Vigente' : 'Sem vigência'),
    },
    {
      key: 'inclusao',
      label: 'Inclusão',
      render: (p) => formatarDataOuVazio(p.dataInclusao),
    },
    {
      key: 'acoes',
      label: 'Ações',
      render: (p) => (
        <Button
          variant="text"
          size="small"
          aria-label={`Consultar proposta ${p.numero || 'sem número'} de ${p.clienteNome || 'cliente indisponível'}`}
          onClick={() => setSelecionada(p.publicId)}
        >
          Consultar
        </Button>
      ),
    },
  ];
  if (!permitido)
    return (
      <Alert variant="warning" title="Acesso restrito">
        Você não possui permissão para consultar propostas.
      </Alert>
    );
  if (selecionada)
    return (
      <PropostaDetalhes
        key={selecionada}
        publicId={selecionada}
        onVoltar={() => setSelecionada(null)}
        onNovaVersao={(id) => {
          setSelecionada(id);
          void refetch();
        }}
        onBusy={onBusy}
      />
    );
  return (
    <div className="flex flex-col gap-4">
      <form
        onSubmit={(event) => {
          event.preventDefault();
          setPagina(1);
          setTermoBusca(busca.trim());
        }}
      >
        <FilterBar>
          <div className="flex flex-col gap-1 flex-1">
            <label htmlFor="propostas-busca" className="form-label">
              Número da proposta
            </label>
            <Input
              id="propostas-busca"
              value={busca}
              onChange={(event) => setBusca(event.target.value)}
            />
          </div>
          <Button type="submit" variant="secondary">
            Pesquisar
          </Button>
          <Button
            variant="text"
            onClick={() => {
              setBusca('');
              setTermoBusca('');
              setPagina(1);
            }}
          >
            Limpar
          </Button>
        </FilterBar>
      </form>
      {error ? (
        <>
          <Alert variant="error" title="Não foi possível carregar as propostas">
            {error.message}
          </Alert>
          <div>
            <Button onClick={() => void refetch()} loading={isLoading}>
              Tentar novamente
            </Button>
          </div>
        </>
      ) : (
        <>
          {data && !isLoading && (
            <ResultsSummary
              currentPage={data.page}
              pageSize={data.pageSize}
              totalItems={data.totalCount}
            />
          )}
          <DataTable
            data={data?.items ?? []}
            columns={columns}
            keyExtractor={(p) => p.publicId}
            isLoading={isLoading}
            aria-label="Lista de propostas"
            emptyTitle="Nenhuma proposta encontrada"
            emptyDescription={
              termoBusca
                ? 'Revise o número pesquisado ou limpe a busca.'
                : 'Ainda não existem propostas disponíveis neste contexto.'
            }
          />
          {data && data.totalCount > data.pageSize && (
            <Pagination
              currentPage={data.page}
              totalPages={Math.ceil(data.totalCount / data.pageSize)}
              onPageChange={setPagina}
              disabled={isLoading}
            />
          )}
        </>
      )}
    </div>
  );
}
