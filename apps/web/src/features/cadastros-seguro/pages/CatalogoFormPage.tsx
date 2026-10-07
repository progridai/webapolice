import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import {
  PageHeader,
  Button,
  Input,
  Textarea,
  Select,
  FormField,
  FormSection,
  Alert,
  Spinner,
  DescriptionList,
  DescriptionItem,
  StatusBadge,
} from '../../../components/ui';
import { useAuthorization } from '../../../auth/AuthorizationProvider';
import {
  catalogosApi,
  type RegistroCatalogo,
  type ValorCampo,
  type BancoOpcao,
} from '../api/catalogos.api';
import type { ConfigCatalogo, CampoCatalogo } from '../catalogos.config';
import { formatarCep } from '../../../shared/utils/formatters';
import { PlanoCoberturas } from '../components/PlanoCoberturas';
export function CatalogoFormPage({
  config,
  leitura = false,
}: {
  config: ConfigCatalogo;
  leitura?: boolean;
}) {
  const { publicId } = useParams();
  const navigate = useNavigate();
  const { possuiPermissao } = useAuthorization();
  const [dados, setDados] = useState<Record<string, ValorCampo>>({ nome: '' });
  const [registro, setRegistro] = useState<RegistroCatalogo | null>(null);
  const [bancos, setBancos] = useState<BancoOpcao[]>([]);
  const [loading, setLoading] = useState(
    !!publicId || config.recurso === 'convenios-cobranca',
  );
  const [salvando, setSalvando] = useState(false);
  const [error, setError] = useState('');
  const [falhou, setFalhou] = useState(false);
  useEffect(() => {
    const controller = new AbortController();
    async function carregar() {
      setLoading(true);
      setError('');
      setFalhou(false);
      try {
        const [row, banks] = await Promise.all([
          publicId
            ? catalogosApi.obter(config.recurso, publicId, controller.signal)
            : Promise.resolve(null),
          config.recurso === 'convenios-cobranca'
            ? catalogosApi.bancos(controller.signal)
            : Promise.resolve([]),
        ]);
        if (!controller.signal.aborted) {
          setRegistro(row);
          setDados(row ?? { nome: '' });
          setBancos(banks);
        }
      } catch (err) {
        if (!controller.signal.aborted) {
          setFalhou(true);
          setError(
            err instanceof Error
              ? err.message
              : 'Não foi possível carregar o cadastro.',
          );
        }
      } finally {
        if (!controller.signal.aborted) setLoading(false);
      }
    }
    void carregar();
    return () => controller.abort();
  }, [config.recurso, publicId]);
  const set = (key: string, value: ValorCampo) =>
    setDados((prev) => ({ ...prev, [key]: value }));
  const salvar = async (e: React.FormEvent) => {
    e.preventDefault();
    setSalvando(true);
    setError('');
    try {
      const body: Record<string, ValorCampo> = {};
      for (const section of config.secoes)
        for (const field of section.campos) {
          const value = dados[field.key];
          body[field.key] =
            typeof value === 'string' ? value.trim() || null : (value ?? null);
        }
      const row = await catalogosApi.salvar(config.recurso, publicId, body);
      navigate(
        `${config.rota}/${row.publicId}${config.recurso === 'planos' && !publicId ? '/editar' : ''}`,
      );
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Não foi possível salvar.');
    } finally {
      setSalvando(false);
    }
  };
  const exibir = (field: CampoCatalogo) => {
    const value = dados[field.key];
    if (field.key === 'estCep')
      return formatarCep(value == null ? null : String(value));
    if (field.type === 'boolean')
      return value == null ? 'Não informado' : value ? 'Sim' : 'Não';
    if (field.type === 'banco')
      return bancos.find((b) => b.codigo === value)?.nome ?? value;
    return value == null || value === '' ? 'Não informado' : String(value);
  };
  if (loading) return <Spinner />;
  return (
    <main
      className="flex flex-col gap-6 p-6 max-w-4xl mx-auto focus:outline-none"
      tabIndex={-1}
    >
      <PageHeader
        title={
          leitura
            ? String(dados.nome ?? config.singular)
            : `${publicId ? 'Editar' : 'Novo'} ${config.singular}`
        }
        titleExtras={
          registro ? (
            <StatusBadge status={registro.ativo ? 'ativo' : 'inativo'} />
          ) : undefined
        }
        actions={
          leitura && possuiPermissao(`${config.permissao}.alterar`) ? (
            <Button
              onClick={() => navigate(`${config.rota}/${publicId}/editar`)}
            >
              Editar
            </Button>
          ) : undefined
        }
      />
      {error && (
        <Alert variant="error" title="Não foi possível concluir">
          {error}
        </Alert>
      )}
      {!falhou && (
        <form onSubmit={(e) => void salvar(e)} className="flex flex-col gap-3">
          {config.secoes.map((section) => (
            <FormSection key={section.titulo} title={section.titulo}>
              {leitura ? (
                <DescriptionList columns={2} density="compact">
                  {section.campos.map((field) => (
                    <DescriptionItem
                      key={field.key}
                      label={field.label}
                      value={exibir(field)}
                    />
                  ))}
                </DescriptionList>
              ) : (
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                  {section.campos.map((field) => (
                    <FormField
                      key={field.key}
                      label={field.label}
                      required={field.key === 'nome'}
                    >
                      {field.type === 'boolean' ? (
                        <Select
                          value={
                            dados[field.key] == null
                              ? ''
                              : String(dados[field.key])
                          }
                          onChange={(e) =>
                            set(
                              field.key,
                              e.target.value === ''
                                ? null
                                : e.target.value === 'true',
                            )
                          }
                          options={[
                            { value: '', label: 'Não informado' },
                            { value: 'true', label: 'Sim' },
                            { value: 'false', label: 'Não' },
                          ]}
                          disabled={salvando}
                        />
                      ) : field.type === 'banco' ? (
                        <Select
                          value={String(dados[field.key] ?? '')}
                          onChange={(e) =>
                            set(field.key, e.target.value || null)
                          }
                          options={[
                            { value: '', label: 'Não informado' },
                            ...bancos.map((b) => ({
                              value: b.codigo,
                              label: `${b.codigo} — ${b.nome}`,
                            })),
                          ]}
                          disabled={salvando}
                        />
                      ) : field.type === 'textarea' ? (
                        <Textarea
                          value={String(dados[field.key] ?? '')}
                          onChange={(e) => set(field.key, e.target.value)}
                          disabled={salvando}
                        />
                      ) : (
                        <Input
                          required={field.key === 'nome'}
                          maxLength={field.maxLength}
                          type={field.type === 'number' ? 'number' : 'text'}
                          step={field.type === 'number' ? 1 : undefined}
                          min={field.key === 'numeroArquivo' ? 0 : undefined}
                          value={String(dados[field.key] ?? '')}
                          onChange={(e) =>
                            set(
                              field.key,
                              field.type === 'number'
                                ? e.target.value === ''
                                  ? null
                                  : Number(e.target.value)
                                : e.target.value,
                            )
                          }
                          disabled={salvando}
                        />
                      )}
                    </FormField>
                  ))}
                </div>
              )}
            </FormSection>
          ))}
          {config.recurso === 'planos' && !publicId && (
            <FormSection title="Coberturas do Plano">
              <p className="text-texto-secundario">
                Salve o Plano para escolher e vincular suas Coberturas na
                próxima etapa.
              </p>
            </FormSection>
          )}
          <div className="flex justify-end gap-2">
            <Button
              type="button"
              variant="secondary"
              onClick={() => navigate(config.rota)}
              disabled={salvando}
            >
              Voltar
            </Button>
            {!leitura && (
              <Button type="submit" loading={salvando}>
                {config.recurso === 'planos' && !publicId
                  ? 'Salvar e vincular coberturas'
                  : 'Salvar'}
              </Button>
            )}
          </div>
        </form>
      )}
      {falhou && (
        <Button variant="secondary" onClick={() => navigate(config.rota)}>
          Voltar
        </Button>
      )}
      {!falhou && publicId && config.recurso === 'planos' && (
        <PlanoCoberturas
          publicId={publicId}
          planoAtivo={registro?.ativo ?? false}
        />
      )}
    </main>
  );
}
