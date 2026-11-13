import { useRef, useState } from 'react';
import { useForm, useFieldArray } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import {
  Alert,
  Button,
  FormActions,
  FormField,
  FormSection,
  Input,
  Textarea,
} from '../../../components/ui';
import { salvarRascunho } from '../api/propostas.api';
import {
  criarPreenchimentoSchema,
  dadosPreenchimento,
  payloadPreenchimento,
  type PreenchimentoFormData,
} from '../schemas/preenchimentoRascunho.schema';
import type { PropostaDetalhe } from '../types/proposta.types';

export function PreenchimentoRascunhoForm({
  proposta,
  onCancelar,
  onSalvo,
  onBusy,
  onRecarregar,
}: {
  proposta: PropostaDetalhe;
  onCancelar: () => void;
  onSalvo: () => void;
  onBusy?: (busy: boolean) => void;
  onRecarregar?: () => void;
}) {
  const enviando = useRef(false);
  const [erro, setErro] = useState<string | null>(null);
  const [descartar, setDescartar] = useState<'cancelar' | 'recarregar' | null>(
    null,
  );
  const {
    control,
    register,
    handleSubmit,
    formState: { errors, isSubmitting, isDirty },
  } = useForm<PreenchimentoFormData>({
    resolver: zodResolver(criarPreenchimentoSchema(proposta)),
    defaultValues: dadosPreenchimento(proposta),
  });
  const { fields, append, remove } = useFieldArray({
    control,
    name: 'beneficiarios',
  });
  const coberturas = proposta.coberturas.filter((c) => c.podeAjustarPremio);
  async function salvar(dados: PreenchimentoFormData) {
    setErro(null);
    onBusy?.(true);
    try {
      await salvarRascunho(
        proposta.publicId,
        payloadPreenchimento(dados, proposta.revisaoPreenchimento),
      );
      onSalvo();
    } catch (cause) {
      setErro(
        cause instanceof Error
          ? cause.message
          : 'Não foi possível salvar o rascunho.',
      );
    } finally {
      onBusy?.(false);
    }
  }
  return (
    <form
      aria-label="Preenchimento do rascunho"
      className="flex flex-col gap-4"
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
      <p className="text-sm text-texto-secundario">
        Salvar mantém a proposta sem vigência. A participação pode ficar abaixo
        de 100% nesta etapa. Valores vazios significam ausência de valor; zero é
        um valor informado.
      </p>
      {erro && (
        <Alert variant="error" title="Não foi possível salvar">
          {erro}
          {onRecarregar && (
            <Button
              type="button"
              variant="secondary"
              disabled={isSubmitting}
              onClick={() =>
                isDirty ? setDescartar('recarregar') : onRecarregar()
              }
            >
              Recarregar proposta
            </Button>
          )}
        </Alert>
      )}
      <FormField label="Observação do rascunho">
        <Textarea
          id="rascunho-observacao"
          {...register('observacao')}
          disabled={isSubmitting}
        />
      </FormField>
      <FormSection title="Beneficiários do rascunho">
        {proposta.beneficiarios.some((b) => !b.podeAlterar) && (
          <p>
            Os beneficiários da estrutura anterior continuam preservados na
            consulta e participam da soma dos percentuais.
          </p>
        )}
        {fields.length === 0 && (
          <p>Nenhum beneficiário indicado no rascunho.</p>
        )}
        {errors.beneficiarios?.root?.message && (
          <Alert variant="error" title="Revise a participação">
            {errors.beneficiarios.root.message}
          </Alert>
        )}
        {errors.beneficiarios?.message && (
          <Alert variant="error" title="Revise a participação">
            {errors.beneficiarios.message}
          </Alert>
        )}
        {fields.map((field, index) => (
          <fieldset
            key={field.id}
            disabled={isSubmitting}
            className="flex flex-col gap-3 border-b border-borda pb-3"
          >
            <legend className="font-medium">Beneficiário {index + 1}</legend>
            <FormField
              label={`Nome do beneficiário ${index + 1}`}
              required
              error={errors.beneficiarios?.[index]?.nome?.message}
            >
              <Input
                id={`beneficiario-nome-${field.id}`}
                maxLength={150}
                {...register(`beneficiarios.${index}.nome`)}
              />
            </FormField>
            <FormField
              label={`Parentesco do beneficiário ${index + 1}`}
              required
              error={
                errors.beneficiarios?.[index]?.parentescoRegistrado?.message
              }
            >
              <Input
                id={`beneficiario-parentesco-${field.id}`}
                maxLength={100}
                {...register(`beneficiarios.${index}.parentescoRegistrado`)}
              />
            </FormField>
            <FormField
              label={`Participação (%) do beneficiário ${index + 1}`}
              required
              error={
                errors.beneficiarios?.[index]?.percentualParticipacao?.message
              }
            >
              <Input
                id={`beneficiario-percentual-${field.id}`}
                inputMode="decimal"
                {...register(`beneficiarios.${index}.percentualParticipacao`)}
              />
            </FormField>
            <div>
              <Button
                type="button"
                variant="secondary"
                onClick={() => remove(index)}
              >
                Remover beneficiário {index + 1}
              </Button>
            </div>
          </fieldset>
        ))}
        <div>
          <Button
            type="button"
            variant="secondary"
            disabled={isSubmitting}
            onClick={() =>
              append({
                publicId: null,
                nome: '',
                parentescoRegistrado: '',
                percentualParticipacao: '',
              })
            }
          >
            Adicionar beneficiário
          </Button>
        </div>
        <p className="text-sm text-texto-secundario">
          As inclusões e remoções só serão gravadas ao salvar.
        </p>
      </FormSection>
      <FormSection title="Prêmios das coberturas">
        {coberturas.length === 0 && (
          <p>Nenhuma cobertura disponível para ajuste.</p>
        )}
        {coberturas.map((c, index) => (
          <fieldset
            key={c.publicId}
            disabled={isSubmitting}
            className="flex flex-col gap-3 border-b border-borda pb-3"
          >
            <legend className="font-medium">
              {c.nomeRegistrado ?? c.nomeAtual ?? `Cobertura ${index + 1}`}
            </legend>
            <FormField
              label={`Prêmio titular da cobertura ${index + 1}`}
              error={errors.coberturas?.[index]?.premioTitular?.message}
            >
              <Input
                id={`premio-titular-${c.publicId}`}
                inputMode="decimal"
                {...register(`coberturas.${index}.premioTitular`)}
              />
            </FormField>
            <FormField
              label={`Prêmio cônjuge da cobertura ${index + 1}`}
              error={errors.coberturas?.[index]?.premioConjuge?.message}
            >
              <Input
                id={`premio-conjuge-${c.publicId}`}
                inputMode="decimal"
                {...register(`coberturas.${index}.premioConjuge`)}
              />
            </FormField>
            {c.justificativaAjustePremio && (
              <p>Última justificativa: {c.justificativaAjustePremio}</p>
            )}
            <FormField
              label={`Justificativa do ajuste da cobertura ${index + 1}`}
              error={
                errors.coberturas?.[index]?.justificativaAjustePremio?.message
              }
            >
              <Textarea
                id={`justificativa-${c.publicId}`}
                maxLength={1000}
                {...register(`coberturas.${index}.justificativaAjustePremio`)}
              />
            </FormField>
          </fieldset>
        ))}
      </FormSection>
      {descartar && (
        <Alert variant="warning" title="Descartar alterações?">
          As alterações deste formulário serão descartadas.{' '}
          <Button
            type="button"
            variant="secondary"
            disabled={isSubmitting}
            onClick={descartar === 'recarregar' ? onRecarregar : onCancelar}
          >
            Descartar alterações
          </Button>
          <Button
            type="button"
            variant="text"
            disabled={isSubmitting}
            onClick={() => setDescartar(null)}
          >
            Continuar editando
          </Button>
        </Alert>
      )}
      <FormActions sticky={false}>
        <Button
          type="button"
          variant="secondary"
          disabled={isSubmitting}
          onClick={() => (isDirty ? setDescartar('cancelar') : onCancelar())}
        >
          Cancelar edição
        </Button>
        <Button type="submit" loading={isSubmitting}>
          Salvar rascunho
        </Button>
      </FormActions>
    </form>
  );
}
