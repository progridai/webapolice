import React from 'react';
import { Card } from '../../../components/ui';
import { Badge } from '../../../components/ui/Badge';
import type { ClienteContatoResponse } from '../types/clienteDetalhe.types';

interface ClienteContatosCardProps {
  contatos: ClienteContatoResponse[];
}

export const ClienteContatosCard: React.FC<ClienteContatosCardProps> = ({ contatos }) => {
  const contatosAtivos = contatos.filter((c) => c.ativo);

  // Ordena para que o contato principal apareça primeiro
  const contatosOrdenados = [...contatosAtivos].sort((a, b) => {
    if (a.principal && !b.principal) return -1;
    if (!a.principal && b.principal) return 1;
    return 0;
  });

  return (
    <Card className="p-6">
      <h3 className="text-lg font-semibold text-slate-900 dark:text-slate-50 mb-4">
        Contatos
      </h3>
      
      {contatosOrdenados.length === 0 ? (
        <p className="text-sm text-slate-500 dark:text-slate-400">Nenhum contato cadastrado.</p>
      ) : (
        <div className="flex flex-col gap-4">
          {contatosOrdenados.map((contato, index) => (
            <div 
              key={index} 
              className="flex items-center justify-between p-3 rounded-md bg-slate-50 dark:bg-slate-800/50 border border-slate-100 dark:border-slate-700/50"
            >
              <div>
                <p className="text-xs font-medium text-slate-500 dark:text-slate-400 uppercase tracking-wider mb-1">
                  {contato.tipo}
                </p>
                <p className="text-base text-slate-900 dark:text-slate-50">{contato.valor}</p>
              </div>
              {contato.principal && (
                <Badge variant="neutral" className="bg-blue-50 text-blue-700 border-blue-200 dark:bg-blue-900/30 dark:text-blue-300 dark:border-blue-800">
                  Principal
                </Badge>
              )}
            </div>
          ))}
        </div>
      )}
    </Card>
  );
};
