import { useEffect, useState, useCallback } from 'react';
import { Link } from 'react-router-dom';
import { PremiosCoberturaModal } from './PremiosCoberturaModal';
import { formatarPremio } from '../../../shared/utils/formatters';
import { ROUTES } from '../../../app/routes/routePaths';
import {
  Alert,
  Button,
  Select,
  DataTable,
  StatusBadge,
  FormField,
  ConfirmDialog,
} from '../../../components/ui';
import type { Column } from '../../../components/ui/DataTable/DataTable';
import { useAuthorization } from '../../../auth/AuthorizationProvider';
import {
  catalogosApi,
  type CoberturaPlano,
  type RegistroCatalogo,
} from '../api/catalogos.api';
export function PlanoCoberturas({
  publicId,
  planoAtivo,
}: {
  publicId: string;
  planoAtivo: boolean;
}) {
  const { possuiPermissao } = useAuthorization();
  const podeGerenciar = possuiPermissao('planos.coberturas.alterar');
  const [rows, setRows] = useState<CoberturaPlano[]>([]);
  const [opcoes, setOpcoes] = useState<RegistroCatalogo[]>([]);
  const [escolha, setEscolha] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(true);
  const [salvando, setSalvando] = useState(false);
  const [editar, setEditar] = useState<CoberturaPlano | null>(null);
  const [confirmar, setConfirmar] = useState<CoberturaPlano | null>(null);
  const carregar = useCallback(
    async (signal?: AbortSignal) => {
      const buscar = async () => {
        const links = await catalogosApi.coberturasPlano(publicId, signal);
        const options: RegistroCatalogo[] = [];
        if (podeGerenciar && possuiPermissao('coberturas.visualizar')) {
          let pagina = 1;
          let total: number;
          do {
            const response = await catalogosApi.listar(
              'coberturas',
              { pagina, tamanhoPagina: 100, ativo: true },
              signal,
            );
            options.push(...response.items);
            total = response.totalCount;
            pagina++;
          } while (options.length < total);
        }
        return { links, options };
      };
      return buscar()
        .then(({ links, options }) => {
          if (!signal?.aborted) {
            setRows(links);
            setOpcoes(options);
            setError('');
          }
        })
        .catch((err) => {
          if (!signal?.aborted)
            setError(
              err instanceof Error
                ? err.message
                : 'Não foi possível carregar as coberturas.',
            );
        })
        .finally(() => {
          if (!signal?.aborted) setLoading(false);
        });
    },
    [publicId, podeGerenciar, possuiPermissao],
  );
  useEffect(() => {
    const c = new AbortController();
    void carregar(c.signal);
    return () => c.abort();
  }, [carregar]);
  const salvar = async (id: string, ativo: boolean) => {
    setSalvando(true);
    setError('');
    try {
      await catalogosApi.vincular(publicId, id, ativo);
      setEscolha('');
      setConfirmar(null);
      await carregar();
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : 'Não foi possível salvar o vínculo.',
      );
    } finally {
      setSalvando(false);
    }
  };
  const columns: Column<CoberturaPlano>[] = [
    { key: 'nome', label: 'Cobertura' },
    {
      key: 'premioTitular',
      label: 'Prêmio titular',
      render: (row) => formatarPremio(row.premioTitular),
    },
    {
      key: 'premioConjuge',
      label: 'Prêmio cônjuge',
      render: (row) => formatarPremio(row.premioConjuge),
    },
    {
      key: 'ativo',
      label: 'Vínculo',
      render: (row) => <StatusBadge status={row.ativo ? 'ativo' : 'inativo'} />,
    },
    {
      key: 'coberturaAtiva',
      label: 'Cadastro base',
      render: (row) => (
        <StatusBadge status={row.coberturaAtiva ? 'ativo' : 'inativo'} />
      ),
    },
    {
      key: 'acoes',
      label: 'Ações',
      render: (row) =>
        podeGerenciar ? (
          <div className="flex gap-2">
            {planoAtivo && (!row.ativo || row.coberturaAtiva) && (
              <Button
                variant="secondary"
                size="small"
                disabled={salvando}
                onClick={() => setEditar(row)}
              >
                Editar prêmios
              </Button>
            )}
            {(row.ativo ||
              (planoAtivo &&
                row.coberturaAtiva &&
                row.premioTitular != null &&
                row.premioConjuge != null)) && (
              <Button
                variant="secondary"
                size="small"
                disabled={salvando}
                onClick={() =>
                  row.ativo
                    ? setConfirmar(row)
                    : void salvar(row.coberturaPublicId, true)
                }
              >
                {row.ativo ? 'Inativar vínculo' : 'Reativar vínculo'}
              </Button>
            )}
          </div>
        ) : null,
    },
  ];
  return (
    <section className="flex flex-col gap-3">
      <h2 className="text-lg font-semibold text-texto-principal">
        Coberturas do Plano
      </h2>
      {error && (
        <Alert variant="error" title="Não foi possível concluir">
          {error}
          <Button variant="secondary" onClick={() => void carregar()}>
            Tentar novamente
          </Button>
        </Alert>
      )}
      {podeGerenciar && !possuiPermissao('coberturas.visualizar') && (
        <Alert variant="info" title="Consulta de coberturas">
          Para adicionar uma cobertura, seu perfil precisa da permissão de
          visualizar Coberturas.
        </Alert>
      )}
      {podeGerenciar &&
        possuiPermissao('coberturas.visualizar') &&
        !loading &&
        !error &&
        opcoes.length === 0 && (
          <Alert variant="info" title="Nenhuma Cobertura ativa cadastrada">
            Cadastre ou reative uma Cobertura para disponibilizá-la neste Plano.
            {possuiPermissao('coberturas.inserir') && (
              <Link to={`${ROUTES.COBERTURAS}/novo`} className="underline ml-2">
                Cadastrar Cobertura
              </Link>
            )}
          </Alert>
        )}
      {podeGerenciar &&
        planoAtivo &&
        possuiPermissao('coberturas.visualizar') && (
          <div className="flex gap-3 items-end">
            <FormField label="Adicionar cobertura">
              <Select
                value={escolha}
                onChange={(e) => setEscolha(e.target.value)}
                disabled={salvando || loading}
                options={[
                  { value: '', label: 'Selecione uma cobertura' },
                  ...opcoes
                    .filter(
                      (o) =>
                        !rows.some(
                          (r) => r.coberturaPublicId === o.publicId && r.ativo,
                        ),
                    )
                    .map((o) => ({ value: o.publicId, label: o.nome })),
                ]}
              />
            </FormField>
            <Button
              disabled={!escolha || loading || salvando}
              onClick={() => {
                const cobertura = opcoes.find((o) => o.publicId === escolha);
                if (cobertura) {
                  const anterior = rows.find(
                    (r) => r.coberturaPublicId === escolha,
                  );
                  setEditar({
                    coberturaPublicId: escolha,
                    nome: cobertura.nome,
                    ativo: true,
                    coberturaAtiva: true,
                    premioTitular: anterior?.premioTitular ?? null,
                    premioConjuge: anterior?.premioConjuge ?? null,
                  });
                }
              }}
            >
              Vincular
            </Button>
          </div>
        )}
      <DataTable
        columns={columns}
        data={rows}
        keyExtractor={(r) => r.coberturaPublicId}
        isLoading={loading}
        emptyTitle="Nenhuma cobertura vinculada"
      />
      {editar && (
        <PremiosCoberturaModal
          nome={editar.nome}
          valores={{
            premioTitular: editar.premioTitular,
            premioConjuge: editar.premioConjuge,
          }}
          onClose={() => setEditar(null)}
          onSalvar={async (premios) => {
            await catalogosApi.salvarPremiosPlano(
              publicId,
              editar.coberturaPublicId,
              premios,
              editar.ativo,
            );
            setEscolha('');
            await carregar();
          }}
        />
      )}
      <ConfirmDialog
        aberto={!!confirmar}
        onClose={() => !salvando && setConfirmar(null)}
        onConfirm={() =>
          confirmar && void salvar(confirmar.coberturaPublicId, false)
        }
        title="Inativar vínculo"
        description="A cobertura permanecerá no histórico do Plano."
        confirmText="Inativar"
        loading={salvando}
      />
    </section>
  );
}
