/**
 * CorretoraForm.tsx
 *
 * Formulário reutilizável para criação e edição de Corretoras.
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
import { corretoraFormSchema, type CorretoraFormData } from '../schemas/corretoraFormSchema';

interface CorretoraFormProps {
  initialData?: Partial<CorretoraFormData>;
  isSubmitting?: boolean;
  onSubmit: (data: CorretoraFormData) => void;
  onCancel: () => void;
}

export const CorretoraForm: React.FC<CorretoraFormProps> = ({
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
  } = useForm<CorretoraFormData>({
    resolver: zodResolver(corretoraFormSchema),
    defaultValues: {
      nome: initialData?.nome || '',
      codigo: initialData?.codigo || '',
      codigoProtheus: initialData?.codigoProtheus || '',
      cnpj: initialData?.cnpj || '',
      observacao: initialData?.observacao || '',
    },
  });

  useEffect(() => {
    if (initialData) {
      reset({
        nome: initialData.nome || '',
        codigo: initialData.codigo || '',
        codigoProtheus: initialData.codigoProtheus || '',
        cnpj: initialData.cnpj || '',
        observacao: initialData.observacao || '',
      });
    }
  }, [initialData, reset]);

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-6" noValidate>
      <FormSection title="Dados da Corretora" icon={<BriefcaseIcon size={20} />}>
        <FormGrid>
          <div className="lg:col-span-8">
            <FormField label="Nome / Razão Social" required error={errors.nome?.message}>
              <Input
                {...register('nome')}
                id="corretora-nome"
                placeholder="Ex: Corretora Exemplo Ltda"
              />
            </FormField>
          </div>

          <div className="lg:col-span-4">
            <FormField label="CNPJ" error={errors.cnpj?.message}>
              <Input
                {...register('cnpj')}
                id="corretora-cnpj"
                placeholder="Ex: 61.198.164/0001-60"
              />
            </FormField>
          </div>

          <div className="lg:col-span-6">
            <FormField label="Código Interno" error={errors.codigo?.message}>
              <Input
                {...register('codigo')}
                id="corretora-codigo"
                placeholder="Código identificador interno (opcional)"
              />
            </FormField>
          </div>

          <div className="lg:col-span-6">
            <FormField label="Código Protheus" error={errors.codigoProtheus?.message}>
              <Input
                {...register('codigoProtheus')}
                id="corretora-codigoProtheus"
                placeholder="Código no sistema Protheus (opcional)"
              />
            </FormField>
          </div>

          <div className="lg:col-span-12">
            <FormField label="Observação" error={errors.observacao?.message}>
              <Textarea
                {...register('observacao')}
                id="corretora-observacao"
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
