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
import { cancelarAgendamento, reagendarProposta } from '../api/propostas.api';
import {
  gestaoAgendamentoSchema,
  type GestaoAgendamentoFormData,
} from '../schemas/gestaoAgendamento.schema';
import type { PropostaDetalhe } from '../types/proposta.types';
import { formatarDataOuVazio } from '../../../shared/utils/formatters';

export function GestaoAgendamentoProposta({
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
  const [acao, setAcao] = useState<'reagendar' | 'cancelar' | null>(null);
  useEffect(() => {
    onBusy(acao !== null);
    return () => onBusy(false);
  }, [acao, onBusy]);
  if (
    !podeAlterar ||
    (!proposta.podeReagendar && !proposta.podeCancelarAgendamento)
  )
    return null;
  return (
    <DetailsSection title="Gerenciar agendamento">
      {acao ? (
        <GestaoAgendamentoForm
          key={acao}
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
          {proposta.podeReagendar && (
            <Button
              variant="secondary"
              disabled={disabled}
              onClick={() => setAcao('reagendar')}
            >
              Reagendar início
            </Button>
          )}
          {proposta.podeCancelarAgendamento && (
            <Button
              variant="secondary"
              disabled={disabled}
              onClick={() => setAcao('cancelar')}
            >
              Cancelar agendamento
            </Button>
          )}
        </div>
      )}
    </DetailsSection>
  );
}

function GestaoAgendamentoForm({
  proposta,
  acao,
  onFechar,
  onConcluida,
}: {
  proposta: PropostaDetalhe;
  acao: 'reagendar' | 'cancelar';
  onFechar: () => void;
  onConcluida: (mensagem: string | null) => void;
}) {
  const enviando = useRef(false);
  const [erro, setErro] = useState<string | null>(null);
  const reagendando = acao === 'reagendar';
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<GestaoAgendamentoFormData>({
    resolver: zodResolver(gestaoAgendamentoSchema),
    defaultValues: { acao, dataInicioVigencia: '', observacao: '' },
  });
  async function salvar(dados: GestaoAgendamentoFormData) {
    setErro(null);
    try {
      if (reagendando) {
        await reagendarProposta(
          proposta.publicId,
          proposta.revisaoPreenchimento,
          dados.dataInicioVigencia,
          dados.observacao,
        );
        onConcluida(
          'Início reagendado. A versão anterior, quando existente, permanece vigente até a ativação.',
        );
      } else {
        await cancelarAgendamento(
          proposta.publicId,
          proposta.revisaoPreenchimento,
          dados.observacao,
        );
        onConcluida(
          'Agendamento cancelado. Esta versão foi encerrada e a proposta anterior foi preservada.',
        );
      }
    } catch (cause) {
      setErro(
        cause instanceof Error
          ? cause.message
          : 'Não foi possível alterar o agendamento.',
      );
    }
  }
  return (
    <form
      aria-label={
        reagendando
          ? 'Reagendamento da proposta'
          : 'Cancelamento do agendamento'
      }
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
      <p>
        Início planejado: {formatarDataOuVazio(proposta.dataInicioVigencia)}.
      </p>
      {reagendando ? (
        <p>
          Informe outra data futura. As condições serão revalidadas e a versão
          anterior continuará vigente até a ativação. O reagendamento não gera
          cobrança.
        </p>
      ) : (
        <Alert variant="warning" title="Encerrar esta versão como Cancelada">
          Esta proposta não entrará em vigência. Seus dados e histórico serão
          preservados. A versão anterior, quando existente, continuará vigente.
          Para novas condições, prepare outra versão a partir da vigente ou uma
          nova tentativa pela Vida quando não houver vigente.
        </Alert>
      )}
      {reagendando && (
        <FormField
          label="Novo início da vigência"
          required
          error={errors.dataInicioVigencia?.message}
        >
          <Input
            id="proposta-inicio-reagendamento"
            type="date"
            disabled={isSubmitting}
            {...register('dataInicioVigencia')}
          />
        </FormField>
      )}
      <FormField
        label="Justificativa"
        required
        error={errors.observacao?.message}
      >
        <Textarea
          id="proposta-justificativa-agendamento"
          maxLength={1000}
          disabled={isSubmitting}
          {...register('observacao')}
        />
      </FormField>
      {erro && (
        <Alert variant="error" title="Não foi possível alterar o agendamento">
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
          {reagendando
            ? 'Salvar nova data'
            : 'Confirmar cancelamento do agendamento'}
        </Button>
      </FormActions>
    </form>
  );
}
