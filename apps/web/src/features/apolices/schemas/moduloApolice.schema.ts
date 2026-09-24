import { z } from 'zod';

// Regra de vigência (dataFim >= dataInicio) replicada do backend:
// CriarModuloApoliceHandler.cs e AlterarModuloApoliceHandler.cs validam
// DataFim < DataInicio com ValidacaoException. Replicado aqui para melhorar UX.
const vigenciaRefinement = (
  data: { dataInicio?: string | null; dataFim?: string | null },
  ctx: z.RefinementCtx
) => {
  if (data.dataInicio && data.dataFim) {
    const inicio = new Date(data.dataInicio + 'T00:00:00');
    const fim = new Date(data.dataFim + 'T00:00:00');
    if (fim < inicio) {
      ctx.addIssue({
        code: z.ZodIssueCode.custom,
        message: 'A data de fim não pode ser anterior à data de início.',
        path: ['dataFim'],
      });
    }
  }
};

// Schema para CRIAR vínculo: moduloPublicId obrigatório
export const criarModuloApoliceSchema = z
  .object({
    // moduloPublicId = publicId do cadastro.modulo (NÃO é o apoliceModuloPublicId)
    moduloPublicId: z.string().uuid({ message: 'Selecione um Módulo.' }),
    dataInicio: z.string().optional().nullable(),
    dataFim: z.string().optional().nullable(),
    observacao: z.string().trim().optional().nullable(),
  })
  .superRefine(vigenciaRefinement);

export type CriarModuloApoliceFormValues = z.infer<typeof criarModuloApoliceSchema>;

// Schema para ALTERAR vínculo: moduloPublicId não faz parte do payload de edição
export const alterarModuloApoliceSchema = z
  .object({
    dataInicio: z.string().optional().nullable(),
    dataFim: z.string().optional().nullable(),
    observacao: z.string().trim().optional().nullable(),
  })
  .superRefine(vigenciaRefinement);

export type AlterarModuloApoliceFormValues = z.infer<typeof alterarModuloApoliceSchema>;
