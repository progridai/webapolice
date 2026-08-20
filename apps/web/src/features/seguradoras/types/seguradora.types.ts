/**
 * seguradora.types.ts
 *
 * Tipos e contratos de dados para o módulo de Seguradoras.
 */

export interface SeguradoraListItem {
  publicId: string;
  nome: string;
  codigo?: string;
  susep?: string;
  cnpj?: string;
  ativo: boolean;
  createdAt: string;
}

export interface SeguradoraDetalhe {
  publicId: string;
  nome: string;
  codigo?: string;
  susep?: string;
  cnpj?: string;
  cnpjLimpo?: string;
  ativo: boolean;
  observacao?: string;
  createdAt: string;
  updatedAt: string;
}

export interface ListarSeguradorasQuery {
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

export interface CriarSeguradoraRequest {
  nome: string;
  codigo?: string;
  susep?: string;
  cnpj?: string;
  observacao?: string;
}

export interface AlterarSeguradoraRequest {
  publicId: string;
  nome: string;
  codigo?: string;
  susep?: string;
  cnpj?: string;
  observacao?: string;
}
