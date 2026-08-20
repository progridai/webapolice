import React, { useEffect, useState } from 'react';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Modal, FormField, Input, Button, Select, ReadOnlyField, Alert } from '../../../../components/ui';
import { ramosApi } from '../../../ramos/api/ramos.api';
import { vincularRamoApolice, atualizarRamoApolice } from '../../api/apolices.api';
import type { ApoliceRamoResult } from '../../types/apolice.types';

const ramoFormSchema = z.object({
  ramoPublicId: z.string().min(1, 'Selecione um Ramo'),
  numeroApolice: z.string().optional(),
  iofPercentual: z.coerce.number().min(0).max(100).optional().or(z.literal('')),
});

export type ApoliceRamoFormData = z.infer<typeof ramoFormSchema>;

interface ApoliceRamoFormModalProps {
  aberto: boolean;
  onClose: () => void;
  apolicePublicId: string;
  ramoEdicao?: ApoliceRamoResult; 
  onSucesso: () => void;
}

export const ApoliceRamoFormModal: React.FC<ApoliceRamoFormModalProps> = ({
  aberto,
  onClose,
  apolicePublicId,
  ramoEdicao,
  onSucesso,
}) => {
  const [ramosOptions, setRamosOptions] = useState<{ value: string; label: string }[]>([]);
  const [carregandoRamos, setCarregandoRamos] = useState(false);
  const [salvando, setSalvando] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const isEdicao = !!ramoEdicao;

  const {
    register,
    handleSubmit,
    control,
    reset,
    formState: { errors },
  } = useForm<ApoliceRamoFormData>({
    resolver: zodResolver(ramoFormSchema),
    defaultValues: {
      ramoPublicId: '',
      numeroApolice: '',
      iofPercentual: '',
    },
  });

  useEffect(() => {
    if (aberto) {
      setError(null);
      if (isEdicao && ramoEdicao) {
        reset({
          ramoPublicId: ramoEdicao.publicId, 
          numeroApolice: ramoEdicao.numeroApolice || '',
          iofPercentual: ramoEdicao.iofPercentual ?? '',
        });
      } else {
        reset({
          ramoPublicId: '',
          numeroApolice: '',
          iofPercentual: '',
        });
        carregarRamos();
      }
    }
  }, [aberto, isEdicao, ramoEdicao, reset]);

  const carregarRamos = async () => {
    try {
      setCarregandoRamos(true);
      setError(null);
      const response = await ramosApi.listar({ pagina: 1, tamanhoPagina: 100, ativo: true });
      setRamosOptions(response.items.map(r => ({
        value: r.publicId,
        label: `${r.codigo} — ${r.nome}`
      })));
    } catch (err) {
      setError('Erro ao carregar Ramos. Tente novamente.');
    } finally {
      setCarregandoRamos(false);
    }
  };

  const onSubmit = async (data: ApoliceRamoFormData) => {
    try {
      setSalvando(true);
      setError(null);
      if (isEdicao && ramoEdicao) {
        await atualizarRamoApolice(apolicePublicId, ramoEdicao.publicId, {
          numeroApolice: data.numeroApolice,
          iofPercentual: data.iofPercentual === '' ? undefined : Number(data.iofPercentual),
        });
      } else {
        await vincularRamoApolice(apolicePublicId, {
          ramoPublicId: data.ramoPublicId,
          numeroApolice: data.numeroApolice,
          iofPercentual: data.iofPercentual === '' ? undefined : Number(data.iofPercentual),
        });
      }
      onSucesso();
      onClose();
    } catch (error: any) {
      setError(error.response?.data?.message || error.message || 'Erro inesperado.');
    } finally {
      setSalvando(false);
    }
  };

  return (
    <Modal
      aberto={aberto}
      onClose={!salvando ? onClose : () => {}}
      title={isEdicao ? 'Editar Vínculo do Ramo' : 'Adicionar Ramo à Apólice'}
      size="medium"
    >
      <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-4 py-2">
        {error && (
          <Alert type="error" title="Falha ao salvar" onClose={() => setError(null)}>
            {error}
          </Alert>
        )}
        <div className="grid grid-cols-1 gap-4">
          {isEdicao && ramoEdicao ? (
            <ReadOnlyField 
              label="Ramo" 
              value={`${ramoEdicao.ramoCodigo} — ${ramoEdicao.ramoNome}`} 
            />
          ) : (
            <Controller
              name="ramoPublicId"
              control={control}
              render={({ field }) => (
                <FormField label="Ramo" required error={errors.ramoPublicId?.message}>
                  <Select
                    {...field}
                    options={ramosOptions}
                    placeholder={carregandoRamos ? "Carregando..." : "Selecione o Ramo"}
                    disabled={carregandoRamos || salvando}
                  />
                </FormField>
              )}
            />
          )}

          <FormField label="Número da Apólice no Ramo" error={errors.numeroApolice?.message}>
            <Input {...register('numeroApolice')} placeholder="Ex: 0001" disabled={salvando} />
          </FormField>

          <FormField label="IOF (%)" error={errors.iofPercentual?.message}>
            <Input 
              {...register('iofPercentual')} 
              type="number" 
              step="0.01" 
              min="0" 
              max="100" 
              placeholder="Ex: 7.38" 
              disabled={salvando} 
            />
          </FormField>
        </div>

        <div className="flex justify-end gap-2 mt-4 pt-4 border-t border-borda-padrao">
          <Button type="button" variant="text" onClick={onClose} disabled={salvando}>
            Cancelar
          </Button>
          <Button type="submit" variant="primary" disabled={salvando} loading={salvando}>
            {isEdicao ? 'Salvar' : 'Adicionar'}
          </Button>
        </div>
      </form>
    </Modal>
  );
};
