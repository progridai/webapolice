import { z } from 'zod';

export const ramoFormSchema = z.object({
  codigo: z.string().min(1, 'Código é obrigatório').max(50, 'Máximo 50 caracteres'),
  nome: z.string().min(1, 'Nome é obrigatório').max(150, 'Máximo 150 caracteres'),
  descricao: z.string().max(500, 'Máximo 500 caracteres').optional(),
});

export type RamoFormData = z.infer<typeof ramoFormSchema>;
