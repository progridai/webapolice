import { z } from 'zod';

export const moduloFormSchema = z.object({
  nome: z.string()
    .min(1, 'O Nome do Módulo é obrigatório.')
    .max(150, 'O Nome deve ter no máximo 150 caracteres.')
    .trim(),
  descricao: z.string()
    .max(500, 'A Descrição deve ter no máximo 500 caracteres.')
    .optional()
    .transform(val => val?.trim() || undefined),
  ativo: z.boolean().default(true),
});

export type ModuloFormData = z.infer<typeof moduloFormSchema>;
