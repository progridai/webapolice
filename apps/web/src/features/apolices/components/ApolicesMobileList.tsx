import React from 'react';
import { Card, CardHeader, CardContent, RowActions, StatusBadge, EyeIcon } from '../../../components/ui';
import type { ApoliceListItem } from '../types/apolice.types';

interface ApolicesMobileListProps {
  apolices: ApoliceListItem[];
  onDetalhar?: (publicId: string) => void;
}

export const ApolicesMobileList: React.FC<ApolicesMobileListProps> = ({ 
  apolices, 
  onDetalhar
}) => {
  const formatDate = (dateString: string | undefined | null) => {
    if (!dateString) return '—';
    try {
      const d = new Date(dateString);
      return new Intl.DateTimeFormat('pt-BR').format(d);
    } catch {
      return dateString;
    }
  };

  return (
    <div className="flex flex-col gap-4">
      {apolices.map((apolice) => (
        <Card key={apolice.publicId} className="w-full">
          <CardHeader className="pb-2 flex-row justify-between items-start">
            <div>
              <h3 className="font-semibold text-lg text-texto-principal">{apolice.numeroPrincipal || 'Sem número'}</h3>
              <p className="text-sm mt-1 text-texto-secundario font-medium">
                {apolice.estipulanteNome}
              </p>
              <p className="text-sm mt-1 text-texto-secundario">
                {apolice.seguradoraNome}
              </p>
              <p className="text-sm mt-1 text-texto-terciario">
                Vigência: {formatDate(apolice.dataInicioVigencia)} até {formatDate(apolice.dataFimVigencia)}
              </p>
              {apolice.quantidadeRamos > 0 && (
                <p className="text-xs mt-2 text-texto-secundario font-mono bg-fundo-secundario inline-block px-2 py-1 rounded">
                  {apolice.resumoRamos || `${apolice.quantidadeRamos} Ramos`}
                </p>
              )}
            </div>
            <StatusBadge status={apolice.ativo ? 'ativo' : 'inativo'} label={apolice.status} />
          </CardHeader>
          <CardContent className="pt-0">
            <div className="flex justify-between items-end mt-4">
              <span className="text-xs text-texto-terciario">
                {/* espaço reservado caso adicione data de emissão */}
              </span>
              
              {onDetalhar && (
                <RowActions
                  primaryAction={{
                    label: 'Detalhes',
                    icon: <EyeIcon />,
                    onClick: () => onDetalhar(apolice.publicId),
                  }}
                  actions={[]}
                  ariaLabel={`Ações para a Apólice ${apolice.numeroPrincipal}`}
                />
              )}
            </div>
          </CardContent>
        </Card>
      ))}
    </div>
  );
};
