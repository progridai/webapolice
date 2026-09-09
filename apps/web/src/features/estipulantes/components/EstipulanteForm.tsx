import React, { useEffect, useState, useRef } from 'react';
import { useForm, useWatch, useFieldArray, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { FormField, Input, Select, Textarea, Button, FormSection, FormGrid, FormActions, HomeIcon, InfoIcon, BriefcaseIcon, ReadOnlyField, Checkbox, PlusIcon } from '../../../components/ui';
import { CnpjInput } from '../../../components/fields/CnpjInput';
import { CepInput } from '../../../components/fields/CepInput';
import { PhoneInput } from '../../../components/fields/PhoneInput';
import { EmailInput } from '../../../components/fields/EmailInput';
import { DateInput } from '../../../components/fields/DateInput';
import { consultarCep } from '../../../shared/api/enderecosApi';
import { isValidCnpj, isValidPhone } from '../../../shared/utils/validators';
import { normalizeCnpj, normalizeCep, normalizePhone } from '../../../shared/utils/normalizers';
import { buscarCidadesPorUf, type CidadeResponse } from '../../clientes/api/localidadesApi';

const ESTADOS_BRASILEIROS = [
  'AC', 'AL', 'AP', 'AM', 'BA', 'CE', 'DF', 'ES', 'GO', 'MA', 'MT', 'MS', 'MG',
  'PA', 'PB', 'PR', 'PE', 'PI', 'RJ', 'RN', 'RS', 'RO', 'RR', 'SC', 'SP', 'SE', 'TO'
];

const contatoSchema = z.object({
  tipoContato: z.string().min(1, 'Selecione o tipo de contato'),
  valor: z.string().optional().or(z.literal('')),
  principal: z.boolean().default(false),
}).superRefine((data, ctx) => {
  if (!data.valor || data.valor.trim() === '') return;
  const val = data.valor.trim();
  if (data.tipoContato === 'EMAIL' && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(val)) {
    ctx.addIssue({ code: z.ZodIssueCode.custom, message: 'E-mail inválido', path: ['valor'] });
  } else if ((data.tipoContato === 'TELEFONE' || data.tipoContato === 'CELULAR') && !isValidPhone(val)) {
    ctx.addIssue({ code: z.ZodIssueCode.custom, message: 'Telefone/Celular inválido', path: ['valor'] });
  } else if (val.length < 3) {
    ctx.addIssue({ code: z.ZodIssueCode.custom, message: 'O contato deve ter no mínimo 3 caracteres', path: ['valor'] });
  }
});

const estipulanteSchema = z.object({
  razaoSocial: z.string().trim().min(3, 'A Razão Social deve ter no mínimo 3 caracteres').max(150, 'Máximo de 150 caracteres'),
  nomeFantasia: z.string().trim().max(100, 'Máximo de 100 caracteres').optional(),
  cnpj: z.string().min(14, 'CNPJ inválido').refine(isValidCnpj, 'CNPJ inválido'),
  codigo: z.string().trim().max(50, 'Máximo de 50 caracteres').optional(),
  grupoPublicId: z.string().optional(),
  seguradoraPublicId: z.string().optional(),
  observacao: z.string().trim().max(1000, 'Máximo de 1000 caracteres').optional(),
  
  endereco: z.object({
    cep: z.string().optional().or(z.literal('')),
    logradouro: z.string().max(100, 'Máximo de 100 caracteres').optional().or(z.literal('')),
    numero: z.string().max(20, 'Máximo de 20 caracteres').optional().or(z.literal('')),
    complemento: z.string().max(100, 'Máximo de 100 caracteres').optional().or(z.literal('')),
    bairro: z.string().max(100, 'Máximo de 100 caracteres').optional().or(z.literal('')),
    uf: z.string().max(2).optional().or(z.literal('')),
    cidadeId: z.coerce.number().optional().or(z.literal(0)),
  }).optional(),
  
  contatos: z.array(contatoSchema),
  
  contatosInstitucionais: z.array(z.object({
    nome: z.string().min(1, 'O Nome é obrigatório').max(100, 'Máximo de 100 caracteres'),
    departamento: z.string().min(1, 'O Departamento é obrigatório').max(50, 'Máximo de 50 caracteres'),
    email: z.string().email('E-mail inválido').optional().or(z.literal('')),
    telefone: z.string().optional().or(z.literal('')).refine(val => !val || isValidPhone(val), 'Telefone inválido'),
    ramal: z.string().max(20, 'Máximo de 20 caracteres').optional().or(z.literal('')),
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
  onDelete?: () => void;
}

export const EstipulanteForm: React.FC<EstipulanteFormProps> = ({
  initialData,
  isEdit = false,
  isSubmitting = false,
  onSubmit,
  onCancel,
  onDelete,
}) => {
  const {
    register,
    handleSubmit,
    control,
    reset,
    setValue,
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
      values.contatos.forEach((_: unknown, idx: number) => {
        setValue(`contatos.${idx}.principal`, idx === index);
      });
    }
  };

  const [cidades, setCidades] = useState<CidadeResponse[]>([]);
  const [loadingCidades, setLoadingCidades] = useState(false);

  const ufSelecionada = useWatch({
    control,
    name: 'endereco.uf',
  });

  const cepSelecionado = useWatch({
    control,
    name: 'endereco.cep',
  });

  const watchedContatos = useWatch({
    control,
    name: 'contatos',
  });

  const lastFetchedCep = useRef(initialData?.endereco?.cep ? normalizeCep(initialData.endereco.cep) : '');

  // Gatilho CEP
  useEffect(() => {
    const rawCep = normalizeCep(cepSelecionado || '');
    if (rawCep.length !== 8) return;
    if (rawCep === lastFetchedCep.current) return;

    const controller = new AbortController();

    async function fetchCep() {
      try {
        const enderecoCompleto = await consultarCep(rawCep, controller.signal);
        
        setValue('endereco.logradouro', enderecoCompleto.logradouro || '', { shouldValidate: true });
        setValue('endereco.bairro', enderecoCompleto.bairro || '', { shouldValidate: true });
        setValue('endereco.uf', enderecoCompleto.uf || '', { shouldValidate: true });
        
        if (enderecoCompleto.cidadeId) {
          setValue('endereco.cidadeId', enderecoCompleto.cidadeId, { shouldValidate: true });
        }
        
        lastFetchedCep.current = rawCep;
      } catch (err: unknown) {
        if (err instanceof Error && err.name !== 'AbortError') {
          console.error('Erro ao buscar CEP:', err);
        }
      }
    }

    fetchCep();
    return () => controller.abort();
  }, [cepSelecionado, setValue]);

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
    const contatosFiltrados = data.contatos
      .filter(c => c.valor && c.valor.trim() !== '')
      .map(c => ({
        ...c,
        valor: (c.tipoContato === 'TELEFONE' || c.tipoContato === 'CELULAR') 
          ? normalizePhone(c.valor!) 
          : c.valor
      }));
      
    const contatosInstFiltrados = data.contatosInstitucionais
      ?.filter(c => c.nome.trim() !== '' && c.departamento.trim() !== '')
      .map(c => ({
        ...c,
        telefone: c.telefone ? normalizePhone(c.telefone) : c.telefone
      }));

    onSubmit({
      ...data,
      cnpj: normalizeCnpj(data.cnpj),
      endereco: hasEndereco ? {
        ...data.endereco,
        cep: normalizeCep(data.endereco!.cep || '')
      } : undefined,
      contatos: contatosFiltrados,
      contatosInstitucionais: contatosInstFiltrados,
    });
  };

  return (
    <form onSubmit={handleSubmit(handleFormSubmit)} className="flex flex-col gap-6">
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
                <CnpjInput 
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
              <Controller
                name="endereco.cep"
                control={control}
                render={({ field }) => (
                  <CepInput
                    {...field}
                    value={field.value || ''}
                    placeholder="00000-000"
                  />
                )}
              />
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
        <div className="flex flex-col gap-2">
          {contatoFields.map((field, index) => (
            <div key={field.id} className="p-3 rounded-lg bg-fundo-aplicacao border border-borda">
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
                    {(() => {
                      const type = watchedContatos?.[index]?.tipoContato || field.tipoContato;
                      if (type === 'EMAIL') {
                        return <EmailInput {...register(`contatos.${index}.valor`)} placeholder="Digite o e-mail" />;
                      } else if (type === 'TELEFONE' || type === 'CELULAR') {
                        return (
                          <Controller
                            name={`contatos.${index}.valor`}
                            control={control}
                            render={({ field: phoneField }) => (
                              <PhoneInput 
                                {...phoneField} 
                                value={phoneField.value || ''} 
                                placeholder="Digite o número" 
                              />
                            )}
                          />
                        );
                      }
                      return <Input {...register(`contatos.${index}.valor`)} placeholder="Digite o valor" />;
                    })()}
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
                <div className="lg:col-span-1 flex items-center pt-6">
                  <Button
                    type="button"
                    variant="text"
                    className="!text-erro"
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
              <PlusIcon size={16} className="mr-2" />
              Adicionar Contato
            </Button>
          </div>
        </div>
      </FormSection>

      <FormSection title="Contatos Institucionais" icon={<BriefcaseIcon size={20} />}>
        <div className="flex flex-col gap-2">
          {contatoInstFields.map((field, index) => (
            <div key={field.id} className="p-3 pt-8 rounded-lg bg-fundo-aplicacao border border-borda relative">
              <div className="absolute top-4 right-4">
                <Button
                  type="button"
                  variant="text"
                  className="!text-erro"
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
                    <EmailInput {...register(`contatosInstitucionais.${index}.email` as const)} placeholder="email@empresa.com" />
                  </FormField>
                </div>
                <div className="lg:col-span-3">
                  <FormField label="Telefone" error={errors.contatosInstitucionais?.[index]?.telefone?.message}>
                    <Controller
                      name={`contatosInstitucionais.${index}.telefone`}
                      control={control}
                      render={({ field: phoneInstField }) => (
                        <PhoneInput 
                          {...phoneInstField} 
                          value={phoneInstField.value || ''} 
                          placeholder="(00) 0000-0000" 
                        />
                      )}
                    />
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
              <PlusIcon size={16} className="mr-2" />
              Adicionar Contato Institucional
            </Button>
          </div>
        </div>
      </FormSection>

      <FormSection title="Configuração Operacional" icon={<BriefcaseIcon size={20} />}>
        <FormGrid>
          <div className="lg:col-span-6">
            <FormField label="Início de Vigência" required error={errors.configuracao?.dataInicioVigencia?.message}>
              <Controller
                name="configuracao.dataInicioVigencia"
                control={control}
                render={({ field: dateInitField }) => (
                  <DateInput 
                    {...dateInitField} 
                    value={dateInitField.value || ''} 
                    placeholder="DD/MM/YYYY" 
                  />
                )}
              />
            </FormField>
          </div>
          <div className="lg:col-span-6">
            <FormField label="Fim de Vigência" error={errors.configuracao?.dataFimVigencia?.message}>
              <Controller
                name="configuracao.dataFimVigencia"
                control={control}
                render={({ field: dateFimField }) => (
                  <DateInput 
                    {...dateFimField} 
                    value={dateFimField.value || ''} 
                    placeholder="DD/MM/YYYY" 
                  />
                )}
              />
            </FormField>
          </div>
        </FormGrid>
      </FormSection>

      <FormActions>
        <div className="flex-grow flex justify-start">
          {isEdit && onDelete && (
            <Button type="button" variant="danger" onClick={onDelete} disabled={isSubmitting}>
              Excluir
            </Button>
          )}
        </div>
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
