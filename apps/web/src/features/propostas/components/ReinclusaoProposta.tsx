import { useEffect, useRef, useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import {
  Alert,
  Button,
  DetailsSection,
  FormActions,
  FormField,
  Select,
} from '../../../components/ui';
import { prepararReinclusao } from '../api/propostas.api';
import {
  reinclusaoPropostaSchema,
  type ReinclusaoPropostaFormData,
} from '../schemas/reinclusaoProposta.schema';
import type { PropostaDetalhe } from '../types/proposta.types';

export function ReinclusaoProposta({
  proposta,
  podeInserir,
  disabled,
  onBusy,
  onCriada,
  onRecarregar,
}: {
  proposta: PropostaDetalhe;
  podeInserir: boolean;
  disabled: boolean;
  onBusy: (busy: boolean) => void;
  onCriada: (id: string) => void;
  onRecarregar: () => void;
}) {
  const [aberta, setAberta] = useState(false);
  useEffect(() => {
    onBusy(aberta);
    return () => onBusy(false);
  }, [aberta, onBusy]);
  if (!podeInserir || !proposta.podePrepararReinclusao) return null;
  return (
    <DetailsSection title="Reinclusão da proposta">
      {aberta ? (
        <ReinclusaoForm
          proposta={proposta}
          onFechar={() => setAberta(false)}
          onCriada={(id) => {
            setAberta(false);
            onCriada(id);
          }}
          onRecarregar={() => {
            setAberta(false);
            onRecarregar();
          }}
        />
      ) : (
        <Button
          variant="secondary"
          disabled={disabled}
          onClick={() => setAberta(true)}
        >
          Preparar reinclusão
        </Button>
      )}
    </DetailsSection>
  );
}

function ReinclusaoForm({
  proposta,
  onFechar,
  onCriada,
  onRecarregar,
}: {
  proposta: PropostaDetalhe;
  onFechar: () => void;
  onCriada: (id: string) => void;
  onRecarregar: () => void;
}) {
  const enviando = useRef(false);
  const [erro, setErro] = useState<string | null>(null);
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<ReinclusaoPropostaFormData>({
    resolver: zodResolver(reinclusaoPropostaSchema),
  });
  async function salvar(dados: ReinclusaoPropostaFormData) {
    setErro(null);
    try {
      onCriada(
        await prepararReinclusao(
          proposta.publicId,
          proposta.revisaoPreenchimento,
          dados.origemCondicoes,
        ),
      );
    } catch (cause) {
      setErro(
        cause instanceof Error
          ? cause.message
          : 'Não foi possível preparar a reinclusão.',
      );
    }
  }
  return (
    <form
      aria-label="Preparação de reinclusão"
      className="flex flex-col gap-3"
      noValidate
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
      <Alert variant="info" title="Criar outra versão de pré proposta">
        A proposta cancelada será preservada. Escolha a origem do Plano, das
        coberturas e dos prêmios. As condições anteriores incluem seus ajustes e
        justificativas; a opção do Módulo usa os valores atuais. Ambas exigem
        elegibilidade na confirmação. Os beneficiários nativos serão copiados
        para revisão. Esta preparação não concede cobertura nem gera cobrança. O
        início será informado ao confirmar, sem anteceder o primeiro dia sem
        cobertura da encerrada. Eventual intervalo permanecerá sem cobertura.
      </Alert>
      <FormField
        label="Origem das condições"
        required
        error={errors.origemCondicoes?.message}
      >
        <Select
          id="reinclusao-origem-condicoes"
          defaultValue=""
          placeholder="Selecione a origem"
          disabled={isSubmitting}
          {...register('origemCondicoes')}
        >
          <option value="ANTERIOR">
            Proposta anterior — preservar condições e ajustes
          </option>
          <option value="MODULO">Módulo atual — usar condições atuais</option>
        </Select>
      </FormField>
      {erro && (
        <Alert variant="error" title="Não foi possível preparar a reinclusão">
          {erro}
          <Button
            type="button"
            variant="secondary"
            disabled={isSubmitting}
            onClick={onRecarregar}
          >
            Recarregar proposta e descartar escolha
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
          Voltar sem criar
        </Button>
        <Button type="submit" loading={isSubmitting}>
          Criar pré proposta de reinclusão
        </Button>
      </FormActions>
    </form>
  );
}
