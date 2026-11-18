import { z } from 'zod';

export const selecaoCondicoesSchema = z.object({
  moduloPublicId: z.string().min(1, 'Escolha o Módulo.'),
  subgrupoPublicId: z.string(),
});
export const aplicarCondicoesSchema = z.object({
  tratamentoAjustes: z.enum(['MANTER', 'MODULO'], {
    error: 'Escolha o tratamento dos ajustes.',
  }),
  observacao: z
    .string()
    .trim()
    .min(1, 'Informe a justificativa.')
    .max(1000, 'Use até 1.000 caracteres.'),
});
export type SelecaoCondicoesForm = z.infer<typeof selecaoCondicoesSchema>;
export type AplicarCondicoesForm = z.infer<typeof aplicarCondicoesSchema>;
