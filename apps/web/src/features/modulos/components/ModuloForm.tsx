import React from 'react';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { 
  Button, 
  Input, 
  Textarea,
  Checkbox,
  FormField,
  FormGrid,
  FormSection,
  FormActions
} from '../../../components/ui';
import { moduloFormSchema, type ModuloFormData } from '../schemas/modulo.schema';

export interface ModuloFormProps {
  initialData?: Partial<ModuloFormData>;
  onSubmit: (data: ModuloFormData) => void;
  onCancel: () => void;
  isSubmitting?: boolean;
}

export const ModuloForm: React.FC<ModuloFormProps> = ({
  initialData,
  onSubmit,
  onCancel,
  isSubmitting = false,
}) => {
  const {
    register,
    handleSubmit,
    control,
    formState: { errors },
  } = useForm<ModuloFormData>({
    resolver: zodResolver(moduloFormSchema),
    defaultValues: {
      nome: initialData?.nome || '',
      descricao: initialData?.descricao || '',
      ativo: initialData?.ativo ?? true,
    },
  });

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
      <FormSection title="Dados do Módulo" icon={undefined}>
        <FormGrid>
          <FormField label="Nome do Módulo" error={errors.nome?.message} required>
            <Input
              placeholder="Ex: Vida, Automóvel, Consórcio"
              {...register('nome')}
            />
          </FormField>

          <FormField label="Status" className="flex items-center pt-8">
            <Controller
              name="ativo"
              control={control}
              render={({ field }) => (
                <Checkbox
                  id="ativo"
                  label="Módulo Ativo"
                  checked={field.value}
                  onChange={(e) => field.onChange(e.target.checked)}
                />
              )}
            />
          </FormField>
        </FormGrid>

        <FormField label="Descrição" error={errors.descricao?.message}>
          <Textarea
            placeholder="Descrição opcional do módulo..."
            rows={4}
            {...register('descricao')}
          />
        </FormField>
      </FormSection>

      <FormActions>
        <Button 
          type="button" 
          variant="secondary" 
          onClick={onCancel}
          disabled={isSubmitting}
        >
          Cancelar
        </Button>
        <Button 
          type="submit" 
          variant="primary" 
          isLoading={isSubmitting}
        >
          Salvar
        </Button>
      </FormActions>
    </form>
  );
};
