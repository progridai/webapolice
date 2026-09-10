/**
 * seguradoraFormSchema.ts
 *
 * Schema de validação Zod para os formulários de cadastro e edição de Seguradora.
 */
import { z } from 'zod';
import { isValidCnpj } from '../../../shared/utils/validators';

export const seguradoraFormSchema = z.object({
  nome: z
    .string()
    .trim()
    .min(1, 'O nome da seguradora é obrigatório.')
    .max(150, 'O nome deve ter no máximo 150 caracteres.'),
  codigo: z
    .string()
    .trim()
    .max(50, 'O código deve ter no máximo 50 caracteres.')
    .optional()
    .or(z.literal('')),
  susep: z
    .string()
    .trim()
    .max(50, 'O código SUSEP deve ter no máximo 50 caracteres.')
    .optional()
    .or(z.literal('')),
  cnpj: z
    .string()
    .trim()
    .max(30, 'O CNPJ deve ter no máximo 30 caracteres.')
    .refine((val) => {
      if (!val) return true;
      const stripped = val.replace(/\\D/g, '');
      if (stripped === '') return true;
      return isValidCnpj(val);
    }, { message: 'CNPJ inválido.' })
    .optional(),
  observacao: z
    .string()
    .trim()
    .optional()
    .or(z.literal('')),
});

export type SeguradoraFormData = z.infer<typeof seguradoraFormSchema>;
