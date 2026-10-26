import React, {
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
} from 'react';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import {
  Modal,
  FormField,
  Input,
  Button,
  Select,
  ReadOnlyField,
  Alert,
} from '../../../../components/ui';
import { ramosApi } from '../../../ramos/api/ramos.api';
import {
  vincularRamoApolice,
  atualizarRamoApolice,
} from '../../api/apolices.api';
import type { ApoliceRamoResult } from '../../types/apolice.types';
import { useConsultaApolice } from '../../hooks/useConsultaApolice';

const ramoFormSchema = z.object({
  ramoPublicId: z.string().min(1, 'Selecione um Ramo'),
  numeroApolice: z.string().optional(),
  iofPercentual: z.coerce
    .number<string | number>()
    .min(0)
    .max(100)
    .optional()
    .or(z.literal('')),
});

type ApoliceRamoFormInput = z.input<typeof ramoFormSchema>;
export type ApoliceRamoFormData = z.output<typeof ramoFormSchema>;

interface ApoliceRamoFormModalProps {
  aberto: boolean;
  onClose: () => void;
  apolicePublicId: string;
  ramoEdicao?: ApoliceRamoResult;
  onSucesso: () => void;
}

export const ApoliceRamoFormModal: React.FC<ApoliceRamoFormModalProps> = ({
  aberto,
  ...props
}) => {
  if (!aberto) return null;
  return (
    <ApoliceRamoForm
      key={`${props.apolicePublicId}/${props.ramoEdicao?.publicId ?? 'novo'}`}
      {...props}
    />
  );
};

const ApoliceRamoForm: React.FC<Omit<ApoliceRamoFormModalProps, 'aberto'>> = ({
  onClose,
  apolicePublicId,
  ramoEdicao,
  onSucesso,
}) => {
  const [salvando, setSalvando] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const active = useRef(false);
  const pending = useRef(false);
  useEffect(() => {
    active.current = true;
    return () => {
      active.current = false;
    };
  }, []);
  const isEdicao = !!ramoEdicao;
  const load = useMemo(
    () =>
      isEdicao
        ? null
        : (signal: AbortSignal) =>
            ramosApi.listar(
              { pagina: 1, tamanhoPagina: 100, ativo: true },
              signal,
            ),
    [isEdicao],
  );
  const catalogo = useConsultaApolice(
    load,
    'Erro ao carregar Ramos. Tente novamente.',
  );
  const ramosOptions =
    catalogo.data?.items.map((r) => ({
      value: r.publicId,
      label: `${r.codigo} — ${r.nome}`,
    })) || [];
  const catalogoIndisponivel =
    !isEdicao &&
    (catalogo.isLoading || !!catalogo.error || ramosOptions.length === 0);
  const handleClose = useCallback(() => {
    if (!pending.current) onClose();
  }, [onClose]);

  const {
    register,
    handleSubmit,
    control,
    formState: { errors },
  } = useForm<ApoliceRamoFormInput, unknown, ApoliceRamoFormData>({
    resolver: zodResolver(ramoFormSchema),
    defaultValues: {
      ramoPublicId: ramoEdicao?.publicId || '',
      numeroApolice: ramoEdicao?.numeroApolice || '',
      iofPercentual: ramoEdicao?.iofPercentual ?? '',
    },
  });

  const onSubmit = async (data: ApoliceRamoFormData) => {
    if (!active.current || pending.current || catalogoIndisponivel) return;
    pending.current = true;
    setSalvando(true);
    setError(null);
    try {
      if (isEdicao && ramoEdicao) {
        await atualizarRamoApolice(apolicePublicId, ramoEdicao.publicId, {
          numeroApolice: data.numeroApolice,
          iofPercentual:
            data.iofPercentual === '' ? undefined : Number(data.iofPercentual),
        });
      } else {
        await vincularRamoApolice(apolicePublicId, {
          ramoPublicId: data.ramoPublicId,
          numeroApolice: data.numeroApolice,
          iofPercentual:
            data.iofPercentual === '' ? undefined : Number(data.iofPercentual),
        });
      }
      if (active.current) {
        onSucesso();
        onClose();
      }
    } catch (error: unknown) {
      if (active.current)
        setError(
          error instanceof Error && error.message
            ? error.message
            : 'Erro inesperado.',
        );
    } finally {
      pending.current = false;
      if (active.current) setSalvando(false);
    }
  };

  return (
    <Modal
      aberto
      onClose={handleClose}
      title={isEdicao ? 'Editar Vínculo do Ramo' : 'Adicionar Ramo à Apólice'}
      size="medium"
    >
      <form
        onSubmit={(event) => {
          void handleSubmit(onSubmit)(event);
        }}
        className="flex flex-col gap-4 py-2"
      >
        {error && (
          <Alert
            variant="error"
            title="Falha ao salvar"
            onClose={() => setError(null)}
          >
            {error}
          </Alert>
        )}
        {!isEdicao && catalogo.error && (
          <Alert variant="error" title="Erro ao carregar catálogo">
            Erro ao carregar Ramos. Tente novamente.
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
          catalogo.data?.items.length === 0 && (
            <p role="status" className="text-sm text-texto-secundario">
              Nenhum ramo ativo disponível.
            </p>
          )}
        <div className="grid grid-cols-1 gap-4">
          {isEdicao && ramoEdicao ? (
            <FormField label="Ramo">
              <ReadOnlyField
                value={`${ramoEdicao.ramoCodigo} — ${ramoEdicao.ramoNome}`}
              />
            </FormField>
          ) : (
            <Controller
              name="ramoPublicId"
              control={control}
              render={({ field }) => (
                <FormField
                  label="Ramo"
                  required
                  error={errors.ramoPublicId?.message}
                >
                  <Select
                    {...field}
                    options={ramosOptions}
                    placeholder={
                      catalogo.isLoading ? 'Carregando...' : 'Selecione o Ramo'
                    }
                    disabled={
                      catalogo.isLoading || salvando || !!catalogo.error
                    }
                  />
                </FormField>
              )}
            />
          )}

          <FormField
            label="Número da Apólice no Ramo"
            error={errors.numeroApolice?.message}
          >
            <Input
              {...register('numeroApolice')}
              placeholder="Ex: 0001"
              disabled={salvando}
            />
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
          <Button
            type="button"
            variant="text"
            onClick={handleClose}
            disabled={salvando}
          >
            Cancelar
          </Button>
          <Button
            type="submit"
            variant="primary"
            disabled={salvando || catalogoIndisponivel}
            loading={salvando}
          >
            {isEdicao ? 'Salvar' : 'Adicionar'}
          </Button>
        </div>
      </form>
    </Modal>
  );
};
