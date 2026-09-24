import React, { useEffect, useState } from 'react';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { 
  Modal, 
  Button, 
  Select, 
  Input, 
  FormGrid, 
  FormField, 
  ReadOnlyField 
} from '../../../../components/ui';
import { ClienteAsyncSelect } from './ClienteAsyncSelect';
import { apoliceVidaSchema, type ApoliceVidaFormValues } from '../../schemas/apoliceVida.schema';
import { useApoliceSubgrupos } from '../../hooks/useApoliceSubgrupos';
import { useApoliceModulos } from '../../hooks/useApoliceModulos';
import type { ApoliceVidaListItem } from '../../types/apolice.types';

interface ApoliceVidaFormModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (data: ApoliceVidaFormValues) => Promise<void>;
  apolicePublicId: string;
  initialData?: ApoliceVidaListItem;
  isSubmitting?: boolean;
}

export const ApoliceVidaFormModal: React.FC<ApoliceVidaFormModalProps> = ({
  isOpen,
  onClose,
  onSubmit,
  apolicePublicId,
  initialData,
  isSubmitting = false
}) => {
  const isEdit = !!initialData;
  const { data: subgrupos, isLoading: loadingSubgrupos } = useApoliceSubgrupos(apolicePublicId);
  const { data: modulos, isLoading: loadingModulos } = useApoliceModulos(apolicePublicId);

  const {
    control,
    handleSubmit,
    watch,
    setValue,
    reset,
    formState: { errors }
  } = useForm<ApoliceVidaFormValues>({
    resolver: zodResolver(apoliceVidaSchema),
    defaultValues: {
      clientePublicId: '',
      apoliceSubgrupoPublicId: '',
      apoliceModuloPublicId: '',
      dataInicioVigencia: '',
      dataFimVigencia: '',
      observacao: ''
    }
  });

  const apoliceSubgrupoPublicId = watch('apoliceSubgrupoPublicId');
  const apoliceModuloPublicId = watch('apoliceModuloPublicId');

  useEffect(() => {
    if (isOpen) {
      if (initialData) {
        reset({
          clientePublicId: initialData.clientePublicId,
          apoliceSubgrupoPublicId: initialData.apoliceSubgrupoPublicId || '',
          apoliceModuloPublicId: initialData.apoliceModuloPublicId || '',
          dataInicioVigencia: initialData.dataInicioVigencia?.split('T')[0] || '',
          dataFimVigencia: initialData.dataFimVigencia?.split('T')[0] || '',
          observacao: initialData.observacao || ''
        });
      } else {
        reset({
          clientePublicId: '',
          apoliceSubgrupoPublicId: '',
          apoliceModuloPublicId: '',
          dataInicioVigencia: '',
          dataFimVigencia: '',
          observacao: ''
        });
      }
    }
  }, [isOpen, initialData, reset]);

  // Se estiver editando e o item atual está inativo, ainda o mantemos na lista de disponíveis para não sumir.
  const subgruposDisponiveis = subgrupos?.filter(s => s.ativo || (isEdit && initialData?.apoliceSubgrupoPublicId === s.subgrupoPublicId)) || [];
  const modulosDisponiveis = modulos?.filter(m => m.ativo || (isEdit && initialData?.apoliceModuloPublicId === m.publicId)) || [];

  return (
    <Modal
      aberto={isOpen}
      onClose={isSubmitting ? undefined : onClose}
      title={isEdit ? 'Editar Participação de Vida' : 'Adicionar Vida'}
      size="large"
    >
      <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-6" noValidate>
        <div className="flex flex-col gap-4">
          <h3 className="text-sm font-medium text-texto-secundario uppercase tracking-wider mb-2">Identificação do Cliente</h3>
          <FormGrid columns={1}>
            {isEdit && initialData ? (
              <ReadOnlyField label="Cliente" value={initialData.clienteNome} />
            ) : (
              <FormField label="Cliente" required error={errors.clientePublicId?.message}>
                <Controller
                  name="clientePublicId"
                  control={control}
                  render={({ field }) => (
                    <ClienteAsyncSelect 
                      value={field.value} 
                      onChange={field.onChange} 
                      error={!!errors.clientePublicId}
                      disabled={isSubmitting}
                    />
                  )}
                />
              </FormField>
            )}
          </FormGrid>
          
          <h3 className="text-sm font-medium text-texto-secundario uppercase tracking-wider mt-4 mb-2">Vínculo na Apólice</h3>
          <FormGrid columns={2}>
            <FormField label="Subgrupo da Apólice" error={errors.apoliceSubgrupoPublicId?.message}>
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
                    {subgruposDisponiveis.map(s => (
                      <option key={s.subgrupoPublicId} value={s.subgrupoPublicId}>{s.nome}</option>
                    ))}
                  </Select>
                )}
              />
            </FormField>

            <FormField label="Módulo da Apólice" error={errors.apoliceModuloPublicId?.message}>
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
                    {modulosDisponiveis.map(m => (
                      <option key={m.publicId} value={m.publicId}>{m.nome}</option>
                    ))}
                  </Select>
                )}
              />
            </FormField>
          </FormGrid>
        </div>

        <div className="flex flex-col gap-4 border-t border-borda pt-4">
          <h3 className="text-sm font-medium text-texto-secundario uppercase tracking-wider mb-2">Período e Observações</h3>
          <FormGrid columns={2}>
            <FormField label="Data de Início" error={errors.dataInicioVigencia?.message}>
              <Controller
                name="dataInicioVigencia"
                control={control}
                render={({ field }) => (
                  <Input type="date" {...field} value={field.value || ''} error={!!errors.dataInicioVigencia} disabled={isSubmitting} />
                )}
              />
            </FormField>
            
            <FormField label="Data de Fim" error={errors.dataFimVigencia?.message}>
              <Controller
                name="dataFimVigencia"
                control={control}
                render={({ field }) => (
                  <Input type="date" {...field} value={field.value || ''} error={!!errors.dataFimVigencia} disabled={isSubmitting} />
                )}
              />
            </FormField>
          </FormGrid>

          <FormGrid columns={1}>
            <FormField label="Observação" error={errors.observacao?.message}>
              <Controller
                name="observacao"
                control={control}
                render={({ field }) => (
                  <Input type="text" {...field} value={field.value || ''} error={!!errors.observacao} disabled={isSubmitting} />
                )}
              />
            </FormField>
          </FormGrid>
        </div>

        <div className="flex justify-end gap-2 mt-4 pt-4 border-t border-borda">
          <Button type="button" variant="ghost" onClick={onClose} disabled={isSubmitting}>
            Cancelar
          </Button>
          <Button type="submit" variant="primary" loading={isSubmitting}>
            Salvar
          </Button>
        </div>
      </form>
    </Modal>
  );
};
