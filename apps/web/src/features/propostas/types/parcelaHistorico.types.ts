export interface ParcelaHistorico {
  posicao: number;
  publicId: string | null;
  propostaPublicId: string;
  propostaNumero: string | null;
  versao: number;
  parcela: number | null;
  competenciaInt: number | null;
  ano: number | null;
  mes: number | null;
  dataVencimento: string | null;
  dataLancamento: string | null;
  dataPagamento: string | null;
  classificacao: string;
  legado: boolean;
  premioLiquido: string | null;
  iof: string | null;
  premioTotal: string | null;
  valorPago: string | null;
  observacao: string | null;
}
