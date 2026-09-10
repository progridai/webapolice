/**
 * subestipulanteFormSchema.ts
 *
 * Schema de validação Zod para os formulários de cadastro e edição de Subestipulante.
 */
import { z } from 'zod';
import { isValidCnpj } from '../../../shared/utils/validators';

export const subestipulanteFormSchema = z.object({
  nome: z
    .string()
    .trim()
    .min(1, 'O nome ou razão social é obrigatório.')
    .max(150, 'O nome deve ter no máximo 150 caracteres.'),
  codigo: z
    .string()
    .trim()
    .max(50, 'O código deve ter no máximo 50 caracteres.')
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

export type SubestipulanteFormData = z.infer<typeof subestipulanteFormSchema>;
