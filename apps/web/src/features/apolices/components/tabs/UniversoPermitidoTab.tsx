import React from 'react';
import { useApoliceUniversoPermitido } from '../../hooks/useApoliceUniversoPermitido';
import { Alert, Button, EmptyState } from '../../../../components/ui';

interface UniversoPermitidoTabProps {
  publicId: string;
}

export const UniversoPermitidoTab: React.FC<UniversoPermitidoTabProps> = ({ publicId }) => {
  const { data, isLoading, error } = useApoliceUniversoPermitido(publicId);

  if (error) {
    return (
      <div className="flex flex-col gap-4 items-start">
        <Alert variant="error" title="Erro ao carregar o Universo Permitido">
          {error.message}
        </Alert>
        <Button onClick={() => window.location.reload()} size="small" loading={isLoading}>
          Tentar novamente
        </Button>
      </div>
    );
  }

  const hasData = data && data.produtos && data.produtos.length > 0;

  if (!isLoading && !hasData) {
    return (
      <EmptyState
        title="Universo Permitido Vazio"
        description="Nenhum produto/plano foi configurado no universo permitido desta apólice."
      />
    );
  }

  if (isLoading) {
    return (
      <div className="flex justify-center p-8">
        <span className="text-texto-secundario">Carregando Universo Permitido...</span>
      </div>
    );
  }

  return (
    <div className="flex flex-col gap-6">
      {data?.produtos.map((produto) => (
        <div key={produto.produtoIdInternal} className="border border-borda rounded-lg overflow-hidden bg-white shadow-sm">
          <div className="bg-fundo-secundario px-4 py-3 border-b border-borda flex justify-between items-center">
            <h3 className="font-semibold text-texto-principal text-lg">Produto {produto.produtoIdInternal}</h3>
            <span className={`px-2 py-1 text-xs rounded-full font-medium ${produto.ativo ? 'bg-green-100 text-green-800' : 'bg-gray-100 text-gray-800'}`}>
              {produto.ativo ? 'Ativo' : 'Inativo'}
            </span>
          </div>
          <div className="p-4 flex flex-col gap-4">
            {produto.planos.length === 0 ? (
              <p className="text-sm text-texto-secundario">Nenhum plano associado.</p>
            ) : (
              produto.planos.map((plano) => (
                <div key={plano.planoIdInternal} className="border border-borda rounded-md p-3 bg-fundo-principal">
                  <div className="flex justify-between items-center mb-2">
                    <h4 className="font-medium text-texto-principal">Plano {plano.planoIdInternal}</h4>
                    <span className={`px-2 py-0.5 text-xs rounded-full ${plano.ativo ? 'bg-green-100 text-green-800' : 'bg-gray-100 text-gray-800'}`}>
                      {plano.ativo ? 'Ativo' : 'Inativo'}
                    </span>
                  </div>
                  {plano.tabelaPrecoIdInternal && (
                    <div className="text-sm text-texto-secundario mb-3">Tabela de Preço: {plano.tabelaPrecoIdInternal}</div>
                  )}
                  
                  <div className="mt-2">
                    <h5 className="text-xs font-semibold text-texto-terciario uppercase tracking-wider mb-2">Coberturas</h5>
                    {plano.coberturas.length === 0 ? (
                      <p className="text-xs text-texto-secundario">Nenhuma cobertura definida.</p>
                    ) : (
                      <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-2">
                        {plano.coberturas.map((cobertura) => (
                          <div key={cobertura.coberturaIdInternal} className="bg-white border border-borda rounded p-2 text-sm flex flex-col">
                            <div className="flex justify-between">
                              <span className="font-medium">Cobertura {cobertura.coberturaIdInternal}</span>
                              <span className={`text-xs ${cobertura.ativo ? 'text-green-600' : 'text-gray-400'}`}>
                                {cobertura.ativo ? 'Ativa' : 'Inativa'}
                              </span>
                            </div>
                            {cobertura.importanciaSeguradaOverride !== null && (
                              <div className="text-xs text-texto-secundario mt-1">IS: R$ {cobertura.importanciaSeguradaOverride}</div>
                            )}
                            {cobertura.premioOverride !== null && (
                              <div className="text-xs text-texto-secundario">Prêmio: R$ {cobertura.premioOverride}</div>
                            )}
                          </div>
                        ))}
                      </div>
                    )}
                  </div>
                </div>
              ))
            )}
          </div>
        </div>
      ))}
    </div>
  );
};
