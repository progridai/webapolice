import React, { useEffect, useState } from 'react';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { FormField, Input, Textarea, Checkbox, FormSection, FormGrid, FormActions, Button } from '../../../components/ui';
import { buscarCidadesPorUf, type CidadeResponse } from '../../clientes/api/localidadesApi';
import { listarCoordenadoresAtivos } from '../api/cooperadosApi';
import type { CooperadoFormData, CooperadoListDto } from '../types/cooperados.types';

const ESTADOS_BRASILEIROS = [
  'AC', 'AL', 'AP', 'AM', 'BA', 'CE', 'DF', 'ES', 'GO', 'MA', 'MT', 'MS', 'MG',
  'PA', 'PB', 'PR', 'PE', 'PI', 'RJ', 'RN', 'RS', 'RO', 'RR', 'SC', 'SP', 'SE', 'TO'
];

const formSchema = z.object({
  tipo: z.coerce.number().min(1).max(2),
  nome: z.string().min(3, 'Nome é obrigatório e deve ter no mínimo 3 caracteres'),
  cpf: z.string().min(11, 'CPF inválido'),
  dataNascimento: z.string().optional().or(z.literal('')),
  codigo: z.string().optional().or(z.literal('')),
  coordenadorId: z.coerce.number().optional().or(z.literal(0)),
  
  rg: z.string().optional().or(z.literal('')),
  orgaoEmissor: z.string().optional().or(z.literal('')),
  dataEmissaoRg: z.string().optional().or(z.literal('')),
  susep: z.string().optional().or(z.literal('')),
  inss: z.string().optional().or(z.literal('')),
  issqn: z.string().optional().or(z.literal('')),
  
  telefone: z.string().optional().or(z.literal('')),
  email: z.string().email('E-mail inválido').optional().or(z.literal('')),
  
  cep: z.string().optional().or(z.literal('')),
  logradouro: z.string().optional().or(z.literal('')),
  numero: z.string().optional().or(z.literal('')),
  complemento: z.string().optional().or(z.literal('')),
  bairro: z.string().optional().or(z.literal('')),
  uf: z.string().max(2).optional().or(z.literal('')),
  cidadeId: z.coerce.number().optional().or(z.literal(0)),
  
  numeroDependentes: z.coerce.number().optional(),
  dataInscricao: z.string().optional().or(z.literal('')),
  credenciado: z.boolean().default(false),
  
  bancoId: z.coerce.number().optional().or(z.literal(0)),
  agencia: z.string().optional().or(z.literal('')),
  contaCorrente: z.string().optional().or(z.literal('')),
  
  observacao: z.string().optional().or(z.literal('')),
}).superRefine((data, ctx) => {
  if (data.tipo === 1 && !data.coordenadorId) {
    ctx.addIssue({
      path: ['coordenadorId'],
      code: z.ZodIssueCode.custom,
      message: 'Coordenador responsável é obrigatório para Cooperados',
    });
  }
});

type FormSchemaType = z.infer<typeof formSchema>;

interface CooperadoFormProps {
  initialData?: CooperadoFormData;
  onSubmit: (data: CooperadoFormData) => Promise<void>;
  onCancel: () => void;
  isLoading?: boolean;
}

export const CooperadoForm: React.FC<CooperadoFormProps> = ({
  initialData,
  onSubmit,
  onCancel,
  isLoading
}) => {
  const [cidades, setCidades] = useState<CidadeResponse[]>([]);
  const [loadingCidades, setLoadingCidades] = useState(false);
  const [coordenadores, setCoordenadores] = useState<CooperadoListDto[]>([]);

  const { register, control, handleSubmit, formState: { errors }, watch } = useForm<FormSchemaType>({
    resolver: zodResolver(formSchema),
    defaultValues: initialData || {
      tipo: 1,
      credenciado: false,
    }
  });

  const ufSelecionada = watch('uf');
  const tipoSelecionado = watch('tipo');

  useEffect(() => {
    async function carregarCidades() {
      if (!ufSelecionada || ufSelecionada.length !== 2) {
        setCidades([]);
        return;
      }
      setLoadingCidades(true);
      try {
        const data = await buscarCidadesPorUf(ufSelecionada);
        setCidades(data);
      } catch (err) {
        console.error('Erro ao carregar cidades', err);
        setCidades([]);
      } finally {
        setLoadingCidades(false);
      }
    }
    carregarCidades();
  }, [ufSelecionada]);

  useEffect(() => {
    async function carregarCoordenadores() {
      try {
        const data = await listarCoordenadoresAtivos();
        setCoordenadores(data);
      } catch (err) {
        console.error('Erro ao carregar coordenadores', err);
      }
    }
    carregarCoordenadores();
  }, []);

  const handleFormSubmit = async (data: FormSchemaType) => {
    await onSubmit(data as unknown as CooperadoFormData);
  };

  return (
    <form onSubmit={handleSubmit(handleFormSubmit)} noValidate>
      <FormSection title="Identificação" description="Dados principais do cooperado ou coordenador.">
        <FormGrid>
          <FormField label="Tipo" required error={errors.tipo?.message}>
            <select {...register('tipo')} className="form-select">
              <option value="1">Cooperado</option>
              <option value="2">Coordenador</option>
            </select>
          </FormField>
          
          <FormField label="Código" error={errors.codigo?.message}>
            <Input {...register('codigo')} placeholder="Código interno" />
          </FormField>

          <FormField label="Nome" required error={errors.nome?.message} className="md:col-span-2">
            <Input {...register('nome')} placeholder="Nome completo" />
          </FormField>

          <FormField label="CPF" required error={errors.cpf?.message}>
            <Input {...register('cpf')} placeholder="000.000.000-00" />
          </FormField>

          <FormField label="Data de Nascimento" error={errors.dataNascimento?.message}>
            <Input type="date" {...register('dataNascimento')} />
          </FormField>

          {tipoSelecionado === 1 && (
            <FormField label="Coordenador responsável" required error={errors.coordenadorId?.message} className="md:col-span-2">
              <select {...register('coordenadorId')} className="form-select">
                <option value="">Selecione um coordenador...</option>
                {coordenadores.map(c => (
                  <option key={c.publicId} value={c.publicId}>{c.nome}</option>
                ))}
              </select>
            </FormField>
          )}
        </FormGrid>
      </FormSection>

      <FormSection title="Documentação" description="Documentos adicionais e registros profissionais.">
        <FormGrid>
          <FormField label="RG" error={errors.rg?.message}>
            <Input {...register('rg')} placeholder="Número do RG" />
          </FormField>
          <FormField label="Órgão Emissor" error={errors.orgaoEmissor?.message}>
            <Input {...register('orgaoEmissor')} placeholder="Ex: SSP" />
          </FormField>
          <FormField label="Data de Emissão" error={errors.dataEmissaoRg?.message}>
            <Input type="date" {...register('dataEmissaoRg')} />
          </FormField>
          <FormField label="SUSEP" error={errors.susep?.message}>
            <Input {...register('susep')} placeholder="Registro SUSEP" />
          </FormField>
          <FormField label="INSS" error={errors.inss?.message}>
            <Input {...register('inss')} placeholder="Registro INSS" />
          </FormField>
          <FormField label="ISSQN" error={errors.issqn?.message}>
            <Input {...register('issqn')} placeholder="Registro ISSQN" />
          </FormField>
        </FormGrid>
      </FormSection>

      <FormSection title="Contato" description="Telefone e E-mail principais.">
        <FormGrid>
          <FormField label="Telefone" error={errors.telefone?.message}>
            <Input {...register('telefone')} placeholder="(00) 00000-0000" />
          </FormField>
          <FormField label="E-mail" error={errors.email?.message}>
            <Input type="email" {...register('email')} placeholder="email@exemplo.com" />
          </FormField>
        </FormGrid>
      </FormSection>

      <FormSection title="Endereço" description="Endereço residencial ou comercial.">
        <FormGrid>
          <FormField label="CEP" error={errors.cep?.message}>
            <Input {...register('cep')} placeholder="00000-000" />
          </FormField>
          <FormField label="Logradouro" className="md:col-span-2" error={errors.logradouro?.message}>
            <Input {...register('logradouro')} placeholder="Rua, Avenida, etc." />
          </FormField>
          <FormField label="Número" error={errors.numero?.message}>
            <Input {...register('numero')} placeholder="Número" />
          </FormField>
          <FormField label="Complemento" error={errors.complemento?.message}>
            <Input {...register('complemento')} placeholder="Apto, Sala, etc." />
          </FormField>
          <FormField label="Bairro" error={errors.bairro?.message}>
            <Input {...register('bairro')} placeholder="Bairro" />
          </FormField>
          <FormField label="Estado (UF)" error={errors.uf?.message}>
            <select {...register('uf')} className="form-select">
              <option value="">Selecione...</option>
              {ESTADOS_BRASILEIROS.map(estado => (
                <option key={estado} value={estado}>{estado}</option>
              ))}
            </select>
          </FormField>
          <FormField label="Cidade" error={errors.cidadeId?.message} className="md:col-span-2">
            <select {...register('cidadeId')} className="form-select" disabled={loadingCidades || !ufSelecionada}>
              <option value="">{loadingCidades ? 'Carregando...' : 'Selecione uma cidade...'}</option>
              {cidades.map(c => (
                <option key={c.id} value={c.id}>{c.nome}</option>
              ))}
            </select>
          </FormField>
        </FormGrid>
      </FormSection>

      <FormSection title="Dados Operacionais" description="Informações operacionais e inscrições.">
        <FormGrid>
          <FormField label="Número de Dependentes" error={errors.numeroDependentes?.message}>
            <Input type="number" {...register('numeroDependentes')} />
          </FormField>
          <FormField label="Data de Inscrição" error={errors.dataInscricao?.message}>
            <Input type="date" {...register('dataInscricao')} />
          </FormField>
          <div className="md:col-span-2 flex items-center h-full pt-6">
            <Controller
              control={control}
              name="credenciado"
              render={({ field }) => (
                <Checkbox
                  id="credenciado"
                  checked={field.value}
                  onCheckedChange={field.onChange}
                  label="Cooperado Credenciado"
                />
              )}
            />
          </div>
        </FormGrid>
      </FormSection>

      <FormSection title="Dados Bancários" description="Informações para repasse financeiro.">
        <FormGrid>
          <FormField label="Banco" error={errors.bancoId?.message} className="md:col-span-2">
            <Input type="number" {...register('bancoId')} placeholder="ID do Banco" />
          </FormField>
          <FormField label="Agência" error={errors.agencia?.message}>
            <Input {...register('agencia')} placeholder="Número da agência" />
          </FormField>
          <FormField label="Conta Corrente" error={errors.contaCorrente?.message}>
            <Input {...register('contaCorrente')} placeholder="Número da conta" />
          </FormField>
        </FormGrid>
      </FormSection>

      <FormSection title="Observações" description="Informações adicionais.">
        <FormField error={errors.observacao?.message}>
          <Textarea {...register('observacao')} placeholder="Observações..." rows={4} />
        </FormField>
      </FormSection>

      <FormActions>
        <Button type="button" variant="outline" onClick={onCancel} disabled={isLoading}>
          Cancelar
        </Button>
        <Button type="submit" variant="primary" disabled={isLoading}>
          {isLoading ? 'Salvando...' : 'Salvar'}
        </Button>
      </FormActions>
    </form>
  );
};
