import { z } from 'zod';

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

const schema = z.object({
  cnpj: z
    .string()
    .trim()
    .max(30, 'O CNPJ deve ter no máximo 30 caracteres.')
    .refine((val) => !val || isValidCnpj(val), { message: 'CNPJ inválido.' })
    .optional()
    .or(z.literal('')),
});

const schema2 = z.object({
  cnpj: z
    .string()
    .trim()
    .max(30, 'O CNPJ deve ter no máximo 30 caracteres.')
    .refine((val) => !val || val === '' || isValidCnpj(val), { message: 'CNPJ inválido.' })
    .optional(),
});

console.log("SCHEMA 1 (or z.literal):");
console.log(schema.safeParse({ cnpj: '12' }));
console.log(schema.safeParse({ cnpj: '61.198.164/0001-60' }));

console.log("\nSCHEMA 2:");
console.log(schema2.safeParse({ cnpj: '12' }));
console.log(schema2.safeParse({ cnpj: '61.198.164/0001-60' }));

console.log("\nEDGE CASES:");
console.log(schema.safeParse({ cnpj: '  .   .   /    -  ' }));
console.log(schema.safeParse({ cnpj: '__.___.___/____-__' }));
console.log(schema.safeParse({ cnpj: '' }));

