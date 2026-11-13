import { z } from 'zod';

export const confirmacaoPropostaSchema = z.object({
  dataInicioVigencia: z.iso.date('Informe uma data de início válida.'),
});
export type ConfirmacaoPropostaFormData = z.infer<
  typeof confirmacaoPropostaSchema
>;
