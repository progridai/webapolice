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

/**
 * Validador puro para CNPJ.
 * Retorna false caso o valor seja vazio, incompleto, sequência idêntica ou matematicamente inválido.
 */
export function isValidCnpj(value?: string | null): boolean {
  if (!value) return false;

  const cnpj = value.replace(/\D/g, '');
  if (cnpj.length !== 14) return false;

  if (/^(\d)\1+$/.test(cnpj)) return false;

  let tamanho = cnpj.length - 2;
  let numeros = cnpj.substring(0, tamanho);
  let digitos = cnpj.substring(tamanho);
  let soma = 0;
  let pos = tamanho - 7;
  
  for (let i = tamanho; i >= 1; i--) {
    soma += parseInt(numeros.charAt(tamanho - i)) * pos--;
    if (pos < 2) pos = 9;
  }
  
  let resultado = soma % 11 < 2 ? 0 : 11 - (soma % 11);
  if (resultado !== parseInt(digitos.charAt(0))) return false;
  
  tamanho = tamanho + 1;
  numeros = cnpj.substring(0, tamanho);
  soma = 0;
  pos = tamanho - 7;
  
  for (let i = tamanho; i >= 1; i--) {
    soma += parseInt(numeros.charAt(tamanho - i)) * pos--;
    if (pos < 2) pos = 9;
  }
  
  resultado = soma % 11 < 2 ? 0 : 11 - (soma % 11);
  return resultado === parseInt(digitos.charAt(1));
}
