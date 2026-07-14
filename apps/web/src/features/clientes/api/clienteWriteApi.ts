/**
 * clienteWriteApi.ts
 *
 * Comunicação direta com o backend para os casos de uso de escrita (Cadastro/Edição).
 */
import { httpClient } from '../../../services/http/httpClient';
import type { CadastrarClienteRequest, AlterarClienteRequest } from '../types/cliente.types';

/**
 * Cadastra um novo cliente.
 */
export async function cadastrarCliente(
  request: CadastrarClienteRequest
): Promise<{ id: string }> {
  const response = await httpClient.post<{ publicId: string }>('/api/clientes', request);
  return { id: response.data.publicId };
}

/**
 * Altera um cliente existente.
 */
export async function alterarCliente(
  publicId: string,
  request: AlterarClienteRequest
): Promise<void> {
  await httpClient.put(`/api/clientes/${publicId}`, request);
}

/**
 * Ativa um cliente.
 */
export async function ativarCliente(publicId: string): Promise<void> {
  await httpClient.post(`/api/clientes/${publicId}/ativar`);
}

/**
 * Inativa um cliente.
 */
export async function inativarCliente(publicId: string): Promise<void> {
  await httpClient.post(`/api/clientes/${publicId}/inativar`);
}
