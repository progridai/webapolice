/**
 * auth.types.ts
 *
 * Tipos internos de autenticação e autorização.
 * Não dependem do objeto bruto do Keycloak — encapsulam o domínio.
 */

/** Usuário autenticado — modelo interno da aplicação */
export interface AuthUser {
  /** ID único do usuário (Keycloak sub) */
  id: string;
  /** Nome de usuário (preferred_username) */
  username: string;
  /** Nome completo (name ou given_name + family_name) */
  name: string;
  /** E-mail do usuário */
  email: string;
  /** Roles do usuário no realm */
  roles: string[];
}

/** Estado de inicialização do AuthProvider */
export type AuthStatus =
  | 'initializing' // Aguardando resolução do Keycloak
  | 'authenticated' // Usuário autenticado
  | 'unauthenticated' // Usuário não autenticado
  | 'error'; // Falha de inicialização

/** Valor exposto pelo AuthContext */
export interface AuthContextValue {
  /** Estado atual da autenticação */
  status: AuthStatus;

  /** `true` enquanto o estado ainda não está resolvido */
  isLoading: boolean;

  /** `true` quando o usuário está autenticado */
  isAuthenticated: boolean;

  /** Usuário autenticado ou `null` */
  user: AuthUser | null;

  /** Roles do usuário ou array vazio */
  roles: string[];

  /** Inicia o fluxo de login via Keycloak (Authorization Code + PKCE) */
  login: (redirectUri?: string) => Promise<void>;

  /** Encerra a sessão e redireciona */
  logout: (redirectUri?: string) => Promise<void>;

  /** Renova o token de acesso proativamente */
  refreshToken: () => Promise<boolean>;

  /** Verifica se o usuário possui uma role específica */
  hasRole: (role: string) => boolean;

  /** Verifica se o usuário possui ao menos uma das roles informadas */
  hasAnyRole: (roles: string[]) => boolean;

  /** Verifica se o usuário possui todas as roles informadas */
  hasAllRoles: (roles: string[]) => boolean;
}
