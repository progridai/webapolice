import { describe, expect, it } from 'vitest';
import { normalizeHumanData, toTitleCase } from './titleCase';

describe('Title Case obrigatório', () => {
  it.each([
    ['JOÃO DA SILVA', 'João da Silva'],
    ["mARIA d'ÁVILA-SOUZA", "Maria d'Ávila-Souza"],
    ['  AÇÃO  E SAÚDE\nSÃO PAULO ', '  Ação  e Saúde\nSão Paulo '],
    ['SEGURO DE VIDA E ACIDENTES', 'Seguro de Vida e Acidentes'],
    ['O PLANO PARA A FAMÍLIA', 'o Plano para a Família'],
    ['PELA SAÚDE OU PELO BEM', 'pela Saúde ou pelo Bem'],
    ['JOÃO DAS NEVES COM MARIA DOS SANTOS', 'João das Neves com Maria dos Santos'],
    ['AÇÃO ÀS EMPRESAS SEM CUSTO', 'Ação às Empresas sem Custo'],
    ['123abc', '123Abc'], ['', ''],
  ])('normaliza %s e ? idempotente', (input, expected) => {
    expect(toTitleCase(input)).toBe(expected);
    expect(toTitleCase(expected)).toBe(expected);
  });
  it('normaliza listas aninhadas sem alterar códigos, contatos, originais ou o objeto de origem', () => {
    const input = { nome: 'JOÃO', contatos: [{ nome: 'MARIA', email: 'ABC@EXAMPLE.COM' }],
      uf: 'RS', codigo: 'abc-X', status: 'ativa', senha: 'aBcD', nomeOriginal: 'JOÃO',
      nomeNormalizado: 'JOAO', observacao: null, quantidade: 2, nome_fantasia: 'EMPRESA LTDA' };
    expect(normalizeHumanData(input)).toEqual({ ...input, nome: 'João',
      contatos: [{ nome: 'Maria', email: 'ABC@EXAMPLE.COM' }], nome_fantasia: 'Empresa Ltda' });
    expect(input.nome).toBe('JOÃO');
  });
});
