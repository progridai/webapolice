/**
 * estipulante.types.ts
 *
 * Contratos de interface baseados na resposta real do backend para Estipulantes.
 */

export type EstipulanteStatus = 'ativo' | 'inativo';
export type StatusEstipulanteEnum = 1 | 2; // 1 = Ativo, 2 = Inativo

export interface EstipulanteListItem {
  publicId: string;
  razaoSocial: string;
  nomeFantasia?: string;
  cnpj: string;
  codigo?: string;
  grupo?: string;
  seguradora?: string;
  ativo: boolean;
  dataCadastro: string;
}

export interface PagedResult<T> {
  itens: T[];
  paginaAtual: number;
  tamanhoPagina: number;
  totalItens: number;
  totalPaginas: number;
}

export interface EstipulantesQuery {
  page?: number;
  pageSize?: number;
  busca?: string; // Razão Social, Nome Fantasia, CNPJ ou Código
  status?: StatusEstipulanteEnum | '';
  sortBy?: string;
  direction?: 'asc' | 'desc';
}

export interface EstipulanteContatoRequest {
  tipoContato: string;
  valor: string;
  principal: boolean;
}

export interface EstipulanteContatoInstitucionalRequest {
  nome: string;
  departamento: string;
  email?: string;
  telefone?: string;
  ramal?: string;
}

export interface EstipulanteEnderecoRequest {
  cep?: string;
  logradouro?: string;
  numero?: string;
  complemento?: string;
  bairro?: string;
  cidadeId?: number;
  uf?: string;
}

export interface EstipulanteConfiguracaoRequest {
  dataInicioVigencia: string; // YYYY-MM-DD
  dataFimVigencia?: string; // YYYY-MM-DD
}

export interface CriarEstipulanteRequest {
  razaoSocial: string;
  nomeFantasia?: string;
  cnpj: string;
  codigo?: string;
  grupoPublicId?: string;
  seguradoraPublicId?: string;
  observacao?: string;
  endereco?: EstipulanteEnderecoRequest;
  contatos?: EstipulanteContatoRequest[];
  contatosInstitucionais?: EstipulanteContatoInstitucionalRequest[];
  configuracao: EstipulanteConfiguracaoRequest;
}

export interface AtualizarEstipulanteRequest {
  razaoSocial: string;
  nomeFantasia?: string;
  codigo?: string;
  grupoPublicId?: string;
  seguradoraPublicId?: string;
  observacao?: string;
  endereco?: EstipulanteEnderecoRequest;
  contatos?: EstipulanteContatoRequest[];
  contatosInstitucionais?: EstipulanteContatoInstitucionalRequest[];
  configuracao: EstipulanteConfiguracaoRequest;
}

export interface EstipulanteResponse {
  publicId: string;
}

export interface EstipulanteDetalheResponse {
  publicId: string;
  razaoSocial: string;
  nomeFantasia?: string;
  cnpj: string;
  codigo?: string;
  grupoPublicId?: string;
  seguradoraPublicId?: string;
  observacao?: string;
  ativo: boolean;
  endereco?: {
    tipoEndereco: string;
    cep?: string;
    logradouro?: string;
    numero?: string;
    complemento?: string;
    bairro?: string;
    cidadeId?: number;
    cidadeNome?: string;
    uf?: string;
  };
  contatos?: {
    tipoContato: string;
    valor: string;
    principal: boolean;
  }[];
  contatosInstitucionais?: {
    nome: string;
    departamento: string;
    email?: string;
    telefone?: string;
    ramal?: string;
  }[];
}

export interface EstipulanteConfiguracaoResponse {
  dataInicioVigencia: string;
  dataFimVigencia?: string;
}
