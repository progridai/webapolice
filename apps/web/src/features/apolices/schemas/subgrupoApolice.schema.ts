import { z } from 'zod';

export const subgrupoApoliceSchema = z.object({
  nome: z
    .string()
    .trim()
    .min(1, 'O nome do Subgrupo é obrigatório.')
    .max(200, 'O nome não pode exceder 200 caracteres.'),
  observacao: z
    .string()
    .trim()
    .nullable()
    .optional(),
});

export type SubgrupoApoliceFormValues = z.infer<typeof subgrupoApoliceSchema>;
