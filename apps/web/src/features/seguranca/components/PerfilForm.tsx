import React, { useEffect } from 'react';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import {
  FormSection,
  FormGrid,
  FormField,
  Input,
  Checkbox,
  FormActions,
  Button,
} from '../../../components/ui';
import { SelecaoPermissoes } from './SelecaoPermissoes';
import type { CatalogoModuloDto } from '../types/seguranca.types';

const perfilSchema = z.object({
  codigo: z.string().min(1, 'Código é obrigatório').max(50, 'Máximo 50 caracteres'),
  nome: z.string().min(1, 'Nome é obrigatório').max(100, 'Máximo 100 caracteres'),
  descricao: z.string().max(255, 'Máximo 255 caracteres').optional(),
  ativo: z.boolean(),
  permissaoPublicIds: z.array(z.string()),
});

export type PerfilFormData = z.infer<typeof perfilSchema>;

interface PerfilFormProps {
  initialData?: Partial<PerfilFormData>;
  catalogo: CatalogoModuloDto[];
  isEdit?: boolean;
  onSubmit: (data: PerfilFormData) => void;
  onCancel: () => void;
  isSubmitting?: boolean;
}

export const PerfilForm: React.FC<PerfilFormProps> = ({
  initialData,
  catalogo,
  isEdit = false,
  onSubmit,
  onCancel,
  isSubmitting = false,
}) => {
  const {
    register,
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<PerfilFormData>({
    resolver: zodResolver(perfilSchema),
    defaultValues: {
      codigo: '',
      nome: '',
      descricao: '',
      ativo: true,
      permissaoPublicIds: [],
      ...initialData,
    },
  });

  useEffect(() => {
    if (initialData) reset(initialData);
  }, [initialData, reset]);

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-8" noValidate>
      <FormSection title="Dados Gerais" description="Informações básicas do perfil.">
        <FormGrid>
          <FormField label="Código" error={errors.codigo?.message} required>
            <Input
              {...register('codigo')}
              disabled={isSubmitting || isEdit} // Código não é editável após criado
              placeholder="Ex: GESTOR_VENDAS"
              error={!!errors.codigo}
            />
          </FormField>
          
          <FormField label="Nome" error={errors.nome?.message} required>
            <Input
              {...register('nome')}
              disabled={isSubmitting}
              placeholder="Ex: Gestor de Vendas"
              error={!!errors.nome}
            />
          </FormField>
          
          <FormField label="Descrição" error={errors.descricao?.message} className="col-span-1 md:col-span-2">
            <Input
              {...register('descricao')}
              disabled={isSubmitting}
              placeholder="Ex: Acesso aos relatórios e aprovações de vendas."
              error={!!errors.descricao}
            />
          </FormField>

          <FormField label="Status" className="col-span-1 md:col-span-2">
            <Controller
              name="ativo"
              control={control}
              render={({ field }) => (
                <Checkbox
                  id="ativo"
                  label="Perfil ativo no sistema"
                  checked={field.value}
                  onChange={(e) => field.onChange(e.target.checked)}
                  disabled={isSubmitting}
                />
              )}
            />
          </FormField>
        </FormGrid>
      </FormSection>

      <FormSection title="Permissões" description="Selecione as permissões que este perfil terá.">
        <div className="bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 rounded-lg p-4">
          <Controller
            name="permissaoPublicIds"
            control={control}
            render={({ field }) => (
              <SelecaoPermissoes
                catalogo={catalogo}
                selecionados={field.value}
                onChange={field.onChange}
                disabled={isSubmitting}
              />
            )}
          />
        </div>
      </FormSection>

      <FormActions>
        <Button type="button" variant="ghost" onClick={onCancel} disabled={isSubmitting}>
          Cancelar
        </Button>
        <Button type="submit" variant="primary" loading={isSubmitting}>
          {isEdit ? 'Salvar Alterações' : 'Cadastrar Perfil'}
        </Button>
      </FormActions>
    </form>
  );
};
