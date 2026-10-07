import { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import {
  Modal,
  Button,
  Input,
  Textarea,
  Alert,
  FormField,
  Select,
} from '../../../../components/ui';
import {
  subgrupoApoliceSchema,
  type SubgrupoApoliceFormValues,
} from '../../schemas/subgrupoApolice.schema';
import {
  criarApoliceSubgrupo,
  alterarApoliceSubgrupo,
} from '../../api/apolices.api';
import type { ApoliceSubgrupoResult } from '../../types/apolice.types';
import {
  catalogosApi,
  type ConvenioOpcao,
} from '../../../cadastros-seguro/api/catalogos.api';
interface SubgrupoApoliceModalProps {
  aberto: boolean;
  onClose: () => void;
  apolicePublicId: string;
  subgrupoEdicao?: ApoliceSubgrupoResult;
  onSucesso: () => void;
}
export function SubgrupoApoliceModal({
  aberto,
  onClose,
  apolicePublicId,
  subgrupoEdicao,
  onSucesso,
}: SubgrupoApoliceModalProps) {
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState('');
  const [opcoes, setOpcoes] = useState<ConvenioOpcao[]>([]);
  const [loading, setLoading] = useState(true);
  const [falhaOpcoes, setFalhaOpcoes] = useState(false);
  const {
    register,
    handleSubmit,
    setError: setFieldError,
    formState: { errors },
  } = useForm<SubgrupoApoliceFormValues>({
    resolver: zodResolver(subgrupoApoliceSchema),
    defaultValues: {
      nome: subgrupoEdicao?.nome ?? '',
      observacao: subgrupoEdicao?.observacao ?? '',
      convenioCobrancaPublicId: subgrupoEdicao?.convenioCobrancaPublicId ?? '',
    },
  });
  useEffect(() => {
    if (!aberto) return;
    const c = new AbortController();
    catalogosApi
      .conveniosSubgrupo(c.signal)
      .then((rows) => {
        if (!c.signal.aborted) setOpcoes(rows);
      })
      .catch((err) => {
        if (!c.signal.aborted) {
          setFalhaOpcoes(true);
          setError(
            err instanceof Error
              ? err.message
              : 'Não foi possível carregar os Convênios.',
          );
        }
      })
      .finally(() => {
        if (!c.signal.aborted) setLoading(false);
      });
    return () => c.abort();
  }, [aberto]);
  const onSubmit = async (data: SubgrupoApoliceFormValues) => {
    if (
      (!subgrupoEdicao || subgrupoEdicao.convenioCobrancaPublicId) &&
      !data.convenioCobrancaPublicId
    ) {
      setFieldError('convenioCobrancaPublicId', {
        message: 'Selecione um Convênio de Cobrança.',
      });
      return;
    }
    setSubmitting(true);
    setError('');
    try {
      const body = {
        ...data,
        convenioCobrancaPublicId: data.convenioCobrancaPublicId || null,
      };
      if (subgrupoEdicao)
        await alterarApoliceSubgrupo(
          apolicePublicId,
          subgrupoEdicao.subgrupoPublicId,
          body,
        );
      else await criarApoliceSubgrupo(apolicePublicId, body);
      onSucesso();
      onClose();
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : 'Não foi possível salvar o Subgrupo.',
      );
    } finally {
      setSubmitting(false);
    }
  };
  const atuais =
    subgrupoEdicao?.convenioCobrancaPublicId &&
    !opcoes.some((o) => o.publicId === subgrupoEdicao.convenioCobrancaPublicId)
      ? [
          {
            publicId: subgrupoEdicao.convenioCobrancaPublicId,
            nome: `${subgrupoEdicao.convenioCobrancaNome ?? 'Convênio atual'} (inativo)`,
            ativo: false,
          },
        ]
      : [];
  return (
    <Modal
      aberto={aberto}
      onClose={() => !submitting && onClose()}
      title={subgrupoEdicao ? 'Editar Subgrupo' : 'Adicionar Subgrupo'}
      size="medium"
      footer={
        <>
          <Button variant="secondary" onClick={onClose} disabled={submitting}>
            Cancelar
          </Button>
          <Button
            type="submit"
            form="subgrupo-form"
            loading={submitting}
            disabled={loading || falhaOpcoes}
          >
            Salvar
          </Button>
        </>
      }
    >
      <form
        id="subgrupo-form"
        onSubmit={handleSubmit(onSubmit)}
        className="flex flex-col gap-3"
      >
        {error && (
          <Alert variant="error" title="Não foi possível concluir">
            {error}
          </Alert>
        )}
        <FormField label="Nome" required error={errors.nome?.message}>
          <Input
            required
            maxLength={200}
            {...register('nome')}
            disabled={submitting}
          />
        </FormField>
        <FormField
          label="Convênio de Cobrança"
          required={
            !subgrupoEdicao || !!subgrupoEdicao.convenioCobrancaPublicId
          }
          error={errors.convenioCobrancaPublicId?.message}
          hint={
            subgrupoEdicao && !subgrupoEdicao.convenioCobrancaPublicId
              ? 'Selecione o Convênio quando estiver disponível.'
              : undefined
          }
        >
          <Select
            {...register('convenioCobrancaPublicId')}
            disabled={loading || submitting}
            options={[
              {
                value: '',
                label: loading ? 'Carregando...' : 'Selecione um Convênio',
              },
              ...[...atuais, ...opcoes].map((o) => ({
                value: o.publicId,
                label: o.nome,
              })),
            ]}
          />
        </FormField>
        <FormField label="Observação" error={errors.observacao?.message}>
          <Textarea {...register('observacao')} disabled={submitting} />
        </FormField>
      </form>
    </Modal>
  );
}
