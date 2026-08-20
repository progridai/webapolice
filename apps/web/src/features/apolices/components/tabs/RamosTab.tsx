import React, { useState } from 'react';
import { Card, CardHeader, CardContent, EmptyState, Button, ConfirmDialog, Alert } from '../../../../components/ui';
import type { ApoliceDetalheResponse, ApoliceRamoResult } from '../../types/apolice.types';
import { useAuthorization } from '../../../../auth/AuthorizationProvider';
import { ApoliceRamoFormModal } from './ApoliceRamoFormModal';
import { inativarRamoApolice } from '../../api/apolices.api';
// Se houvesse um context ou hook para refetch, eu chamaria onMutateSucesso
// Vamos receber o refetch como callback para facilitar

interface RamosTabProps {
  apolice: ApoliceDetalheResponse;
  onMutateSucesso?: () => void;
}

export const RamosTab: React.FC<RamosTabProps> = ({ apolice, onMutateSucesso }) => {
  const { possuiPermissao } = useAuthorization();
  
  const ramos = apolice.ramos || [];

  const podeInserir = possuiPermissao('apolices.ramos.inserir');
  const podeAlterar = possuiPermissao('apolices.ramos.alterar');
  const podeInativar = possuiPermissao('apolices.ramos.inativar');

  const [modalAberto, setModalAberto] = useState(false);
  const [ramoParaEditar, setRamoParaEditar] = useState<ApoliceRamoResult | undefined>();
  
  const [inativarRamoId, setInativarRamoId] = useState<string | null>(null);
  const [inativando, setInativando] = useState(false);
  
  const [feedback, setFeedback] = useState<{ type: 'success' | 'error', message: string } | null>(null);

  const handleAdicionar = () => {
    setRamoParaEditar(undefined);
    setModalAberto(true);
  };

  const handleEditar = (ramo: ApoliceRamoResult) => {
    setRamoParaEditar(ramo);
    setModalAberto(true);
  };

  const confirmarInativacao = async () => {
    if (!inativarRamoId) return;
    try {
      setInativando(true);
      setFeedback(null);
      await inativarRamoApolice(apolice.publicId, inativarRamoId);
      setFeedback({ type: 'success', message: 'Vínculo inativado com sucesso.' });
      setInativarRamoId(null);
      if (onMutateSucesso) onMutateSucesso();
    } catch (error: any) {
      setFeedback({
        type: 'error',
        message: error.response?.data?.message || 'Ocorreu um erro inesperado.',
      });
    } finally {
      setInativando(false);
    }
  };

  const renderCabecalho = () => (
    <div className="flex justify-between items-center mb-6">
      <div>
        <h2 className="text-xl font-semibold text-texto-principal">Ramos Vinculados</h2>
        <p className="text-sm text-texto-terciario mt-1">Gerencie os ramos ativos desta apólice.</p>
      </div>
      {podeInserir && (
        <Button onClick={handleAdicionar} variant="primary">
          Adicionar Ramo
        </Button>
      )}
    </div>
  );

  return (
    <div>
      {renderCabecalho()}

      {feedback && (
        <Alert 
          type={feedback.type} 
          title={feedback.type === 'success' ? 'Sucesso' : 'Erro'} 
          onClose={() => setFeedback(null)}
          className="mb-4"
        >
          {feedback.message}
        </Alert>
      )}

      {ramos.length === 0 ? (
        <EmptyState
          title="Nenhum Ramo Vinculado"
          description="Esta apólice ainda não possui ramos configurados."
        />
      ) : (
        <div className="flex flex-col gap-4">
          {ramos.map((ramo) => (
            <Card key={ramo.publicId}>
              <CardHeader className="flex justify-between items-center">
                <h3 className="text-lg font-medium text-texto-principal">
                  {ramo.ramoNome} <span className="text-sm text-texto-terciario">({ramo.ramoCodigo})</span>
                  {!ramo.ativo && <span className="ml-2 px-2 py-0.5 rounded-full bg-red-100 text-xs text-red-700 font-semibold border border-red-200">Inativo</span>}
                </h3>
                <div className="flex items-center gap-2">
                  {podeAlterar && ramo.ativo && (
                    <Button variant="secondary" size="small" onClick={() => handleEditar(ramo)}>
                      Editar
                    </Button>
                  )}
                  {podeInativar && ramo.ativo && (
                    <Button variant="danger" size="small" onClick={() => setInativarRamoId(ramo.publicId)}>
                      Inativar
                    </Button>
                  )}
                </div>
              </CardHeader>
              <CardContent>
                <dl className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div>
                    <dt className="text-sm font-medium text-texto-terciario">Número da Apólice no Ramo</dt>
                    <dd className="mt-1 text-sm text-texto-principal">{ramo.numeroApolice || '—'}</dd>
                  </div>
                  <div>
                    <dt className="text-sm font-medium text-texto-terciario">Percentual de IOF</dt>
                    <dd className="mt-1 text-sm text-texto-principal">{ramo.iofPercentual !== undefined ? `${ramo.iofPercentual}%` : '—'}</dd>
                  </div>
                </dl>
              </CardContent>
            </Card>
          ))}
        </div>
      )}

      {modalAberto && (
        <ApoliceRamoFormModal
          aberto={modalAberto}
          onClose={() => setModalAberto(false)}
          apolicePublicId={apolice.publicId}
          ramoEdicao={ramoParaEditar}
          onSucesso={() => {
            if (onMutateSucesso) onMutateSucesso();
          }}
        />
      )}

      <ConfirmDialog
        aberto={!!inativarRamoId}
        onClose={() => !inativando && setInativarRamoId(null)}
        onConfirm={confirmarInativacao}
        title="Inativar Vínculo"
        description="Tem certeza que deseja inativar o vínculo deste Ramo com a Apólice? O histórico permanecerá visível."
        confirmText="Inativar Vínculo"
        variant="danger"
        loading={inativando}
      />
    </div>
  );
};
