import type { CooperadoFormData } from '../types/cooperados.types';
import { normalizeCpf, normalizeCep, normalizePhone, normalizeText } from '../../../shared/utils/normalizers';

/**
 * Converte o estado do formulário para o Payload DTO esperado pela API,
 * aplicando a normalização defensiva e estrita exigida pelo contrato canônico.
 */
export function toCadastrarCooperadoRequest(data: CooperadoFormData): any {
  const payload: any = { ...data };

  // Remove campos vazios para não falhar bind na API
  Object.keys(payload).forEach(key => {
    if (payload[key] === '' || payload[key] === null || payload[key] === undefined) {
      delete payload[key];
    }
  });

  // Zero IDs to null
  if (payload.cidadeId === 0) delete payload.cidadeId;
  if (payload.bancoId === 0) delete payload.bancoId;
  if (payload.numeroDependentes === 0) delete payload.numeroDependentes;

  // Normalização Estrita (Campos com máscara)
  if (payload.cpf) payload.cpf = normalizeCpf(payload.cpf);
  if (payload.telefone) payload.telefone = normalizePhone(payload.telefone);
  if (payload.cep) payload.cep = normalizeCep(payload.cep);

  // Normalização Textual (trim, remover espaços excessivos)
  if (payload.rg) payload.rg = normalizeText(payload.rg);
  if (payload.orgaoEmissor) payload.orgaoEmissor = normalizeText(payload.orgaoEmissor);
  if (payload.agencia) payload.agencia = normalizeText(payload.agencia);
  if (payload.contaCorrente) payload.contaCorrente = normalizeText(payload.contaCorrente);
  if (payload.observacao) payload.observacao = normalizeText(payload.observacao);
  if (payload.email) payload.email = normalizeText(payload.email);

  return payload;
}
