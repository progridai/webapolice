import { z } from 'zod';

export const reinclusaoPropostaSchema = z.object({
  origemCondicoes: z.enum(['ANTERIOR', 'MODULO'], {
    error: 'Escolha a origem das condições.',
  }),
});
export type ReinclusaoPropostaFormData = z.infer<
  typeof reinclusaoPropostaSchema
>;
