export interface OpcaoCondicoes {
  publicId: string;
  nome: string;
}
export interface OpcoesCondicoes {
  modulos: OpcaoCondicoes[];
  subgrupos: OpcaoCondicoes[];
  moduloPublicId: string | null;
  subgrupoPublicId: string | null;
}
export interface SelecaoCondicoes {
  revisaoEsperada: number;
  moduloPublicId: string;
  subgrupoPublicId: string | null;
}
export interface CondicoesRegistradas {
  moduloPublicId: string | null;
  moduloNomeRegistrado: string | null;
  subgrupoPublicId: string | null;
  subgrupoNomeRegistrado: string | null;
  plano: {
    publicId: string | null;
    nome: string | null;
    ramo: string | null;
    paga: boolean | null;
    reajuste: boolean | null;
  } | null;
  coberturas: {
    publicId: string | null;
    vinculoPublicId: string | null;
    nome: string | null;
    nomeReduzido: string | null;
    basica: string | null;
    reajuste: boolean | null;
    premioTitular: number | null;
    premioConjuge: number | null;
    premioAjustado: boolean;
    premioTitularInicial: number | null;
    premioConjugeInicial: number | null;
    justificativaAjustePremio: string | null;
  }[];
}
export interface ComparacaoCondicoes {
  comparacao: string;
  revisaoPreenchimento: number;
  atual: CondicoesRegistradas;
  modulo: CondicoesRegistradas;
  mantendoAjustes: CondicoesRegistradas;
  alteracaoModulo: boolean;
  alteracaoMantendoAjustes: boolean;
}
export interface AplicarCondicoesDados extends SelecaoCondicoes {
  comparacao: string;
  tratamentoAjustes: 'MANTER' | 'MODULO';
  observacao: string;
}
