import { z } from 'zod';

export const apoliceVidaSchema = z.object({
  clientePublicId: z.string().min(1, 'Cliente é obrigatório'),
  contexto: z.enum(['direto', 'subestipulante', 'modulo'], {
    required_error: 'Selecione o contexto',
  }),
  subestipulantePublicId: z.string().nullable().optional(),
  moduloPublicId: z.string().nullable().optional(),
  dataInicioVigencia: z.string().nullable().optional(),
  dataFimVigencia: z.string().nullable().optional(),
  observacao: z.string().max(500, 'Máximo 500 caracteres').nullable().optional(),
}).refine((data) => {
  if (data.contexto === 'subestipulante' || data.contexto === 'modulo') {
    return !!data.subestipulantePublicId;
  }
  return true;
}, {
  message: 'Subestipulante é obrigatório para este contexto',
  path: ['subestipulantePublicId']
}).refine((data) => {
  if (data.contexto === 'modulo') {
    return !!data.moduloPublicId;
  }
  return true;
}, {
  message: 'Módulo é obrigatório para este contexto',
  path: ['moduloPublicId']
}).refine((data) => {
  if (data.dataInicioVigencia && data.dataFimVigencia) {
    return new Date(data.dataFimVigencia) >= new Date(data.dataInicioVigencia);
  }
  return true;
}, {
  message: 'Data Final deve ser maior ou igual à Data Inicial',
  path: ['dataFimVigencia']
});

export type ApoliceVidaFormValues = z.infer<typeof apoliceVidaSchema>;
