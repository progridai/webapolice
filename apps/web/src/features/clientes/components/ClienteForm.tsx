import React, { useEffect, useState } from 'react';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { FormField } from '../../../components/ui/FormField';
import { Input } from '../../../components/ui/Input';
import { Select } from '../../../components/ui/Select';
import { Textarea } from '../../../components/ui/Textarea';
import { Checkbox } from '../../../components/ui/Checkbox';
import { Button } from '../../../components/ui/Button';
import { FormSection, FormGrid, FormActions, ReadOnlyField, UsersIcon, HomeIcon, InfoIcon } from '../../../components/ui';
import { buscarCidadesPorUf, type CidadeResponse } from '../api/localidadesApi';

const ESTADOS_BRASILEIROS = [
  'AC', 'AL', 'AP', 'AM', 'BA', 'CE', 'DF', 'ES', 'GO', 'MA', 'MT', 'MS', 'MG',
  'PA', 'PB', 'PR', 'PE', 'PI', 'RJ', 'RN', 'RS', 'RO', 'RR', 'SC', 'SP', 'SE', 'TO'
];

// Esquema de Validação
const enderecoSchema = z.object({
  cep: z.string().optional(),
  logradouro: z.string().optional(),
  numero: z.string().optional(),
  complemento: z.string().optional(),
  bairro: z.string().optional(),
  cidadeId: z.coerce.number().optional(),
  uf: z.string().max(2).optional(),
});

const clienteSchema = z.object({
  tipoPessoa: z.coerce.number().min(1, 'Selecione o tipo de pessoa').max(2),
  nome: z.string().min(3, 'O nome deve ter no mínimo 3 caracteres'),
  documento: z.string().min(11, 'Documento inválido'),
  dataNascimento: z.string().min(1, 'A data de nascimento é obrigatória'),
  sexo: z.coerce.number().optional(),
  observacao: z.string().optional(),
  falecido: z.boolean().default(false),
  dataObito: z.string().optional().or(z.literal('')),
  email: z.string().email('E-mail inválido').optional().or(z.literal('')),
  telefone: z.string().optional(),
  celular: z.string().optional(),
  endereco: enderecoSchema.optional(),
}).superRefine((data, ctx) => {
  if (data.falecido && !data.dataObito) {
    ctx.addIssue({
      path: ['dataObito'],
      code: z.ZodIssueCode.custom,
      message: 'Data de óbito é obrigatória se falecido.',
    });
  }
});

export type ClienteFormData = z.infer<typeof clienteSchema>;

interface ClienteFormProps {
  initialData?: Partial<ClienteFormData>;
  isEdit?: boolean;
  isSubmitting?: boolean;
  onSubmit: (data: ClienteFormData) => void;
  onCancel: () => void;
}

export const ClienteForm: React.FC<ClienteFormProps> = ({
  initialData,
  isEdit = false,
  isSubmitting = false,
  onSubmit,
  onCancel,
}) => {
  const [cidades, setCidades] = useState<CidadeResponse[]>([]);
  const [loadingCidades, setLoadingCidades] = useState(false);

  const {
    register,
    handleSubmit,
    control,
    watch,
    reset,
    formState: { errors },
  } = useForm<ClienteFormData>({
    resolver: zodResolver(clienteSchema),
    defaultValues: {
      tipoPessoa: initialData?.tipoPessoa || 1,
      nome: initialData?.nome || '',
      documento: initialData?.documento || '',
      dataNascimento: initialData?.dataNascimento || '',
      sexo: initialData?.sexo || undefined,
      observacao: initialData?.observacao || '',
      falecido: initialData?.falecido || false,
      dataObito: initialData?.dataObito || '',
      email: initialData?.email || '',
      telefone: initialData?.telefone || '',
      celular: initialData?.celular || '',
      endereco: initialData?.endereco || {},
    },
  });

  useEffect(() => {
    if (initialData) {
      reset({
        tipoPessoa: initialData.tipoPessoa || 1,
        nome: initialData.nome || '',
        documento: initialData.documento || '',
        dataNascimento: initialData.dataNascimento || '',
        sexo: initialData.sexo || undefined,
        observacao: initialData.observacao || '',
        falecido: initialData.falecido || false,
        dataObito: initialData.dataObito || '',
        email: initialData.email || '',
        telefone: initialData.telefone || '',
        celular: initialData.celular || '',
        endereco: initialData.endereco || {},
      });
    }
  }, [initialData, reset]);

  const isFalecido = watch('falecido');
  const ufSelecionada = watch('endereco.uf');

  useEffect(() => {
    async function carregarCidades() {
      if (!ufSelecionada || ufSelecionada.length !== 2) {
        setCidades([]);
        return;
      }
      try {
        setLoadingCidades(true);
        const data = await buscarCidadesPorUf(ufSelecionada);
        setCidades(data);
      } catch (err) {
        console.error('Erro ao buscar cidades', err);
        setCidades([]);
      } finally {
        setLoadingCidades(false);
      }
    }
    carregarCidades();
  }, [ufSelecionada]);

  return (
    <form onSubmit={handleSubmit(onSubmit)}>
      <FormSection title="Dados Principais" icon={<UsersIcon size={20} />}>
        <FormGrid>
          <div className="lg:col-span-6">
            <FormField label="Tipo de Pessoa" required error={errors.tipoPessoa?.message}>
              <Select {...register('tipoPessoa')} disabled={isEdit}>
                <option value={1}>Pessoa Física</option>
                <option value={2}>Pessoa Jurídica</option>
              </Select>
            </FormField>
          </div>

          <div className="lg:col-span-6">
            <FormField label="Documento (CPF/CNPJ)" required error={errors.documento?.message}>
              {isEdit ? (
                <ReadOnlyField value={initialData?.documento || ''} />
              ) : (
                <Input 
                  {...register('documento')} 
                  placeholder="Digite apenas números" 
                />
              )}
            </FormField>
          </div>

          <div className="lg:col-span-12">
            <FormField label="Nome Completo / Razão Social" required error={errors.nome?.message}>
              <Input {...register('nome')} placeholder="Digite o nome completo" />
            </FormField>
          </div>

          <div className="lg:col-span-6">
            <FormField label="Data de Nascimento" required error={errors.dataNascimento?.message}>
              <Input type="date" {...register('dataNascimento')} />
            </FormField>
          </div>

          <div className="lg:col-span-6">
            <FormField label="Sexo" error={errors.sexo?.message}>
              <Select {...register('sexo')}>
                <option value="">Não informado</option>
                <option value={1}>Masculino</option>
                <option value={2}>Feminino</option>
              </Select>
            </FormField>
          </div>
        </FormGrid>
      </FormSection>

      <FormSection title="Contato">
        <FormGrid>
          <div className="lg:col-span-4">
            <FormField label="E-mail" error={errors.email?.message}>
              <Input type="email" {...register('email')} placeholder="exemplo@email.com" />
            </FormField>
          </div>
          
          <div className="lg:col-span-4">
            <FormField label="Telefone" error={errors.telefone?.message}>
              <Input {...register('telefone')} placeholder="(00) 0000-0000" />
            </FormField>
          </div>

          <div className="lg:col-span-4">
            <FormField label="Celular" error={errors.celular?.message}>
              <Input {...register('celular')} placeholder="(00) 90000-0000" />
            </FormField>
          </div>
        </FormGrid>
      </FormSection>

      <FormSection title="Endereço" icon={<HomeIcon size={20} />}>
        <FormGrid>
          <div className="lg:col-span-3">
            <FormField label="CEP" error={errors.endereco?.cep?.message}>
              <Input {...register('endereco.cep')} placeholder="00000-000" />
            </FormField>
          </div>
          
          <div className="lg:col-span-6">
            <FormField label="Logradouro" error={errors.endereco?.logradouro?.message}>
              <Input {...register('endereco.logradouro')} placeholder="Rua, Avenida..." />
            </FormField>
          </div>

          <div className="lg:col-span-3">
            <FormField label="Número" error={errors.endereco?.numero?.message}>
              <Input {...register('endereco.numero')} placeholder="123" />
            </FormField>
          </div>

          <div className="lg:col-span-3">
            <FormField label="Complemento" error={errors.endereco?.complemento?.message}>
              <Input {...register('endereco.complemento')} placeholder="Apto, Bloco..." />
            </FormField>
          </div>

          <div className="lg:col-span-3">
            <FormField label="Bairro" error={errors.endereco?.bairro?.message}>
              <Input {...register('endereco.bairro')} placeholder="Bairro" />
            </FormField>
          </div>
          
          <div className="lg:col-span-2">
            <FormField label="UF" error={errors.endereco?.uf?.message}>
              <Select {...register('endereco.uf')}>
                <option value="">Selecione...</option>
                {ESTADOS_BRASILEIROS.map(uf => (
                  <option key={uf} value={uf}>{uf}</option>
                ))}
              </Select>
            </FormField>
          </div>

          <div className="lg:col-span-4">
            <FormField label="Cidade" error={errors.endereco?.cidadeId?.message}>
              <Select {...register('endereco.cidadeId')} disabled={loadingCidades || cidades.length === 0}>
                <option value="">{loadingCidades ? 'Carregando...' : 'Selecione a cidade'}</option>
                {cidades.map(c => (
                  <option key={c.id} value={c.id}>{c.nome}</option>
                ))}
              </Select>
            </FormField>
          </div>
        </FormGrid>
      </FormSection>

      <FormSection title="Informações Adicionais" icon={<InfoIcon size={20} />}>
        <FormGrid>
          <div className="lg:col-span-12">
            <Controller
              name="falecido"
              control={control}
              render={({ field }) => (
                <Checkbox
                  id="falecido"
                  checked={field.value}
                  onChange={(e) => field.onChange(e.target.checked)}
                  label="Cliente Falecido/Extinto"
                />
              )}
            />
          </div>

          {isFalecido && (
            <div className="lg:col-span-6">
              <FormField label="Data de Óbito" required error={errors.dataObito?.message}>
                <Input type="date" {...register('dataObito')} />
              </FormField>
            </div>
          )}

          <div className="lg:col-span-12">
            <FormField label="Observações" error={errors.observacao?.message}>
              <Textarea {...register('observacao')} placeholder="Observações internas..." rows={3} />
            </FormField>
          </div>
        </FormGrid>
      </FormSection>

      <FormActions>
        <Button type="button" variant="text" onClick={onCancel} disabled={isSubmitting}>
          Cancelar
        </Button>
        <Button type="submit" variant="primary" disabled={isSubmitting} loading={isSubmitting}>
          Salvar Cliente
        </Button>
      </FormActions>
    </form>
  );
};
