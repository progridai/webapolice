import { EncerramentoProposta } from './EncerramentoProposta';
import { GestaoAgendamentoProposta } from './GestaoAgendamentoProposta';
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
} from '../../../components/ui';
import { confirmarProposta, efetivarVigencia } from '../api/propostas.api';
import {
  confirmacaoPropostaSchema,
  type ConfirmacaoPropostaFormData,
} from '../schemas/confirmacaoProposta.schema';
import type {
  ConfirmacaoPropostaResultado,
  PropostaDetalhe,
} from '../types/proposta.types';
import { formatarDataOuVazio } from '../../../shared/utils/formatters';

export function ConfirmacaoProposta({
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
  const [confirmando, setConfirmando] = useState(false);
  const [encerrando, setEncerrando] = useState(false);
  const [gerindo, setGerindo] = useState(false);
  const [efetivando, setEfetivando] = useState(false);
  const [erro, setErro] = useState<string | null>(null);
  const enviando = useRef(false);
  const ocupado = confirmando || efetivando || gerindo || encerrando;
  useEffect(() => {
    onBusy(ocupado);
    return () => onBusy(false);
  }, [ocupado, onBusy]);

  async function efetivar() {
    if (enviando.current) return;
    enviando.current = true;
    setEfetivando(true);
    setErro(null);
    try {
      const resultado = await efetivarVigencia(
        proposta.publicId,
        proposta.revisaoPreenchimento,
      );
      onConcluida(
        resultado.ativada
          ? proposta.reinclusao
            ? 'Reinclusão efetivada. A proposta de origem permanece cancelada.'
            : 'Vigência ativada. A substituição da versão anterior, quando existente, foi concluída.'
          : null,
      );
    } catch (cause) {
      setErro(
        cause instanceof Error
          ? cause.message
          : 'Não foi possível efetivar a vigência.',
      );
    } finally {
      enviando.current = false;
      setEfetivando(false);
    }
  }

  return (
    <>
      {proposta.situacaoVigenciaCodigo === 'CANCELADA' && (
        <Alert
          variant="info"
          title={
            proposta.dataFimVigencia
              ? 'Vigência cancelada'
              : 'Agendamento cancelado'
          }
        >
          {proposta.dataFimVigencia
            ? 'A vigência foi encerrada na data registrada, sem reativar versões anteriores. Os dados e a justificativa foram preservados no histórico.'
            : 'Esta versão foi encerrada sem entrar em vigência. A proposta anterior, quando existente, foi preservada. A justificativa está no histórico.'}
        </Alert>
      )}
      <EncerramentoProposta
        proposta={proposta}
        podeAlterar={podeAlterar}
        disabled={disabled || confirmando || efetivando || gerindo}
        onBusy={setEncerrando}
        onConcluida={onConcluida}
      />
      <GestaoAgendamentoProposta
        proposta={proposta}
        podeAlterar={podeAlterar}
        disabled={disabled || confirmando || efetivando || encerrando}
        onBusy={setGerindo}
        onConcluida={onConcluida}
      />
      {proposta.situacaoVigenciaCodigo === 'AGENDADA' && (
        <Alert variant="info" title="Início de vigência agendado">
          A proposta está confirmada para início em{' '}
          {formatarDataOuVazio(proposta.dataInicioVigencia)}. A versão anterior,
          {proposta.reinclusao
            ? 'de origem, permanece cancelada; a reinclusão só terá vigência na ativação.'
            : 'quando existente, só será substituída na ativação da nova vigência.'}
        </Alert>
      )}
      {proposta.situacaoVigenciaCodigo === 'ATIVACAO_PENDENTE' && (
        <Alert variant="warning" title="Ativação da vigência pendente">
          {proposta.motivoPendenciaVigencia ??
            'A data de início chegou e a vigência aguarda processamento.'}
        </Alert>
      )}
      {erro && (
        <Alert variant="error" title="Não foi possível efetivar a vigência">
          {erro}
          <Button
            variant="secondary"
            disabled={ocupado}
            onClick={() => onConcluida(null)}
          >
            Atualizar proposta
          </Button>
        </Alert>
      )}
      {podeAlterar && proposta.podeConfirmar && (
        <DetailsSection title="Confirmação da proposta">
          {confirmando ? (
            <ConfirmacaoPropostaForm
              proposta={proposta}
              onCancelar={() => setConfirmando(false)}
              onConfirmada={(resultado) => {
                setConfirmando(false);
                onConcluida(
                  resultado.agendada
                    ? proposta.reinclusao
                      ? 'Reinclusão confirmada com início agendado. A origem permanece cancelada e o intervalo anterior ao novo início permanece sem cobertura.'
                      : 'Proposta confirmada com início agendado. A versão anterior, quando existente, permanece vigente até a ativação.'
                    : proposta.reinclusao
                      ? 'Reinclusão confirmada e vigente a partir da data informada. A origem permanece cancelada.'
                      : 'Proposta confirmada e vigente. A substituição da versão anterior, quando existente, foi concluída.',
                );
              }}
              onRecarregar={() => {
                setConfirmando(false);
                onConcluida(null);
              }}
            />
          ) : (
            <Button
              disabled={disabled || efetivando || gerindo || encerrando}
              onClick={() => setConfirmando(true)}
            >
              Confirmar proposta
            </Button>
          )}
        </DetailsSection>
      )}
      {podeAlterar && proposta.podeEfetivarVigencia && (
        <DetailsSection title="Efetivar vigência">
          <p>
            {proposta.reinclusao ? (
              'A ativação revalidará as condições da reinclusão e preservará a origem cancelada.'
            ) : (
              <>
                A data de início chegou. A ativação revalidará as condições da
                participação e substituirá a versão anterior somente se todas as
                verificações passarem.
              </>
            )}
          </p>
          <div>
            <Button
              disabled={disabled || gerindo || confirmando || encerrando}
              loading={efetivando}
              onClick={() => void efetivar()}
            >
              Efetivar vigência agora
            </Button>
          </div>
        </DetailsSection>
      )}
    </>
  );
}

export function ConfirmacaoPropostaForm({
  proposta,
  onCancelar,
  onConfirmada,
  onRecarregar,
}: {
  proposta: PropostaDetalhe;
  onCancelar: () => void;
  onConfirmada: (resultado: ConfirmacaoPropostaResultado) => void;
  onRecarregar: () => void;
}) {
  const enviando = useRef(false);
  const [erro, setErro] = useState<string | null>(null);
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<ConfirmacaoPropostaFormData>({
    resolver: zodResolver(confirmacaoPropostaSchema),
    defaultValues: { dataInicioVigencia: '' },
  });
  async function confirmar(dados: ConfirmacaoPropostaFormData) {
    setErro(null);
    try {
      onConfirmada(
        await confirmarProposta(
          proposta.publicId,
          proposta.revisaoPreenchimento,
          dados.dataInicioVigencia,
        ),
      );
    } catch (cause) {
      setErro(
        cause instanceof Error
          ? cause.message
          : 'Não foi possível confirmar a proposta.',
      );
    }
  }
  return (
    <form
      aria-label="Confirmação da proposta"
      noValidate
      className="flex flex-col gap-3"
      onSubmit={(event) => {
        if (enviando.current) {
          event.preventDefault();
          return;
        }
        enviando.current = true;
        void handleSubmit(confirmar)(event).finally(() => {
          enviando.current = false;
        });
      }}
    >
      <p>
        Confira as condições registradas e os beneficiários, que devem somar
        100%. Após confirmar, o preenchimento ficará disponível para consulta.
      </p>
      <p>
        {proposta.reinclusao ? (
          <>
            A origem permanece cancelada. Informe início igual ou posterior a{' '}
            {formatarDataOuVazio(proposta.dataFimVigenciaOrigem)}. Hoje ou
            retroativa válida efetiva a reinclusão agora; futura aguarda
            ativação. Eventual intervalo permanece sem cobertura. Esta
            confirmação não gera cobrança.
          </>
        ) : (
          <>
            Para início futuro, a versão anterior continua vigente até a
            ativação. Para hoje ou data retroativa válida, a substituição será
            efetivada agora a partir da data informada. Esta confirmação não
            gera cobrança.
          </>
        )}
      </p>
      <FormField
        label="Data de início da vigência"
        required
        error={errors.dataInicioVigencia?.message}
      >
        <Input
          id="proposta-inicio-confirmacao"
          type="date"
          disabled={isSubmitting}
          {...register('dataInicioVigencia')}
        />
      </FormField>
      {erro && (
        <Alert variant="error" title="Não foi possível confirmar">
          {erro}
          <Button
            type="button"
            variant="secondary"
            disabled={isSubmitting}
            onClick={onRecarregar}
          >
            Recarregar proposta e descartar a data
          </Button>
        </Alert>
      )}
      <FormActions sticky={false}>
        <Button
          type="button"
          variant="secondary"
          disabled={isSubmitting}
          onClick={onCancelar}
        >
          Cancelar confirmação
        </Button>
        <Button type="submit" loading={isSubmitting}>
          Confirmar com esta data
        </Button>
      </FormActions>
    </form>
  );
}
