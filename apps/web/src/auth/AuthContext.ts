/**
 * AuthContext.ts
 *
 * Contexto React tipado para autenticação.
 * Separado do Provider para evitar dependências circulares.
 */
import { createContext } from 'react';
import type { AuthContextValue } from './auth.types';

export const AuthContext = createContext<AuthContextValue | undefined>(undefined);
