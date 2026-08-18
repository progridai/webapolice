export type TipoCooperado = 1 | 2; // 1: Cooperado, 2: Coordenador
export type StatusCooperado = 1 | 2; // 1: Ativo, 2: Inativo

export interface CooperadoListDto {
  publicId: string;
  nome: string;
  cpfMascarado: string;
  codigo?: string;
  email?: string;
  telefone?: string;
  tipo: TipoCooperado;
  statusId: StatusCooperado;
  dataCadastroUtc: string;
}

export interface ListagemPaginadaResult<T> {
  itens: T[];
  totalGeral: number;
  totalPaginas: number;
  paginaAtual: number;
  tamanhoPagina: number;
  temPaginaAnterior: boolean;
  temPaginaProxima: boolean;
}

export interface CooperadosFiltersState {
  page: number;
  limit: number;
  nome?: string;
  cpf?: string;
  status?: string;
  sortBy?: string;
  direction?: 'asc' | 'desc';
}

export interface CooperadoDetalheDto {
  publicId: string;
  nome: string;
  cpf: string;
  dataNascimento?: string;
  telefone?: string;
  email?: string;
  cep?: string;
  logradouro?: string;
  numero?: string;
  complemento?: string;
  bairro?: string;
  cidadeId?: number;
  uf?: string;
  tipo: TipoCooperado;
  codigo?: string;
  rg?: string;
  orgaoEmissor?: string;
  dataEmissaoRg?: string;
  susep?: string;
  inss?: string;
  issqn?: string;
  numeroDependentes?: number;
  dataInscricao?: string;
  credenciado?: boolean;
  coordenadorId?: number;
  bancoId?: number;
  agencia?: string;
  contaCorrente?: string;
  observacao?: string;
  desativado: boolean;
  dataDesativado?: string;
}

export interface CooperadoFormData {
  nome: string;
  cpf: string;
  dataNascimento?: string;
  telefone?: string;
  email?: string;
  cep?: string;
  logradouro?: string;
  numero?: string;
  complemento?: string;
  bairro?: string;
  cidadeId?: number;
  uf?: string;
  tipo: TipoCooperado;
  codigo?: string;
  rg?: string;
  orgaoEmissor?: string;
  dataEmissaoRg?: string;
  susep?: string;
  inss?: string;
  issqn?: string;
  numeroDependentes?: number;
  dataInscricao?: string;
  credenciado?: boolean;
  coordenadorId?: number;
  bancoId?: number;
  agencia?: string;
  contaCorrente?: string;
  observacao?: string;
}
