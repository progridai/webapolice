import React from 'react';
import { Card, CardHeader, CardContent, EmptyState } from '../../../../components/ui';
import type { ApoliceDetalheResponse } from '../../types/apolice.types';

interface ConfiguracoesTabProps {
  apolice: ApoliceDetalheResponse;
}

export const ConfiguracoesTab: React.FC<ConfiguracoesTabProps> = ({ apolice }) => {
  const config = apolice.configuracao;

  if (!config) {
    return (
      <EmptyState
        title="Nenhuma Configuração"
        description="Esta apólice ainda não possui configurações operacionais definidas."
      />
    );
  }

  return (
    <div className="flex flex-col gap-6">
      <Card>
        <CardHeader>
          <h3 className="text-lg font-medium text-texto-principal">Vigência e Movimentação</h3>
        </CardHeader>
        <CardContent>
          <dl className="grid grid-cols-1 md:grid-cols-3 gap-6">
            <div>
              <dt className="text-sm font-medium text-texto-terciario">Tipo de Adesão</dt>
              <dd className="mt-1 text-sm text-texto-principal">{config.tipoAdesao || '—'}</dd>
            </div>
            <div>
              <dt className="text-sm font-medium text-texto-terciario">Custeio</dt>
              <dd className="mt-1 text-sm text-texto-principal">{config.custeio || '—'}</dd>
            </div>
            <div>
              <dt className="text-sm font-medium text-texto-terciario">Carência (Dias)</dt>
              <dd className="mt-1 text-sm text-texto-principal">{config.carenciaDias !== undefined && config.carenciaDias !== null ? config.carenciaDias : '—'}</dd>
            </div>
          </dl>
        </CardContent>
      </Card>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <div className="bg-white rounded-lg border border-borda overflow-hidden">
            <div className="px-6 py-4 border-b border-borda bg-fundo-secundario">
            <h3 className="text-lg font-medium text-texto-principal">Sinistro</h3>
            </div>
            <div className="p-6">
            <dl className="grid grid-cols-1 sm:grid-cols-2 gap-x-4 gap-y-6">
                <div>
                <dt className="text-sm font-medium text-texto-terciario">Prazo de Aviso (Dias)</dt>
                <dd className="mt-1 text-sm text-texto-principal">{config.prazoAvisoSinistroDias || '—'}</dd>
                </div>
            </dl>
            </div>
        </div>

        <Card>
          <CardHeader>
            <h3 className="text-lg font-medium text-texto-principal">Regras de Reajuste e Operação</h3>
          </CardHeader>
          <CardContent>
            <dl className="grid grid-cols-1 gap-4">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <dt className="text-sm font-medium text-texto-terciario">Mês Base</dt>
                  <dd className="mt-1 text-sm text-texto-principal">{config.mesBaseReajuste || '—'}</dd>
                </div>
                <div>
                  <dt className="text-sm font-medium text-texto-terciario">Índice</dt>
                  <dd className="mt-1 text-sm text-texto-principal">{config.indiceReajuste || '—'}</dd>
                </div>
              </div>
              <div className="grid grid-cols-2 gap-4 pt-2 border-t border-borda">
                <div>
                  <dt className="text-sm font-medium text-texto-terciario">Cobre Cônjuge</dt>
                  <dd className="mt-1 text-sm text-texto-principal">{config.cobreConjuge ? 'Sim' : 'Não'}</dd>
                </div>
                <div>
                  <dt className="text-sm font-medium text-texto-terciario">Controla Excedente</dt>
                  <dd className="mt-1 text-sm text-texto-principal">{config.controlaExcedente ? 'Sim' : 'Não'}</dd>
                </div>
              </div>
            </dl>
          </CardContent>
        </Card>
      </div>
    </div>
  );
};
