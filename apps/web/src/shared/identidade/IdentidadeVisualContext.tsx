/* eslint-disable react-refresh/only-export-components */
import React, { createContext, useContext, useState, useEffect } from 'react';
import type { IdentidadeVisualOrganizacao } from './types';
import { IdentidadeVisualService } from './IdentidadeVisualService';

interface IdentidadeVisualContextProps {
  identidadeAtual: IdentidadeVisualOrganizacao;
  aplicarIdentidade: (configuracao: Partial<IdentidadeVisualOrganizacao>) => void;
  restaurarPadrao: () => void;
}

const IdentidadeVisualContext = createContext<IdentidadeVisualContextProps | undefined>(undefined);

export const IdentidadeVisualProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [identidadeAtual, setIdentidadeAtual] = useState<IdentidadeVisualOrganizacao>(
    IdentidadeVisualService.obterIdentidadeAtual()
  );

  const aplicarIdentidade = (configuracao: Partial<IdentidadeVisualOrganizacao>) => {
    const novaIdentidade = IdentidadeVisualService.aplicarIdentidadeVisual(configuracao);
    setIdentidadeAtual(novaIdentidade);
  };

  const restaurarPadrao = () => {
    const padrao = IdentidadeVisualService.restaurarIdentidadePadrao();
    setIdentidadeAtual(padrao);
  };

  useEffect(() => {
    // Ao montar o Provider, aplicar caso já não esteja no padrão
    if (identidadeAtual.organizacaoId !== 'padrao') {
      IdentidadeVisualService.aplicarIdentidadeVisual(identidadeAtual);
    }
  }, [identidadeAtual]);

  return (
    <IdentidadeVisualContext.Provider value={{ identidadeAtual, aplicarIdentidade, restaurarPadrao }}>
      {children}
    </IdentidadeVisualContext.Provider>
  );
};

export const useIdentidadeVisual = () => {
  const context = useContext(IdentidadeVisualContext);
  if (!context) {
    throw new Error('useIdentidadeVisual deve ser usado dentro de um IdentidadeVisualProvider');
  }
  return context;
};
