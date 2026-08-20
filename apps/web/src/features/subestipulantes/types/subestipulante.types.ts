/**
 * subestipulante.types.ts
 *
 * Tipos e contratos de dados para o módulo de Subestipulantes.
 */

export interface SubestipulanteListItem {
  publicId: string;
  nome: string;
  codigo?: string;
  cnpj?: string;
  ativo: boolean;
  createdAt: string;
}

export interface SubestipulanteDetalhe {
  publicId: string;
  nome: string;
  codigo?: string;
  cnpj?: string;
  cnpjLimpo?: string;
  ativo: boolean;
  observacao?: string;
  createdAt: string;
  updatedAt: string;
}

export interface ListarSubestipulantesQuery {
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

export interface CriarSubestipulanteRequest {
  nome: string;
  codigo?: string;
  cnpj?: string;
  observacao?: string;
}

export interface AlterarSubestipulanteRequest {
  publicId: string;
  nome: string;
  codigo?: string;
  cnpj?: string;
  observacao?: string;
}
