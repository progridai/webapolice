# Estabilização dos Tipos dos Campos e do Seletor Assíncrono

Data: 22/10/2026. Continuação do [lote de Segurança](40-estabilizacao-erros-seguranca.md).

## Resultado

Removidos os seis apontamentos restantes de `no-explicit-any`: quatro spreads de propriedades nos campos de CEP, CNPJ, CPF e telefone, a referência do `AsyncSelect` e seu callback de seleção. O lint global passou de 33 para 27 erros, mantendo quatro avisos, sem novos apontamentos por arquivo/regra.

A execução atual do ESLint não aponta mais `no-explicit-any`. Isso não significa ausência de `any` em todo o repositório: testes antigos ainda possuem supressões dessa regra, registradas nos lotes anteriores. O lint global continua reprovado por outras regras.

Adicionados 31 cenários de regressão. A suíte completa passou com 662 testes em 62 arquivos; TypeScript e build aprovados. Os cinco componentes alterados e os dois testes novos passaram no ESLint sem erros ou avisos.

## Correções

- **Campos com máscara:** as propriedades nativas são entregues ao `IMaskInput` com verificação de tipos, sem `props as any`. O contrato público `InputProps` foi mantido. Seu `value` admite valores além de texto; quando presente, ele é convertido para string antes de chegar ao IMask, que exige texto para essas máscaras. `undefined` continua representando a ausência de valor controlado.
- As máscaras e o modo de retorno formatado foram preservados, incluindo telefone fixo/celular. `inputRef`, callbacks, nome do campo, classes e atributos nativos continuam encaminhados. O callback das máscaras mantém o formato já utilizado, com `target.name` e `target.value`; o adaptador de evento preexistente não foi reescrito neste lote.
- **Seletor assíncrono:** a referência agora usa `SelectInstance<AsyncOptionItem, false>`, exportado pelo `react-select`. A instanciação do seletor declara opção e seleção única, permitindo entregar `onChange` diretamente, sem cast. Os métodos da referência, como `focus` e `clearValue`, ficam disponíveis com seu tipo real.
- Foram preservados carregamento, cache, opções iniciais, seleção, limpeza, blur e encaminhamento do identificador público da opção. Não foram alterados estilos, cores ou tokens; as ocorrências de design system já registradas continuam pendentes.

Os contratos foram consultados nas declarações e implementação das bibliotecas instaladas. Não houve instalação ou atualização de dependências, novas supressões, novos casts para ocultar contratos ou flexibilização de compilação. Não foram criados campos de negócio ou alterados endpoints, banco, permissões e regras de Title Case. As alterações locais anteriores foram preservadas.

## Validações

| Verificação | Resultado |
|---|---|
| Testes direcionados dos campos e seletor | 31 aprovados, zero falhas, dois arquivos |
| `npm.cmd run test:run` | 662 aprovados, zero falhas, 62 arquivos |
| `npm.cmd run typecheck` | Aprovado: zero erros, código de saída 0 |
| `npm.cmd run build` | Aprovado, código de saída 0 |
| ESLint dos cinco componentes e dois testes novos | Zero erros e avisos |
| `npm.cmd run lint` global | 27 erros e 4 avisos; antes, 33 erros e 4 avisos |
| `no-explicit-any` no lint global | Zero ocorrências sinalizadas; antes, seis |
| `npm.cmd run lint:design-system` | Sete ocorrências preexistentes em `AsyncSelect.tsx` e `DesignSystemPage.tsx` |
| `npm.cmd run format:check` | 278 arquivos pendentes, sem alteração da contagem |
| `git diff --check` | Aprovado |

`MaskedInputs.test.tsx` cobre 25 cenários: CEP, CNPJ, CPF, telefone fixo e celular nos quatro estados de tema (claro, escuro, sistema claro/escuro), além de valores numéricos, limpeza do valor controlado, `readOnly` e `disabled`. Verifica máscara exibida, nome/valor do callback, referência para o input real, foco/blur, classes de erro e atributos de acessibilidade.

`AsyncSelect.test.tsx` cobre seis cenários com a biblioteca real: busca/seleção/limpeza e referência tipada nos quatro estados de tema, descarte de resposta de busca antiga e recuperação com nova consulta após falha. Confere carregamento, estado vazio, callback com opção ou `null` e valor público no campo oculto. A primeira execução encontrou uma expectativa incorreta do teste: o `react-select` também passa um callback interno ao carregador. O teste foi ajustado para conferir a consulta enviada, preservando a interface pública baseada em Promise.

A suíte completa inclui os formulários existentes que usam os campos com máscara. Os testes de tema conferem comportamento e atributos no DOM; não substituem homologação visual em navegador ou medição de contraste. Não houve operações no ambiente publicado. Backend não alterado; seus testes não foram executados.

Relatórios locais ignorados pelo Git em `scratch/lint-etapa13-*`, incluindo retrato inicial dos arquivos e comparativo de lint. A formatação ficou restrita aos testes novos; as alterações nos componentes existentes permaneceram locais à tipagem e à passagem de valor.

## Arquivos alterados

Caminhos relativos a `apps/web`:

- `src/components/fields/CepInput.tsx`.
- `src/components/fields/CnpjInput.tsx`.
- `src/components/fields/CpfInput.tsx`.
- `src/components/fields/PhoneInput.tsx`.
- `src/components/ui/Select/AsyncSelect.tsx`.
- `src/components/fields/MaskedInputs.test.tsx` (novo).
- `src/components/ui/Select/AsyncSelect.test.tsx` (novo).

Documentação: este registro e continuidade nos documentos 29 e 40.

## Próxima etapa

Atualização de 22/10/2026: o [lote de efeitos das consultas de Apólices](42-estabilizacao-efeitos-consultas-apolices.md) revisou listagem, Módulos, Subgrupos e Subestipulantes. A suíte passou a 704 testes aprovados, com TypeScript e build aprovados; restam 23 erros e quatro avisos no lint global. Os resultados acima preservam o encerramento do lote de campos e seletor.

Revisar os efeitos e suas dependências, começando pelos hooks de consulta de Apólices. Preservar retry/refetch, filtros, carregamento, erro e cancelamento, incluindo consultas concorrentes e desmontagem. As correções devem resolver o ciclo de vida apontado pelo lint, sem adicionar supressões ou apenas adiar atualizações para silenciar a regra.

Permanecem pendentes outras regras de lint, design system, formatação geral e homologação integrada, conforme o [plano de estabilização](29-estabilizacao-frontend.md).
