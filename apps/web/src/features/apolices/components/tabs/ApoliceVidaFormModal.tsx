import React from 'react';
import { VinculoVidaSelect } from './VinculoVidaSelect';
import { useForm, Controller, useWatch } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import {
  Modal,
  Alert,
  Button,
  Select,
  Input,
  FormGrid,
  FormField,
  ReadOnlyField,
} from '../../../../components/ui';
import { ClienteAsyncSelect } from './ClienteAsyncSelect';
import {
  apoliceVidaSchema,
  type ApoliceVidaFormValues,
} from '../../schemas/apoliceVida.schema';
import { useApoliceSubgrupos } from '../../hooks/useApoliceSubgrupos';
import { useApoliceModulos } from '../../hooks/useApoliceModulos';
import type { ApoliceVidaListItem } from '../../types/apolice.types';

interface ApoliceVidaFormModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (data: ApoliceVidaFormValues) => Promise<void>;
  apolicePublicId: string;
  estipulanteNome?: string;
  podeCadastrarVinculo: boolean;
  initialData?: ApoliceVidaListItem;
  isSubmitting?: boolean;
  submitError?: string | null;
}

export const ApoliceVidaFormModal: React.FC<ApoliceVidaFormModalProps> = (
  props,
) => (
  <Modal
    aberto={props.isOpen}
    onClose={() => {
      if (!props.isSubmitting) props.onClose();
    }}
    title={props.initialData ? 'Editar Participação de Vida' : 'Adicionar Vida'}
    size="large"
  >
    {props.isOpen && (
      <ApoliceVidaForm
        key={`${props.apolicePublicId}:${props.initialData?.apoliceVidaPublicId || 'nova'}`}
        {...props}
      />
    )}
  </Modal>
);

const ApoliceVidaForm: React.FC<ApoliceVidaFormModalProps> = ({
  onClose,
  submitError,
  onSubmit,
  apolicePublicId,
  estipulanteNome,
  podeCadastrarVinculo,
  initialData,
  isSubmitting = false,
}) => {
  const isEdit = !!initialData;
  const { data: subgrupos, isLoading: loadingSubgrupos } =
    useApoliceSubgrupos(apolicePublicId);
  const { data: modulos, isLoading: loadingModulos } =
    useApoliceModulos(apolicePublicId);

  const {
    control,
    handleSubmit,
    setValue,
    setError,
    formState: { errors },
  } = useForm<ApoliceVidaFormValues>({
    resolver: zodResolver(apoliceVidaSchema),
    defaultValues: {
      clientePublicId: initialData?.clientePublicId || '',
      clienteVinculoPublicId: initialData?.clienteVinculoPublicId || '',
      completarVinculoSemEstipulante: false,
      novaMatricula: null,
      apoliceSubgrupoPublicId: initialData?.apoliceSubgrupoPublicId || '',
      apoliceModuloPublicId: initialData?.apoliceModuloPublicId || '',
      dataInicioVigencia: initialData?.dataInicioVigencia?.split('T')[0] || '',
      dataFimVigencia: initialData?.dataFimVigencia?.split('T')[0] || '',
      observacao: initialData?.observacao || '',
    },
  });
  const clienteId = useWatch({ control, name: 'clientePublicId' });
  const vinculoId = useWatch({ control, name: 'clienteVinculoPublicId' });
  const novaMatricula = useWatch({ control, name: 'novaMatricula' });
  const salvar = handleSubmit(async (data) => {
    if (!isEdit && !data.clienteVinculoPublicId && data.novaMatricula == null) {
      setError('clienteVinculoPublicId', {
        message: 'Selecione o vínculo do cliente.',
      });
      return;
    }
    await onSubmit(data);
  });

  // Se estiver editando e o item atual está inativo, ainda o mantemos na lista de disponíveis para não sumir.
  const subgruposDisponiveis =
    subgrupos?.filter(
      (s) =>
        s.ativo ||
        (isEdit && initialData?.apoliceSubgrupoPublicId === s.subgrupoPublicId),
    ) || [];
  const modulosDisponiveis =
    modulos?.filter(
      (m) =>
        m.ativo ||
        (isEdit && initialData?.apoliceModuloPublicId === m.publicId),
    ) || [];

  return (
    <form onSubmit={salvar} className="flex flex-col gap-6" noValidate>
      {submitError && (
        <Alert variant="error" title="Não foi possível salvar a participação">
          {submitError}
        </Alert>
      )}
      <div className="flex flex-col gap-4">
        <h3 className="text-sm font-medium text-texto-secundario uppercase tracking-wider mb-2">
          Identificação do Cliente
        </h3>
        <div>
          {isEdit && initialData ? (
            <FormField label="Cliente">
              <ReadOnlyField value={initialData.clienteNome} />
            </FormField>
          ) : (
            <FormField
              label="Cliente"
              required
              error={errors.clientePublicId?.message}
            >
              <Controller
                name="clientePublicId"
                control={control}
                render={({ field }) => (
                  <ClienteAsyncSelect
                    value={field.value}
                    onChange={(value) => {
                      field.onChange(value);
                      setValue('clienteVinculoPublicId', '');
                      setValue('completarVinculoSemEstipulante', false);
                      setValue('novaMatricula', null);
                    }}
                    error={!!errors.clientePublicId}
                    disabled={isSubmitting}
                  />
                )}
              />
            </FormField>
          )}
        </div>

        <VinculoVidaSelect
          key={clienteId}
          apoliceId={apolicePublicId}
          clienteId={clienteId}
          estipulanteNome={estipulanteNome}
          podeCadastrar={podeCadastrarVinculo}
          novaMatricula={novaMatricula ?? null}
          onNovaMatriculaChange={(value) => {
            setValue('clienteVinculoPublicId', '');
            setValue('completarVinculoSemEstipulante', false);
            setValue('novaMatricula', value, { shouldValidate: true });
          }}
          value={vinculoId || ''}
          onChange={(value, requerEstipulante) => {
            setValue('completarVinculoSemEstipulante', requerEstipulante);
            setValue('clienteVinculoPublicId', value, { shouldValidate: true });
          }}
          atual={
            initialData?.clienteVinculoPublicId
              ? {
                  publicId: initialData.clienteVinculoPublicId,
                  matricula: initialData.vinculoMatricula || '',
                  estipulante: null,
                }
              : undefined
          }
          bloqueado={!!initialData?.possuiPropostas}
          disabled={isSubmitting}
          required={!isEdit}
          error={errors.clienteVinculoPublicId?.message}
          matriculaError={errors.novaMatricula?.message}
        />

        <h3 className="text-sm font-medium text-texto-secundario uppercase tracking-wider mt-4 mb-2">
          Vínculo na Apólice
        </h3>
        <FormGrid>
          <FormField
            label="Subgrupo da Apólice"
            error={errors.apoliceSubgrupoPublicId?.message}
          >
            <Controller
              name="apoliceSubgrupoPublicId"
              control={control}
              render={({ field }) => (
                <Select
                  {...field}
                  value={field.value || ''}
                  error={!!errors.apoliceSubgrupoPublicId}
                  disabled={isSubmitting || loadingSubgrupos}
                >
                  <option value="">Selecione...</option>
                  {subgruposDisponiveis.map((s) => (
                    <option key={s.subgrupoPublicId} value={s.subgrupoPublicId}>
                      {s.nome}
                    </option>
                  ))}
                </Select>
              )}
            />
          </FormField>

          <FormField
            label="Módulo da Apólice"
            error={errors.apoliceModuloPublicId?.message}
          >
            <Controller
              name="apoliceModuloPublicId"
              control={control}
              render={({ field }) => (
                <Select
                  {...field}
                  value={field.value || ''}
                  error={!!errors.apoliceModuloPublicId}
                  disabled={isSubmitting || loadingModulos}
                >
                  <option value="">Selecione...</option>
                  {modulosDisponiveis.map((m) => (
                    <option key={m.publicId} value={m.publicId}>
                      {m.nome}
                    </option>
                  ))}
                </Select>
              )}
            />
          </FormField>
        </FormGrid>
      </div>

      <div className="flex flex-col gap-4 border-t border-borda pt-4">
        <h3 className="text-sm font-medium text-texto-secundario uppercase tracking-wider mb-2">
          Período e Observações
        </h3>
        <FormGrid>
          <FormField
            label="Data de Início"
            error={errors.dataInicioVigencia?.message}
          >
            <Controller
              name="dataInicioVigencia"
              control={control}
              render={({ field }) => (
                <Input
                  type="date"
                  {...field}
                  value={field.value || ''}
                  error={!!errors.dataInicioVigencia}
                  disabled={isSubmitting}
                />
              )}
            />
          </FormField>

          <FormField
            label="Data de Fim"
            error={errors.dataFimVigencia?.message}
          >
            <Controller
              name="dataFimVigencia"
              control={control}
              render={({ field }) => (
                <Input
                  type="date"
                  {...field}
                  value={field.value || ''}
                  error={!!errors.dataFimVigencia}
                  disabled={isSubmitting}
                />
              )}
            />
          </FormField>
        </FormGrid>

        <div>
          <FormField label="Observação" error={errors.observacao?.message}>
            <Controller
              name="observacao"
              control={control}
              render={({ field }) => (
                <Input
                  type="text"
                  {...field}
                  value={field.value || ''}
                  error={!!errors.observacao}
                  disabled={isSubmitting}
                />
              )}
            />
          </FormField>
        </div>
      </div>

      <div className="flex justify-end gap-2 mt-4 pt-4 border-t border-borda">
        <Button
          type="button"
          variant="text"
          onClick={onClose}
          disabled={isSubmitting}
        >
          Cancelar
        </Button>
        <Button type="submit" variant="primary" loading={isSubmitting}>
          Salvar
        </Button>
      </div>
    </form>
  );
};
