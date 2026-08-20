import React, { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { FormField, Input, Textarea, Button, FormSection, FormGrid, FormActions, BriefcaseIcon } from '../../../components/ui';
import { ramoFormSchema, type RamoFormData } from '../schemas/ramoFormSchema';

interface RamoFormProps {
  initialData?: Partial<RamoFormData>;
  isSubmitting?: boolean;
  onSubmit: (data: RamoFormData) => void;
  onCancel: () => void;
}

export const RamoForm: React.FC<RamoFormProps> = ({
  initialData,
  isSubmitting = false,
  onSubmit,
  onCancel,
}) => {
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<RamoFormData>({
    resolver: zodResolver(ramoFormSchema),
    defaultValues: {
      codigo: initialData?.codigo || '',
      nome: initialData?.nome || '',
      descricao: initialData?.descricao || '',
    },
  });

  useEffect(() => {
    if (initialData) {
      reset({
        codigo: initialData.codigo || '',
        nome: initialData.nome || '',
        descricao: initialData.descricao || '',
      });
    }
  }, [initialData, reset]);

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-6">
      <FormSection title="Dados do Ramo" icon={<BriefcaseIcon size={20} />}>
        <FormGrid>
          <div className="lg:col-span-4">
            <FormField label="Código" required error={errors.codigo?.message}>
              <Input {...register('codigo')} placeholder="Código do Ramo" disabled={!!initialData?.codigo} />
            </FormField>
          </div>

          <div className="lg:col-span-8">
            <FormField label="Nome" required error={errors.nome?.message}>
              <Input {...register('nome')} placeholder="Nome do Ramo" />
            </FormField>
          </div>

          <div className="lg:col-span-12">
            <FormField label="Descrição" error={errors.descricao?.message}>
              <Textarea {...register('descricao')} placeholder="Descrição do Ramo..." rows={3} />
            </FormField>
          </div>
        </FormGrid>
      </FormSection>

      <FormActions>
        <div className="flex-grow flex justify-start">
        </div>
        <Button type="button" variant="text" onClick={onCancel} disabled={isSubmitting}>
          Cancelar
        </Button>
        <Button type="submit" variant="primary" disabled={isSubmitting} loading={isSubmitting}>
          Salvar
        </Button>
      </FormActions>
    </form>
  );
};
