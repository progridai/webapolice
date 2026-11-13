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
  descartarRascunho,
  efetivarCancelamentoVigencia,
  solicitarCancelamentoVigencia,
} from '../api/propostas.api';
import {
  encerramentoPropostaSchema,
  type EncerramentoPropostaFormData,
} from '../schemas/encerramentoProposta.schema';
import type { PropostaDetalhe } from '../types/proposta.types';
import { formatarDataOuVazio } from '../../../shared/utils/formatters';

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
  const [acao, setAcao] = useState<'cancelarVigencia' | 'descartar' | null>(
    null,
  );
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
        <Alert variant="info" title="Rascunho descartado">
          Os dados e o histórico desta tentativa foram preservados. A versão
          anterior, quando existente, continua vigente. Outra tentativa pode ser
          preparada sem reutilizar esta numeração.
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
                    Descartar rascunho
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
  acao: 'cancelarVigencia' | 'descartar';
  onFechar: () => void;
  onConcluida: (mensagem: string | null) => void;
}) {
  const enviando = useRef(false);
  const [erro, setErro] = useState<string | null>(null);
  const cancelando = acao === 'cancelarVigencia';
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
      if (cancelando) {
        const resultado = await solicitarCancelamentoVigencia(
          proposta.publicId,
          proposta.revisaoPreenchimento,
          dados.dataCancelamentoVigencia,
          dados.observacao,
        );
        onConcluida(
          resultado.agendada
            ? 'Cancelamento agendado. A proposta permanece vigente até a data informada.'
            : 'Vigência cancelada a partir da data informada. As versões anteriores foram preservadas.',
        );
      } else {
        await descartarRascunho(
          proposta.publicId,
          proposta.revisaoPreenchimento,
          dados.observacao,
        );
        onConcluida(
          'Rascunho descartado. Seus dados e o histórico foram preservados, liberando outra tentativa.',
        );
      }
    } catch (cause) {
      setErro(
        cause instanceof Error
          ? cause.message
          : 'Não foi possível encerrar a proposta.',
      );
    }
  }
  return (
    <form
      aria-label={
        cancelando ? 'Cancelamento da vigência' : 'Descarte de rascunho'
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
      <Alert
        variant="warning"
        title={
          cancelando
            ? 'Encerrar a vigência registrada'
            : 'Encerrar esta tentativa como Descartada'
        }
      >
        {cancelando
          ? 'Informe o primeiro dia sem vigência. Hoje ou data retroativa válida encerra agora; data futura agenda e mantém a proposta vigente até o processamento. Resolva primeiro qualquer rascunho ou substituição agendada. Versões anteriores não serão reativadas. A situação da Vida permanece independente e esta operação não recalcula nem ajusta cobranças.'
          : 'O rascunho será encerrado sem confirmação ou vigência. Seus dados e histórico serão preservados. A proposta anterior, quando existente, continuará vigente. Outra tentativa poderá ser preparada com nova numeração.'}
      </Alert>
      {cancelando && (
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
        <Alert variant="error" title="Não foi possível encerrar a proposta">
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
          {cancelando
            ? 'Confirmar cancelamento da vigência'
            : 'Confirmar descarte do rascunho'}
        </Button>
      </FormActions>
    </form>
  );
}
