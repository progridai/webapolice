import { useEffect, useState } from 'react';
import {
  Modal,
  Button,
  Input,
  Select,
  Textarea,
  Alert,
  FormField,
} from '../../../../components/ui';
import {
  criarModuloApoliceSchema,
  alterarModuloApoliceSchema,
} from '../../schemas/moduloApolice.schema';
import {
  criarApoliceModulo,
  alterarApoliceModulo,
} from '../../api/apolices.api';
import {
  modulosGlobaisApi,
  type ModuloGlobalListItem,
} from '../../api/modulosGlobais.api';
import type { ApoliceModuloResult } from '../../types/apolice.types';
import { PlanoModuloEditor } from '../PlanoModuloEditor';
import { useAuthorization } from '../../../../auth/AuthorizationProvider';

interface ModuloApoliceModalProps {
  aberto: boolean;
  onClose: () => void;
  apolicePublicId: string;
  modulosVinculados: ApoliceModuloResult[];
  moduloEdicao?: ApoliceModuloResult;
  somenteLeitura?: boolean;
  onSucesso: () => void;
}

export function ModuloApoliceModal({
  aberto,
  onClose,
  apolicePublicId,
  modulosVinculados,
  moduloEdicao,
  somenteLeitura = false,
  onSucesso,
}: ModuloApoliceModalProps) {
  const { possuiPermissao } = useAuthorization();
  // O UUID contextual permanece após criar o vínculo, permitindo cadastrar o Plano em seguida.
  const [vinculoPublicId, setVinculoPublicId] = useState(
    moduloEdicao?.publicId,
  );
  const [moduloPublicId, setModuloPublicId] = useState('');
  const [dataInicio, setDataInicio] = useState(
    moduloEdicao?.dataInicio?.substring(0, 10) ?? '',
  );
  const [dataFim, setDataFim] = useState(
    moduloEdicao?.dataFim?.substring(0, 10) ?? '',
  );
  const [observacao, setObservacao] = useState(moduloEdicao?.observacao ?? '');
  const [catalogo, setCatalogo] = useState<ModuloGlobalListItem[]>([]);
  const [carregando, setCarregando] = useState(!moduloEdicao);
  const [erroCatalogo, setErroCatalogo] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [erro, setErro] = useState('');
  const [sucesso, setSucesso] = useState('');
  const [errosCampos, setErrosCampos] = useState<Record<string, string>>({});
  const podeEditarVinculo =
    !somenteLeitura &&
    (!vinculoPublicId || possuiPermissao('apolices.modulos.alterar'));
  useEffect(() => {
    if (vinculoPublicId || !aberto) return;
    let cancelado = false;
    void modulosGlobaisApi
      .listar({ ativo: true, tamanhoPagina: 200 })
      .then((res) => {
        if (cancelado) return;
        const vinculados = new Set(
          modulosVinculados.map((m) => m.moduloPublicId),
        );
        setCatalogo(res.items.filter((m) => !vinculados.has(m.publicId)));
      })
      .catch((err) => {
        if (!cancelado)
          setErroCatalogo(
            err instanceof Error
              ? err.message
              : 'Não foi possível carregar os Módulos disponíveis.',
          );
      })
      .finally(() => {
        if (!cancelado) setCarregando(false);
      });
    return () => {
      cancelado = true;
    };
  }, [aberto, vinculoPublicId, modulosVinculados]);
  async function salvar(e: React.FormEvent) {
    e.preventDefault();
    setErro('');
    setSucesso('');
    const dados = {
      moduloPublicId,
      dataInicio: dataInicio || null,
      dataFim: dataFim || null,
      observacao: observacao.trim() || null,
    };
    const validacao = vinculoPublicId
      ? alterarModuloApoliceSchema.safeParse(dados)
      : criarModuloApoliceSchema.safeParse(dados);
    const campos: Record<string, string> = {};
    if (!validacao.success)
      validacao.error.issues.forEach((i) => {
        campos[String(i.path[0])] = i.message;
      });
    setErrosCampos(campos);
    if (!validacao.success) return;
    setSubmitting(true);
    try {
      if (vinculoPublicId) {
        await alterarApoliceModulo(apolicePublicId, vinculoPublicId, dados);
        setSucesso('Vínculo atualizado.');
      } else {
        const salvo = await criarApoliceModulo(apolicePublicId, dados);
        setVinculoPublicId(salvo.publicId);
        setSucesso(
          'Módulo vinculado. Cadastre abaixo o Plano e suas Coberturas.',
        );
      }
      onSucesso();
    } catch (err) {
      setErro(
        err instanceof Error
          ? err.message
          : 'Não foi possível salvar o vínculo.',
      );
    } finally {
      setSubmitting(false);
    }
  }
  const nomeModulo =
    moduloEdicao?.nome ??
    catalogo.find((m) => m.publicId === moduloPublicId)?.nome;
  return (
    <Modal
      aberto={aberto}
      onClose={() => !submitting && onClose()}
      title={
        somenteLeitura
          ? 'Consultar Módulo da Apólice'
          : vinculoPublicId
            ? 'Editar Vínculo de Módulo'
            : 'Vincular Módulo'
      }
      size="large"
      footer={
        <>
          <Button variant="secondary" disabled={submitting} onClick={onClose}>
            {vinculoPublicId ? 'Concluir' : 'Cancelar'}
          </Button>
          {podeEditarVinculo && (
            <Button
              type="submit"
              form="modulo-apolice-form"
              loading={submitting}
            >
              {vinculoPublicId
                ? 'Salvar vínculo'
                : 'Salvar vínculo e continuar'}
            </Button>
          )}
        </>
      }
    >
      <div className="flex flex-col gap-4">
        {erro && (
          <Alert variant="error" title="Erro ao salvar vínculo">
            {erro}
          </Alert>
        )}
        {sucesso && (
          <Alert variant="success" title="Salvo">
            {sucesso}
          </Alert>
        )}
        <form
          id="modulo-apolice-form"
          onSubmit={(e) => void salvar(e)}
          className="flex flex-col gap-3"
        >
          <fieldset
            disabled={submitting || !podeEditarVinculo}
            className="flex flex-col gap-3"
          >
            {!vinculoPublicId ? (
              <>
                <FormField
                  label="Módulo"
                  required
                  error={errosCampos.moduloPublicId}
                >
                  <Select
                    placeholder={
                      carregando
                        ? 'Carregando Módulos...'
                        : 'Selecione um Módulo'
                    }
                    value={moduloPublicId}
                    disabled={carregando}
                    options={catalogo.map((m) => ({
                      label: m.nome,
                      value: m.publicId,
                    }))}
                    onChange={(e) => setModuloPublicId(e.target.value)}
                  />
                </FormField>
                {erroCatalogo && (
                  <Alert variant="error" title="Erro ao carregar Módulos">
                    {erroCatalogo}
                  </Alert>
                )}
                {!carregando && !erroCatalogo && catalogo.length === 0 && (
                  <p className="text-sm text-texto-secundario">
                    Nenhum Módulo disponível para vincular a esta Apólice.
                  </p>
                )}
                <p className="text-sm text-texto-secundario">
                  Salve o vínculo para cadastrar seu Plano e suas Coberturas na
                  próxima etapa.
                </p>
              </>
            ) : (
              <p className="font-medium text-texto-principal">
                Módulo: {nomeModulo}
              </p>
            )}
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
              <FormField
                label="Início de Vigência"
                error={errosCampos.dataInicio}
              >
                <Input
                  type="date"
                  value={dataInicio}
                  onChange={(e) => setDataInicio(e.target.value)}
                />
              </FormField>
              <FormField label="Fim de Vigência" error={errosCampos.dataFim}>
                <Input
                  type="date"
                  value={dataFim}
                  onChange={(e) => setDataFim(e.target.value)}
                />
              </FormField>
            </div>
            <FormField label="Observação" error={errosCampos.observacao}>
              <Textarea
                value={observacao}
                onChange={(e) => setObservacao(e.target.value)}
              />
            </FormField>
          </fieldset>
        </form>
        {vinculoPublicId && (
          <PlanoModuloEditor
            key={vinculoPublicId}
            apolicePublicId={apolicePublicId}
            moduloPublicId={vinculoPublicId}
          />
        )}
      </div>
    </Modal>
  );
}
