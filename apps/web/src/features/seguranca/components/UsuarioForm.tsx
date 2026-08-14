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
import { SelecaoPerfis } from './SelecaoPerfis';
import type { PerfilDto } from '../types/seguranca.types';

// O schema se adapta se for edição (não exige senhas)
const baseSchema = z.object({
  username: z.string().min(1, 'Username é obrigatório').max(100),
  nome: z.string().min(1, 'Nome é obrigatório').max(150),
  email: z.string().email('E-mail inválido').max(150),
  ativo: z.boolean(),
  perfilPublicIds: z.array(z.string()),
});

const getUsuarioSchema = (isEdit: boolean) => {
  if (isEdit) return baseSchema;
  
  return baseSchema
    .extend({
      senhaTemporaria: z.string().min(6, 'Senha deve ter pelo menos 6 caracteres'),
      confirmacaoSenhaTemporaria: z.string(),
    })
    .refine((data) => data.senhaTemporaria === data.confirmacaoSenhaTemporaria, {
      message: 'As senhas não coincidem',
      path: ['confirmacaoSenhaTemporaria'],
    });
};

export type UsuarioFormData = z.infer<ReturnType<typeof getUsuarioSchema>>;

interface UsuarioFormProps {
  initialData?: Partial<UsuarioFormData>;
  perfisDisponiveis: PerfilDto[];
  isEdit?: boolean;
  onSubmit: (data: UsuarioFormData) => void;
  onCancel: () => void;
  isSubmitting?: boolean;
}

export const UsuarioForm: React.FC<UsuarioFormProps> = ({
  initialData,
  perfisDisponiveis,
  isEdit = false,
  onSubmit,
  onCancel,
  isSubmitting = false,
}) => {
  const schema = getUsuarioSchema(isEdit);
  const {
    register,
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<UsuarioFormData>({
    resolver: zodResolver(schema),
    defaultValues: {
      username: '',
      nome: '',
      email: '',
      senhaTemporaria: '',
      confirmacaoSenhaTemporaria: '',
      ativo: true,
      perfilPublicIds: [],
      ...initialData,
    },
  });

  useEffect(() => {
    if (initialData) reset(initialData);
  }, [initialData, reset]);

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-6" noValidate>
      <FormSection title="Dados Gerais" description="Informações de acesso do usuário.">
        <FormGrid>
          <FormField label="Username" error={errors.username?.message} required>
            <Input
              {...register('username')}
              disabled={isSubmitting || isEdit} // Não editável após criado
              placeholder="Ex: jdoe"
              error={!!errors.username}
            />
          </FormField>
          
          <FormField label="Nome Completo" error={errors.nome?.message} required>
            <Input
              {...register('nome')}
              disabled={isSubmitting}
              placeholder="Ex: John Doe"
              error={!!errors.nome}
            />
          </FormField>
          
          <FormField label="E-mail" error={errors.email?.message} required>
            <Input
              type="email"
              {...register('email')}
              disabled={isSubmitting}
              placeholder="Ex: john.doe@empresa.com"
              error={!!errors.email}
            />
          </FormField>

          {!isEdit && (
            <>
              <FormField label="Senha Temporária" error={errors.senhaTemporaria?.message} required>
                <Input
                  type="password"
                  {...register('senhaTemporaria')}
                  disabled={isSubmitting}
                  error={!!errors.senhaTemporaria}
                />
              </FormField>
              
              <FormField label="Confirmar Senha" error={errors.confirmacaoSenhaTemporaria?.message} required>
                <Input
                  type="password"
                  {...register('confirmacaoSenhaTemporaria')}
                  disabled={isSubmitting}
                  error={!!errors.confirmacaoSenhaTemporaria}
                />
              </FormField>
            </>
          )}

          <FormField label="Status" className="sm:col-span-full lg:col-span-full">
            <Controller
              name="ativo"
              control={control}
              render={({ field }) => (
                <Checkbox
                  id="ativo"
                  label="Usuário ativo para acessar o sistema"
                  checked={field.value}
                  onChange={(e) => field.onChange(e.target.checked)}
                  disabled={isSubmitting}
                />
              )}
            />
          </FormField>
        </FormGrid>
      </FormSection>

      <FormSection title="Perfis de Acesso" description="Atribua papéis de segurança a este usuário.">
        <Controller
          name="perfilPublicIds"
          control={control}
          render={({ field }) => (
            <SelecaoPerfis
              perfis={perfisDisponiveis}
              selecionados={field.value}
              onChange={field.onChange}
              disabled={isSubmitting}
            />
          )}
        />
      </FormSection>

      <FormActions>
        <Button type="button" variant="ghost" onClick={onCancel} disabled={isSubmitting}>
          Cancelar
        </Button>
        <Button type="submit" variant="primary" loading={isSubmitting}>
          {isEdit ? 'Salvar Alterações' : 'Cadastrar Usuário'}
        </Button>
      </FormActions>
    </form>
  );
};
