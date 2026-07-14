import React from 'react';
import { Card } from '../../../components/ui';
import { Badge } from '../../../components/ui/Badge';
import type { ClienteVinculoResponse } from '../types/clienteDetalhe.types';

interface ClienteVinculosCardProps {
  vinculos: ClienteVinculoResponse[];
}

export const ClienteVinculosCard: React.FC<ClienteVinculosCardProps> = ({ vinculos }) => {
  return (
    <Card className="p-6">
      <h3 className="text-lg font-semibold text-slate-900 dark:text-slate-50 mb-4">
        Vínculos
      </h3>
      
      {vinculos.length === 0 ? (
        <p className="text-sm text-slate-500 dark:text-slate-400">Nenhum vínculo cadastrado.</p>
      ) : (
        <div className="flex flex-col gap-4">
          {vinculos.map((vinculo, index) => (
            <div 
              key={index} 
              className="p-4 rounded-md bg-slate-50 dark:bg-slate-800/50 border border-slate-100 dark:border-slate-700/50"
            >
              <div className="flex items-center justify-between mb-3 border-b border-slate-200 dark:border-slate-700 pb-3">
                <div>
                  <p className="text-xs font-medium text-slate-500 dark:text-slate-400 uppercase tracking-wider mb-1">
                    Matrícula
                  </p>
                  <p className="text-base font-semibold text-slate-900 dark:text-slate-50">
                    {vinculo.matricula}
                  </p>
                </div>
                <Badge variant={vinculo.ativo ? 'success' : 'neutral'}>
                  {vinculo.ativo ? 'Ativo' : 'Inativo'}
                </Badge>
              </div>

              <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4">
                {vinculo.estipulante && (
                  <div>
                    <p className="text-xs font-medium text-slate-500 dark:text-slate-400">Estipulante</p>
                    <p className="text-sm text-slate-900 dark:text-slate-50 mt-0.5">{vinculo.estipulante}</p>
                  </div>
                )}
                {vinculo.subestipulante && (
                  <div>
                    <p className="text-xs font-medium text-slate-500 dark:text-slate-400">Subestipulante</p>
                    <p className="text-sm text-slate-900 dark:text-slate-50 mt-0.5">{vinculo.subestipulante}</p>
                  </div>
                )}
                {vinculo.grupo && (
                  <div>
                    <p className="text-xs font-medium text-slate-500 dark:text-slate-400">Grupo</p>
                    <p className="text-sm text-slate-900 dark:text-slate-50 mt-0.5">{vinculo.grupo}</p>
                  </div>
                )}
                {vinculo.subgrupo && (
                  <div>
                    <p className="text-xs font-medium text-slate-500 dark:text-slate-400">Subgrupo</p>
                    <p className="text-sm text-slate-900 dark:text-slate-50 mt-0.5">{vinculo.subgrupo}</p>
                  </div>
                )}
                {vinculo.lotacao && (
                  <div>
                    <p className="text-xs font-medium text-slate-500 dark:text-slate-400">Lotação</p>
                    <p className="text-sm text-slate-900 dark:text-slate-50 mt-0.5">{vinculo.lotacao}</p>
                  </div>
                )}
              </div>
            </div>
          ))}
        </div>
      )}
    </Card>
  );
};
