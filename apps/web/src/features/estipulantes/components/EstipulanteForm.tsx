import React, { useEffect, useState } from 'react';
import { useForm, useWatch, useFieldArray, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { FormField, Input, Select, Textarea, Button, FormSection, FormGrid, FormActions, HomeIcon, InfoIcon, BriefcaseIcon, ReadOnlyField, Checkbox } from '../../../components/ui';
import { buscarCidadesPorUf, type CidadeResponse } from '../../clientes/api/localidadesApi';

const ESTADOS_BRASILEIROS = [
  'AC', 'AL', 'AP', 'AM', 'BA', 'CE', 'DF', 'ES', 'GO', 'MA', 'MT', 'MS', 'MG',
  'PA', 'PB', 'PR', 'PE', 'PI', 'RJ', 'RN', 'RS', 'RO', 'RR', 'SC', 'SP', 'SE', 'TO'
];

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

const estipulanteSchema = z.object({
  razaoSocial: z.string().trim().min(3, 'A Razão Social deve ter no mínimo 3 caracteres'),
  nomeFantasia: z.string().trim().optional(),
  cnpj: z.string().min(14, 'CNPJ inválido').transform(val => val.replace(/\D/g, '')),
  codigo: z.string().trim().optional(),
  grupoPublicId: z.string().optional(),
  seguradoraPublicId: z.string().optional(),
  observacao: z.string().trim().optional(),
  
  endereco: z.object({
    cep: z.string().optional().or(z.literal('')),
    logradouro: z.string().optional().or(z.literal('')),
    numero: z.string().optional().or(z.literal('')),
    complemento: z.string().optional().or(z.literal('')),
    bairro: z.string().optional().or(z.literal('')),
    uf: z.string().max(2).optional().or(z.literal('')),
    cidadeId: z.coerce.number().optional().or(z.literal(0)),
  }).optional(),
  
  contatos: z.array(contatoSchema),
  
  contatosInstitucionais: z.array(z.object({
    nome: z.string().min(1, 'O Nome é obrigatório'),
    departamento: z.string().min(1, 'O Departamento é obrigatório'),
    email: z.string().email('E-mail inválido').optional().or(z.literal('')),
    telefone: z.string().optional().or(z.literal('')),
    ramal: z.string().optional().or(z.literal('')),
  })).optional(),
  
  configuracao: z.object({
    dataInicioVigencia: z.string().min(1, 'A data de início da vigência é obrigatória'),
    dataFimVigencia: z.string().optional().or(z.literal('')),
  }).superRefine((data, ctx) => {
    if (data.dataInicioVigencia && data.dataFimVigencia) {
      if (new Date(data.dataFimVigencia) < new Date(data.dataInicioVigencia)) {
        ctx.addIssue({
          path: ['dataFimVigencia'],
          code: z.ZodIssueCode.custom,
          message: 'Data final deve ser maior ou igual a inicial',
        });
      }
    }
  }),
});

export type EstipulanteFormData = z.infer<typeof estipulanteSchema>;

interface EstipulanteFormProps {
  initialData?: Partial<EstipulanteFormData>;
  isEdit?: boolean;
  isSubmitting?: boolean;
  onSubmit: (data: EstipulanteFormData) => void;
  onCancel: () => void;
}

export const EstipulanteForm: React.FC<EstipulanteFormProps> = ({
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
    reset,
    formState: { errors },
  } = useForm<EstipulanteFormData>({
    resolver: zodResolver(estipulanteSchema),
    defaultValues: {
      razaoSocial: initialData?.razaoSocial || '',
      nomeFantasia: initialData?.nomeFantasia || '',
      cnpj: initialData?.cnpj || '',
      codigo: initialData?.codigo || '',
      grupoPublicId: initialData?.grupoPublicId || '',
      seguradoraPublicId: initialData?.seguradoraPublicId || '',
      observacao: initialData?.observacao || '',
      endereco: initialData?.endereco || {
        cep: '',
        logradouro: '',
        numero: '',
        complemento: '',
        bairro: '',
        uf: '',
        cidadeId: undefined,
      },
      contatos: initialData?.contatos || [{ tipoContato: 'EMAIL', valor: '', principal: true }],
      contatosInstitucionais: initialData?.contatosInstitucionais || [],
      configuracao: initialData?.configuracao || {
        dataInicioVigencia: '',
        dataFimVigencia: '',
      },
    },
  });

  useEffect(() => {
    if (initialData) {
      reset({
        razaoSocial: initialData.razaoSocial || '',
        nomeFantasia: initialData.nomeFantasia || '',
        cnpj: initialData.cnpj || '',
        codigo: initialData.codigo || '',
        grupoPublicId: initialData.grupoPublicId || '',
        seguradoraPublicId: initialData.seguradoraPublicId || '',
        observacao: initialData.observacao || '',
        endereco: initialData.endereco || {
          cep: '', logradouro: '', numero: '', complemento: '', bairro: '', uf: '', cidadeId: undefined
        },
        contatos: initialData.contatos && initialData.contatos.length > 0 
          ? initialData.contatos 
          : [{ tipoContato: 'EMAIL', valor: '', principal: true }],
        contatosInstitucionais: initialData.contatosInstitucionais || [],
        configuracao: initialData.configuracao || {
          dataInicioVigencia: '', dataFimVigencia: ''
        },
      });
    }
  }, [initialData, reset]);

  const {
    fields: contatoFields,
    append: appendContato,
    remove: removeContato,
  } = useFieldArray({
    control,
    name: 'contatos',
  });

  const {
    fields: contatoInstFields,
    append: appendContatoInst,
    remove: removeContatoInst,
  } = useFieldArray({
    control,
    name: 'contatosInstitucionais',
  });

  const handleMakeContatoPrincipal = (index: number) => {
    const values = control._formValues;
    if (values.contatos) {
      values.contatos.forEach((_: any, idx: number) => {
        control._subjects.values.next({
          name: `contatos.${idx}.principal`,
          value: idx === index
        });
      });
    }
  };

  const [cidades, setCidades] = useState<CidadeResponse[]>([]);
  const [loadingCidades, setLoadingCidades] = useState(false);

  const ufSelecionada = useWatch({
    control,
    name: 'endereco.uf',
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

  const handleFormSubmit = (data: EstipulanteFormData) => {
    // Sanitização dos dados antes do envio
    const hasEndereco = data.endereco && (
      data.endereco.cep || data.endereco.logradouro || data.endereco.cidadeId || data.endereco.uf
    );
    const contatosFiltrados = data.contatos.filter(c => c.valor && c.valor.trim() !== '');
    const contatosInstFiltrados = data.contatosInstitucionais?.filter(c => c.nome.trim() !== '' && c.departamento.trim() !== '');

    onSubmit({
      ...data,
      endereco: hasEndereco ? data.endereco : undefined,
      contatos: contatosFiltrados,
      contatosInstitucionais: contatosInstFiltrados,
    });
  };

  return (
    <form onSubmit={handleSubmit(handleFormSubmit)}>
      <FormSection title="Dados Principais" icon={<BriefcaseIcon size={20} />}>
        <FormGrid>
          <div className="lg:col-span-12">
            <FormField label="Razão Social" required error={errors.razaoSocial?.message}>
              <Input {...register('razaoSocial')} placeholder="Digite a Razão Social da empresa" />
            </FormField>
          </div>

          <div className="lg:col-span-6">
            <FormField label="Nome Fantasia" error={errors.nomeFantasia?.message}>
              <Input {...register('nomeFantasia')} placeholder="Digite o Nome Fantasia (opcional)" />
            </FormField>
          </div>

          <div className="lg:col-span-6">
            <FormField label="CNPJ" required error={errors.cnpj?.message}>
              {isEdit ? (
                <ReadOnlyField value={initialData?.cnpj ? initialData.cnpj.replace(/^(\d{2})(\d{3})(\d{3})(\d{4})(\d{2})$/, '$1.$2.$3/$4-$5') : ''} />
              ) : (
                <Input 
                  {...register('cnpj')} 
                  placeholder="00.000.000/0000-00" 
                />
              )}
            </FormField>
          </div>

          <div className="lg:col-span-4">
            <FormField label="Código" error={errors.codigo?.message}>
              <Input {...register('codigo')} placeholder="Código opcional" />
            </FormField>
          </div>

          <div className="lg:col-span-4">
            <FormField label="Grupo" error={errors.grupoPublicId?.message}>
              <Select {...register('grupoPublicId')} disabled={true}>
                <option value="">Selecione um grupo (Em breve)</option>
              </Select>
            </FormField>
          </div>

          <div className="lg:col-span-4">
            <FormField label="Seguradora" error={errors.seguradoraPublicId?.message}>
              <Select {...register('seguradoraPublicId')} disabled={true}>
                <option value="">Selecione uma seguradora (Em breve)</option>
              </Select>
            </FormField>
          </div>

          <div className="lg:col-span-12">
            <FormField label="Observações" error={errors.observacao?.message}>
              <Textarea {...register('observacao')} placeholder="Observações internas..." rows={3} />
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

          <div className="lg:col-span-7">
            <FormField label="Logradouro" error={errors.endereco?.logradouro?.message}>
              <Input {...register('endereco.logradouro')} placeholder="Rua, Avenida..." />
            </FormField>
          </div>

          <div className="lg:col-span-2">
            <FormField label="Número" error={errors.endereco?.numero?.message}>
              <Input {...register('endereco.numero')} placeholder="123" />
            </FormField>
          </div>

          <div className="lg:col-span-3">
            <FormField label="Complemento" error={errors.endereco?.complemento?.message}>
              <Input {...register('endereco.complemento')} placeholder="Andar, Sala..." />
            </FormField>
          </div>

          <div className="lg:col-span-4">
            <FormField label="Bairro" error={errors.endereco?.bairro?.message}>
              <Input {...register('endereco.bairro')} placeholder="Bairro" />
            </FormField>
          </div>

          <div className="lg:col-span-2">
            <FormField label="UF" error={errors.endereco?.uf?.message}>
              <Select {...register('endereco.uf')}>
                <option value="">...</option>
                {ESTADOS_BRASILEIROS.map(uf => (
                  <option key={uf} value={uf}>{uf}</option>
                ))}
              </Select>
            </FormField>
          </div>

          <div className="lg:col-span-3">
            <FormField label="Cidade" error={errors.endereco?.cidadeId?.message}>
              <Controller
                name="endereco.cidadeId"
                control={control}
                render={({ field }) => (
                  <Select 
                    {...field} 
                    value={field.value || ""} 
                    disabled={loadingCidades || cidades.length === 0}
                  >
                    <option value="">{loadingCidades ? 'Carregando...' : 'Selecione'}</option>
                    {cidades.map(c => (
                      <option key={c.id} value={c.id}>{c.nome}</option>
                    ))}
                  </Select>
                )}
              />
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
          <div className="flex justify-start mt-2">
            <Button
              type="button"
              variant="outline"
              onClick={() => appendContato({ tipoContato: 'EMAIL', valor: '', principal: false })}
            >
              + Adicionar Contato
            </Button>
          </div>
        </div>
      </FormSection>

      <FormSection title="Contatos Institucionais" icon={<BriefcaseIcon size={20} />}>
        <div className="flex flex-col gap-4">
          {contatoInstFields.map((field, index) => (
            <div key={field.id} className="border border-gray-200 dark:border-gray-700 p-4 rounded-lg bg-gray-50 dark:bg-gray-800/30 relative">
              <div className="absolute top-4 right-4">
                <Button
                  type="button"
                  variant="text"
                  className="text-red-500 hover:text-red-700 p-0"
                  onClick={() => removeContatoInst(index)}
                >
                  Remover
                </Button>
              </div>
              <FormGrid>
                <div className="lg:col-span-6">
                  <FormField label="Nome" required error={errors.contatosInstitucionais?.[index]?.nome?.message}>
                    <Input {...register(`contatosInstitucionais.${index}.nome` as const)} placeholder="Nome do contato" />
                  </FormField>
                </div>
                <div className="lg:col-span-6">
                  <FormField label="Departamento" required error={errors.contatosInstitucionais?.[index]?.departamento?.message}>
                    <Input {...register(`contatosInstitucionais.${index}.departamento` as const)} placeholder="Ex: Financeiro, RH" />
                  </FormField>
                </div>
                <div className="lg:col-span-6">
                  <FormField label="E-mail" error={errors.contatosInstitucionais?.[index]?.email?.message}>
                    <Input type="email" {...register(`contatosInstitucionais.${index}.email` as const)} placeholder="email@empresa.com" />
                  </FormField>
                </div>
                <div className="lg:col-span-3">
                  <FormField label="Telefone" error={errors.contatosInstitucionais?.[index]?.telefone?.message}>
                    <Input {...register(`contatosInstitucionais.${index}.telefone` as const)} placeholder="(00) 0000-0000" />
                  </FormField>
                </div>
                <div className="lg:col-span-3">
                  <FormField label="Ramal" error={errors.contatosInstitucionais?.[index]?.ramal?.message}>
                    <Input {...register(`contatosInstitucionais.${index}.ramal` as const)} placeholder="Ex: 123" />
                  </FormField>
                </div>
              </FormGrid>
            </div>
          ))}
          <div className="flex justify-start mt-2">
            <Button
              type="button"
              variant="outline"
              onClick={() => appendContatoInst({ nome: '', departamento: '', email: '', telefone: '', ramal: '' })}
            >
              + Adicionar Contato Institucional
            </Button>
          </div>
        </div>
      </FormSection>

      <FormSection title="Configuração Operacional" icon={<BriefcaseIcon size={20} />}>
        <FormGrid>
          <div className="lg:col-span-6">
            <FormField label="Início de Vigência" required error={errors.configuracao?.dataInicioVigencia?.message}>
              <Input type="date" {...register('configuracao.dataInicioVigencia')} />
            </FormField>
          </div>
          <div className="lg:col-span-6">
            <FormField label="Fim de Vigência" error={errors.configuracao?.dataFimVigencia?.message}>
              <Input type="date" {...register('configuracao.dataFimVigencia')} />
            </FormField>
          </div>
        </FormGrid>
      </FormSection>

      <FormActions>
        <Button type="button" variant="text" onClick={onCancel} disabled={isSubmitting}>
          Cancelar
        </Button>
        <Button type="submit" variant="primary" disabled={isSubmitting} loading={isSubmitting}>
          Salvar Estipulante
        </Button>
      </FormActions>
    </form>
  );
};
