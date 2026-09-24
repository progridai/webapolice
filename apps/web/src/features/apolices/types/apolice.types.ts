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
  apoliceSubgrupoPublicId?: string;
  subgrupoNome?: string;
  apoliceModuloPublicId?: string;
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
  apoliceSubgrupoPublicId?: string;
  apoliceModuloPublicId?: string;
  vigenciaDataReferencia?: string;
}

export interface CriarApoliceVidaRequest {
  clientePublicId: string;
  apoliceSubgrupoPublicId?: string | null;
  apoliceModuloPublicId?: string | null;
  dataInicioVigencia?: string | null;
  dataFimVigencia?: string | null;
  observacao?: string | null;
}

export interface AlterarApoliceVidaRequest {
  dataInicioVigencia?: string | null;
  dataFimVigencia?: string | null;
  observacao?: string | null;
  apoliceSubgrupoPublicId?: string | null;
  apoliceModuloPublicId?: string | null;
}

export interface ApoliceSubestipulanteResult {
  subestipulantePublicId: string;
  nome: string;
  documento?: string;
  codigo?: string;
  dataInicio?: string;
  dataFim?: string;
  ativo: boolean;
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

export interface ApoliceSubgrupoResult {
  subgrupoPublicId: string;
  nome: string;
  observacao?: string;
  ativo: boolean;
}

export interface CriarSubgrupoApoliceRequest {
  nome: string;
  observacao?: string | null;
}

export interface AlterarSubgrupoApoliceRequest {
  nome: string;
  observacao?: string | null;
}

// ── Módulos da Apólice ───────────────────────────────────────────────────────
// publicId  → apoliceModuloPublicId (seguro.apolice_modulo)
// moduloPublicId → publicId do cadastro global (cadastro.modulo)

export interface ApoliceModuloResult {
  publicId: string;          // apoliceModuloPublicId — identificador do vínculo
  moduloPublicId: string;    // publicId do cadastro.modulo
  nome: string;
  descricao?: string;
  dataInicio?: string;
  dataFim?: string;
  observacao?: string;
  ativo: boolean;
}

export interface CriarModuloApoliceRequest {
  moduloPublicId: string;    // obrigatório — publicId do cadastro.modulo
  dataInicio?: string | null;
  dataFim?: string | null;
  observacao?: string | null;
}

export interface AlterarModuloApoliceRequest {
  dataInicio?: string | null;
  dataFim?: string | null;
  observacao?: string | null;
}

