export interface ApoliceListItem {
  publicId: string;
  numeroPrincipal: string;
  estipulanteNome: string;
  seguradoraNome: string;
  dataInicioVigencia: string; // Date string
  dataFimVigencia: string; // Date string
  status: string;
  ativo: boolean;
  quantidadeRamos: number;
  resumoRamos: string;
}

export interface ApolicesQuery {
  page?: number;
  pageSize?: number;
  busca?: string;
  status?: string;
  ativo?: boolean;
  estipulanteId?: string;
  seguradoraId?: string;
  tipoRamo?: string;
  vigenciaDataReferencia?: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface ApoliceRamoResult {
  publicId: string;
  ramoCodigo: string;
  ramoNome: string;
  numeroApolice?: string;
  iofPercentual?: number;
  ativo: boolean;
}

export interface ApoliceDetalheResponse {
  publicId: string;
  nome: string;
  estipulanteId: number;
  estipulanteNome: string;
  seguradoraId: number;
  seguradoraNome: string;
  corretoraId?: number;
  corretoraNome?: string;
  dataInicioVigencia?: string;
  dataFimVigencia?: string;
  dataAniversario?: string;
  status: string;
  ativo: boolean;
  observacao?: string;
  ramos: ApoliceRamoResult[];
  configuracao?: {
    tipoAdesao?: string;
    custeio?: string;
    carenciaDias?: number;
    mesBaseReajuste?: number;
    indiceReajuste?: string;
    cobreConjuge: boolean;
    controlaExcedente: boolean;
    diaCorteFaturamento?: number;
    prazoAvisoSinistroDias?: number;
  };
}

export interface ApoliceVidaListItem {
  apoliceVidaPublicId: string;
  clientePublicId: string;
  clienteNome: string;
  clienteDocumentoMascarado?: string;
  contexto: 'direto' | 'subestipulante' | 'modulo';
  subestipulantePublicId?: string;
  subestipulanteNome?: string;
  moduloPublicId?: string;
  moduloNome?: string;
  dataInicioVigencia?: string;
  dataFimVigencia?: string;
  ativo: boolean;
  status: string;
  observacao?: string;
}

export interface ApoliceVidaQuery {
  page?: number;
  pageSize?: number;
  busca?: string;
  status?: string;
  subestipulantePublicId?: string;
  moduloPublicId?: string;
  vigenciaDataReferencia?: string;
}

export interface CriarApoliceVidaRequest {
  clientePublicId: string;
  subestipulantePublicId?: string | null;
  moduloPublicId?: string | null;
  dataInicioVigencia?: string | null;
  dataFimVigencia?: string | null;
  observacao?: string | null;
}

export interface AlterarApoliceVidaRequest {
  dataInicioVigencia?: string | null;
  dataFimVigencia?: string | null;
  observacao?: string | null;
}

export interface ApoliceSubestipulanteModuloResult {
  moduloPublicId: string;
  moduloNome: string;
  moduloDescricao?: string;
  moduloAtivoGlobal: boolean;
  vinculoAtivo: boolean;
  dataInicio?: string;
  dataFim?: string;
}

export interface ApoliceSubestipulanteResult {
  subestipulantePublicId: string;
  nome: string;
  documento?: string;
  codigo?: string;
  dataInicio?: string;
  dataFim?: string;
  ativo: boolean;
  modulos: ApoliceSubestipulanteModuloResult[];
}

export interface ApoliceCoberturaResult {
  coberturaIdInternal: number;
  ativo: boolean;
  importanciaSeguradaOverride?: number;
  premioOverride?: number;
}

export interface ApolicePlanoResult {
  planoIdInternal: number;
  tabelaPrecoIdInternal?: number;
  ativo: boolean;
  coberturas: ApoliceCoberturaResult[];
}

export interface ApoliceProdutoResult {
  produtoIdInternal: number;
  ativo: boolean;
  planos: ApolicePlanoResult[];
}

export interface ApoliceUniversoPermitidoResult {
  produtos: ApoliceProdutoResult[];
}

export interface ApoliceHistoricoResult {
  acao: string;
  descricao?: string;
  usuarioPublicId?: string;
  dataAcao: string;
}
