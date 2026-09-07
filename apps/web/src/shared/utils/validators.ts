/**
 * Validador puro para CPF.
 * Retorna false caso o valor seja vazio, incompleto, sequência idêntica ou matematicamente inválido.
 */
export function isValidCpf(value?: string | null): boolean {
  if (!value) return false;
  
  const cpf = value.replace(/\D/g, '');
  if (cpf.length !== 11) return false;
  
  if (/^(\d)\1+$/.test(cpf)) return false;
  
  let soma = 0;
  for (let i = 0; i < 9; i++) {
    soma += parseInt(cpf.charAt(i)) * (10 - i);
  }
  
  let resto = 11 - (soma % 11);
  let digitoVerificador1 = resto === 10 || resto === 11 ? 0 : resto;
  if (digitoVerificador1 !== parseInt(cpf.charAt(9))) return false;
  
  soma = 0;
  for (let i = 0; i < 10; i++) {
    soma += parseInt(cpf.charAt(i)) * (11 - i);
  }
  
  resto = 11 - (soma % 11);
  let digitoVerificador2 = resto === 10 || resto === 11 ? 0 : resto;
  
  return digitoVerificador2 === parseInt(cpf.charAt(10));
}

/**
 * Validador puro para CEP (8 dígitos).
 * Retorna false caso vazio.
 */
export function isValidCep(value?: string | null): boolean {
  if (!value) return false;
  const cep = value.replace(/\D/g, '');
  return cep.length === 8;
}

/**
 * Validador puro para telefone (Fixo ou Celular).
 * Retorna false caso vazio.
 */
export function isValidPhone(value?: string | null): boolean {
  if (!value) return false;
  const phone = value.replace(/\D/g, '');
  return phone.length === 10 || phone.length === 11;
}
