export interface RamoSimulacao {
  publicId: string;
  nome: string;
  codigo: string;
  iofPercentual: number | null;
}
export interface SimulacaoMensalResultado {
  revisaoPreenchimento: number;
  incluiConjuge: boolean;
  ramo: RamoSimulacao;
  calculadaEm: string;
  coberturas: {
    publicId: string;
    nome: string | null;
    premioAjustado: boolean;
    premioTitular: string;
    premioConjuge: string;
    liquido: string;
  }[];
  liquidoTitular: string;
  liquidoConjuge: string;
  liquidoTotal: string;
  iof: string;
  totalMensal: string;
}
