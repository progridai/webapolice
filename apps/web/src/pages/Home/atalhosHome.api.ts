import { httpClient } from '../../services/http';

export interface PreferenciasHome {
  atalhosHome: string[] | null;
}

const endpoint = '/api/seguranca/me/atalhos-home';

export async function obterAtalhosHome(signal: AbortSignal) {
  const response = await httpClient.get<PreferenciasHome>(endpoint, { signal });
  return response.data;
}

export async function salvarAtalhosHome(
  atalhosHome: string[],
  signal: AbortSignal,
) {
  const response = await httpClient.put<PreferenciasHome>(
    endpoint,
    { atalhosHome },
    { signal },
  );
  return response.data;
}
