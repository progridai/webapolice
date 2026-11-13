export interface Proposta {
  publicId: string;
  numero: string | null;
  integradaApolice: boolean;
  premioLiquido: number | null;
  dataInclusao: string | null;
  status: string;
  statusCodigo: string;
  apolicePublicId: string | null;
  apoliceVidaPublicId: string | null;
  apoliceNome: string | null;
  clientePublicId: string | null;
  clienteNome: string | null;
  clienteVinculoPublicId: string | null;
  vinculoMatricula: string | null;
  vigente: boolean;
  versao: number;
  createdAt: string;
}

export interface PropostaDetalhe extends Proposta {
  dataCancelamentoVigencia: string | null;
  cancelamentoSolicitadoEm: string | null;
  descartadaEm: string | null;
  podeDescartarRascunho: boolean;
  podeCancelarVigencia: boolean;
  podeEfetivarCancelamento: boolean;
  canceladaEm: string | null;
  podeReagendar: boolean;
  podeCancelarAgendamento: boolean;
  confirmadaEm: string | null;
  substituidaEm: string | null;
  dataInicioVigencia: string | null;
  dataFimVigencia: string | null;
  motivoPendenciaVigencia: string | null;
  pendenciaVigenciaEm: string | null;
  podeConfirmar: boolean;
  podeEfetivarVigencia: boolean;
  situacaoVigenciaCodigo:
    | 'SEM_VIGENCIA'
    | 'AGENDADA'
    | 'ATIVACAO_PENDENTE'
    | 'VIGENTE'
    | 'SUBSTITUIDA'
    | 'CANCELADA'
    | 'DESCARTADA'
    | 'CANCELAMENTO_AGENDADO'
    | 'CANCELAMENTO_PENDENTE';
  revisaoPreenchimento: number;
  podeEditarRascunho: boolean;
  propostaOrigemPublicId: string | null;
  valorParcela: number | null;
  iofPercentual: number | null;
  iofValor: number | null;
  dataPrimeiroVencimento: string | null;
  dataProximoVencimento: string | null;
  observacao: string | null;
  moduloNomeRegistrado: string | null;
  subgrupoNomeRegistrado: string | null;
  historico: {
    dataAlteracao: string;
    motivo: string | null;
    observacao: string | null;
    propostaAnteriorPublicId: string | null;
    versaoAnterior: number;
    propostaNovaPublicId: string | null;
    versaoNova: number;
  }[];
  itens: {
    publicId: string;
    produtoNomeAtual: string | null;
    planoNomeAtual: string | null;
    planoNomeRegistrado: string | null;
    valor: number | null;
    ativo: boolean;
    planoContextual: boolean;
    ramo: string | null;
    planoPaga: boolean | null;
    planoReajuste: boolean | null;
  }[];
  coberturas: {
    publicId: string;
    itemPublicId: string | null;
    nomeAtual: string | null;
    nomeRegistrado: string | null;
    premioTitular: number | null;
    premioConjuge: number | null;
    ativo: boolean;
    nomeReduzidoRegistrado: string | null;
    basicaRegistrada: string | null;
    reajusteRegistrado: boolean | null;
    podeAjustarPremio: boolean;
    premioAjustado: boolean;
    premioTitularInicial: number | null;
    premioConjugeInicial: number | null;
    justificativaAjustePremio: string | null;
  }[];
  beneficiarios: {
    publicId: string;
    podeAlterar: boolean;
    nome: string | null;
    parentesco: string | null;
    percentualParticipacao: number | null;
  }[];
}

export interface PreencherRascunhoDados {
  revisaoEsperada: number;
  observacao: string | null;
  beneficiarios: {
    publicId: string | null;
    nome: string;
    parentescoRegistrado: string;
    percentualParticipacao: number | null;
  }[];
  coberturas: {
    publicId: string;
    premioTitular: number | null;
    premioConjuge: number | null;
    justificativaAjustePremio: string;
  }[];
}

export interface PropostasQuery {
  pagina: number;
  tamanhoPagina: number;
  termoBusca?: string;
  apolicePublicId?: string;
  apoliceVidaPublicId?: string;
}

export interface ConfirmacaoPropostaResultado {
  vigente: boolean;
  agendada: boolean;
  revisaoPreenchimento: number;
}
export interface EfetivacaoVigenciaResultado {
  ativada: boolean;
  jaEfetivada: boolean;
  revisaoPreenchimento: number;
  pendenciaVigencia: string | null;
}

export interface GestaoAgendamentoResultado {
  revisaoPreenchimento: number;
}

export interface CancelamentoVigenciaResultado {
  cancelada: boolean;
  agendada: boolean;
  revisaoPreenchimento: number;
  pendenciaVigencia: string | null;
}
