/**
 * cliente.types.ts
 *
 * Contratos de interface baseados na resposta real do backend.
 */

export type StatusClienteEnum = 1 | 2;

export type ClienteStatus = 'ativo' | 'inativo';

export interface ClienteListItem {
  id: number;
  nome: string;
  cpfMascarado: string;
  status: ClienteStatus;
  dataCadastroUtc: string;
}

export interface PagedResult<T> {
  itens: T[];
  paginaAtual: number;
  tamanhoPagina: number;
  totalItens: number;
  totalPaginas: number;
}

export interface ClientesQuery {
  page?: number;
  pageSize?: number;
  nome?: string;
  cpf?: string;
  status?: StatusClienteEnum | '';
  sortBy?: string;
  direction?: 'asc' | 'desc';
}
