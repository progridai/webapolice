import React from 'react';
import { Card } from '../../../components/ui';
import { Badge } from '../../../components/ui/Badge';
import type { ClienteEnderecoResponse } from '../types/clienteDetalhe.types';

interface ClienteEnderecosCardProps {
  enderecos: ClienteEnderecoResponse[];
}

export const ClienteEnderecosCard: React.FC<ClienteEnderecosCardProps> = ({ enderecos }) => {
  const enderecosAtivos = enderecos.filter((e) => e.ativo);

  const enderecosOrdenados = [...enderecosAtivos].sort((a, b) => {
    if (a.principal && !b.principal) return -1;
    if (!a.principal && b.principal) return 1;
    return 0;
  });

  return (
    <Card className="p-6">
      <h3 className="text-lg font-semibold text-slate-900 dark:text-slate-50 mb-4">
        Endereços
      </h3>
      
      {enderecosOrdenados.length === 0 ? (
        <p className="text-sm text-slate-500 dark:text-slate-400">Nenhum endereço cadastrado.</p>
      ) : (
        <div className="flex flex-col gap-4">
          {enderecosOrdenados.map((endereco, index) => (
            <div 
              key={index} 
              className="flex flex-col sm:flex-row sm:items-start sm:justify-between gap-4 p-4 rounded-md bg-slate-50 dark:bg-slate-800/50 border border-slate-100 dark:border-slate-700/50"
            >
              <div>
                <div className="flex items-center gap-2 mb-2">
                  <p className="text-sm font-semibold text-slate-900 dark:text-slate-50">
                    {endereco.tipo}
                  </p>
                  {endereco.principal && (
                    <Badge variant="neutral" className="bg-blue-50 text-blue-700 border-blue-200 dark:bg-blue-900/30 dark:text-blue-300 dark:border-blue-800">
                      Principal
                    </Badge>
                  )}
                </div>
                <p className="text-sm text-slate-700 dark:text-slate-300">
                  {endereco.logradouro}, {endereco.numero} {endereco.complemento && `- ${endereco.complemento}`}
                </p>
                <p className="text-sm text-slate-700 dark:text-slate-300">
                  {endereco.bairro} - {endereco.cidade}/{endereco.uf}
                </p>
                <p className="text-sm text-slate-500 dark:text-slate-400 mt-1">
                  CEP: {endereco.cep}
                </p>
              </div>
            </div>
          ))}
        </div>
      )}
    </Card>
  );
};
