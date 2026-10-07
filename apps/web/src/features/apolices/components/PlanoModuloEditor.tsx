import { Link } from 'react-router-dom';
import { ROUTES } from '../../../app/routes/routePaths';
import { useCallback, useEffect, useState } from 'react';
import {
  Alert,
  Button,
  FormField,
  Input,
  Select,
  StatusBadge,
} from '../../../components/ui';
import { useAuthorization } from '../../../auth/AuthorizationProvider';
import { formatarPremio } from '../../../shared/utils/formatters';
import { planoModuloApi } from '../api/planoModulo.api';
import type {
  ConfiguracaoPlanoModulo,
  PlanoModulo,
  PlanoModuloDados,
  CoberturaModulo,
  CoberturaModuloDados,
} from '../api/planoModulo.api';

const mensagem = (err: unknown) =>
  err instanceof Error
    ? err.message
    : 'Não foi possível salvar. Tente novamente.';
const simNao = (valor: string): boolean | null =>
  valor === '' ? null : valor === 'true';
const inicial = (valor: boolean | null | undefined) =>
  valor == null ? '' : String(valor);
const opcoes = [
  { value: '', label: 'Não informado' },
  { value: 'true', label: 'Sim' },
  { value: 'false', label: 'Não' },
];

export function PlanoModuloEditor({
  apolicePublicId,
  moduloPublicId,
}: {
  apolicePublicId: string;
  moduloPublicId: string;
}) {
  const { possuiPermissao } = useAuthorization();
  const [config, setConfig] = useState<ConfiguracaoPlanoModulo | null>(null);
  const [erro, setErro] = useState('');
  const [carregando, setCarregando] = useState(true);
  const [editarPlano, setEditarPlano] = useState(false);
  const [editarCobertura, setEditarCobertura] = useState<
    CoberturaModulo | null | undefined
  >(undefined);
  const carregar = useCallback(
    (signal?: AbortSignal) => {
      return planoModuloApi
        .obter(apolicePublicId, moduloPublicId, signal)
        .then((dados) => {
          if (!signal?.aborted) {
            setConfig(dados);
            setErro('');
          }
        })
        .catch((err) => {
          if (!signal?.aborted) setErro(mensagem(err));
        })
        .finally(() => {
          if (!signal?.aborted) setCarregando(false);
        });
    },
    [apolicePublicId, moduloPublicId],
  );
  useEffect(() => {
    const controller = new AbortController();
    void carregar(controller.signal);
    return () => controller.abort();
  }, [carregar]);
  const podeAlterar =
    possuiPermissao('apolices.modulos.alterar') && config?.podeAlterar;
  if (carregando) return <p role="status">Carregando Plano e Coberturas...</p>;
  if (erro)
    return (
      <Alert variant="error" title="Erro ao carregar Plano">
        <p>{erro}</p>
        <Button onClick={() => void carregar()}>Tentar novamente</Button>
      </Alert>
    );
  const plano = config?.plano;
  return (
    <section
      className="flex flex-col gap-4 border-t border-borda pt-4"
      aria-label="Plano e Coberturas do Módulo"
    >
      <div>
        <h3 className="font-semibold text-texto-principal">
          Plano e Coberturas deste Módulo
        </h3>
        <p className="text-sm text-texto-secundario">
          Este Módulo possui um único Plano. Selecione Coberturas do cadastro e
          defina os prêmios próprios para este vínculo na Apólice.
        </p>
      </div>
      {possuiPermissao('coberturas.visualizar') && (
        <Link
          to={ROUTES.COBERTURAS}
          target="_blank"
          rel="noopener noreferrer"
          className="text-marca-principal underline"
        >
          Cadastro de Coberturas
        </Link>
      )}
      {!config?.podeAlterar && (
        <p className="text-sm text-texto-secundario">
          A Apólice ou o Módulo está inativo. Os dados estão disponíveis para
          consulta.
        </p>
      )}
      {podeAlterar && (!plano || editarPlano) ? (
        <PlanoFormulario
          key={plano?.publicId ?? 'novo'}
          plano={plano}
          onCancelar={plano ? () => setEditarPlano(false) : undefined}
          onSalvar={async (dados) => {
            const salvo = await planoModuloApi.salvarPlano(
              apolicePublicId,
              moduloPublicId,
              dados,
            );
            setConfig((anterior) => anterior && { ...anterior, plano: salvo });
            setEditarPlano(false);
          }}
        />
      ) : plano ? (
        <div className="flex flex-col gap-2">
          <div className="flex items-center justify-between gap-3">
            <p className="font-medium">{plano.nome}</p>
            <StatusBadge status={plano.ativo ? 'ativo' : 'inativo'} />
            {podeAlterar && (
              <Button
                variant="secondary"
                size="small"
                onClick={() => setEditarPlano(true)}
              >
                Editar Plano
              </Button>
            )}
          </div>
          <p className="text-sm text-texto-secundario">
            Ramo: {plano.ramo || 'Não informado'} • Paga:{' '}
            {plano.paga == null ? 'Não informado' : plano.paga ? 'Sim' : 'Não'}{' '}
            • Reajuste:{' '}
            {plano.reajuste == null
              ? 'Não informado'
              : plano.reajuste
                ? 'Sim'
                : 'Não'}
          </p>
        </div>
      ) : (
        <p>Nenhum Plano cadastrado neste Módulo.</p>
      )}
      {plano && (
        <>
          <div className="flex justify-between items-center gap-3">
            <h4 className="font-medium">Coberturas do Plano</h4>
            {podeAlterar && plano.ativo && editarCobertura === undefined && (
              <Button size="small" onClick={() => setEditarCobertura(null)}>
                Vincular Cobertura
              </Button>
            )}
          </div>
          {!plano.ativo && (
            <p className="text-sm text-texto-secundario">
              Reative o Plano para alterar suas Coberturas.
            </p>
          )}
          {plano.coberturas.length === 0 ? (
            <p className="text-sm text-texto-secundario">
              Nenhuma Cobertura vinculada a este Plano.
            </p>
          ) : (
            <div className="overflow-x-auto">
              <table
                className="w-full text-sm"
                aria-label="Coberturas do Plano deste Módulo"
              >
                <thead>
                  <tr className="text-left border-b border-borda">
                    <th className="p-2">Cobertura</th>
                    <th className="p-2">Prêmio titular</th>
                    <th className="p-2">Prêmio cônjuge</th>
                    <th className="p-2">Status</th>
                    <th className="p-2">Ações</th>
                  </tr>
                </thead>
                <tbody>
                  {plano.coberturas.map((c) => (
                    <tr key={c.publicId} className="border-b border-borda">
                      <td className="p-2">
                        {c.nome}
                        <div className="text-texto-secundario text-xs">
                          {c.nomeReduzido}
                          {c.basica ? ` • ${c.basica}` : ''}
                        </div>
                      </td>
                      <td className="p-2">{formatarPremio(c.premioTitular)}</td>
                      <td className="p-2">{formatarPremio(c.premioConjuge)}</td>
                      <td className="p-2">
                        <StatusBadge status={c.ativo ? 'ativo' : 'inativo'} />
                        {!c.coberturaAtiva && (
                          <p className="text-xs text-texto-secundario">
                            Cadastro inativo
                          </p>
                        )}
                      </td>
                      <td className="p-2">
                        {podeAlterar &&
                          plano.ativo &&
                          editarCobertura === undefined && (
                            <Button
                              variant="secondary"
                              size="small"
                              onClick={() => setEditarCobertura(c)}
                            >
                              Editar vínculo
                            </Button>
                          )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
          {editarCobertura !== undefined && podeAlterar && plano.ativo && (
            <CoberturaFormulario
              key={editarCobertura?.publicId ?? 'nova'}
              cobertura={editarCobertura}
              apolicePublicId={apolicePublicId}
              moduloPublicId={moduloPublicId}
              vinculadas={plano.coberturas}
              onCancelar={() => setEditarCobertura(undefined)}
              onSalvar={async (dados) => {
                const salvo = await planoModuloApi.salvarCobertura(
                  apolicePublicId,
                  moduloPublicId,
                  editarCobertura?.publicId,
                  dados,
                );
                setConfig((anterior) =>
                  anterior?.plano
                    ? {
                        ...anterior,
                        plano: {
                          ...anterior.plano,
                          coberturas: [
                            ...anterior.plano.coberturas.filter(
                              (c) => c.publicId !== salvo.publicId,
                            ),
                            salvo,
                          ],
                        },
                      }
                    : anterior,
                );
                setEditarCobertura(undefined);
              }}
            />
          )}
        </>
      )}
    </section>
  );
}

function PlanoFormulario({
  plano,
  onCancelar,
  onSalvar,
}: {
  plano?: PlanoModulo | null;
  onCancelar?: () => void;
  onSalvar: (dados: PlanoModuloDados) => Promise<void>;
}) {
  const [nome, setNome] = useState(plano?.nome ?? '');
  const [ramo, setRamo] = useState(plano?.ramo ?? '');
  const [paga, setPaga] = useState(inicial(plano?.paga));
  const [reajuste, setReajuste] = useState(inicial(plano?.reajuste));
  const [ativo, setAtivo] = useState(plano?.ativo ?? true);
  const [salvando, setSalvando] = useState(false);
  const [erro, setErro] = useState('');
  async function salvar(e: React.FormEvent) {
    e.preventDefault();
    setErro('');
    if (!nome.trim()) {
      setErro('Informe o nome do Plano.');
      return;
    }
    setSalvando(true);
    try {
      await onSalvar({
        nome: nome.trim(),
        ramo: ramo.trim() || null,
        paga: simNao(paga),
        reajuste: simNao(reajuste),
        ativo,
      });
    } catch (err) {
      setErro(mensagem(err));
    } finally {
      setSalvando(false);
    }
  }
  return (
    <form
      onSubmit={(e) => void salvar(e)}
      className="flex flex-col gap-3"
      aria-label="Cadastro do Plano do Módulo"
    >
      {erro && (
        <Alert variant="error" title="Erro ao salvar Plano">
          {erro}
        </Alert>
      )}
      <fieldset
        disabled={salvando}
        className="grid grid-cols-1 sm:grid-cols-2 gap-3"
      >
        <FormField label="Nome do Plano" required>
          <Input
            value={nome}
            maxLength={150}
            onChange={(e) => setNome(e.target.value)}
            required
          />
        </FormField>
        <FormField label="Ramo">
          <Input
            value={ramo}
            maxLength={80}
            onChange={(e) => setRamo(e.target.value)}
          />
        </FormField>
        <FormField label="Paga">
          <Select
            options={opcoes}
            value={paga}
            onChange={(e) => setPaga(e.target.value)}
          />
        </FormField>
        <FormField label="Reajuste do Plano">
          <Select
            options={opcoes}
            value={reajuste}
            onChange={(e) => setReajuste(e.target.value)}
          />
        </FormField>
        <label className="flex items-center gap-2">
          <input
            type="checkbox"
            checked={ativo}
            onChange={(e) => setAtivo(e.target.checked)}
          />
          Plano ativo
        </label>
      </fieldset>
      <div className="flex justify-end gap-2">
        {onCancelar && (
          <Button variant="secondary" disabled={salvando} onClick={onCancelar}>
            Cancelar edição do Plano
          </Button>
        )}
        <Button type="submit" loading={salvando}>
          {plano ? 'Salvar Plano' : 'Criar Plano'}
        </Button>
      </div>
    </form>
  );
}

function CoberturaFormulario({
  cobertura,
  apolicePublicId,
  moduloPublicId,
  vinculadas,
  onCancelar,
  onSalvar,
}: {
  cobertura: CoberturaModulo | null;
  apolicePublicId: string;
  moduloPublicId: string;
  vinculadas: CoberturaModulo[];
  onCancelar: () => void;
  onSalvar: (dados: CoberturaModuloDados) => Promise<void>;
}) {
  const [coberturaPublicId, setCoberturaPublicId] = useState(
    cobertura?.coberturaPublicId ?? '',
  );
  const [opcoesCoberturas, setOpcoesCoberturas] = useState<
    { publicId: string; nome: string }[]
  >([]);
  const [carregando, setCarregando] = useState(!cobertura);
  const [erroCatalogo, setErroCatalogo] = useState('');
  const [ativo, setAtivo] = useState(cobertura?.ativo ?? true);
  const [titular, setTitular] = useState(
    cobertura ? cobertura.premioTitular.toFixed(2).replace('.', ',') : '',
  );
  const [conjuge, setConjuge] = useState(
    cobertura ? cobertura.premioConjuge.toFixed(2).replace('.', ',') : '',
  );
  const [salvando, setSalvando] = useState(false);
  const [erro, setErro] = useState('');
  const carregarOpcoes = useCallback(
    (signal?: AbortSignal) => {
      async function buscar() {
        const todos: { publicId: string; nome: string }[] = [];
        let pagina = 1;
        while (!signal?.aborted) {
          const res = await planoModuloApi.opcoesCoberturas(
            apolicePublicId,
            moduloPublicId,
            pagina,
            signal,
          );
          todos.push(...res.items);
          if (!res.items.length || todos.length >= res.totalCount) break;
          pagina++;
        }
        return todos;
      }
      return buscar()
        .then((todos) => {
          if (!signal?.aborted) {
            setOpcoesCoberturas(todos);
            setErroCatalogo('');
          }
        })
        .catch((err) => {
          if (!signal?.aborted) setErroCatalogo(mensagem(err));
        })
        .finally(() => {
          if (!signal?.aborted) setCarregando(false);
        });
    },
    [apolicePublicId, moduloPublicId],
  );
  useEffect(() => {
    if (cobertura) return;
    const controller = new AbortController();
    void carregarOpcoes(controller.signal);
    return () => controller.abort();
  }, [cobertura, carregarOpcoes]);
  const disponiveis = opcoesCoberturas.filter(
    (c) => !vinculadas.some((v) => v.coberturaPublicId === c.publicId),
  );
  async function salvar(e: React.FormEvent) {
    e.preventDefault();
    setErro('');
    if (!coberturaPublicId) {
      setErro('Selecione uma Cobertura do cadastro.');
      return;
    }
    if (
      ![titular, conjuge].every(
        (v) =>
          /^\d+(?:[.,]\d{1,2})?$/.test(v.trim()) &&
          Number(v.replace(',', '.')) < 1e16,
      )
    ) {
      setErro(
        'Informe os dois prêmios em R$, com valores não negativos e até duas casas decimais. Use zero quando não houver cobrança.',
      );
      return;
    }
    setSalvando(true);
    try {
      await onSalvar({
        coberturaPublicId,
        premioTitular: Number(titular.replace(',', '.')),
        premioConjuge: Number(conjuge.replace(',', '.')),
        ativo,
      });
    } catch (err) {
      setErro(mensagem(err));
    } finally {
      setSalvando(false);
    }
  }
  return (
    <form
      onSubmit={(e) => void salvar(e)}
      className="flex flex-col gap-3 border border-borda rounded p-3"
      aria-label="Vínculo da Cobertura no Plano do Módulo"
    >
      <h4 className="font-medium">
        {cobertura
          ? 'Editar vínculo da Cobertura'
          : 'Vincular Cobertura ao Plano'}
      </h4>
      {erro && (
        <Alert variant="error" title="Erro ao salvar vínculo">
          {erro}
        </Alert>
      )}
      {erroCatalogo && (
        <Alert variant="error" title="Erro ao carregar Coberturas">
          {erroCatalogo}
        </Alert>
      )}
      <fieldset
        disabled={salvando}
        className="grid grid-cols-1 sm:grid-cols-2 gap-3"
      >
        {cobertura ? (
          <p className="sm:col-span-2 font-medium">
            Cobertura: {cobertura.nome}
          </p>
        ) : (
          <FormField label="Cobertura" required>
            <Select
              value={coberturaPublicId}
              disabled={carregando || !!erroCatalogo}
              placeholder={
                carregando
                  ? 'Carregando Coberturas...'
                  : 'Selecione uma Cobertura'
              }
              options={disponiveis.map((c) => ({
                value: c.publicId,
                label: c.nome,
              }))}
              onChange={(e) => setCoberturaPublicId(e.target.value)}
              required
            />
          </FormField>
        )}
        {!cobertura && (
          <div className="text-sm text-texto-secundario">
            {!carregando && !erroCatalogo && disponiveis.length === 0 && (
              <p>
                Nenhuma Cobertura ativa disponível. Cadastre uma Cobertura ou
                edite um vínculo existente.
              </p>
            )}
            <Button
              variant="secondary"
              size="small"
              disabled={carregando}
              onClick={() => {
                setCarregando(true);
                void carregarOpcoes();
              }}
            >
              Atualizar Coberturas
            </Button>
          </div>
        )}
        <FormField
          label="Prêmio titular (R$)"
          required
          hint="Use zero quando não houver cobrança."
        >
          <Input
            inputMode="decimal"
            value={titular}
            onChange={(e) => setTitular(e.target.value)}
            required
          />
        </FormField>
        <FormField label="Prêmio cônjuge (R$)" required>
          <Input
            inputMode="decimal"
            value={conjuge}
            onChange={(e) => setConjuge(e.target.value)}
            required
          />
        </FormField>
        <label className="flex items-center gap-2">
          <input
            type="checkbox"
            checked={ativo}
            disabled={
              !!cobertura && !cobertura.coberturaAtiva && !cobertura.ativo
            }
            onChange={(e) => setAtivo(e.target.checked)}
          />
          Vínculo ativo
        </label>
        {cobertura && !cobertura.coberturaAtiva && (
          <p className="text-sm text-texto-secundario">
            A Cobertura está inativa no cadastro. Para reativar um vínculo,
            reative primeiro a Cobertura.
          </p>
        )}
      </fieldset>
      <div className="flex justify-end gap-2">
        <Button variant="secondary" disabled={salvando} onClick={onCancelar}>
          Cancelar edição do vínculo
        </Button>
        <Button
          type="submit"
          loading={salvando}
          disabled={
            !cobertura && (carregando || !!erroCatalogo || !disponiveis.length)
          }
        >
          Salvar vínculo da Cobertura
        </Button>
      </div>
    </form>
  );
}
