import React, { useState, useMemo } from 'react';
import { useApoliceVidas } from '../../hooks/useApoliceVidas';
import {
  DataTable,
  StatusBadge,
  Pagination,
  Alert,
  Button,
  EmptyState,
  ConfirmDialog,
  Modal,
} from '../../../../components/ui';
import type { Column } from '../../../../components/ui/DataTable/DataTable';
import type {
  ApoliceVidaListItem,
  ApoliceVidaQuery,
} from '../../types/apolice.types';
import { useAuthorization } from '../../../../auth/AuthorizationProvider';
import { PropostasConsulta } from '../../../propostas/components/PropostasConsulta';
import { CriarPropostaDialog } from '../../../propostas/components/CriarPropostaDialog';
import { ApoliceVidaFormModal } from './ApoliceVidaFormModal';
import {
  inativarApoliceVida,
  excluirApoliceVida,
  criarApoliceVida,
  atualizarApoliceVida,
} from '../../api/apolices.api';

interface VidasTabProps {
  publicId: string;
  estipulanteNome?: string;
  apoliceAtiva?: boolean;
}

export const VidasTab: React.FC<VidasTabProps> = ({
  publicId,
  estipulanteNome,
  apoliceAtiva = true,
}) => {
  const [query, setQuery] = useState<ApoliceVidaQuery>({
    page: 1,
    pageSize: 10,
  });
  const { data, isLoading, error, retry } = useApoliceVidas(publicId, query);
  const { possuiPermissao, possuiRecurso, possuiModulo } = useAuthorization();

  const podeInserir = possuiPermissao('apolices.vidas.inserir');
  const podeAlterar = possuiPermissao('apolices.vidas.alterar');
  const podeInativar = possuiPermissao('apolices.vidas.inativar');
  const podeExcluir = possuiPermissao('apolices.vidas.excluir');
  const propostasDisponiveis =
    possuiRecurso('PROPOSTAS') && possuiModulo('APOLICES');
  const podeConsultarPropostas =
    propostasDisponiveis && possuiPermissao('propostas.visualizar');
  const podeCriarPropostas =
    propostasDisponiveis &&
    apoliceAtiva &&
    possuiPermissao('propostas.inserir');
  const [vidaCriacao, setVidaCriacao] = useState<ApoliceVidaListItem | null>(
    null,
  );
  const [propostaCriada, setPropostaCriada] = useState<string | null>(null);
  const [criandoVersao, setCriandoVersao] = useState(false);
  const [vidaPropostas, setVidaPropostas] =
    useState<ApoliceVidaListItem | null>(null);

  const [submitError, setSubmitError] = useState<string | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [selectedVida, setSelectedVida] = useState<
    ApoliceVidaListItem | undefined
  >(undefined);

  const [confirmDialog, setConfirmDialog] = useState<{
    isOpen: boolean;
    vidaId?: string;
    isLoading: boolean;
    acao: 'encerrar' | 'excluir';
    clienteNome?: string;
    erro?: string;
  }>({
    isOpen: false,
    isLoading: false,
    acao: 'encerrar',
  });

  const handlePageChange = (newPage: number) =>
    setQuery((prev) => ({ ...prev, page: newPage }));

  const handleAdd = () => {
    setSubmitError(null);
    setSelectedVida(undefined);
    setIsModalOpen(true);
  };

  const handleEdit = (vida: ApoliceVidaListItem) => {
    setSubmitError(null);
    setSelectedVida(vida);
    setIsModalOpen(true);
  };

  const handleEncerrarClick = (vidaId: string) => {
    setConfirmDialog({
      isOpen: true,
      vidaId,
      isLoading: false,
      acao: 'encerrar',
    });
  };

  const confirmarAcao = async () => {
    if (!confirmDialog.vidaId || confirmDialog.isLoading) return;
    setConfirmDialog((prev) => ({ ...prev, isLoading: true, erro: undefined }));
    try {
      if (confirmDialog.acao === 'excluir') {
        await excluirApoliceVida(publicId, confirmDialog.vidaId);
      } else {
        await inativarApoliceVida(publicId, confirmDialog.vidaId);
      }
      setConfirmDialog({ isOpen: false, isLoading: false, acao: 'encerrar' });
      if (
        confirmDialog.acao === 'excluir' &&
        data?.items.length === 1 &&
        (query.page ?? 1) > 1
      ) {
        setQuery((prev) => ({ ...prev, page: (prev.page ?? 1) - 1 }));
      } else {
        void retry();
      }
    } catch (err) {
      setConfirmDialog((prev) => ({
        ...prev,
        isLoading: false,
        erro:
          err instanceof Error
            ? err.message
            : 'Não foi possível concluir a operação.',
      }));
    }
  };

  const handleFormSubmit = async (
    formData: import('../../schemas/apoliceVida.schema').ApoliceVidaFormValues,
  ) => {
    setIsSubmitting(true);
    setSubmitError(null);
    try {
      if (selectedVida) {
        // Edit mode (enviar apenas o permitido no payload)
        await atualizarApoliceVida(publicId, selectedVida.apoliceVidaPublicId, {
          clienteVinculoPublicId: formData.clienteVinculoPublicId || null,
          completarVinculoSemEstipulante:
            formData.completarVinculoSemEstipulante ?? false,
          novaMatricula: formData.novaMatricula ?? null,
          dataInicioVigencia: formData.dataInicioVigencia || null,
          dataFimVigencia: formData.dataFimVigencia || null,
          observacao: formData.observacao || null,
          apoliceSubgrupoPublicId: formData.apoliceSubgrupoPublicId || null,
          apoliceModuloPublicId: formData.apoliceModuloPublicId || null,
        });
      } else {
        // Create mode
        await criarApoliceVida(publicId, {
          clientePublicId: formData.clientePublicId,
          clienteVinculoPublicId: formData.clienteVinculoPublicId || null,
          completarVinculoSemEstipulante:
            formData.completarVinculoSemEstipulante ?? false,
          novaMatricula: formData.novaMatricula ?? null,
          apoliceSubgrupoPublicId: formData.apoliceSubgrupoPublicId || null,
          apoliceModuloPublicId: formData.apoliceModuloPublicId || null,
          dataInicioVigencia: formData.dataInicioVigencia || null,
          dataFimVigencia: formData.dataFimVigencia || null,
          observacao: formData.observacao || null,
        });
      }
      setIsModalOpen(false);
      retry();
    } catch (err) {
      setSubmitError(
        err instanceof Error
          ? err.message
          : 'Erro ao salvar a participação de vida.',
      );
    } finally {
      setIsSubmitting(false);
    }
  };

  const columns = useMemo<Column<ApoliceVidaListItem>[]>(() => {
    const cols: Column<ApoliceVidaListItem>[] = [
      {
        key: 'cliente',
        label: 'Cliente',
        render: (vida) => (
          <div className="flex flex-col">
            <span className="font-medium text-texto-principal">
              {vida.clienteNome}
            </span>
            {vida.clienteDocumentoMascarado && (
              <span className="text-xs text-texto-terciario">
                {vida.clienteDocumentoMascarado}
              </span>
            )}
          </div>
        ),
      },
      {
        key: 'matricula',
        label: 'Matrícula',
        render: (vida) => vida.vinculoMatricula || 'Não informada',
      },
      {
        key: 'subgrupo',
        label: 'Subgrupo',
        render: (vida) => (
          <span className="text-sm text-texto-principal">
            {vida.subgrupoNome || '—'}
          </span>
        ),
      },
      {
        key: 'modulo',
        label: 'Módulo',
        render: (vida) => (
          <span className="text-sm text-texto-principal">
            {vida.moduloNome || '—'}
          </span>
        ),
      },
      {
        key: 'vigencia',
        label: 'Vigência',
        render: (vida) => (
          <span className="text-texto-secundario text-sm">
            {vida.dataInicioVigencia
              ? new Date(vida.dataInicioVigencia).toLocaleDateString('pt-BR')
              : '—'}
            {' até '}
            {vida.dataFimVigencia
              ? new Date(vida.dataFimVigencia).toLocaleDateString('pt-BR')
              : '—'}
          </span>
        ),
      },
      {
        key: 'status',
        label: 'Status',
        render: (vida) => (
          <StatusBadge status={vida.ativo ? 'ativo' : 'inativo'} />
        ),
      },
    ];

    if (
      podeAlterar ||
      podeInativar ||
      podeExcluir ||
      podeConsultarPropostas ||
      podeCriarPropostas
    ) {
      cols.push({
        key: 'acoes',
        label: '',
        render: (vida) => (
          <div className="flex items-center justify-end gap-2">
            {podeCriarPropostas && vida.ativo && !vida.possuiPropostas && (
              <Button
                variant="text"
                size="small"
                disabled={!vida.clienteVinculoPublicId}
                title={
                  !vida.clienteVinculoPublicId
                    ? 'Informe uma matrícula compatível na Vida antes de criar a proposta.'
                    : undefined
                }
                onClick={() => setVidaCriacao(vida)}
              >
                Criar proposta
              </Button>
            )}
            {podeConsultarPropostas && (
              <Button
                variant="text"
                size="small"
                onClick={() => {
                  setPropostaCriada(null);
                  setVidaPropostas(vida);
                }}
              >
                Consultar propostas
              </Button>
            )}
            {podeAlterar && (
              <Button
                variant="text"
                size="small"
                onClick={() => handleEdit(vida)}
              >
                Editar
              </Button>
            )}
            {podeInativar && vida.ativo && (
              <Button
                variant="text"
                size="small"
                className="text-error hover:bg-error/10"
                onClick={() => handleEncerrarClick(vida.apoliceVidaPublicId)}
              >
                Encerrar
              </Button>
            )}
            {podeExcluir && (
              <Button
                variant="text"
                size="small"
                disabled={vida.possuiPropostas}
                title={
                  vida.possuiPropostas
                    ? 'Esta participação possui propostas. Utilize Encerrar.'
                    : 'Excluir participação incluída por engano'
                }
                onClick={() =>
                  setConfirmDialog({
                    isOpen: true,
                    isLoading: false,
                    acao: 'excluir',
                    vidaId: vida.apoliceVidaPublicId,
                    clienteNome: vida.clienteNome,
                  })
                }
              >
                Excluir
              </Button>
            )}
          </div>
        ),
      });
    }

    return cols;
  }, [
    podeAlterar,
    podeInativar,
    podeExcluir,
    podeConsultarPropostas,
    podeCriarPropostas,
  ]);

  if (error) {
    return (
      <div className="flex flex-col gap-4 items-start">
        <Alert variant="error" title="Erro ao carregar as vidas">
          {error.message}
        </Alert>
        <Button onClick={retry} size="small" loading={isLoading}>
          Tentar novamente
        </Button>
      </div>
    );
  }

  const hasData = data && data.items && data.items.length > 0;

  return (
    <div className="flex flex-col gap-4">
      {podeInserir && (
        <div className="flex justify-end">
          <Button variant="primary" onClick={handleAdd}>
            Adicionar Vida
          </Button>
        </div>
      )}

      {!isLoading && !hasData ? (
        <EmptyState
          title="Nenhuma vida encontrada"
          description="Esta apólice não possui beneficiários ou vidas cadastradas."
        />
      ) : (
        <>
          <DataTable
            data={data?.items || []}
            columns={columns}
            keyExtractor={(item) => item.apoliceVidaPublicId}
            isLoading={isLoading}
            aria-label="Lista de Vidas da Apólice"
          />

          {data && Math.ceil(data.totalCount / data.pageSize) > 1 && (
            <div className="flex justify-center md:justify-end mt-4 pt-4 border-t border-borda">
              <Pagination
                currentPage={data.page}
                totalPages={Math.ceil(data.totalCount / data.pageSize)}
                onPageChange={handlePageChange}
                disabled={isLoading}
              />
            </div>
          )}
        </>
      )}

      {podeConsultarPropostas && vidaPropostas && (
        <Modal
          aberto
          onClose={() => {
            if (!criandoVersao) setVidaPropostas(null);
          }}
          title={`Propostas de ${vidaPropostas.clienteNome}`}
          size="large"
        >
          <p className="text-sm text-texto-secundario mb-3">
            Matrícula: {vidaPropostas.vinculoMatricula || 'Não informada'}.{' '}
            {estipulanteNome ? `Estipulante: ${estipulanteNome}.` : ''}
          </p>
          <PropostasConsulta
            key={`${publicId}/${vidaPropostas.apoliceVidaPublicId}`}
            apolicePublicId={publicId}
            apoliceVidaPublicId={vidaPropostas.apoliceVidaPublicId}
            initialPublicId={propostaCriada ?? undefined}
            onBusy={setCriandoVersao}
          />
        </Modal>
      )}
      {podeCriarPropostas && vidaCriacao && (
        <CriarPropostaDialog
          key={vidaCriacao.apoliceVidaPublicId}
          apolicePublicId={publicId}
          vida={vidaCriacao}
          estipulanteNome={estipulanteNome}
          onClose={() => setVidaCriacao(null)}
          onCriada={(id) => {
            setPropostaCriada(id);
            setVidaPropostas(vidaCriacao);
            setVidaCriacao(null);
            void retry();
          }}
        />
      )}
      {propostaCriada && !podeConsultarPropostas && (
        <Alert variant="success" title="Rascunho criado">
          A proposta foi criada sem vigência e sem gerar cobrança.
        </Alert>
      )}

      <ApoliceVidaFormModal
        estipulanteNome={estipulanteNome}
        podeCadastrarVinculo={possuiPermissao('clientes.alterar')}
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        onSubmit={handleFormSubmit}
        apolicePublicId={publicId}
        initialData={selectedVida}
        isSubmitting={isSubmitting}
        submitError={submitError}
      />

      <ConfirmDialog
        aberto={confirmDialog.isOpen}
        title={
          confirmDialog.acao === 'excluir'
            ? 'Excluir Participação'
            : 'Encerrar Participação'
        }
        description={
          <div className="flex flex-col gap-3">
            <p>
              {confirmDialog.acao === 'excluir'
                ? `Deseja excluir a participação de ${confirmDialog.clienteNome} nesta apólice? Ela será removida da lista de Vidas. O cadastro do cliente, a matrícula e o histórico serão preservados.`
                : 'Deseja encerrar esta participação na Apólice? O Cadastro Global do Cliente será preservado e a participação continuará disponível no histórico.'}
            </p>
            {confirmDialog.erro && (
              <Alert variant="error" title="Não foi possível concluir">
                {confirmDialog.erro}
              </Alert>
            )}
          </div>
        }
        confirmText={confirmDialog.acao === 'excluir' ? 'Excluir' : 'Encerrar'}
        cancelText="Cancelar"
        variant="danger"
        loading={confirmDialog.isLoading}
        onConfirm={() => void confirmarAcao()}
        onClose={() => {
          if (!confirmDialog.isLoading)
            setConfirmDialog({
              isOpen: false,
              isLoading: false,
              acao: 'encerrar',
            });
        }}
      />
    </div>
  );
};
