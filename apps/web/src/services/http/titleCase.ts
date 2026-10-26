/** Política de dados humanos: manter em sincronia com PadronizacaoTexto e docs/26. */
const humanFields = new Set([
  'nome',
  'nomefantasia',
  'razaosocial',
  'nomeconjuge',
  'nomeempresa',
  'nomereduzido',
  'nomeformatado',
  'descricao',
  'observacao',
  'titulo',
  'logradouro',
  'complemento',
  'bairro',
  'departamento',
  'contato',
  'empresa',
  'equipe',
  'funcao',
  'setor',
  'motivo',
  'causa',
  'instituidor',
  'instituto',
  'estnome',
  'estcidade',
  'estendereco',
  'estcomplemento',
  'estbairro',
]);

const lowercaseWords = new Set([
  'a',
  'o',
  'as',
  'os',
  'um',
  'uma',
  'uns',
  'umas',
  'de',
  'da',
  'do',
  'das',
  'dos',
  'em',
  'na',
  'no',
  'nas',
  'nos',
  'por',
  'para',
  'com',
  'sem',
  'sob',
  'sobre',
  'entre',
  'até',
  'ante',
  'após',
  'contra',
  'desde',
  'perante',
  'trás',
  'durante',
  'mediante',
  'e',
  'ou',
  'nem',
  'mas',
  'porém',
  'contudo',
  'todavia',
  'entretanto',
  'pois',
  'porque',
  'portanto',
  'logo',
  'que',
  'se',
  'como',
  'quando',
  'enquanto',
  'embora',
  'caso',
  'conquanto',
  'senão',
  'à',
  'às',
  'ao',
  'aos',
  'pelo',
  'pela',
  'pelos',
  'pelas',
  'num',
  'numa',
  'nuns',
  'numas',
  'd',
]);

export function toTitleCase(value: string): string {
  return value
    .toLocaleLowerCase('pt-BR')
    .replace(/\p{L}[\p{L}\p{M}]*/gu, (word) =>
      lowercaseWords.has(word)
        ? word
        : word[0].toLocaleUpperCase('pt-BR') + word.slice(1),
    );
}

/** Produz uma cópia; preserva campos técnicos, null, números e estruturas aninhadas. */
export function normalizeHumanData(value: unknown, field = ''): unknown {
  if (typeof value === 'string') {
    return humanFields.has(field.replaceAll('_', '').toLowerCase())
      ? toTitleCase(value)
      : value;
  }
  if (Array.isArray(value))
    return value.map((item) => normalizeHumanData(item, field));
  if (
    value !== null &&
    typeof value === 'object' &&
    Object.getPrototypeOf(value) === Object.prototype
  ) {
    return Object.fromEntries(
      Object.entries(value).map(([key, item]) => [
        key,
        normalizeHumanData(item, key),
      ]),
    );
  }
  return value;
}
