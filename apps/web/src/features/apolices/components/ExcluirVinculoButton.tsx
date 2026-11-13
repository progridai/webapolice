import { useRef, useState } from 'react';
import { createPortal } from 'react-dom';
import { Alert, Button, ConfirmDialog } from '../../../components/ui';
import { useAuthorization } from '../../../auth/AuthorizationProvider';
import {
  excluirVinculoApolice,
  permissoesExclusaoVinculos,
  type TipoVinculoApolice,
} from '../api/exclusaoVinculos.api';

export function ExcluirVinculoButton({
  apolicePublicId,
  tipo,
  vinculoPublicId,
  identificacao,
  onSucesso,
}: {
  apolicePublicId: string;
  tipo: TipoVinculoApolice;
  vinculoPublicId?: string;
  identificacao: string;
  onSucesso: () => void;
}) {
  const { possuiPermissao } = useAuthorization();
  const [aberto, setAberto] = useState(false);
  const [excluindo, setExcluindo] = useState(false);
  const [erro, setErro] = useState<string | null>(null);
  const emAndamento = useRef(false);
  if (!possuiPermissao(permissoesExclusaoVinculos[tipo]) || !vinculoPublicId)
    return null;
  async function confirmar() {
    if (emAndamento.current || !vinculoPublicId) return;
    emAndamento.current = true;
    setExcluindo(true);
    setErro(null);
    try {
      await excluirVinculoApolice(apolicePublicId, tipo, vinculoPublicId);
      setAberto(false);
      onSucesso();
    } catch (error) {
      setErro(
        error instanceof Error
          ? error.message
          : 'Não foi possível excluir o vínculo. Tente novamente.',
      );
    } finally {
      emAndamento.current = false;
      setExcluindo(false);
    }
  }
  return (
    <>
      <Button
        variant="text"
        size="small"
        aria-label={`Excluir ${identificacao}`}
        onClick={() => {
          setErro(null);
          setAberto(true);
        }}
      >
        Excluir
      </Button>
      {createPortal(
        <div
          onKeyDownCapture={(event) => {
            if (event.key === 'Escape') {
              event.preventDefault();
              event.stopPropagation();
              if (!emAndamento.current) setAberto(false);
            }
          }}
        >
          <ConfirmDialog
            aberto={aberto}
            title="Excluir Vínculo"
            description={
              <div className="flex flex-col gap-3">
                <p>
                  Deseja excluir {identificacao} desta apólice? Os cadastros
                  globais e o histórico serão preservados. A exclusão será
                  bloqueada se houver registros dependentes.
                </p>
                {erro && (
                  <Alert variant="error" title="Não foi possível excluir">
                    {erro}
                  </Alert>
                )}
              </div>
            }
            confirmText="Excluir"
            cancelText="Cancelar"
            variant="danger"
            loading={excluindo}
            onConfirm={() => void confirmar()}
            onClose={() => {
              if (!emAndamento.current) setAberto(false);
            }}
          />
        </div>,
        document.body,
      )}
    </>
  );
}
