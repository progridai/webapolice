import { httpClient } from '../../../services/http/httpClient';
import type {
  ConferenciaFinanceiraDados,
  ConferenciaFinanceiraResultado,
  FinanceiroConfirmacaoDados,
  FinanceiroEstado,
  LoteMensal,
} from '../types/financeiro.types';
export async function obterFinanceiro(
  id: string,
  signal: AbortSignal,
): Promise<FinanceiroEstado> {
  return (
    await httpClient.get<FinanceiroEstado>(`/api/propostas/${id}/financeiro`, {
      signal,
    })
  ).data;
}
export async function conferirFinanceiro(
  id: string,
  dados: ConferenciaFinanceiraDados,
  signal: AbortSignal,
): Promise<ConferenciaFinanceiraResultado> {
  return (
    await httpClient.post<ConferenciaFinanceiraResultado>(
      `/api/propostas/${id}/financeiro/conferencia`,
      dados,
      { signal },
    )
  ).data;
}
export async function atualizarFinanceiro(
  id: string,
  revisaoEsperada: number,
  financeiro: FinanceiroConfirmacaoDados,
): Promise<number> {
  return (
    await httpClient.put<number>(`/api/propostas/${id}/financeiro`, {
      revisaoEsperada,
      financeiro,
    })
  ).data;
}
export async function conferirLote(
  id: string,
  competencia: number,
  signal: AbortSignal,
): Promise<LoteMensal> {
  return (
    await httpClient.get<LoteMensal>(
      `/api/apolices/${id}/parcelas?competencia=${competencia}`,
      { signal },
    )
  ).data;
}
export async function gerarLote(
  id: string,
  competencia: number,
  conferencia: string,
): Promise<LoteMensal> {
  return (
    await httpClient.post<LoteMensal>(`/api/apolices/${id}/parcelas/lote`, {
      competencia,
      conferencia,
    })
  ).data;
}
export async function fecharLote(
  id: string,
  competencia: number,
  conferencia: string,
): Promise<LoteMensal> {
  return (
    await httpClient.post<LoteMensal>(
      `/api/apolices/${id}/parcelas/fechamento`,
      { competencia, conferencia },
    )
  ).data;
}
export async function baixarParcela(
  id: string,
  parcela: string,
  revisaoEsperada: number,
  dataPagamento: string,
): Promise<boolean> {
  return (
    await httpClient.post<boolean>(
      `/api/apolices/${id}/parcelas/${parcela}/baixa`,
      { revisaoEsperada, dataPagamento },
    )
  ).data;
}
