import { useEffect, useRef, useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import {
  Alert,
  Button,
  DetailsSection,
  FormActions,
  FormField,
  Input,
  Textarea,
} from '../../../components/ui';
import {
  alterarCancelamentoVigencia,
  descartarRascunho,
  efetivarCancelamentoVigencia,
  retirarCancelamentoVigencia,
  solicitarCancelamentoVigencia,
} from '../api/propostas.api';
import {
  encerramentoPropostaSchema,
  type EncerramentoPropostaFormData,
} from '../schemas/encerramentoProposta.schema';
import type { PropostaDetalhe } from '../types/proposta.types';
import { formatarDataOuVazio } from '../../../shared/utils/formatters';

type AcaoEncerramento = EncerramentoPropostaFormData['acao'];
const textosAcao = {
  cancelarVigencia: {
    formulario: 'Cancelamento da vigência',
    titulo: 'Encerrar a vigência registrada',
    explicacao:
      'Informe o primeiro dia sem vigência. Hoje ou data retroativa válida encerra agora; data futura agenda e mantém a proposta vigente até o processamento. Resolva primeiro qualquer pré proposta ou substituição agendada. Versões anteriores não serão reativadas. A situação da Vida permanece independente e esta operação não recalcula nem ajusta cobranças.',
    confirmar: 'Confirmar cancelamento da vigência',
  },
  descartar: {
    formulario: 'Descarte de pré proposta',
    titulo: 'Encerrar esta tentativa como Descartada',
    explicacao:
      'A pré proposta será encerrada sem confirmação ou vigência. Seus dados e histórico serão preservados. A proposta anterior, quando existente, continuará vigente. Outra tentativa poderá ser preparada com nova numeração.',
    confirmar: 'Confirmar descarte da pré proposta',
  },
  alterarCancelamento: {
    formulario: 'Alteração do pedido de cancelamento',
    titulo: 'Alterar a data do pedido de cancelamento',
    explicacao:
      'Informe outra data como primeiro dia sem vigência. Hoje ou data retroativa válida cancela a vigência imediatamente; data futura mantém a proposta vigente até o processamento. É permitido alterar um pedido pendente, mesmo com a data alcançada, até sua efetivação. O histórico será preservado. A Vida permanece independente e não há ajuste de cobranças ou reativação de versões anteriores.',
    confirmar: 'Confirmar alteração do pedido',
  },
  retirarCancelamento: {
    formulario: 'Retirada do pedido de cancelamento',
    titulo: 'Retirar o pedido e manter a vigência',
    explicacao:
      'A proposta continuará vigente e o pedido de cancelamento será retirado, preservando o histórico. É permitido retirar um pedido pendente, mesmo com a data alcançada, até sua efetivação. A Vida, as condições e as cobranças serão preservadas.',
    confirmar: 'Confirmar retirada do pedido',
  },
} satisfies Record<
  AcaoEncerramento,
  { formulario: string; titulo: string; explicacao: string; confirmar: string }
>;

export function EncerramentoProposta({
  proposta,
  podeAlterar,
  disabled,
  onBusy,
  onConcluida,
}: {
  proposta: PropostaDetalhe;
  podeAlterar: boolean;
  disabled: boolean;
  onBusy: (busy: boolean) => void;
  onConcluida: (mensagem: string | null) => void;
}) {
  const [acao, setAcao] = useState<AcaoEncerramento | null>(null);
  const [efetivando, setEfetivando] = useState(false);
  const [erro, setErro] = useState<string | null>(null);
  const enviando = useRef(false);
  useEffect(() => {
    onBusy(acao !== null || efetivando);
    return () => onBusy(false);
  }, [acao, efetivando, onBusy]);
  async function efetivar() {
    if (enviando.current) return;
    enviando.current = true;
    setEfetivando(true);
    setErro(null);
    try {
      const resultado = await efetivarCancelamentoVigencia(
        proposta.publicId,
        proposta.revisaoPreenchimento,
      );
      onConcluida(
        resultado.cancelada
          ? 'Vigência cancelada a partir da data registrada. As versões anteriores foram preservadas.'
          : null,
      );
    } catch (cause) {
      setErro(
        cause instanceof Error
          ? cause.message
          : 'Não foi possível efetivar o cancelamento.',
      );
    } finally {
      enviando.current = false;
      setEfetivando(false);
    }
  }
  return (
    <>
      {proposta.situacaoVigenciaCodigo === 'DESCARTADA' && (
        <Alert variant="info" title="Pré proposta descartada">
          {proposta.reinclusao ? (
            'Os dados e o histórico desta tentativa de reinclusão foram preservados. A origem permanece cancelada. Outra tentativa pode ser preparada pela proposta de origem.'
          ) : (
            <>
              Os dados e o histórico desta tentativa foram preservados. A versão
              anterior, quando existente, continua vigente. Outra tentativa pode
              ser preparada sem reutilizar esta numeração.
            </>
          )}
        </Alert>
      )}
      {proposta.situacaoVigenciaCodigo === 'CANCELAMENTO_AGENDADO' && (
        <Alert variant="warning" title="Cancelamento de vigência agendado">
          A proposta continua vigente até{' '}
          {formatarDataOuVazio(proposta.dataCancelamentoVigencia)}, primeiro dia
          sem vigência. A solicitação está registrada no histórico; novas
          versões ficam bloqueadas. Versões anteriores não serão reativadas.
        </Alert>
      )}
      {proposta.situacaoVigenciaCodigo === 'CANCELAMENTO_PENDENTE' && (
        <Alert variant="warning" title="Cancelamento de vigência pendente">
          {proposta.motivoPendenciaVigencia ??
            'A data de cancelamento chegou e o encerramento aguarda processamento.'}
        </Alert>
      )}
      {erro && (
        <Alert variant="error" title="Não foi possível efetivar o cancelamento">
          {erro}
          <Button
            variant="secondary"
            disabled={efetivando}
            onClick={() => onConcluida(null)}
          >
            Atualizar proposta
          </Button>
        </Alert>
      )}
      {podeAlterar &&
        (proposta.podeCancelarVigencia ||
          proposta.podeDescartarRascunho ||
          proposta.podeAlterarCancelamento ||
          proposta.podeRetirarCancelamento ||
          proposta.podeEfetivarCancelamento) && (
          <DetailsSection title="Encerramento da proposta">
            {acao ? (
              <EncerramentoForm
                proposta={proposta}
                acao={acao}
                onFechar={() => setAcao(null)}
                onConcluida={(mensagem) => {
                  setAcao(null);
                  onConcluida(mensagem);
                }}
              />
            ) : (
              <div className="flex flex-wrap gap-2">
                {proposta.podeAlterarCancelamento && (
                  <Button
                    variant="secondary"
                    disabled={disabled || efetivando}
                    onClick={() => setAcao('alterarCancelamento')}
                  >
                    Alterar pedido de cancelamento
                  </Button>
                )}
                {proposta.podeRetirarCancelamento && (
                  <Button
                    variant="secondary"
                    disabled={disabled || efetivando}
                    onClick={() => setAcao('retirarCancelamento')}
                  >
                    Retirar pedido de cancelamento
                  </Button>
                )}
                {proposta.podeCancelarVigencia && (
                  <Button
                    variant="secondary"
                    disabled={disabled || efetivando}
                    onClick={() => setAcao('cancelarVigencia')}
                  >
                    Cancelar vigência
                  </Button>
                )}
                {proposta.podeDescartarRascunho && (
                  <Button
                    variant="secondary"
                    disabled={disabled || efetivando}
                    onClick={() => setAcao('descartar')}
                  >
                    Descartar pré proposta
                  </Button>
                )}
                {proposta.podeEfetivarCancelamento && (
                  <Button
                    disabled={disabled}
                    loading={efetivando}
                    onClick={() => void efetivar()}
                  >
                    Efetivar cancelamento agora
                  </Button>
                )}
              </div>
            )}
          </DetailsSection>
        )}
    </>
  );
}

function EncerramentoForm({
  proposta,
  acao,
  onFechar,
  onConcluida,
}: {
  proposta: PropostaDetalhe;
  acao: AcaoEncerramento;
  onFechar: () => void;
  onConcluida: (mensagem: string | null) => void;
}) {
  const enviando = useRef(false);
  const [erro, setErro] = useState<string | null>(null);
  const informaData =
    acao === 'cancelarVigencia' || acao === 'alterarCancelamento';
  const textos = textosAcao[acao];
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<EncerramentoPropostaFormData>({
    resolver: zodResolver(encerramentoPropostaSchema),
    defaultValues: { acao, dataCancelamentoVigencia: '', observacao: '' },
  });
  async function salvar(dados: EncerramentoPropostaFormData) {
    setErro(null);
    try {
      if (informaData) {
        const executar =
          acao === 'alterarCancelamento'
            ? alterarCancelamentoVigencia
            : solicitarCancelamentoVigencia;
        const resultado = await executar(
          proposta.publicId,
          proposta.revisaoPreenchimento,
          dados.dataCancelamentoVigencia,
          dados.observacao,
        );
        onConcluida(
          resultado.agendada
            ? acao === 'alterarCancelamento'
              ? 'Pedido de cancelamento alterado. A proposta permanece vigente até a nova data informada.'
              : 'Cancelamento agendado. A proposta permanece vigente até a data informada.'
            : 'Vigência cancelada a partir da data informada. As versões anteriores foram preservadas.',
        );
      } else if (acao === 'retirarCancelamento') {
        await retirarCancelamentoVigencia(
          proposta.publicId,
          proposta.revisaoPreenchimento,
          dados.observacao,
        );
        onConcluida(
          'Pedido de cancelamento retirado. A proposta continua vigente e o histórico foi preservado.',
        );
      } else {
        await descartarRascunho(
          proposta.publicId,
          proposta.revisaoPreenchimento,
          dados.observacao,
        );
        onConcluida(
          'Pré proposta descartada. Seus dados e o histórico foram preservados, liberando outra tentativa.',
        );
      }
    } catch (cause) {
      setErro(
        cause instanceof Error
          ? cause.message
          : 'Não foi possível concluir a operação.',
      );
    }
  }
  return (
    <form
      aria-label={textos.formulario}
      noValidate
      className="flex flex-col gap-3"
      onSubmit={(event) => {
        if (enviando.current) {
          event.preventDefault();
          return;
        }
        enviando.current = true;
        void handleSubmit(salvar)(event).finally(() => {
          enviando.current = false;
        });
      }}
    >
      <Alert variant="warning" title={textos.titulo}>
        {proposta.reinclusao && acao === 'descartar'
          ? 'Esta pré proposta de reinclusão será descartada, preservando seus dados e histórico. A origem permanecerá cancelada. Outra tentativa poderá ser preparada pela proposta de origem com nova numeração.'
          : textos.explicacao}
      </Alert>
      {(acao === 'alterarCancelamento' || acao === 'retirarCancelamento') && (
        <p>
          Data registrada no pedido:{' '}
          {formatarDataOuVazio(proposta.dataCancelamentoVigencia)}.
        </p>
      )}
      {informaData && (
        <FormField
          label="Data de cancelamento da vigência"
          required
          error={errors.dataCancelamentoVigencia?.message}
        >
          <Input
            id="proposta-data-cancelamento"
            type="date"
            disabled={isSubmitting}
            {...register('dataCancelamentoVigencia')}
          />
        </FormField>
      )}
      <FormField
        label="Justificativa"
        required
        error={errors.observacao?.message}
      >
        <Textarea
          id="proposta-justificativa-encerramento"
          maxLength={1000}
          disabled={isSubmitting}
          {...register('observacao')}
        />
      </FormField>
      {erro && (
        <Alert variant="error" title="Não foi possível concluir a operação">
          {erro}
          <Button
            type="button"
            variant="secondary"
            disabled={isSubmitting}
            onClick={() => onConcluida(null)}
          >
            Recarregar proposta e descartar alterações
          </Button>
        </Alert>
      )}
      <FormActions sticky={false}>
        <Button
          type="button"
          variant="secondary"
          disabled={isSubmitting}
          onClick={onFechar}
        >
          Voltar sem alterar
        </Button>
        <Button type="submit" loading={isSubmitting}>
          {textos.confirmar}
        </Button>
      </FormActions>
    </form>
  );
}
