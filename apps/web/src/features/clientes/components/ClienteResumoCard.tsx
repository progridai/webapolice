import React from 'react';
import { Card } from '../../../components/ui';
import { Badge } from '../../../components/ui/Badge';
import type { ClienteDetalheResponse } from '../types/clienteDetalhe.types';
import { formatarDataOuVazio } from '../../../shared/utils/formatters';

interface ClienteResumoCardProps {
  cliente: ClienteDetalheResponse;
}

export const ClienteResumoCard: React.FC<ClienteResumoCardProps> = ({ cliente }) => {
  const isAtivo = cliente.status.codigo === 'ativo';

  return (
    <Card className="p-6 flex flex-col gap-4">
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h2 className="text-2xl font-bold text-slate-900 dark:text-slate-50 mb-1">
            {cliente.nome}
          </h2>
          <p className="text-sm text-slate-500 dark:text-slate-400">
            {cliente.documentoMascarado || 'Documento não informado'}
          </p>
        </div>
        <div>
          <Badge variant={isAtivo ? 'success' : 'neutral'}>
            {cliente.status.nome || cliente.status.codigo}
          </Badge>
        </div>
      </div>
      
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4 pt-4 border-t border-slate-200 dark:border-slate-700">
        <div>
          <p className="text-xs font-semibold text-slate-500 dark:text-slate-400 uppercase tracking-wider">
            Nascimento
          </p>
          <p className="text-sm text-slate-900 dark:text-slate-50 mt-1">
            {formatarDataOuVazio(cliente.dataNascimento)}
          </p>
        </div>
        {cliente.falecido && (
          <div>
            <p className="text-xs font-semibold text-red-500 uppercase tracking-wider">
              Falecido
            </p>
            <p className="text-sm text-slate-900 dark:text-slate-50 mt-1">
              Sim {cliente.dataObito ? `(${formatarDataOuVazio(cliente.dataObito)})` : ''}
            </p>
          </div>
        )}
      </div>
    </Card>
  );
};
