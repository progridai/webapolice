/**
 * SubestipulanteForm.tsx
 *
 * Formulário reutilizável para criação e edição de Subestipulantes.
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
import { subestipulanteFormSchema, type SubestipulanteFormData } from '../schemas/subestipulanteFormSchema';

interface SubestipulanteFormProps {
  initialData?: Partial<SubestipulanteFormData>;
  isSubmitting?: boolean;
  onSubmit: (data: SubestipulanteFormData) => void;
  onCancel: () => void;
}

export const SubestipulanteForm: React.FC<SubestipulanteFormProps> = ({
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
  } = useForm<SubestipulanteFormData>({
    resolver: zodResolver(subestipulanteFormSchema),
    defaultValues: {
      nome: initialData?.nome || '',
      codigo: initialData?.codigo || '',
      cnpj: initialData?.cnpj || '',
      observacao: initialData?.observacao || '',
    },
  });

  useEffect(() => {
    if (initialData) {
      reset({
        nome: initialData.nome || '',
        codigo: initialData.codigo || '',
        cnpj: initialData.cnpj || '',
        observacao: initialData.observacao || '',
      });
    }
  }, [initialData, reset]);

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-6" noValidate>
      <FormSection title="Dados do Subestipulante" icon={<BriefcaseIcon size={20} />}>
        <FormGrid>
          <div className="lg:col-span-8">
            <FormField label="Nome / Razão Social" required error={errors.nome?.message}>
              <Input
                {...register('nome')}
                id="subestipulante-nome"
                placeholder="Nome ou Razão Social"
              />
            </FormField>
          </div>

          <div className="lg:col-span-4">
            <FormField label="CNPJ" error={errors.cnpj?.message}>
              <Input
                {...register('cnpj')}
                id="subestipulante-cnpj"
                placeholder="Ex: 61.198.164/0001-60"
              />
            </FormField>
          </div>

          <div className="lg:col-span-6">
            <FormField label="Código Interno" error={errors.codigo?.message}>
              <Input
                {...register('codigo')}
                id="subestipulante-codigo"
                placeholder="Código identificador interno (opcional)"
              />
            </FormField>
          </div>
        </FormGrid>
      </FormSection>

      <FormSection title="Outras Informações">
        <FormGrid>
          <div className="lg:col-span-12">
            <FormField label="Observações Internas" error={errors.observacao?.message}>
              <Textarea
                {...register('observacao')}
                id="subestipulante-observacao"
                rows={4}
                placeholder="Anotações gerais sobre o subestipulante..."
              />
            </FormField>
          </div>
        </FormGrid>
      </FormSection>

      <FormActions>
        <Button
          type="button"
          variant="outline"
          onClick={onCancel}
          disabled={isSubmitting}
        >
          Cancelar
        </Button>
        <Button type="submit" disabled={isSubmitting}>
          {isSubmitting ? 'Salvando...' : 'Salvar Subestipulante'}
        </Button>
      </FormActions>
    </form>
  );
};
