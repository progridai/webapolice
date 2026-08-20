/**
 * seguradoraFormSchema.ts
 *
 * Schema de validação Zod para os formulários de cadastro e edição de Seguradora.
 */
import { z } from 'zod';

export const seguradoraFormSchema = z.object({
  nome: z
    .string()
    .min(1, 'O nome da seguradora é obrigatório.')
    .max(150, 'O nome deve ter no máximo 150 caracteres.'),
  codigo: z
    .string()
    .max(50, 'O código deve ter no máximo 50 caracteres.')
    .optional()
    .or(z.literal('')),
  susep: z
    .string()
    .max(50, 'O código SUSEP deve ter no máximo 50 caracteres.')
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

export type SeguradoraFormData = z.infer<typeof seguradoraFormSchema>;
