/* eslint-disable react-refresh/only-export-components */
import React, { createContext, useContext, useState, useEffect } from 'react';

export type TemaPreferido = 'claro' | 'escuro' | 'sistema';
export type TemaResolvido = 'light' | 'dark';

export interface ThemeContextProps {
  temaPreferido: TemaPreferido;
  temaResolvido: TemaResolvido;
  alterarTema: (tema: TemaPreferido) => void;
  // Aliases em snake_case para conformidade absoluta
  tema_preferido: TemaPreferido;
  tema_resolvido: TemaResolvido;
  alterar_tema: (tema: TemaPreferido) => void;
}

const ThemeContext = createContext<ThemeContextProps | undefined>(undefined);

const LOCAL_STORAGE_KEY = 'webapolice-tema';

export const ThemeProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [temaPreferido, setTemaPreferido] = useState<TemaPreferido>(() => {
    if (typeof window !== 'undefined') {
      const saved = localStorage.getItem(LOCAL_STORAGE_KEY);
      if (saved === 'claro' || saved === 'escuro' || saved === 'sistema') {
        return saved;
      }
    }
    return 'sistema';
  });

  const [temaResolvido, setTemaResolvido] = useState<TemaResolvido>(() => {
    if (typeof window !== 'undefined') {
      const saved = localStorage.getItem(LOCAL_STORAGE_KEY);
      if (saved === 'claro') return 'light';
      if (saved === 'escuro') return 'dark';
      return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
    }
    return 'light';
  });

  const alterarTema = (tema: TemaPreferido) => {
    setTemaPreferido(tema);
    localStorage.setItem(LOCAL_STORAGE_KEY, tema);
  };

  useEffect(() => {
    const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
    
    const resolveAndApplyTheme = () => {
      const resolved: TemaResolvido =
        temaPreferido === 'sistema'
          ? (mediaQuery.matches ? 'dark' : 'light')
          : (temaPreferido === 'claro' ? 'light' : 'dark');
      
      setTemaResolvido(resolved);
      document.documentElement.setAttribute('data-theme', resolved);
    };

    resolveAndApplyTheme();

    const handleSystemThemeChange = () => {
      if (temaPreferido === 'sistema') {
        resolveAndApplyTheme();
      }
    };

    if (mediaQuery.addEventListener) {
      mediaQuery.addEventListener('change', handleSystemThemeChange);
    } else {
      mediaQuery.addListener(handleSystemThemeChange);
    }

    return () => {
      if (mediaQuery.removeEventListener) {
        mediaQuery.removeEventListener('change', handleSystemThemeChange);
      } else {
        mediaQuery.removeListener(handleSystemThemeChange);
      }
    };
  }, [temaPreferido]);

  const value: ThemeContextProps = {
    temaPreferido,
    temaResolvido,
    alterarTema,
    tema_preferido: temaPreferido,
    tema_resolvido: temaResolvido,
    alterar_tema: alterarTema
  };

  return <ThemeContext.Provider value={value}>{children}</ThemeContext.Provider>;
};

export const useTema = () => {
  const context = useContext(ThemeContext);
  if (!context) {
    throw new Error('useTema deve ser utilizado dentro de um ThemeProvider');
  }
  return context;
};

export const useTheme = useTema;
