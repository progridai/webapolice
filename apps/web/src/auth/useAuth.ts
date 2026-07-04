/**
 * useAuth.ts
 *
 * Hook tipado para consumo do AuthContext.
 * Lança erro descritivo quando usado fora do AuthProvider.
 *
 * REGRA: Use este hook em vez de acessar o Keycloak diretamente.
 */
import { useContext } from 'react';
import { AuthContext } from './AuthContext';
import type { AuthContextValue } from './auth.types';

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error(
      'useAuth deve ser utilizado dentro de um AuthProvider.\n' +
        'Certifique-se de que o componente está dentro da árvore do AppProviders.'
    );
  }
  return context;
}
