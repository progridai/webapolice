import React, { useEffect, useState } from 'react';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Modal, Button, Input, Select, Alert } from '../../../../components/ui';
import { subestipulanteApoliceSchema, type SubestipulanteApoliceFormValues } from '../../schemas/subestipulanteApolice.schema';
import { vincularSubestipulanteApolice, atualizarSubestipulanteApolice } from '../../api/apolices.api';
import { subestipulantesApi } from '../../../subestipulantes/api/subestipulantes.api';
import type { SubestipulanteListItem } from '../../../subestipulantes/types/subestipulante.types';
import type { ApoliceSubestipulanteResult } from '../../types/apolice.types';

interface SubestipulanteApoliceModalProps {
  aberto: boolean;
  onClose: () => void;
  apolicePublicId: string;
  subestipulanteEdicao?: ApoliceSubestipulanteResult;
  onSucesso: () => void;
}

export const SubestipulanteApoliceModal: React.FC<SubestipulanteApoliceModalProps> = ({
  aberto,
  onClose,
  apolicePublicId,
  subestipulanteEdicao,
  onSucesso,
}) => {
  const [submitting, setSubmitting] = useState(false);
  const [errorFeedback, setErrorFeedback] = useState<string | null>(null);
  
  const [catalogoSubestipulantes, setCatalogoSubestipulantes] = useState<SubestipulanteListItem[]>([]);
  const [carregandoCatalogo, setCarregandoCatalogo] = useState(false);
  
  const isEdicao = !!subestipulanteEdicao;

  const {
    register,
    handleSubmit,
    control,
    reset,
    formState: { errors },
  } = useForm<SubestipulanteApoliceFormValues>({
    resolver: zodResolver(subestipulanteApoliceSchema),
    defaultValues: {
      subestipulantePublicId: '',
      dataInicio: '',
      dataFim: '',
    },
  });

  useEffect(() => {
    if (aberto) {
      if (isEdicao) {
        reset({
          subestipulantePublicId: subestipulanteEdicao.subestipulantePublicId,
          dataInicio: subestipulanteEdicao.dataInicio || '',
          dataFim: subestipulanteEdicao.dataFim || '',
        });
      } else {
        reset({
          subestipulantePublicId: '',
          dataInicio: '',
          dataFim: '',
        });
        carregarCatalogo();
      }
      setErrorFeedback(null);
    }
  }, [aberto, isEdicao, subestipulanteEdicao, reset]);

  const carregarCatalogo = async () => {
    try {
      setCarregandoCatalogo(true);
      const res = await subestipulantesApi.listar({ ativo: true, tamanhoPagina: 100 });
      setCatalogoSubestipulantes(res.itens);
    } catch (error) {
      console.error('Erro ao carregar catálogo de subestipulantes:', error);
    } finally {
      setCarregandoCatalogo(false);
    }
  };

  const onSubmit = async (data: SubestipulanteApoliceFormValues) => {
    try {
      setSubmitting(true);
      setErrorFeedback(null);

      const payload = {
        dataInicio: data.dataInicio || undefined,
        dataFim: data.dataFim || undefined,
      };

      if (isEdicao) {
        await atualizarSubestipulanteApolice(apolicePublicId, data.subestipulantePublicId, payload);
      } else {
        await vincularSubestipulanteApolice(apolicePublicId, {
          subestipulantePublicId: data.subestipulantePublicId,
          ...payload
        });
      }

      onSucesso();
      onClose();
    } catch (error: any) {
      setErrorFeedback(error.response?.data?.message || 'Ocorreu um erro ao salvar o vínculo.');
    } finally {
      setSubmitting(false);
    }
  };

  const footer = (
    <>
      <Button type="button" variant="ghost" onClick={onClose} disabled={submitting}>
        Cancelar
      </Button>
      <Button type="submit" form="subestipulante-form" variant="primary" loading={submitting}>
        Salvar
      </Button>
    </>
  );

  return (
    <Modal
      aberto={aberto}
      onClose={onClose}
      size="medium"
      title={isEdicao ? 'Editar Vínculo' : 'Adicionar Subestipulante'}
      footer={footer}
    >
      <form id="subestipulante-form" onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-4">
        {errorFeedback && (
          <Alert variant="error" title="Atenção">
            {errorFeedback}
          </Alert>
        )}

        <div className="flex flex-col gap-1">
          <label htmlFor="subestipulantePublicId" className="form-label font-medium text-texto-principal">
            Subestipulante <span className="text-red-500">*</span>
          </label>
          
          {isEdicao ? (
            <div className="p-3 bg-gray-50 border border-gray-200 rounded-md">
              <p className="font-medium text-gray-700">{subestipulanteEdicao.nome}</p>
              <p className="text-sm text-gray-500">{subestipulanteEdicao.documento || subestipulanteEdicao.codigo}</p>
            </div>
          ) : (
            <Controller
              name="subestipulantePublicId"
              control={control}
              render={({ field }) => (
                <Select
                  {...field}
                  id="subestipulantePublicId"
                  error={!!errors.subestipulantePublicId}
                  disabled={carregandoCatalogo}
                  placeholder={carregandoCatalogo ? "Carregando..." : "Selecione um subestipulante..."}
                >
                  <option value="" disabled>Selecione um subestipulante...</option>
                  {catalogoSubestipulantes.map(sub => (
                    <option key={sub.publicId} value={sub.publicId}>
                      {sub.nome} {sub.cnpj ? `(${sub.cnpj})` : ''}
                    </option>
                  ))}
                </Select>
              )}
            />
          )}
          {errors.subestipulantePublicId && (
            <p className="form-error">{errors.subestipulantePublicId.message}</p>
          )}
        </div>

        <div className="grid grid-cols-2 gap-4">
          <div className="flex flex-col gap-1">
            <label htmlFor="dataInicio" className="form-label font-medium text-texto-principal">
              Data de Início
            </label>
            <Input
              id="dataInicio"
              type="date"
              {...register('dataInicio')}
              error={!!errors.dataInicio}
            />
            {errors.dataInicio && (
              <p className="form-error">{errors.dataInicio.message}</p>
            )}
          </div>
          <div className="flex flex-col gap-1">
            <label htmlFor="dataFim" className="form-label font-medium text-texto-principal">
              Data de Fim
            </label>
            <Input
              id="dataFim"
              type="date"
              {...register('dataFim')}
              error={!!errors.dataFim}
            />
            {errors.dataFim && (
              <p className="form-error">{errors.dataFim.message}</p>
            )}
          </div>
        </div>
      </form>
    </Modal>
  );
};
