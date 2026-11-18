import { httpClient } from '../../../services/http';
import type {
  AplicarCondicoesDados,
  ComparacaoCondicoes,
  OpcoesCondicoes,
  SelecaoCondicoes,
} from '../types/revisaoCondicoes.types';
import type { GestaoAgendamentoResultado } from '../types/proposta.types';

export async function obterOpcoesCondicoes(id: string, signal?: AbortSignal) {
  return (
    await httpClient.get<OpcoesCondicoes>(
      `/api/propostas/${id}/condicoes/opcoes`,
      { signal },
    )
  ).data;
}
export async function compararCondicoes(
  id: string,
  dados: SelecaoCondicoes,
  signal?: AbortSignal,
) {
  return (
    await httpClient.post<ComparacaoCondicoes>(
      `/api/propostas/${id}/condicoes/comparacao`,
      dados,
      { signal },
    )
  ).data;
}
export async function aplicarCondicoes(
  id: string,
  dados: AplicarCondicoesDados,
) {
  return (
    await httpClient.post<GestaoAgendamentoResultado>(
      `/api/propostas/${id}/condicoes/aplicar`,
      dados,
    )
  ).data;
}
