import { httpClient } from '../../../services/http';
import type {
  RamoSimulacao,
  SimulacaoMensalResultado,
} from '../types/simulacaoMensal.types';

export async function obterRamosSimulacao(id: string, signal: AbortSignal) {
  return (
    await httpClient.get<RamoSimulacao[]>(
      `/api/propostas/${id}/simulacao-mensal/opcoes`,
      { signal },
    )
  ).data;
}
export async function simularValorMensal(
  id: string,
  revisaoEsperada: number,
  ramoPublicId: string,
  signal: AbortSignal,
) {
  return (
    await httpClient.post<SimulacaoMensalResultado>(
      `/api/propostas/${id}/simulacao-mensal`,
      { revisaoEsperada, ramoPublicId },
      { signal },
    )
  ).data;
}
