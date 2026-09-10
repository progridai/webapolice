const { z } = require('zod');

function isValidCnpj(value) {
  if (!value) return false;
  const cnpj = value.replace(/\D/g, '');
  if (cnpj.length !== 14) return false;
  return false; // just mock
}

const schema = z.object({
  cnpj: z
    .string()
    .trim()
    .max(30, 'max')
    .refine((val) => !val || isValidCnpj(val), { message: 'invalid' })
    .optional()
    .or(z.literal('')),
});

console.log(schema.safeParse({ cnpj: '12.345' }));
console.log(schema.safeParse({ cnpj: '' }));
