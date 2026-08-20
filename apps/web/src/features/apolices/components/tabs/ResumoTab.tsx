import React from 'react';
import { Card, CardHeader, CardContent } from '../../../../components/ui';
import type { ApoliceDetalheResponse } from '../../types/apolice.types';

interface ResumoTabProps {
  apolice: ApoliceDetalheResponse;
}

export const ResumoTab: React.FC<ResumoTabProps> = ({ apolice }) => {
  return (
    <Card>
      <CardHeader>
        <h3 className="text-lg font-medium text-texto-principal">Resumo do Contrato</h3>
      </CardHeader>
      <CardContent>
        <dl className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <dt className="text-sm font-medium text-texto-terciario">Estipulante</dt>
            <dd className="mt-1 text-sm text-texto-principal">{apolice.estipulanteNome}</dd>
          </div>
          <div>
            <dt className="text-sm font-medium text-texto-terciario">Seguradora</dt>
            <dd className="mt-1 text-sm text-texto-principal">{apolice.seguradoraNome}</dd>
          </div>
          <div>
            <dt className="text-sm font-medium text-texto-terciario">Corretora</dt>
            <dd className="mt-1 text-sm text-texto-principal">{apolice.corretoraNome || '—'}</dd>
          </div>
          <div>
            <dt className="text-sm font-medium text-texto-terciario">Status</dt>
            <dd className="mt-1 text-sm text-texto-principal">{apolice.status} ({apolice.ativo ? 'Ativo' : 'Inativo'})</dd>
          </div>
          <div>
            <dt className="text-sm font-medium text-texto-terciario">Vigência</dt>
            <dd className="mt-1 text-sm text-texto-principal">
              {apolice.dataInicioVigencia || '—'} a {apolice.dataFimVigencia || '—'}
            </dd>
          </div>
          {apolice.observacao && (
            <div className="col-span-1 md:col-span-2 mt-2">
              <dt className="text-sm font-medium text-texto-terciario">Observações</dt>
              <dd className="mt-1 text-sm text-texto-principal whitespace-pre-wrap">{apolice.observacao}</dd>
            </div>
          )}
        </dl>
      </CardContent>
    </Card>
  );
};
