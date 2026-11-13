import { httpClient } from '../../../services/http/httpClient';
import type { PagedResult } from '../../../shared/types/pagination';
export interface ApoliceVinculoCliente {
  publicId: string;
  nome: string | null;
  numeros: string[];
  ativa: boolean;
  participacaoAtiva: boolean;
}
export interface ClienteVinculo {
  publicId: string;
  matricula: string;
  ativo: boolean;
  estipulantePublicId: string | null;
  estipulante: string | null;
  grupo: string | null;
  subgrupo: string | null;
  lotacao: string | null;
  apolices: ApoliceVinculoCliente[];
}
export interface SalvarClienteVinculo {
  estipulantePublicId: string;
  matricula: string;
  ativo: boolean;
}
export async function listarClienteVinculos(
  clienteId: string,
  pagina: number,
  signal: AbortSignal,
) {
  return (
    await httpClient.get<PagedResult<ClienteVinculo>>(
      `/api/clientes/${clienteId}/vinculos?pagina=${pagina}&tamanhoPagina=10`,
      { signal },
    )
  ).data;
}
export async function salvarClienteVinculo(
  clienteId: string,
  id: string | undefined,
  data: SalvarClienteVinculo,
) {
  const url = `/api/clientes/${clienteId}/vinculos`;
  if (id) await httpClient.put(`${url}/${id}`, data);
  else await httpClient.post(url, data);
}

export async function listarEstipulantesVinculo(
  clienteId: string,
  busca: string,
  pagina: number,
  signal: AbortSignal,
) {
  return (
    await httpClient.get<PagedResult<{ publicId: string; nome: string }>>(
      `/api/clientes/${clienteId}/vinculos/estipulantes-disponiveis?busca=${encodeURIComponent(busca)}&pagina=${pagina}&tamanhoPagina=20`,
      { signal },
    )
  ).data;
}

export async function alterarSituacaoClienteVinculo(
  clienteId: string,
  id: string,
  ativo: boolean,
) {
  await httpClient.patch(`/api/clientes/${clienteId}/vinculos/${id}/situacao`, {
    ativo,
  });
}
