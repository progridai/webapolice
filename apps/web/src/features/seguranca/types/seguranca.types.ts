/**
 * seguranca.types.ts
 *
 * Contratos baseados nos DTOs reais do backend (Parte 8.1).
 * NÃO adicionar campos não presentes no contrato real da API.
 */

// ─── Paginação ───────────────────────────────────────────────────────────────

export interface PagedResult<T> {
  itens: T[];
  paginaAtual: number;
  tamanhoPagina: number;
  totalItens: number;
  totalPaginas: number;
}

// ─── Perfis ──────────────────────────────────────────────────────────────────

/** PerfilDto — retornado nas listagens e como sub-objeto em usuários */
export interface PerfilDto {
  publicId: string;
  codigo: string;
  nome: string;
  descricao: string;
  ativo: boolean;
  perfilSistema: boolean;
  acessoTotal: boolean;
}

/** PerfilDetalheDto — retornado por GET /perfis/:publicId */
export interface PerfilDetalheDto {
  publicId: string;
  codigo: string;
  nome: string;
  descricao: string;
  ativo: boolean;
  perfilSistema: boolean;
  acessoTotal: boolean;
  permissoesPublicIds: string[];
}

export interface PerfisQuery {
  page?: number;
  pageSize?: number;
  busca?: string;
  ativo?: boolean | '';
}

/** POST /perfis */
export interface CriarPerfilRequest {
  codigo: string;
  nome: string;
  descricao: string;
  ativo: boolean;
  permissaoPublicIds: string[];
}

/** PUT /perfis/:publicId */
export interface AtualizarPerfilRequest {
  nome: string;
  descricao: string;
  ativo: boolean;
  permissaoPublicIds: string[];
}

// ─── Usuários ─────────────────────────────────────────────────────────────────

/** UsuarioListDto — retornado por GET /usuarios */
export interface UsuarioListDto {
  publicId: string;
  username: string;
  nome: string;
  email: string;
  ativo: boolean;
  ultimoLoginEm: string | null;
  perfis: string[]; // lista de nomes/códigos dos perfis
}

/** UsuarioDetalheDto — retornado por GET /usuarios/:publicId */
export interface UsuarioDetalheDto {
  publicId: string;
  keycloakSub: string;
  username: string;
  nome: string;
  email: string;
  ativo: boolean;
  ultimoLoginEm: string | null;
  createdAt: string;
  updatedAt: string;
  perfisAtribuidos: PerfilDto[];
  perfisDisponiveis: PerfilDto[];
}

export interface UsuariosQuery {
  page?: number;
  pageSize?: number;
  busca?: string;
  ativo?: boolean | '';
}

/** POST /usuarios */
export interface CriarUsuarioRequest {
  username: string;
  nome: string;
  email: string;
  senhaTemporaria: string;
  ativo: boolean;
  perfilPublicIds: string[];
}

/** PUT /usuarios/:publicId */
export interface AtualizarUsuarioRequest {
  nome: string;
  email: string;
  ativo: boolean;
  perfilPublicIds: string[];
}

// ─── Catálogo ─────────────────────────────────────────────────────────────────

export interface CatalogoPermissaoDto {
  publicId: string;
  codigo: string;
  nome: string;
  descricao: string;
}

export interface CatalogoRecursoDto {
  publicId: string;
  codigo: string;
  nome: string;
  descricao: string;
  rotaFrontend: string;
  permissoes: CatalogoPermissaoDto[];
}

export interface CatalogoModuloDto {
  publicId: string;
  codigo: string;
  nome: string;
  descricao: string;
  icone: string;
  recursos: CatalogoRecursoDto[];
}

// ─── Auditoria ────────────────────────────────────────────────────────────────

/** AuditoriaListDto — campos retornados pela listagem paginada */
export interface AuditoriaListDto {
  publicId: string;
  acao: string;
  entidadeTipo: string;
  entidadeId: string;
  createdAt: string;
  dadosAnteriores: string | null;
  dadosNovos: string | null;
}

/** AuditoriaDetalheDto — mesmo contrato, usado na tela de detalhes */
export type AuditoriaDetalheDto = AuditoriaListDto;

export interface AuditoriaQuery {
  page?: number;
  pageSize?: number;
  acao?: string;
  entidade?: string;
  dataInicial?: string;
  dataFinal?: string;
}
