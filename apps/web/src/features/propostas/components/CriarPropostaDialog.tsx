import {
  Alert,
  ConfirmDialog,
  DescriptionItem,
  DescriptionList,
} from '../../../components/ui';
import type { ApoliceVidaListItem } from '../../apolices/types/apolice.types';
import { useCriarProposta } from '../hooks/useCriarProposta';

export function CriarPropostaDialog({
  apolicePublicId,
  vida,
  estipulanteNome,
  onClose,
  onCriada,
}: {
  apolicePublicId: string;
  vida: ApoliceVidaListItem;
  estipulanteNome?: string;
  onClose: () => void;
  onCriada: (id: string) => void;
}) {
  const { criar, isSubmitting, error } = useCriarProposta();
  return (
    <ConfirmDialog
      aberto
      title="Criar pré proposta"
      confirmText="Criar pré proposta"
      cancelText="Cancelar"
      loading={isSubmitting}
      onClose={() => {
        if (!isSubmitting) onClose();
      }}
      onConfirm={() =>
        void criar(apolicePublicId, vida.apoliceVidaPublicId, onCriada)
      }
      description={
        <div className="flex flex-col gap-3">
          <DescriptionList columns={2} density="compact">
            <DescriptionItem label="Cliente" value={vida.clienteNome} />
            <DescriptionItem label="Matrícula" value={vida.vinculoMatricula} />
            <DescriptionItem label="Estipulante" value={estipulanteNome} />
            <DescriptionItem label="Subgrupo" value={vida.subgrupoNome} />
            <DescriptionItem label="Módulo" value={vida.moduloNome} />
          </DescriptionList>
          <p>
            A pré proposta será criada sem vigência e sem gerar cobrança. O
            Plano e as Coberturas ativos do Módulo serão copiados com suas
            condições atuais para consulta.
          </p>
          {!vida.apoliceModuloPublicId && (
            <Alert variant="warning" title="Vida sem Módulo">
              A pré proposta será criada sem itens ou coberturas. O
              preenchimento deverá ser concluído em uma etapa posterior.
            </Alert>
          )}
          <p>
            A vigência e as parcelas serão tratadas após o preenchimento e a
            confirmação.
          </p>
          {error && (
            <Alert
              variant="error"
              title="Não foi possível criar a pré proposta"
            >
              {error}
            </Alert>
          )}
        </div>
      }
    />
  );
}
