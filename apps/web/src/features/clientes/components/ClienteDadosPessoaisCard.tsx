import React from 'react';
import { Card } from '../../../components/ui';
import type { ClienteDetalheResponse } from '../types/clienteDetalhe.types';

interface ClienteDadosPessoaisCardProps {
  cliente: ClienteDetalheResponse;
}

export const ClienteDadosPessoaisCard: React.FC<ClienteDadosPessoaisCardProps> = ({ cliente }) => {
  return (
    <Card className="p-6">
      <h3 className="text-lg font-semibold text-slate-900 dark:text-slate-50 mb-4">
        Dados pessoais
      </h3>
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
        <div>
          <p className="text-sm font-medium text-slate-500 dark:text-slate-400">Nome completo</p>
          <p className="text-base text-slate-900 dark:text-slate-50 mt-1">{cliente.nome || 'Não informado'}</p>
        </div>
        <div>
          <p className="text-sm font-medium text-slate-500 dark:text-slate-400">Documento principal</p>
          <p className="text-base text-slate-900 dark:text-slate-50 mt-1">{cliente.documentoMascarado || 'Não informado'}</p>
        </div>
        {/* Adicione outros campos de dados pessoais aqui conforme o DTO for evoluindo */}
      </div>
    </Card>
  );
};
