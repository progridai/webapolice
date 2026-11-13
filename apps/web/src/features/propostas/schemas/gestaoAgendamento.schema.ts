import { z } from 'zod';
import { justificativaPropostaSchema } from './justificativaProposta.schema';

export const gestaoAgendamentoSchema = z
  .object({
    acao: z.enum(['reagendar', 'cancelar']),
    dataInicioVigencia: z.string(),
    observacao: justificativaPropostaSchema,
  })
  .superRefine((dados, ctx) => {
    if (
      dados.acao === 'reagendar' &&
      !z.iso.date().safeParse(dados.dataInicioVigencia).success
    ) {
      ctx.addIssue({
        code: 'custom',
        path: ['dataInicioVigencia'],
        message: 'Informe uma data de início válida.',
      });
    }
  });
export type GestaoAgendamentoFormData = z.infer<typeof gestaoAgendamentoSchema>;
