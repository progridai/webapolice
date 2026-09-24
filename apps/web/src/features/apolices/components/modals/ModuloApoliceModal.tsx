import React, { useEffect, useState } from 'react';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Modal, Button, Input, Select, Textarea, Alert } from '../../../../components/ui';
import {
  criarModuloApoliceSchema,
  alterarModuloApoliceSchema,
  type CriarModuloApoliceFormValues,
  type AlterarModuloApoliceFormValues,
} from '../../schemas/moduloApolice.schema';
import { criarApoliceModulo, alterarApoliceModulo } from '../../api/apolices.api';
import { modulosGlobaisApi, type ModuloGlobalListItem } from '../../api/modulosGlobais.api';
import type { ApoliceModuloResult } from '../../types/apolice.types';

interface ModuloApoliceModalProps {
  aberto: boolean;
  onClose: () => void;
  apolicePublicId: string;
  /** Módulos já vinculados à Apólice (para filtrar do catálogo no modo criação) */
  modulosVinculados: ApoliceModuloResult[];
  /** Vínculo a ser editado; undefined = modo criação */
  moduloEdicao?: ApoliceModuloResult;
  onSucesso: () => void;
}

export const ModuloApoliceModal: React.FC<ModuloApoliceModalProps> = ({
  aberto,
  onClose,
  apolicePublicId,
  modulosVinculados,
  moduloEdicao,
  onSucesso,
}) => {
  const [submitting, setSubmitting] = useState(false);
  const [errorFeedback, setErrorFeedback] = useState<string | null>(null);

  const [catalogoModulos, setCatalogoModulos] = useState<ModuloGlobalListItem[]>([]);
  const [carregandoCatalogo, setCarregandoCatalogo] = useState(false);

  const isEdicao = !!moduloEdicao;

  // ── Formulário Criar ──────────────────────────────────────────────────────
  const criarForm = useForm<CriarModuloApoliceFormValues>({
    resolver: zodResolver(criarModuloApoliceSchema),
    defaultValues: { moduloPublicId: '', dataInicio: '', dataFim: '', observacao: '' },
  });

  // ── Formulário Editar ─────────────────────────────────────────────────────
  const editarForm = useForm<AlterarModuloApoliceFormValues>({
    resolver: zodResolver(alterarModuloApoliceSchema),
    defaultValues: { dataInicio: '', dataFim: '', observacao: '' },
  });

  // ── Inicialização ao abrir ────────────────────────────────────────────────
  useEffect(() => {
    if (!aberto) return;
    setErrorFeedback(null);

    if (isEdicao && moduloEdicao) {
      editarForm.reset({
        dataInicio: moduloEdicao.dataInicio
          ? moduloEdicao.dataInicio.substring(0, 10)
          : '',
        dataFim: moduloEdicao.dataFim
          ? moduloEdicao.dataFim.substring(0, 10)
          : '',
        observacao: moduloEdicao.observacao || '',
      });
    } else {
      criarForm.reset({ moduloPublicId: '', dataInicio: '', dataFim: '', observacao: '' });
      carregarCatalogo();
    }
  }, [aberto, isEdicao, moduloEdicao]);

  // ── Catálogo de Módulos Globais ───────────────────────────────────────────
  // Carrega apenas módulos ativos e filtra os que já possuem vínculo com a
  // Apólice (inclusive inativos), conforme regra de unicidade (apolice_id, modulo_id)
  // WHERE deleted_at IS NULL. O backend é a autoridade final desta regra.
  const carregarCatalogo = async () => {
    try {
      setCarregandoCatalogo(true);
      const res = await modulosGlobaisApi.listar({ ativo: true, tamanhoPagina: 200 });

      // IDs de módulos do cadastro global que já têm vínculo com esta apólice
      const idsJaVinculados = new Set(modulosVinculados.map((m) => m.moduloPublicId));

      const disponiveis = res.items.filter((m) => !idsJaVinculados.has(m.publicId));
      setCatalogoModulos(disponiveis);
    } catch (err) {
      console.error('Erro ao carregar catálogo de módulos:', err);
    } finally {
      setCarregandoCatalogo(false);
    }
  };

  // ── Submit Criar ──────────────────────────────────────────────────────────
  const onSubmitCriar = async (data: CriarModuloApoliceFormValues) => {
    try {
      setSubmitting(true);
      setErrorFeedback(null);
      // moduloPublicId = publicId do cadastro.modulo (NÃO é o apoliceModuloPublicId)
      await criarApoliceModulo(apolicePublicId, {
        moduloPublicId: data.moduloPublicId,
        dataInicio: data.dataInicio || null,
        dataFim: data.dataFim || null,
        observacao: data.observacao || null,
      });
      onSucesso();
      onClose();
    } catch (err: any) {
      setErrorFeedback(
        err.response?.data?.detail ||
          err.response?.data?.message ||
          err.message ||
          'Erro inesperado ao vincular o Módulo.'
      );
    } finally {
      setSubmitting(false);
    }
  };

  // ── Submit Editar ─────────────────────────────────────────────────────────
  const onSubmitEditar = async (data: AlterarModuloApoliceFormValues) => {
    if (!moduloEdicao) return;
    try {
      setSubmitting(true);
      setErrorFeedback(null);
      // Usa publicId do vínculo (apoliceModuloPublicId = seguro.apolice_modulo)
      // NÃO usa moduloEdicao.moduloPublicId aqui
      await alterarApoliceModulo(apolicePublicId, moduloEdicao.publicId, {
        dataInicio: data.dataInicio || null,
        dataFim: data.dataFim || null,
        observacao: data.observacao || null,
      });
      onSucesso();
      onClose();
    } catch (err: any) {
      setErrorFeedback(
        err.response?.data?.detail ||
          err.response?.data?.message ||
          err.message ||
          'Erro inesperado ao alterar o vínculo.'
      );
    } finally {
      setSubmitting(false);
    }
  };

  // ── Footer ────────────────────────────────────────────────────────────────
  const formId = isEdicao ? 'modulo-apolice-editar-form' : 'modulo-apolice-criar-form';

  const footer = (
    <>
      <Button type="button" variant="ghost" onClick={onClose} disabled={submitting}>
        Cancelar
      </Button>
      <Button type="submit" form={formId} variant="primary" loading={submitting}>
        Salvar
      </Button>
    </>
  );

  // ── Campos de datas e observação (compartilhados entre os dois modos) ─────
  const CamposCompartilhados = ({
    register,
    control,
    errors,
  }: {
    register: any;
    control: any;
    errors: any;
  }) => (
    <>
      <div className="grid grid-cols-2 gap-4">
        <div className="flex flex-col gap-1">
          <label htmlFor="dataInicio" className="form-label font-medium text-texto-principal">
            Início de Vigência
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
            Fim de Vigência
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
    </>
  );

  // ── Render ────────────────────────────────────────────────────────────────
  return (
    <Modal
      aberto={aberto}
      onClose={onClose}
      title={isEdicao ? 'Editar Vínculo de Módulo' : 'Vincular Módulo'}
      size="medium"
      footer={footer}
    >
      {/* ── Modo Criação ── */}
      {!isEdicao && (
        <form
          id="modulo-apolice-criar-form"
          onSubmit={criarForm.handleSubmit(onSubmitCriar)}
          className="flex flex-col gap-4"
        >
          {errorFeedback && (
            <Alert variant="error" title="Erro ao vincular">
              {errorFeedback}
            </Alert>
          )}

          <div className="flex flex-col gap-1">
            <label htmlFor="moduloPublicId" className="form-label font-medium text-texto-principal">
              Módulo <span className="text-red-500">*</span>
            </label>
            <Controller
              name="moduloPublicId"
              control={criarForm.control}
              render={({ field }) => (
                <Select
                  {...field}
                  id="moduloPublicId"
                  error={!!criarForm.formState.errors.moduloPublicId}
                  disabled={carregandoCatalogo}
                  placeholder={carregandoCatalogo ? 'Carregando...' : 'Selecione um módulo...'}
                >
                  <option value="" disabled>
                    Selecione um módulo...
                  </option>
                  {catalogoModulos.map((m) => (
                    <option key={m.publicId} value={m.publicId}>
                      {m.nome}
                      {m.descricao ? ` — ${m.descricao}` : ''}
                    </option>
                  ))}
                </Select>
              )}
            />
            {criarForm.formState.errors.moduloPublicId && (
              <p className="form-error">{criarForm.formState.errors.moduloPublicId.message}</p>
            )}
            {!carregandoCatalogo && catalogoModulos.length === 0 && (
              <p className="text-sm text-texto-terciario mt-1">
                Nenhum módulo disponível para vincular a esta apólice.
              </p>
            )}
          </div>

          <CamposCompartilhados
            register={criarForm.register}
            control={criarForm.control}
            errors={criarForm.formState.errors}
          />
        </form>
      )}

      {/* ── Modo Edição ── */}
      {isEdicao && moduloEdicao && (
        <form
          id="modulo-apolice-editar-form"
          onSubmit={editarForm.handleSubmit(onSubmitEditar)}
          className="flex flex-col gap-4"
        >
          {errorFeedback && (
            <Alert variant="error" title="Erro ao salvar">
              {errorFeedback}
            </Alert>
          )}

          {/* Módulo em modo somente leitura — não pode ser alterado na edição */}
          <div className="flex flex-col gap-1">
            <span className="form-label font-medium text-texto-principal">Módulo</span>
            <div className="p-3 bg-fundo-secundario border border-borda rounded-md">
              <p className="font-medium text-texto-principal">{moduloEdicao.nome}</p>
              {moduloEdicao.descricao && (
                <p className="text-sm text-texto-secundario mt-0.5">{moduloEdicao.descricao}</p>
              )}
            </div>
          </div>

          <CamposCompartilhados
            register={editarForm.register}
            control={editarForm.control}
            errors={editarForm.formState.errors}
          />
        </form>
      )}
    </Modal>
  );
};
