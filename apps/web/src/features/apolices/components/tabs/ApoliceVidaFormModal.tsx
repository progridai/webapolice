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
import { useApoliceSubestipulantes } from '../../hooks/useApoliceSubestipulantes';
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
  const { data: subestipulantes, isLoading: loadingSubestipulantes } = useApoliceSubestipulantes(apolicePublicId);

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
      contexto: 'direto',
      subestipulantePublicId: '',
      moduloPublicId: '',
      dataInicioVigencia: '',
      dataFimVigencia: '',
      observacao: ''
    }
  });

  const contexto = watch('contexto');
  const subestipulantePublicId = watch('subestipulantePublicId');

  useEffect(() => {
    if (isOpen) {
      if (initialData) {
        reset({
          clientePublicId: initialData.clientePublicId,
          contexto: initialData.contexto,
          subestipulantePublicId: initialData.subestipulantePublicId || '',
          moduloPublicId: initialData.moduloPublicId || '',
          dataInicioVigencia: initialData.dataInicioVigencia?.split('T')[0] || '',
          dataFimVigencia: initialData.dataFimVigencia?.split('T')[0] || '',
          observacao: initialData.observacao || ''
        });
      } else {
        reset({
          clientePublicId: '',
          contexto: 'direto',
          subestipulantePublicId: '',
          moduloPublicId: '',
          dataInicioVigencia: '',
          dataFimVigencia: '',
          observacao: ''
        });
      }
    }
  }, [isOpen, initialData, reset]);

  // Limpeza em cascata no modo criação
  useEffect(() => {
    if (isEdit) return; // Não limpa no modo edição pois os campos são read-only

    if (contexto === 'direto') {
      setValue('subestipulantePublicId', '');
      setValue('moduloPublicId', '');
    } else if (contexto === 'subestipulante') {
      setValue('moduloPublicId', '');
    }
  }, [contexto, setValue, isEdit]);

  const handleSubestipulanteChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    setValue('subestipulantePublicId', e.target.value);
    if (!isEdit) {
      setValue('moduloPublicId', ''); // limpa módulo quando troca o subestipulante
    }
  };

  const selectedSubestipulante = subestipulantes?.find(s => s.subestipulantePublicId === subestipulantePublicId);
  const modulosDisponiveis = selectedSubestipulante?.modulos.filter(m => m.vinculoAtivo) || [];
  const subestipulantesDisponiveis = subestipulantes?.filter(s => s.ativo) || [];

  return (
    <Modal
      aberto={isOpen}
      onClose={isSubmitting ? undefined : onClose}
      title={isEdit ? 'Editar Participação de Vida' : 'Adicionar Vida'}
      size="large"
    >
      <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-6" noValidate>
        {isEdit ? (
          <div className="flex flex-col gap-4">
            <h3 className="text-sm font-medium text-texto-secundario uppercase tracking-wider mb-2">Informações Estruturais</h3>
            <FormGrid columns={1}>
              <ReadOnlyField label="Cliente" value={initialData.clienteNome} />
            </FormGrid>
            <FormGrid columns={2}>
              <ReadOnlyField 
                label="Contexto" 
                value={initialData.contexto === 'direto' ? 'Direto na Apólice' : initialData.contexto === 'subestipulante' ? 'Subestipulante' : 'Módulo'} 
              />
              {initialData.subestipulanteNome && (
                <ReadOnlyField label="Subestipulante" value={initialData.subestipulanteNome} />
              )}
              {initialData.moduloNome && (
                <ReadOnlyField label="Módulo" value={initialData.moduloNome} />
              )}
            </FormGrid>
          </div>
        ) : (
          <div className="flex flex-col gap-4">
            <h3 className="text-sm font-medium text-texto-secundario uppercase tracking-wider mb-2">Identificação do Cliente</h3>
            <FormGrid columns={1}>
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
            </FormGrid>
            
            <h3 className="text-sm font-medium text-texto-secundario uppercase tracking-wider mt-4 mb-2">Contexto da Participação</h3>
            <FormGrid columns={3}>
              <FormField label="Contexto" required error={errors.contexto?.message}>
                <Controller
                  name="contexto"
                  control={control}
                  render={({ field }) => (
                    <Select
                      {...field}
                      error={!!errors.contexto}
                      disabled={isSubmitting}
                    >
                      <option value="direto">Direto na Apólice</option>
                      <option value="subestipulante">Subestipulante</option>
                      <option value="modulo">Subestipulante + Módulo</option>
                    </Select>
                  )}
                />
              </FormField>
              
              {(contexto === 'subestipulante' || contexto === 'modulo') && (
                <FormField label="Subestipulante da Apólice" required error={errors.subestipulantePublicId?.message}>
                  <Select
                    value={subestipulantePublicId}
                    onChange={handleSubestipulanteChange}
                    error={!!errors.subestipulantePublicId}
                    disabled={isSubmitting || loadingSubestipulantes}
                  >
                    <option value="" disabled>Selecione um subestipulante...</option>
                    {subestipulantesDisponiveis.map(s => (
                      <option key={s.subestipulantePublicId} value={s.subestipulantePublicId}>{s.nome}</option>
                    ))}
                  </Select>
                </FormField>
              )}

              {contexto === 'modulo' && (
                <FormField label="Módulo" required error={errors.moduloPublicId?.message}>
                  <Controller
                    name="moduloPublicId"
                    control={control}
                    render={({ field }) => (
                      <Select
                        {...field}
                        error={!!errors.moduloPublicId}
                        disabled={isSubmitting || !subestipulantePublicId}
                      >
                        <option value="" disabled>Selecione um módulo...</option>
                        {modulosDisponiveis.map(m => (
                          <option key={m.moduloPublicId} value={m.moduloPublicId}>{m.moduloNome}</option>
                        ))}
                      </Select>
                    )}
                  />
                </FormField>
              )}
            </FormGrid>
          </div>
        )}

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
