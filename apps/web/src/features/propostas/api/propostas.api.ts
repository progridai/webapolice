import type { FinanceiroConfirmacaoDados } from '../types/financeiro.types';
import { httpClient } from '../../../services/http/httpClient';
import type { PagedResult } from '../../../shared/types/pagination';
import type {
  CancelamentoVigenciaResultado,
  GestaoAgendamentoResultado,
  ConfirmacaoPropostaResultado,
  EfetivacaoVigenciaResultado,
  PreencherRascunhoDados,
  Proposta,
  PropostaDetalhe,
  PropostasQuery,
} from '../types/proposta.types';

export async function salvarRascunho(
  publicId: string,
  dados: PreencherRascunhoDados,
): Promise<void> {
  await httpClient.put(`/api/propostas/${publicId}/rascunho`, dados);
}

export async function listarPropostas(
  query: PropostasQuery,
  signal: AbortSignal,
): Promise<PagedResult<Proposta>> {
  const params = new URLSearchParams({
    pagina: String(query.pagina),
    tamanhoPagina: String(query.tamanhoPagina),
  });
  if (query.clientePublicId)
    params.set('clientePublicId', query.clientePublicId);
  if (query.clienteVinculoPublicId)
    params.set('clienteVinculoPublicId', query.clienteVinculoPublicId);
  if (query.termoBusca) params.set('termoBusca', query.termoBusca);
  if (query.apolicePublicId)
    params.set('apolicePublicId', query.apolicePublicId);
  if (query.apoliceVidaPublicId)
    params.set('apoliceVidaPublicId', query.apoliceVidaPublicId);
  return (
    await httpClient.get<PagedResult<Proposta>>(`/api/propostas?${params}`, {
      signal,
    })
  ).data;
}

export async function obterProposta(
  publicId: string,
  signal: AbortSignal,
): Promise<PropostaDetalhe> {
  return (
    await httpClient.get<PropostaDetalhe>(`/api/propostas/${publicId}`, {
      signal,
    })
  ).data;
}

export async function criarPropostaDaVida(
  apolicePublicId: string,
  vidaPublicId: string,
  propostaOrigemPublicId?: string,
): Promise<string> {
  const response = await httpClient.post<{ publicId: string }>(
    `/api/apolices/${apolicePublicId}/vidas/${vidaPublicId}/propostas`,
    { propostaOrigemPublicId: propostaOrigemPublicId ?? null },
  );
  return response.data.publicId;
}

export async function prepararReinclusao(
  publicId: string,
  revisaoEsperada: number,
  origemCondicoes: 'ANTERIOR' | 'MODULO',
): Promise<string> {
  return (
    await httpClient.post<{ publicId: string }>(
      `/api/propostas/${publicId}/reinclusao`,
      { revisaoEsperada, origemCondicoes },
    )
  ).data.publicId;
}

export async function confirmarProposta(
  publicId: string,
  revisaoEsperada: number,
  dataInicioVigencia: string,
  financeiro?: FinanceiroConfirmacaoDados,
): Promise<ConfirmacaoPropostaResultado> {
  return (
    await httpClient.post<ConfirmacaoPropostaResultado>(
      `/api/propostas/${publicId}/confirmacao`,
      {
        revisaoEsperada,
        dataInicioVigencia,
        ...(financeiro ? { financeiro } : {}),
      },
    )
  ).data;
}
export async function efetivarVigencia(
  publicId: string,
  revisaoEsperada: number,
): Promise<EfetivacaoVigenciaResultado> {
  return (
    await httpClient.post<EfetivacaoVigenciaResultado>(
      `/api/propostas/${publicId}/vigencia/efetivar`,
      { revisaoEsperada },
    )
  ).data;
}

export async function reagendarProposta(
  publicId: string,
  revisaoEsperada: number,
  dataInicioVigencia: string,
  observacao: string,
): Promise<GestaoAgendamentoResultado> {
  return (
    await httpClient.post<GestaoAgendamentoResultado>(
      `/api/propostas/${publicId}/agendamento/reagendar`,
      { revisaoEsperada, dataInicioVigencia, observacao },
    )
  ).data;
}
export async function cancelarAgendamento(
  publicId: string,
  revisaoEsperada: number,
  observacao: string,
): Promise<GestaoAgendamentoResultado> {
  return (
    await httpClient.post<GestaoAgendamentoResultado>(
      `/api/propostas/${publicId}/agendamento/cancelar`,
      { revisaoEsperada, observacao },
    )
  ).data;
}

export async function solicitarCancelamentoVigencia(
  publicId: string,
  revisaoEsperada: number,
  dataCancelamentoVigencia: string,
  observacao: string,
): Promise<CancelamentoVigenciaResultado> {
  return (
    await httpClient.post<CancelamentoVigenciaResultado>(
      `/api/propostas/${publicId}/vigencia/cancelamento`,
      { revisaoEsperada, dataCancelamentoVigencia, observacao },
    )
  ).data;
}
export async function efetivarCancelamentoVigencia(
  publicId: string,
  revisaoEsperada: number,
): Promise<CancelamentoVigenciaResultado> {
  return (
    await httpClient.post<CancelamentoVigenciaResultado>(
      `/api/propostas/${publicId}/vigencia/cancelamento/efetivar`,
      { revisaoEsperada },
    )
  ).data;
}
export async function alterarCancelamentoVigencia(
  publicId: string,
  revisaoEsperada: number,
  dataCancelamentoVigencia: string,
  observacao: string,
): Promise<CancelamentoVigenciaResultado> {
  return (
    await httpClient.post<CancelamentoVigenciaResultado>(
      `/api/propostas/${publicId}/vigencia/cancelamento/alterar`,
      { revisaoEsperada, dataCancelamentoVigencia, observacao },
    )
  ).data;
}
export async function retirarCancelamentoVigencia(
  publicId: string,
  revisaoEsperada: number,
  observacao: string,
): Promise<GestaoAgendamentoResultado> {
  return (
    await httpClient.post<GestaoAgendamentoResultado>(
      `/api/propostas/${publicId}/vigencia/cancelamento/retirar`,
      { revisaoEsperada, observacao },
    )
  ).data;
}
export async function descartarRascunho(
  publicId: string,
  revisaoEsperada: number,
  observacao: string,
): Promise<GestaoAgendamentoResultado> {
  return (
    await httpClient.post<GestaoAgendamentoResultado>(
      `/api/propostas/${publicId}/rascunho/descarte`,
      { revisaoEsperada, observacao },
    )
  ).data;
}
