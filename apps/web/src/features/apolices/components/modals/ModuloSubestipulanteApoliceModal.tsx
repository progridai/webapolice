import React, { useEffect, useState } from 'react';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Modal, Button, Input, Select, Alert } from '../../../../components/ui';
import { moduloSubestipulanteApoliceSchema, type ModuloSubestipulanteApoliceFormValues } from '../../schemas/moduloSubestipulanteApolice.schema';
import { vincularModuloSubestipulanteApolice, atualizarModuloSubestipulanteApolice } from '../../api/apolices.api';
import { modulosGlobaisApi, type ModuloGlobalListItem } from '../../api/modulosGlobais.api';
import type { ApoliceSubestipulanteModuloResult } from '../../types/apolice.types';

interface ModuloSubestipulanteApoliceModalProps {
  aberto: boolean;
  onClose: () => void;
  apolicePublicId: string;
  subestipulantePublicId: string;
  moduloEdicao?: ApoliceSubestipulanteModuloResult;
  onSucesso: () => void;
}

export const ModuloSubestipulanteApoliceModal: React.FC<ModuloSubestipulanteApoliceModalProps> = ({
  aberto,
  onClose,
  apolicePublicId,
  subestipulantePublicId,
  moduloEdicao,
  onSucesso,
}) => {
  const [submitting, setSubmitting] = useState(false);
  const [errorFeedback, setErrorFeedback] = useState<string | null>(null);
  
  const [catalogoModulos, setCatalogoModulos] = useState<ModuloGlobalListItem[]>([]);
  const [carregandoCatalogo, setCarregandoCatalogo] = useState(false);
  
  const isEdicao = !!moduloEdicao;

  const {
    register,
    handleSubmit,
    control,
    reset,
    formState: { errors },
  } = useForm<ModuloSubestipulanteApoliceFormValues>({
    resolver: zodResolver(moduloSubestipulanteApoliceSchema),
    defaultValues: {
      moduloPublicId: '',
      dataInicio: '',
      dataFim: '',
    },
  });

  useEffect(() => {
    if (aberto) {
      if (isEdicao) {
        reset({
          moduloPublicId: moduloEdicao.moduloPublicId,
          dataInicio: moduloEdicao.dataInicio || '',
          dataFim: moduloEdicao.dataFim || '',
        });
      } else {
        reset({
          moduloPublicId: '',
          dataInicio: '',
          dataFim: '',
        });
        carregarCatalogo();
      }
      setErrorFeedback(null);
    }
  }, [aberto, isEdicao, moduloEdicao, reset]);

  const carregarCatalogo = async () => {
    try {
      setCarregandoCatalogo(true);
      const res = await modulosGlobaisApi.listar({ ativo: true, tamanhoPagina: 100 });
      setCatalogoModulos(res.items || []);
    } catch (error) {
      console.error('Erro ao carregar catálogo de módulos globais:', error);
    } finally {
      setCarregandoCatalogo(false);
    }
  };

  const onSubmit = async (data: ModuloSubestipulanteApoliceFormValues) => {
    try {
      setSubmitting(true);
      setErrorFeedback(null);

      const payload = {
        dataInicio: data.dataInicio || undefined,
        dataFim: data.dataFim || undefined,
      };

      if (isEdicao) {
        await atualizarModuloSubestipulanteApolice(
          apolicePublicId,
          subestipulantePublicId,
          data.moduloPublicId,
          payload
        );
      } else {
        await vincularModuloSubestipulanteApolice(apolicePublicId, subestipulantePublicId, {
          moduloPublicId: data.moduloPublicId,
          ...payload
        });
      }

      onSucesso();
      onClose();
    } catch (error: any) {
      setErrorFeedback(error.message || 'Ocorreu um erro ao salvar o módulo.');
    } finally {
      setSubmitting(false);
    }
  };

  const footer = (
    <>
      <Button type="button" variant="ghost" onClick={onClose} disabled={submitting}>
        Cancelar
      </Button>
      <Button type="submit" form="modulo-subestipulante-form" variant="primary" loading={submitting}>
        Salvar
      </Button>
    </>
  );

  return (
    <Modal
      aberto={aberto}
      onClose={onClose}
      size="medium"
      title={isEdicao ? 'Editar Vigência do Módulo' : 'Adicionar Módulo'}
      footer={footer}
    >
      <form id="modulo-subestipulante-form" onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-4">
        {errorFeedback && (
          <Alert variant="error" title="Atenção">
            {errorFeedback}
          </Alert>
        )}

        <div className="flex flex-col gap-1">
          <label htmlFor="moduloPublicId" className="form-label font-medium text-texto-principal">
            Módulo <span className="text-red-500">*</span>
          </label>
          
          {isEdicao ? (
            <div className="p-3 bg-gray-50 border border-gray-200 rounded-md flex flex-col">
              <span className="font-medium text-gray-700">{moduloEdicao.moduloNome}</span>
              {moduloEdicao.moduloDescricao && (
                <span className="text-sm text-gray-500">{moduloEdicao.moduloDescricao}</span>
              )}
            </div>
          ) : (
            <Controller
              name="moduloPublicId"
              control={control}
              render={({ field }) => (
                <Select
                  {...field}
                  id="moduloPublicId"
                  error={!!errors.moduloPublicId}
                  disabled={carregandoCatalogo}
                  placeholder={carregandoCatalogo ? "Carregando..." : "Selecione um módulo..."}
                >
                  <option value="" disabled>Selecione um módulo...</option>
                  {catalogoModulos.map(mod => (
                    <option key={mod.publicId} value={mod.publicId}>
                      {mod.nome} {mod.descricao ? `— ${mod.descricao}` : ''}
                    </option>
                  ))}
                </Select>
              )}
            />
          )}
          {errors.moduloPublicId && (
            <p className="form-error">{errors.moduloPublicId.message}</p>
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
