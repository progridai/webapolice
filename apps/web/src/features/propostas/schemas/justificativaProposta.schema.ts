import { z } from 'zod';

export const justificativaPropostaSchema = z
  .string()
  .trim()
  .min(1, 'Informe a justificativa.')
  .max(1000, 'Use até 1.000 caracteres.');
