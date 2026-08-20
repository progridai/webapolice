/**
 * SeguradoraForm.tsx
 *
 * Formulário reutilizável para criação e edição de Seguradoras.
 */
import React, { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import {
  FormField,
  Input,
  Textarea,
  Button,
  FormSection,
  FormGrid,
  FormActions,
  BriefcaseIcon,
} from '../../../components/ui';
import { seguradoraFormSchema, type SeguradoraFormData } from '../schemas/seguradoraFormSchema';

interface SeguradoraFormProps {
  initialData?: Partial<SeguradoraFormData>;
  isSubmitting?: boolean;
  onSubmit: (data: SeguradoraFormData) => void;
  onCancel: () => void;
}

export const SeguradoraForm: React.FC<SeguradoraFormProps> = ({
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
  } = useForm<SeguradoraFormData>({
    resolver: zodResolver(seguradoraFormSchema),
    defaultValues: {
      nome: initialData?.nome || '',
      codigo: initialData?.codigo || '',
      susep: initialData?.susep || '',
      cnpj: initialData?.cnpj || '',
      observacao: initialData?.observacao || '',
    },
  });

  useEffect(() => {
    if (initialData) {
      reset({
        nome: initialData.nome || '',
        codigo: initialData.codigo || '',
        susep: initialData.susep || '',
        cnpj: initialData.cnpj || '',
        observacao: initialData.observacao || '',
      });
    }
  }, [initialData, reset]);

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-6" noValidate>
      <FormSection title="Dados da Seguradora" icon={<BriefcaseIcon size={20} />}>
        <FormGrid>
          <div className="lg:col-span-8">
            <FormField label="Nome / Razão Social" required error={errors.nome?.message}>
              <Input
                {...register('nome')}
                id="seguradora-nome"
                placeholder="Ex: Porto Seguro Companhia de Seguros Gerais"
              />
            </FormField>
          </div>

          <div className="lg:col-span-4">
            <FormField label="CNPJ" error={errors.cnpj?.message}>
              <Input
                {...register('cnpj')}
                id="seguradora-cnpj"
                placeholder="Ex: 61.198.164/0001-60"
              />
            </FormField>
          </div>

          <div className="lg:col-span-6">
            <FormField label="Código Interno" error={errors.codigo?.message}>
              <Input
                {...register('codigo')}
                id="seguradora-codigo"
                placeholder="Código identificador interno (opcional)"
              />
            </FormField>
          </div>

          <div className="lg:col-span-6">
            <FormField label="Código SUSEP" error={errors.susep?.message}>
              <Input
                {...register('susep')}
                id="seguradora-susep"
                placeholder="Ex: 05886 (código oficial SUSEP)"
              />
            </FormField>
          </div>

          <div className="lg:col-span-12">
            <FormField label="Observação" error={errors.observacao?.message}>
              <Textarea
                {...register('observacao')}
                id="seguradora-observacao"
                placeholder="Informações complementares ou notas operacionais..."
                rows={4}
              />
            </FormField>
          </div>
        </FormGrid>
      </FormSection>

      <FormActions>
        <div className="flex-grow flex justify-start" />
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
