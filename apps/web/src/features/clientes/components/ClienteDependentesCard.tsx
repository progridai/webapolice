import React from 'react';
import { Card } from '../../../components/ui';
import type { ClienteDependenteResponse } from '../types/clienteDetalhe.types';
import { formatarDataOuVazio } from '../../../shared/utils/formatters';

interface ClienteDependentesCardProps {
  dependentes: ClienteDependenteResponse[];
}

export const ClienteDependentesCard: React.FC<ClienteDependentesCardProps> = ({ dependentes }) => {
  return (
    <Card className="p-6">
      <h3 className="text-lg font-semibold text-slate-900 dark:text-slate-50 mb-4">
        Dependentes
      </h3>
      
      {dependentes.length === 0 ? (
        <p className="text-sm text-slate-500 dark:text-slate-400">Nenhum dependente cadastrado.</p>
      ) : (
        <div className="flex flex-col gap-4">
          {dependentes.map((dependente, index) => (
            <div 
              key={index} 
              className="p-4 rounded-md bg-slate-50 dark:bg-slate-800/50 border border-slate-100 dark:border-slate-700/50"
            >
              <div className="flex flex-col sm:flex-row sm:items-start sm:justify-between gap-2">
                <div>
                  <p className="text-base font-medium text-slate-900 dark:text-slate-50">
                    {dependente.nome}
                  </p>
                  <p className="text-sm text-slate-500 dark:text-slate-400">
                    {dependente.tipoRelacao}
                  </p>
                </div>
                <div className="text-left sm:text-right">
                  <p className="text-sm font-medium text-slate-700 dark:text-slate-300">
                    {dependente.documentoMascarado || 'Documento não informado'}
                  </p>
                  {dependente.dataNascimento && (
                    <p className="text-xs text-slate-500 dark:text-slate-400 mt-0.5">
                      Nasc: {formatarDataOuVazio(dependente.dataNascimento)}
                    </p>
                  )}
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </Card>
  );
};
