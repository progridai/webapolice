import type { SimulacaoMensalResultado } from './simulacaoMensal.types';
export interface FinanceiroConfirmacaoDados {
  competenciaInicial: number;
  vencimentoInicial: string;
  ramoPublicId: string;
  conferencia: string;
}
export interface ConferenciaFinanceiraDados {
  revisaoEsperada: number;
  dataInicioVigencia: string;
  competenciaInicial: number;
  vencimentoInicial: string;
  ramoPublicId: string;
}
export interface ConferenciaFinanceiraResultado {
  conferencia: string;
  competenciaInicial: number;
  vencimentoInicial: string;
  competenciaReferencia: number;
  vencimentoReferencia: string;
  valores: SimulacaoMensalResultado;
}
export interface FinanceiroEstado {
  ramoPublicId: string | null;
  competenciaInicial: number | null;
  vencimentoInicial: string | null;
  competenciaReferencia: number | null;
  vencimentoReferencia: string | null;
  premioLiquido: string | null;
  iofPercentual: number | null;
  iof: string | null;
  totalMensal: string | null;
  conferenciaAtual: boolean;
}
export interface ParcelaLote {
  publicId: string;
  parcela: number;
  competencia: number;
  vencimento: string;
  total: string;
  aberto: string;
  revisaoBaixa: number;
  dataPagamento: string | null;
}
export interface VidaLote {
  clienteNome: string | null;
  matricula: string | null;
  vidaPublicId: string | null;
  propostaPublicId: string;
  numero: string | null;
  versao: number;
  situacao: 'INCLUIR' | 'EXISTENTE' | 'EXCLUIDA' | 'PENDENTE';
  motivo: string | null;
  novaMensalidade: string;
  saldoAnterior: string;
  totalAcumulado: string;
  parcelas: ParcelaLote[];
}
export interface LoteMensal {
  apolicePublicId: string;
  competencia: number;
  conferencia: string;
  geradaEm: string | null;
  fechadaEm: string | null;
  podeGerar: boolean;
  podeFechar: boolean;
  impedimentos: string[];
  vidas: VidaLote[];
}
