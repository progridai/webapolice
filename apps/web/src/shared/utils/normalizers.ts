/**
 * Normaliza textos realizando trim e remoção de espaços duplos.
 */
export function normalizeText(value?: string | null): string {
  if (!value) return '';
  return value.trim().replace(/\s+/g, ' ');
}

/**
 * Retorna apenas os dígitos numéricos, com tamanho máximo de 11.
 */
export function normalizeCpf(value?: string | null): string {
  if (!value) return '';
  return value.replace(/\D/g, '').substring(0, 11);
}

/**
 * Retorna apenas os dígitos numéricos, com tamanho máximo de 14.
 */
export function normalizeCnpj(value?: string | null): string {
  if (!value) return '';
  return value.replace(/\D/g, '').substring(0, 14);
}

/**
 * Retorna apenas os dígitos numéricos, com tamanho máximo de 8.
 */
export function normalizeCep(value?: string | null): string {
  if (!value) return '';
  return value.replace(/\D/g, '').substring(0, 8);
}

/**
 * Retorna apenas os dígitos numéricos, com tamanho máximo de 11.
 */
export function normalizePhone(value?: string | null): string {
  if (!value) return '';
  return value.replace(/\D/g, '').substring(0, 11);
}
