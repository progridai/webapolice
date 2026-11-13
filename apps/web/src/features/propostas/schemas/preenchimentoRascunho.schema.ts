import { z } from 'zod';
import type {
  PropostaDetalhe,
  PreencherRascunhoDados,
} from '../types/proposta.types';

export function numeroPreenchimento(valor: string): number | null {
  return valor.trim() === '' ? null : Number(valor.trim().replace(',', '.'));
}

const decimal = (casas: number, opcional: boolean) =>
  z.string().refine((valor) => {
    if (valor.trim() === '') return opcional;
    const n = numeroPreenchimento(valor);
    if (
      !new RegExp(`^\\d+(?:[.,]\\d{1,${casas}})?$`).test(valor.trim()) ||
      n === null ||
      !Number.isFinite(n) ||
      n < 0 ||
      n > Number.MAX_SAFE_INTEGER / 100
    )
      return false;
    // Recusar valores cuja representação numérica mudaria ao serializar o JSON.
    const canonico = (texto: string) => {
      const [inteira, fracao = ''] = texto.split('.');
      return `${BigInt(inteira)}.${fracao.replace(/0+$/, '')}`;
    };
    return canonico(valor.trim().replace(',', '.')) === canonico(String(n));
  }, `Informe um valor não negativo com até ${casas} casas decimais.`);

export function criarPreenchimentoSchema(proposta: PropostaDetalhe) {
  return z
    .object({
      observacao: z.string(),
      beneficiarios: z.array(
        z.object({
          publicId: z.string().nullable(),
          nome: z.string().trim().min(1, 'Informe o nome.').max(150),
          parentescoRegistrado: z
            .string()
            .trim()
            .min(1, 'Informe o parentesco.')
            .max(100),
          percentualParticipacao: decimal(4, false).refine((v) => {
            const n = numeroPreenchimento(v);
            return n !== null && n > 0 && n <= 100;
          }, 'Informe um percentual maior que zero e até 100%.'),
        }),
      ),
      coberturas: z.array(
        z.object({
          publicId: z.string(),
          premioTitular: decimal(2, true),
          premioConjuge: decimal(2, true),
          justificativaAjustePremio: z.string().trim().max(1000),
        }),
      ),
    })
    .superRefine((dados, ctx) => {
      const protegidos = proposta.beneficiarios
        .filter((b) => !b.podeAlterar)
        .reduce(
          (soma, b) =>
            soma + Math.round((b.percentualParticipacao ?? 0) * 10000),
          0,
        );
      const total = dados.beneficiarios.reduce(
        (soma, b) =>
          soma +
          Math.round(
            (numeroPreenchimento(b.percentualParticipacao) ?? 0) * 10000,
          ),
        protegidos,
      );
      if (total > 1000000)
        ctx.addIssue({
          code: 'custom',
          path: ['beneficiarios'],
          message: 'A soma dos percentuais não pode ultrapassar 100%.',
        });
      dados.coberturas.forEach((c, i) => {
        const original = proposta.coberturas.find(
          (item) => item.publicId === c.publicId,
        );
        if (
          original &&
          (original.premioTitular !== numeroPreenchimento(c.premioTitular) ||
            original.premioConjuge !== numeroPreenchimento(c.premioConjuge)) &&
          !c.justificativaAjustePremio
        )
          ctx.addIssue({
            code: 'custom',
            path: ['coberturas', i, 'justificativaAjustePremio'],
            message: 'Justifique este ajuste de prêmio.',
          });
      });
    });
}

export type PreenchimentoFormData = z.infer<
  ReturnType<typeof criarPreenchimentoSchema>
>;

export function dadosPreenchimento(
  proposta: PropostaDetalhe,
): PreenchimentoFormData {
  return {
    observacao: proposta.observacao ?? '',
    beneficiarios: proposta.beneficiarios
      .filter((b) => b.podeAlterar)
      .map((b) => ({
        publicId: b.publicId,
        nome: b.nome ?? '',
        parentescoRegistrado: b.parentesco ?? '',
        percentualParticipacao: String(b.percentualParticipacao ?? ''),
      })),
    coberturas: proposta.coberturas
      .filter((c) => c.podeAjustarPremio)
      .map((c) => ({
        publicId: c.publicId,
        premioTitular: String(c.premioTitular ?? ''),
        premioConjuge: String(c.premioConjuge ?? ''),
        justificativaAjustePremio: '',
      })),
  };
}

export function payloadPreenchimento(
  dados: PreenchimentoFormData,
  revisao: number,
): PreencherRascunhoDados {
  return {
    revisaoEsperada: revisao,
    observacao: dados.observacao,
    beneficiarios: dados.beneficiarios.map((b) => ({
      ...b,
      percentualParticipacao: numeroPreenchimento(b.percentualParticipacao),
    })),
    coberturas: dados.coberturas.map((c) => ({
      ...c,
      premioTitular: numeroPreenchimento(c.premioTitular),
      premioConjuge: numeroPreenchimento(c.premioConjuge),
    })),
  };
}
