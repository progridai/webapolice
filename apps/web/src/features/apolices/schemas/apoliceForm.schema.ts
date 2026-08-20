import { z } from 'zod';

export const apoliceFormSchema = z.object({
  nome: z.string().min(3, 'O Nome deve ter no mínimo 3 caracteres'),
  estipulanteId: z.string().min(1, 'Selecione um Estipulante'),
  seguradoraId: z.string().min(1, 'Selecione uma Seguradora'),
  corretoraId: z.string().optional(),
  dataInicioVigencia: z.string().min(1, 'A Data de Início é obrigatória'),
  dataFimVigencia: z.string().optional().or(z.literal('')),
  dataAniversario: z.string().optional().or(z.literal('')),
  observacao: z.string().max(500, 'A observação deve ter no máximo 500 caracteres').optional(),
}).refine(
  (data) => {
    if (data.dataFimVigencia && data.dataFimVigencia !== '') {
      return new Date(data.dataFimVigencia) >= new Date(data.dataInicioVigencia);
    }
    return true;
  },
  {
    message: 'A Data de Fim não pode ser menor que a Data de Início',
    path: ['dataFimVigencia'],
  }
);

export type ApoliceFormValues = z.infer<typeof apoliceFormSchema>;
