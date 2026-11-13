import { z } from 'zod';
import { justificativaPropostaSchema } from './justificativaProposta.schema';

export const encerramentoPropostaSchema = z
  .object({
    acao: z.enum(['cancelarVigencia', 'descartar']),
    dataCancelamentoVigencia: z.string(),
    observacao: justificativaPropostaSchema,
  })
  .superRefine((dados, ctx) => {
    if (
      dados.acao === 'cancelarVigencia' &&
      !z.iso.date().safeParse(dados.dataCancelamentoVigencia).success
    ) {
      ctx.addIssue({
        code: 'custom',
        path: ['dataCancelamentoVigencia'],
        message: 'Informe uma data de cancelamento válida.',
      });
    }
  });
export type EncerramentoPropostaFormData = z.infer<
  typeof encerramentoPropostaSchema
>;
