import { z } from 'zod';

export const subestipulanteApoliceSchema = z.object({
  subestipulantePublicId: z.string().uuid({ message: 'Subestipulante é obrigatório' }),
  dataInicio: z.string().optional().nullable(),
  dataFim: z.string().optional().nullable(),
}).superRefine((data, ctx) => {
  if (data.dataInicio && data.dataFim) {
    const inicio = new Date(data.dataInicio + 'T00:00:00');
    const fim = new Date(data.dataFim + 'T00:00:00');

    if (fim < inicio) {
      ctx.addIssue({
        code: z.ZodIssueCode.custom,
        message: 'A data final não pode ser anterior à data inicial',
        path: ['dataFim'],
      });
    }
  }
});

export type SubestipulanteApoliceFormValues = z.infer<typeof subestipulanteApoliceSchema>;
