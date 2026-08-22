import { z } from 'zod';

export const moduloSubestipulanteApoliceSchema = z.object({
  moduloPublicId: z.string().min(1, 'O módulo é obrigatório.'),
  dataInicio: z.string().optional().nullable(),
  dataFim: z.string().optional().nullable(),
}).refine((data) => {
  if (data.dataInicio && data.dataFim) {
    const start = new Date(data.dataInicio);
    const end = new Date(data.dataFim);
    return end >= start;
  }
  return true;
}, {
  message: 'A data de fim não pode ser anterior à data de início.',
  path: ['dataFim'],
});

export type ModuloSubestipulanteApoliceFormValues = z.infer<typeof moduloSubestipulanteApoliceSchema>;
