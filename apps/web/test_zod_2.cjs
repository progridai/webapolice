const { z } = require('zod');

const enderecoSchema = z.object({
  cep: z.string().optional(),
  logradouro: z.string().optional(),
  numero: z.string().optional(),
  complemento: z.string().optional(),
  bairro: z.string().optional(),
  cidadeId: z.coerce.number().optional(),
  uf: z.string().max(2).optional(),
});

const clienteSchema = z.object({
  endereco: enderecoSchema.optional(),
});

const result = clienteSchema.parse({
  endereco: {}
});

console.log(result.endereco);
console.log(Object.values(result.endereco).some(val => val !== "" && val !== undefined && val !== 0));
