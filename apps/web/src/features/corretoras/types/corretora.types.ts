/**
 * corretora.types.ts
 *
 * Tipos e contratos de dados para o módulo de Corretoras.
 */

export interface CorretoraListItem {
  publicId: string;
  nome: string;
  codigo?: string;
  codigoProtheus?: string;
  cnpj?: string;
  ativo: boolean;
  createdAt: string;
}

export interface CorretoraDetalhe {
  publicId: string;
  nome: string;
  codigo?: string;
  codigoProtheus?: string;
  cnpj?: string;
  cnpjLimpo?: string;
  ativo: boolean;
  observacao?: string;
  createdAt: string;
  updatedAt: string;
}

export interface ListarCorretorasQuery {
  pagina?: number;
  tamanhoPagina?: number;
  busca?: string;
  ativo?: boolean;
}

export interface PagedResult<T> {
  itens: T[];
  paginaAtual: number;
  tamanhoPagina: number;
  totalItens: number;
  totalPaginas: number;
}

export interface CriarCorretoraRequest {
  nome: string;
  codigo?: string;
  codigoProtheus?: string;
  cnpj?: string;
  observacao?: string;
}

export interface AlterarCorretoraRequest {
  publicId: string;
  nome: string;
  codigo?: string;
  codigoProtheus?: string;
  cnpj?: string;
  observacao?: string;
}
