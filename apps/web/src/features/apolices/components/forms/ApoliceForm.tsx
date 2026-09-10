import React from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useNavigate } from 'react-router-dom';
import { ROUTES } from '../../../../app/routes/routePaths';
import { Button, Input, FormField, Card, CardHeader, CardContent, FormGrid } from '../../../../components/ui';
import { apoliceFormSchema, type ApoliceFormValues } from '../../schemas/apoliceForm.schema';
import { listarEstipulantes } from '../../../estipulantes/api/estipulantes.api';
import { seguradorasApi } from '../../../seguradoras/api/seguradoras.api';
import { corretorasApi } from '../../../corretoras/api/corretoras.api';
import { AsyncEntityField } from './AsyncEntityField';

interface ApoliceFormProps {
  initialData?: Partial<ApoliceFormValues>;
  onSubmit: (data: ApoliceFormValues) => Promise<void>;
  isLoading?: boolean;
}

export const ApoliceForm: React.FC<ApoliceFormProps> = ({ initialData, onSubmit, isLoading }) => {
  const navigate = useNavigate();

  const { register, handleSubmit, control, formState: { errors } } = useForm<ApoliceFormValues>({
    resolver: zodResolver(apoliceFormSchema),
    defaultValues: {
      nome: initialData?.nome || '',
      estipulanteId: initialData?.estipulanteId || '',
      seguradoraId: initialData?.seguradoraId || '',
      corretoraId: initialData?.corretoraId || '',
      dataInicioVigencia: initialData?.dataInicioVigencia || '',
      dataFimVigencia: initialData?.dataFimVigencia || '',
      dataAniversario: initialData?.dataAniversario || '',
      observacao: initialData?.observacao || '',
    },
  });

  const handleVoltar = () => {
    navigate(-1);
  };

  const loadEstipulantes = async (inputValue: string) => {
    const result = await listarEstipulantes({ busca: inputValue, page: 1, pageSize: 20 });
    return result.itens.map(item => ({ value: String(item.id), label: item.razaoSocial || item.nomeFantasia || item.nome || 'Sem Nome' }));
  };

  const loadSeguradoras = async (inputValue: string) => {
    const result = await seguradorasApi.listar({ busca: inputValue, page: 1, pageSize: 20 });
    return result.itens.map(item => ({ value: String(item.id), label: item.razaoSocial || item.nomeFantasia || item.nome || 'Sem Nome' }));
  };

  const loadCorretoras = async (inputValue: string) => {
    const result = await corretorasApi.listar({ busca: inputValue, page: 1, pageSize: 20 });
    return result.itens.map(item => ({ value: String(item.id), label: item.razaoSocial || item.nomeFantasia || item.nome || 'Sem Nome' }));
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
      <Card>
        <CardHeader>
          <h2 className="text-lg font-medium text-texto-principal">Dados da Apólice</h2>
          <p className="text-sm text-texto-terciario">Informações principais do contrato coletivo</p>
        </CardHeader>
        <CardContent className="space-y-4">
          <FormGrid>
            <div className="lg:col-span-12">
              <FormField label="Nome (Denominação)" required error={errors.nome?.message}>
                <Input
                  id="nome"
                  placeholder="Ex: Vida em Grupo - Empresas XYZ"
                  {...register('nome')}
                />
              </FormField>
            </div>

            <div className="lg:col-span-6">
              <FormField label="Estipulante" required error={errors.estipulanteId?.message}>
                <AsyncEntityField
                  name="estipulanteId"
                  control={control}
                  label="Estipulante"
                  placeholder="Selecione ou busque o estipulante"
                  searcher={loadEstipulantes}
                  initialLabel={(initialData as any)?._estipulanteNome}
                />
              </FormField>
            </div>
            
            <div className="lg:col-span-6">
              <FormField label="Seguradora" required error={errors.seguradoraId?.message}>
                <AsyncEntityField
                  name="seguradoraId"
                  control={control}
                  label="Seguradora"
                  placeholder="Selecione ou busque a seguradora"
                  searcher={loadSeguradoras}
                  initialLabel={(initialData as any)?._seguradoraNome}
                />
              </FormField>
            </div>

            <div className="lg:col-span-12">
              <FormField label="Corretora (Opcional)" error={errors.corretoraId?.message}>
                <AsyncEntityField
                  name="corretoraId"
                  control={control}
                  label="Corretora"
                  placeholder="Selecione ou busque a corretora (Opcional)"
                  searcher={loadCorretoras}
                  initialLabel={(initialData as any)?._corretoraNome}
                />
              </FormField>
            </div>

            <div className="lg:col-span-4">
              <FormField label="Início da Vigência" required error={errors.dataInicioVigencia?.message}>
                <Input
                  id="dataInicioVigencia"
                  type="date"
                  {...register('dataInicioVigencia')}
                />
              </FormField>
            </div>

            <div className="lg:col-span-4">
              <FormField label="Fim da Vigência" error={errors.dataFimVigencia?.message}>
                <Input
                  id="dataFimVigencia"
                  type="date"
                  {...register('dataFimVigencia')}
                />
              </FormField>
            </div>

            <div className="lg:col-span-4">
              <FormField label="Data de Aniversário" error={errors.dataAniversario?.message}>
                <Input
                  id="dataAniversario"
                  type="date"
                  {...register('dataAniversario')}
                />
              </FormField>
            </div>

            <div className="lg:col-span-12">
              <FormField label="Observação" error={errors.observacao?.message}>
                <Input
                  id="observacao"
                  placeholder="Anotações internas..."
                  {...register('observacao')}
                />
              </FormField>
            </div>
          </FormGrid>
        </CardContent>
      </Card>

      <div className="flex justify-end gap-3 pt-4 border-t border-borda">
        <Button type="button" variant="outline" onClick={handleVoltar} disabled={isLoading}>
          Cancelar
        </Button>
        <Button type="submit" disabled={isLoading} className="min-w-[120px]">
          {isLoading ? 'Salvando...' : 'Salvar Apólice'}
        </Button>
      </div>
    </form>
  );
};
