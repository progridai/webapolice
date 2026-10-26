import React, {
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
} from 'react';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import {
  Modal,
  Button,
  Input,
  Select,
  Alert,
  FormField,
  ReadOnlyField,
} from '../../../../components/ui';
import {
  subestipulanteApoliceSchema,
  type SubestipulanteApoliceFormValues,
} from '../../schemas/subestipulanteApolice.schema';
import {
  vincularSubestipulanteApolice,
  atualizarSubestipulanteApolice,
} from '../../api/apolices.api';
import { subestipulantesApi } from '../../../subestipulantes/api/subestipulantes.api';
import type { ApoliceSubestipulanteResult } from '../../types/apolice.types';
import { useConsultaApolice } from '../../hooks/useConsultaApolice';

interface SubestipulanteApoliceModalProps {
  aberto: boolean;
  onClose: () => void;
  apolicePublicId: string;
  subestipulanteEdicao?: ApoliceSubestipulanteResult;
  onSucesso: () => void;
}

export const SubestipulanteApoliceModal: React.FC<
  SubestipulanteApoliceModalProps
> = ({ aberto, ...props }) => {
  if (!aberto) return null;
  return (
    <SubestipulanteApoliceForm
      key={`${props.apolicePublicId}/${props.subestipulanteEdicao?.subestipulantePublicId ?? 'novo'}`}
      {...props}
    />
  );
};

// Cada abertura/contexto possui seu formulário e suas operações pendentes.
const SubestipulanteApoliceForm: React.FC<
  Omit<SubestipulanteApoliceModalProps, 'aberto'>
> = ({ onClose, apolicePublicId, subestipulanteEdicao, onSucesso }) => {
  const [submitting, setSubmitting] = useState(false);
  const [errorFeedback, setErrorFeedback] = useState<string | null>(null);
  const active = useRef(false);
  const pending = useRef(false);
  useEffect(() => {
    active.current = true;
    return () => {
      active.current = false;
    };
  }, []);
  const isEdicao = !!subestipulanteEdicao;
  const load = useMemo(
    () =>
      isEdicao
        ? null
        : (signal: AbortSignal) =>
            subestipulantesApi.listar(
              { ativo: true, tamanhoPagina: 100 },
              signal,
            ),
    [isEdicao],
  );
  const catalogo = useConsultaApolice(
    load,
    'Erro ao carregar Subestipulantes. Tente novamente.',
  );
  const catalogoIndisponivel =
    !isEdicao &&
    (catalogo.isLoading || !!catalogo.error || !catalogo.data?.itens.length);
  const handleClose = useCallback(() => {
    if (!pending.current) onClose();
  }, [onClose]);

  const {
    register,
    handleSubmit,
    control,
    formState: { errors },
  } = useForm<SubestipulanteApoliceFormValues>({
    resolver: zodResolver(subestipulanteApoliceSchema),
    defaultValues: {
      subestipulantePublicId:
        subestipulanteEdicao?.subestipulantePublicId || '',
      dataInicio: subestipulanteEdicao?.dataInicio || '',
      dataFim: subestipulanteEdicao?.dataFim || '',
    },
  });

  const onSubmit = async (data: SubestipulanteApoliceFormValues) => {
    if (!active.current || pending.current || catalogoIndisponivel) return;
    pending.current = true;
    setSubmitting(true);
    setErrorFeedback(null);
    try {
      const payload = {
        dataInicio: data.dataInicio || undefined,
        dataFim: data.dataFim || undefined,
      };
      if (isEdicao) {
        await atualizarSubestipulanteApolice(
          apolicePublicId,
          data.subestipulantePublicId,
          payload,
        );
      } else {
        await vincularSubestipulanteApolice(apolicePublicId, {
          subestipulantePublicId: data.subestipulantePublicId,
          ...payload,
        });
      }
      if (active.current) {
        onSucesso();
        onClose();
      }
    } catch (error: unknown) {
      if (active.current)
        setErrorFeedback(
          error instanceof Error && error.message
            ? error.message
            : 'Ocorreu um erro ao salvar o vínculo.',
        );
    } finally {
      pending.current = false;
      if (active.current) setSubmitting(false);
    }
  };

  return (
    <Modal
      aberto
      onClose={handleClose}
      size="medium"
      title={isEdicao ? 'Editar Vínculo' : 'Adicionar Subestipulante'}
      footer={
        <>
          <Button
            type="button"
            variant="text"
            onClick={handleClose}
            disabled={submitting}
          >
            Cancelar
          </Button>
          <Button
            type="submit"
            form="subestipulante-form"
            variant="primary"
            loading={submitting}
            disabled={catalogoIndisponivel}
          >
            Salvar
          </Button>
        </>
      }
    >
      <form
        id="subestipulante-form"
        onSubmit={(event) => {
          void handleSubmit(onSubmit)(event);
        }}
        className="flex flex-col gap-4"
      >
        {errorFeedback && (
          <Alert variant="error" title="Atenção">
            {errorFeedback}
          </Alert>
        )}
        {!isEdicao && catalogo.error && (
          <Alert variant="error" title="Erro ao carregar catálogo">
            Erro ao carregar Subestipulantes. Tente novamente.
            <Button
              type="button"
              variant="secondary"
              size="small"
              onClick={() => void catalogo.refetch()}
            >
              Tentar novamente
            </Button>
          </Alert>
        )}
        {!isEdicao &&
          !catalogo.isLoading &&
          !catalogo.error &&
          catalogo.data?.itens.length === 0 && (
            <p role="status" className="text-sm text-texto-secundario">
              Nenhum subestipulante ativo disponível.
            </p>
          )}
        {subestipulanteEdicao ? (
          <div>
            <FormField label="Subestipulante">
              <ReadOnlyField value={subestipulanteEdicao.nome} />
            </FormField>
            <p className="text-sm text-texto-secundario">
              {subestipulanteEdicao.documento || subestipulanteEdicao.codigo}
            </p>
          </div>
        ) : (
          <Controller
            name="subestipulantePublicId"
            control={control}
            render={({ field }) => (
              <FormField
                label="Subestipulante"
                required
                error={errors.subestipulantePublicId?.message}
              >
                <Select
                  {...field}
                  id="subestipulantePublicId"
                  disabled={
                    catalogo.isLoading || submitting || !!catalogo.error
                  }
                  placeholder={
                    catalogo.isLoading
                      ? 'Carregando...'
                      : 'Selecione um subestipulante...'
                  }
                >
                  <option value="" disabled>
                    Selecione um subestipulante...
                  </option>
                  {catalogo.data?.itens.map((sub) => (
                    <option key={sub.publicId} value={sub.publicId}>
                      {sub.nome} {sub.cnpj ? `(${sub.cnpj})` : ''}
                    </option>
                  ))}
                </Select>
              </FormField>
            )}
          />
        )}
        <div className="grid grid-cols-2 gap-4">
          <FormField label="Data de Início" error={errors.dataInicio?.message}>
            <Input
              id="dataInicio"
              type="date"
              {...register('dataInicio')}
              disabled={submitting}
            />
          </FormField>
          <FormField label="Data de Fim" error={errors.dataFim?.message}>
            <Input
              id="dataFim"
              type="date"
              {...register('dataFim')}
              disabled={submitting}
            />
          </FormField>
        </div>
      </form>
    </Modal>
  );
};
