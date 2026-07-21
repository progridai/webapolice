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
  documentoMascarado: string;
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

export interface ContatoRequest {
  tipoContato: string;
  valor: string;
  principal: boolean;
}

export interface EnderecoRequest {
  tipoEndereco: string;
  cep?: string;
  logradouro?: string;
  numero?: string;
  complemento?: string;
  bairro?: string;
  cidadeId?: number;
  uf?: string;
  principal: boolean;
}

export interface CadastrarClienteRequest {
  tipoPessoa: number; // 1 = PF, 2 = PJ
  nome: string;
  documento: string;
  dataNascimento?: string; // YYYY-MM-DD
  sexo?: number;
  observacao?: string;
  falecido: boolean;
  dataObito?: string;
  contatos: ContatoRequest[];
  enderecos: EnderecoRequest[];
}

export interface AlterarClienteRequest {
  nome: string;
  documento?: string;
  dataNascimento?: string;
  sexo?: number;
  observacao?: string;
  falecido: boolean;
  dataObito?: string;
  contatos: ContatoRequest[];
  enderecos: EnderecoRequest[];
}

export interface ClienteDetalhe {
  publicId: string;
  tipoPessoa: number;
  nome: string;
  documentoMascarado: string;
  dataNascimento?: string;
  sexo?: number;
  observacao?: string;
  falecido: boolean;
  dataObito?: string;
  email?: string;
  telefone?: string;
  celular?: string;
  endereco?: EnderecoRequest;
  status: ClienteStatus;
}
