import React, { useEffect, useState } from 'react';
import { useForm, Controller, useFieldArray, useWatch, type UseFormRegister, type Control, type FieldErrors } from 'react-hook-form';
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

// Esquemas de Validação
const contatoSchema = z.object({
  tipoContato: z.string().min(1, 'Selecione o tipo de contato'),
  valor: z.string().optional().or(z.literal('')),
  principal: z.boolean().default(false),
}).refine(data => {
  if (data.valor && data.valor.trim().length > 0) {
    return data.valor.trim().length >= 3;
  }
  return true;
}, {
  message: 'O contato deve ter no mínimo 3 caracteres',
  path: ['valor']
});

const enderecoSchema = z.object({
  tipoEndereco: z.string().min(1, 'Selecione o tipo de endereço'),
  cep: z.string().optional().or(z.literal('')),
  logradouro: z.string().optional().or(z.literal('')),
  numero: z.string().optional().or(z.literal('')),
  complemento: z.string().optional().or(z.literal('')),
  bairro: z.string().optional().or(z.literal('')),
  cidadeId: z.coerce.number().optional().or(z.literal(0)),
  uf: z.string().max(2).optional().or(z.literal('')),
  principal: z.boolean().default(false),
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
  contatos: z.array(contatoSchema),
  enderecos: z.array(enderecoSchema),
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

interface EnderecoRowProps {
  index: number;
  register: UseFormRegister<ClienteFormData>;
  control: Control<ClienteFormData>;
  errors: FieldErrors<ClienteFormData>;
  onRemove: () => void;
  canRemove: boolean;
  onMakePrincipal: () => void;
}

const EnderecoRow: React.FC<EnderecoRowProps> = ({
  index,
  register,
  control,
  errors,
  onRemove,
  canRemove,
  onMakePrincipal,
}) => {
  const [cidades, setCidades] = useState<CidadeResponse[]>([]);
  const [loadingCidades, setLoadingCidades] = useState(false);

  const ufSelecionada = useWatch({
    control,
    name: `enderecos.${index}.uf`,
  });

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
    <div className="border border-gray-200 dark:border-gray-700 p-4 rounded-lg bg-gray-50 dark:bg-gray-800/30 mb-4">
      <FormGrid>
        <div className="lg:col-span-3">
          <FormField label="Tipo de Endereço" required error={errors?.tipoEndereco?.message}>
            <Select {...register(`enderecos.${index}.tipoEndereco`)}>
              <option value="RESIDENCIAL">Residencial</option>
              <option value="COMERCIAL">Comercial</option>
              <option value="CORRESPONDENCIA">Correspondência</option>
              <option value="OUTRO">Outro</option>
            </Select>
          </FormField>
        </div>

        <div className="lg:col-span-3">
          <FormField label="CEP" error={errors?.cep?.message}>
            <Input {...register(`enderecos.${index}.cep`)} placeholder="00000-000" />
          </FormField>
        </div>

        <div className="lg:col-span-6">
          <FormField label="Logradouro" error={errors?.logradouro?.message}>
            <Input {...register(`enderecos.${index}.logradouro`)} placeholder="Rua, Avenida..." />
          </FormField>
        </div>

        <div className="lg:col-span-2">
          <FormField label="Número" error={errors?.numero?.message}>
            <Input {...register(`enderecos.${index}.numero`)} placeholder="123" />
          </FormField>
        </div>

        <div className="lg:col-span-4">
          <FormField label="Complemento" error={errors?.complemento?.message}>
            <Input {...register(`enderecos.${index}.complemento`)} placeholder="Apto, Bloco..." />
          </FormField>
        </div>

        <div className="lg:col-span-3">
          <FormField label="Bairro" error={errors?.bairro?.message}>
            <Input {...register(`enderecos.${index}.bairro`)} placeholder="Bairro" />
          </FormField>
        </div>

        <div className="lg:col-span-1">
          <FormField label="UF" error={errors?.uf?.message}>
            <Select {...register(`enderecos.${index}.uf`)}>
              <option value="">...</option>
              {ESTADOS_BRASILEIROS.map(uf => (
                <option key={uf} value={uf}>{uf}</option>
              ))}
            </Select>
          </FormField>
        </div>

        <div className="lg:col-span-2">
          <FormField label="Cidade" error={errors?.cidadeId?.message}>
            <Select {...register(`enderecos.${index}.cidadeId`)} disabled={loadingCidades || cidades.length === 0}>
              <option value="">{loadingCidades ? 'Carregando...' : 'Selecione'}</option>
              {cidades.map(c => (
                <option key={c.id} value={c.id}>{c.nome}</option>
              ))}
            </Select>
          </FormField>
        </div>

        <div className="lg:col-span-2 flex items-center pt-6">
          <Controller
            name={`enderecos.${index}.principal`}
            control={control}
            render={({ field: checkboxField }) => (
              <Checkbox
                id={`endereco-principal-${index}`}
                checked={checkboxField.value}
                onChange={(e) => {
                  if (e.target.checked) {
                    onMakePrincipal();
                  } else {
                    checkboxField.onChange(false);
                  }
                }}
                label="Principal"
              />
            )}
          />
        </div>

        <div className="lg:col-span-10 flex items-center justify-end pt-5">
          <Button
            type="button"
            variant="text"
            className="text-red-500 hover:text-red-700"
            onClick={onRemove}
            disabled={!canRemove}
          >
            Remover Endereço
          </Button>
        </div>
      </FormGrid>
    </div>
  );
};

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
  const {
    register,
    handleSubmit,
    control,
    watch,
    reset,
    setValue,
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
      contatos: initialData?.contatos || [{ tipoContato: 'EMAIL', valor: '', principal: true }],
      enderecos: initialData?.enderecos || [{ tipoEndereco: 'RESIDENCIAL', cep: '', logradouro: '', numero: '', complemento: '', bairro: '', cidadeId: undefined, uf: '', principal: true }],
    },
  });

  const { fields: contatoFields, append: appendContato, remove: removeContato } = useFieldArray({
    control,
    name: 'contatos',
  });

  const { fields: enderecoFields, append: appendEndereco, remove: removeEndereco } = useFieldArray({
    control,
    name: 'enderecos',
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
        contatos: initialData.contatos && initialData.contatos.length > 0
          ? initialData.contatos
          : [{ tipoContato: 'EMAIL', valor: '', principal: true }],
        enderecos: initialData.enderecos && initialData.enderecos.length > 0
          ? initialData.enderecos
          : [{ tipoEndereco: 'RESIDENCIAL', cep: '', logradouro: '', numero: '', complemento: '', bairro: '', cidadeId: undefined, uf: '', principal: true }],
      });
    }
  }, [initialData, reset]);

  // eslint-disable-next-line react-hooks/incompatible-library
  const isFalecido = watch('falecido');

  const handleMakeContatoPrincipal = (index: number) => {
    contatoFields.forEach((_, idx) => {
      setValue(`contatos.${idx}.principal`, idx === index);
    });
  };

  const handleMakeEnderecoPrincipal = (index: number) => {
    enderecoFields.forEach((_, idx) => {
      setValue(`enderecos.${idx}.principal`, idx === index);
    });
  };

  const handleFormSubmit = (data: ClienteFormData) => {
    const contatosFiltrados = data.contatos.filter(c => c.valor && c.valor.trim() !== '');
    const enderecosFiltrados = data.enderecos.filter(e => 
      (e.cep && e.cep.trim() !== '') || 
      (e.logradouro && e.logradouro.trim() !== '') || 
      (e.cidadeId && Number(e.cidadeId) !== 0)
    );

    onSubmit({
      ...data,
      contatos: contatosFiltrados,
      enderecos: enderecosFiltrados,
    });
  };

  return (
    <form onSubmit={handleSubmit(handleFormSubmit)}>
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

      <FormSection title="Contatos" icon={<InfoIcon size={20} />}>
        <div className="flex flex-col gap-4">
          {contatoFields.map((field, index) => (
            <div key={field.id} className="border border-gray-200 dark:border-gray-700 p-4 rounded-lg bg-gray-50 dark:bg-gray-800/30">
              <FormGrid>
                <div className="lg:col-span-3">
                  <FormField label="Tipo de Contato" required error={errors.contatos?.[index]?.tipoContato?.message}>
                    <Select {...register(`contatos.${index}.tipoContato`)}>
                      <option value="EMAIL">E-mail</option>
                      <option value="TELEFONE">Telefone</option>
                      <option value="CELULAR">Celular</option>
                    </Select>
                  </FormField>
                </div>
                <div className="lg:col-span-6">
                  <FormField label="Contato" error={errors.contatos?.[index]?.valor?.message}>
                    <Input {...register(`contatos.${index}.valor`)} placeholder="Digite o e-mail ou número" />
                  </FormField>
                </div>
                <div className="lg:col-span-2 flex items-center pt-6">
                  <Controller
                    name={`contatos.${index}.principal`}
                    control={control}
                    render={({ field: checkboxField }) => (
                      <Checkbox
                        id={`contato-principal-${index}`}
                        checked={checkboxField.value}
                        onChange={(e) => {
                          if (e.target.checked) {
                            handleMakeContatoPrincipal(index);
                          } else {
                            checkboxField.onChange(false);
                          }
                        }}
                        label="Principal"
                      />
                    )}
                  />
                </div>
                <div className="lg:col-span-1 flex items-center justify-end pt-5">
                  <Button
                    type="button"
                    variant="text"
                    className="text-red-500 hover:text-red-700"
                    onClick={() => removeContato(index)}
                    disabled={contatoFields.length <= 1}
                  >
                    Remover
                  </Button>
                </div>
              </FormGrid>
            </div>
          ))}
          <div className="flex justify-start">
            <Button
              type="button"
              variant="outline"
              onClick={() => appendContato({ tipoContato: 'EMAIL', valor: '', principal: false })}
            >
              Adicionar Contato
            </Button>
          </div>
        </div>
      </FormSection>

      <FormSection title="Endereços" icon={<HomeIcon size={20} />}>
        <div className="flex flex-col">
          {enderecoFields.map((field, index) => (
            <EnderecoRow
              key={field.id}
              index={index}
              register={register}
              control={control}
              errors={errors.enderecos?.[index]}
              onRemove={() => removeEndereco(index)}
              canRemove={enderecoFields.length > 1}
              onMakePrincipal={() => handleMakeEnderecoPrincipal(index)}
            />
          ))}
          <div className="flex justify-start">
            <Button
              type="button"
              variant="outline"
              onClick={() => appendEndereco({ tipoEndereco: 'RESIDENCIAL', cep: '', logradouro: '', numero: '', complemento: '', bairro: '', cidadeId: undefined, uf: '', principal: false })}
            >
              Adicionar Endereço
            </Button>
          </div>
        </div>
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
