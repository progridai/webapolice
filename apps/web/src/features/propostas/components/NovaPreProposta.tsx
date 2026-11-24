import { useCallback, useEffect, useRef, useState } from 'react';
import {
  Alert,
  Button,
  DataTable,
  DetailsSection,
  FilterBar,
  FormField,
  Input,
  Pagination,
  ResultsSummary,
} from '../../../components/ui';
import type { Column } from '../../../components/ui/DataTable/DataTable';
import { useAuthorization } from '../../../auth/AuthorizationProvider';
import { useConsulta } from '../../../shared/hooks/useConsulta';
import {
  listarApolices,
  listarApoliceVidas,
} from '../../apolices/api/apolices.api';
import type {
  ApoliceListItem,
  ApoliceVidaListItem,
} from '../../apolices/types/apolice.types';
import { CriarPropostaDialog } from './CriarPropostaDialog';

export function NovaPreProposta({
  onCancelar,
  onCriada,
}: {
  onCancelar: () => void;
  onCriada: (id: string) => void;
}) {
  const { possuiPermissao, possuiModulo, possuiRecurso } = useAuthorization();
  const [confirmando, setConfirmando] = useState(false);
  const [apolice, setApolice] = useState<ApoliceListItem | null>(null);
  const permitido =
    possuiModulo('APOLICES') &&
    possuiRecurso('PROPOSTAS') &&
    possuiPermissao('propostas.inserir') &&
    possuiPermissao('propostas.visualizar');
  return (
    <DetailsSection title="Nova pré proposta">
      <p>
        Selecione uma Apólice ativa e a Vida para criar a primeira pré proposta.
        Para uma Vida que já possui proposta, consulte o histórico e use a
        operação de nova versão ou reinclusão.
      </p>
      {!permitido ? (
        <Alert variant="warning" title="Acesso restrito">
          Você não possui permissão para criar pré propostas.
        </Alert>
      ) : !possuiPermissao('apolices.visualizar') ? (
        <Alert variant="warning" title="Consulta de Apólices necessária">
          Seu perfil precisa da permissão de consultar Apólices para selecionar
          a Apólice e a Vida neste fluxo.
        </Alert>
      ) : apolice ? (
        <SelecionarVida
          key={apolice.publicId}
          apolice={apolice}
          onConfirmando={setConfirmando}
          onVoltar={() => setApolice(null)}
          onCriada={onCriada}
        />
      ) : (
        <SelecionarApolice onSelecionar={setApolice} />
      )}
      <div>
        <Button variant="secondary" disabled={confirmando} onClick={onCancelar}>
          Cancelar nova pré proposta
        </Button>
      </div>
    </DetailsSection>
  );
}
function SelecionarApolice({
  onSelecionar,
}: {
  onSelecionar: (apolice: ApoliceListItem) => void;
}) {
  const titulo = useRef<HTMLHeadingElement>(null);
  useEffect(() => {
    titulo.current?.focus();
  }, []);
  const [busca, setBusca] = useState('');
  const [termo, setTermo] = useState('');
  const [pagina, setPagina] = useState(1);
  const load = useCallback(
    (signal: AbortSignal) =>
      listarApolices(
        { page: pagina, pageSize: 20, busca: termo || undefined, ativo: true },
        signal,
      ),
    [pagina, termo],
  );
  const { data, error, isLoading, refetch } = useConsulta(
    load,
    'Não foi possível carregar as Apólices.',
  );
  const columns: Column<ApoliceListItem>[] = [
    {
      key: 'numero',
      label: 'Apólice',
      render: (a) => a.numeroPrincipal || 'Sem número',
    },
    {
      key: 'estipulante',
      label: 'Estipulante',
      render: (a) => a.estipulanteNome,
    },
    { key: 'seguradora', label: 'Seguradora', render: (a) => a.seguradoraNome },
    {
      key: 'acoes',
      label: 'Selecionar',
      render: (a) => (
        <Button
          variant="text"
          disabled={isLoading || !a.ativo}
          aria-label={`Selecionar apólice ${a.numeroPrincipal || 'sem número'} de ${a.estipulanteNome}`}
          onClick={() => onSelecionar(a)}
        >
          Selecionar Apólice
        </Button>
      ),
    },
  ];
  return (
    <div className="flex flex-col gap-3">
      <h3
        ref={titulo}
        tabIndex={-1}
        className="text-lg font-semibold text-texto-principal"
      >
        1. Selecione a Apólice
      </h3>
      <form
        onSubmit={(e) => {
          e.preventDefault();
          setPagina(1);
          if (termo === busca.trim()) void refetch();
          else setTermo(busca.trim());
        }}
      >
        <FilterBar>
          <FormField label="Buscar Apólice">
            <Input
              id="nova-proposta-apolice-busca"
              value={busca}
              onChange={(e) => setBusca(e.target.value)}
            />
          </FormField>
          <Button type="submit" variant="secondary">
            Pesquisar Apólices
          </Button>
        </FilterBar>
      </form>
      {error ? (
        <Alert variant="error" title="Falha ao consultar Apólices">
          {error.message}
          <Button variant="secondary" onClick={() => void refetch()}>
            Tentar novamente
          </Button>
        </Alert>
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
            keyExtractor={(a) => a.publicId}
            isLoading={isLoading}
            aria-label="Apólices para nova pré proposta"
            emptyTitle="Nenhuma Apólice ativa encontrada"
            emptyDescription="Revise a busca ou cadastre uma Apólice ativa antes de criar a pré proposta."
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
function impedimento(vida: ApoliceVidaListItem) {
  if (!vida.ativo) return 'Vida inativa';
  if (vida.possuiPropostas)
    return 'Já possui proposta; consulte para preparar nova versão ou reinclusão';
  if (!vida.clienteVinculoPublicId)
    return 'Complete o vínculo do Cliente antes de criar a pré proposta';
  return null;
}
function SelecionarVida({
  apolice,
  onVoltar,
  onCriada,
  onConfirmando,
}: {
  apolice: ApoliceListItem;
  onVoltar: () => void;
  onCriada: (id: string) => void;
  onConfirmando: (confirmando: boolean) => void;
}) {
  const titulo = useRef<HTMLHeadingElement>(null);
  useEffect(() => {
    titulo.current?.focus();
  }, []);
  const [busca, setBusca] = useState('');
  const [termo, setTermo] = useState('');
  const [pagina, setPagina] = useState(1);
  const [vida, setVida] = useState<ApoliceVidaListItem | null>(null);
  useEffect(() => {
    onConfirmando(!!vida);
    return () => onConfirmando(false);
  }, [vida, onConfirmando]);
  const load = useCallback(
    (signal: AbortSignal) =>
      listarApoliceVidas(
        apolice.publicId,
        { page: pagina, pageSize: 20, busca: termo || undefined },
        signal,
      ),
    [apolice.publicId, pagina, termo],
  );
  const { data, error, isLoading, refetch } = useConsulta(
    load,
    'Não foi possível carregar as Vidas.',
  );
  const columns: Column<ApoliceVidaListItem>[] = [
    { key: 'cliente', label: 'Cliente', render: (v) => v.clienteNome },
    {
      key: 'matricula',
      label: 'Matrícula',
      render: (v) => v.vinculoMatricula || 'Não informada',
    },
    {
      key: 'subgrupo',
      label: 'Subgrupo',
      render: (v) => v.subgrupoNome || 'Não informado',
    },
    {
      key: 'modulo',
      label: 'Módulo',
      render: (v) => v.moduloNome || 'Não informado',
    },
    {
      key: 'elegibilidade',
      label: 'Disponibilidade',
      render: (v) => impedimento(v) ?? 'Disponível para primeira pré proposta',
    },
    {
      key: 'acoes',
      label: 'Selecionar',
      render: (v) => (
        <Button
          variant="text"
          disabled={isLoading || !!vida || impedimento(v) !== null}
          aria-label={`Selecionar Vida de ${v.clienteNome}, matrícula ${v.vinculoMatricula || 'não informada'}`}
          onClick={() => setVida(v)}
        >
          Selecionar Vida
        </Button>
      ),
    },
  ];
  return (
    <div className="flex flex-col gap-3">
      <h3
        ref={titulo}
        tabIndex={-1}
        className="text-lg font-semibold text-texto-principal"
      >
        2. Selecione a Vida
      </h3>
      <p>
        Apólice: {apolice.numeroPrincipal || 'Sem número'} —{' '}
        {apolice.estipulanteNome} — {apolice.seguradoraNome}.
      </p>
      <div>
        <Button variant="secondary" disabled={!!vida} onClick={onVoltar}>
          Trocar Apólice
        </Button>
      </div>
      <form
        onSubmit={(e) => {
          e.preventDefault();
          if (vida) return;
          setPagina(1);
          if (termo === busca.trim()) void refetch();
          else setTermo(busca.trim());
        }}
      >
        <FilterBar>
          <FormField label="Buscar Vida por nome ou documento">
            <Input
              id="nova-proposta-vida-busca"
              value={busca}
              disabled={!!vida}
              onChange={(e) => setBusca(e.target.value)}
            />
          </FormField>
          <Button type="submit" variant="secondary" disabled={!!vida}>
            Pesquisar Vidas
          </Button>
        </FilterBar>
      </form>
      {error ? (
        <Alert variant="error" title="Falha ao consultar Vidas">
          {error.message}
          <Button
            variant="secondary"
            disabled={!!vida}
            onClick={() => void refetch()}
          >
            Tentar novamente
          </Button>
        </Alert>
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
            keyExtractor={(v) => v.apoliceVidaPublicId}
            isLoading={isLoading}
            aria-label="Vidas para nova pré proposta"
            emptyTitle="Nenhuma Vida encontrada"
            emptyDescription="Revise a busca ou vincule uma Vida à Apólice antes de criar a pré proposta."
          />
          {data && data.totalCount > data.pageSize && (
            <Pagination
              currentPage={data.page}
              totalPages={Math.ceil(data.totalCount / data.pageSize)}
              onPageChange={setPagina}
              disabled={isLoading || !!vida}
            />
          )}
        </>
      )}
      {vida && (
        <CriarPropostaDialog
          apolicePublicId={apolice.publicId}
          vida={vida}
          estipulanteNome={apolice.estipulanteNome}
          onClose={() => setVida(null)}
          onCriada={onCriada}
        />
      )}
    </div>
  );
}
