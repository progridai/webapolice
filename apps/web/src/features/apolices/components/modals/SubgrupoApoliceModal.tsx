import React, { useEffect, useState } from 'react';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Modal, Button, Input, Textarea, Alert } from '../../../../components/ui';
import { subgrupoApoliceSchema, type SubgrupoApoliceFormValues } from '../../schemas/subgrupoApolice.schema';
import { criarApoliceSubgrupo, alterarApoliceSubgrupo } from '../../api/apolices.api';
import type { ApoliceSubgrupoResult } from '../../types/apolice.types';

interface SubgrupoApoliceModalProps {
  aberto: boolean;
  onClose: () => void;
  apolicePublicId: string;
  subgrupoEdicao?: ApoliceSubgrupoResult;
  onSucesso: () => void;
}

export const SubgrupoApoliceModal: React.FC<SubgrupoApoliceModalProps> = ({
  aberto,
  onClose,
  apolicePublicId,
  subgrupoEdicao,
  onSucesso,
}) => {
  const [submitting, setSubmitting] = useState(false);
  const [errorFeedback, setErrorFeedback] = useState<string | null>(null);
  
  const isEdicao = !!subgrupoEdicao;

  const {
    register,
    handleSubmit,
    control,
    reset,
    formState: { errors },
  } = useForm<SubgrupoApoliceFormValues>({
    resolver: zodResolver(subgrupoApoliceSchema),
    defaultValues: {
      nome: '',
      observacao: '',
    },
  });

  useEffect(() => {
    if (aberto) {
      setErrorFeedback(null);
      if (isEdicao && subgrupoEdicao) {
        reset({
          nome: subgrupoEdicao.nome,
          observacao: subgrupoEdicao.observacao || '',
        });
      } else {
        reset({
          nome: '',
          observacao: '',
        });
      }
    }
  }, [aberto, isEdicao, subgrupoEdicao, reset]);

  const onSubmit = async (data: SubgrupoApoliceFormValues) => {
    try {
      setSubmitting(true);
      setErrorFeedback(null);

      if (isEdicao && subgrupoEdicao) {
        await alterarApoliceSubgrupo(apolicePublicId, subgrupoEdicao.subgrupoPublicId, data);
      } else {
        await criarApoliceSubgrupo(apolicePublicId, data);
      }

      onSucesso();
      onClose();
    } catch (err: any) {
      if (err.response?.data?.detail) {
        setErrorFeedback(err.response.data.detail);
      } else {
        setErrorFeedback(err.message || 'Erro inesperado ao salvar o Subgrupo da Apólice.');
      }
    } finally {
      setSubmitting(false);
    }
  };

  const footer = (
    <>
      <Button type="button" variant="ghost" onClick={onClose} disabled={submitting}>
        Cancelar
      </Button>
      <Button type="submit" form="subgrupo-form" variant="primary" disabled={submitting}>
        {submitting ? 'Salvando...' : 'Salvar'}
      </Button>
    </>
  );

  return (
    <Modal
      aberto={aberto}
      onClose={onClose}
      title={isEdicao ? 'Editar Subgrupo' : 'Adicionar Subgrupo'}
      size="medium"
      footer={footer}
    >
      <form id="subgrupo-form" onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-4">
        {errorFeedback && (
          <Alert variant="error" title="Erro ao salvar">
            {errorFeedback}
          </Alert>
        )}

        <div className="flex flex-col gap-4">
          <Input
            label="Nome"
            placeholder="Ex: Matriz, Filial Centro"
            error={errors.nome?.message}
            required
            {...register('nome')}
          />
          
          <Controller
            name="observacao"
            control={control}
            render={({ field }) => (
              <Textarea
                label="Observação"
                placeholder="Observações adicionais..."
                error={errors.observacao?.message}
                {...field}
                value={field.value || ''}
              />
            )}
          />
        </div>
      </form>
    </Modal>
  );
};
