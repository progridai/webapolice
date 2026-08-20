/**
 * corretoraFormSchema.ts
 *
 * Schema de validação Zod para os formulários de cadastro e edição de Corretora.
 */
import { z } from 'zod';

export const corretoraFormSchema = z.object({
  nome: z
    .string()
    .min(1, 'O nome da corretora é obrigatório.')
    .max(150, 'O nome deve ter no máximo 150 caracteres.'),
  codigo: z
    .string()
    .max(50, 'O código deve ter no máximo 50 caracteres.')
    .optional()
    .or(z.literal('')),
  codigoProtheus: z
    .string()
    .max(50, 'O código Protheus deve ter no máximo 50 caracteres.')
    .optional()
    .or(z.literal('')),
  cnpj: z
    .string()
    .max(30, 'O CNPJ deve ter no máximo 30 caracteres.')
    .optional()
    .or(z.literal('')),
  observacao: z
    .string()
    .optional()
    .or(z.literal('')),
});

export type CorretoraFormData = z.infer<typeof corretoraFormSchema>;
