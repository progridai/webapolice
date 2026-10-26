# Formatação dos Componentes Básicos de Formulário

Data: 22/10/2026. Segundo lote de formatação, após [tokens e identidade](54-formatacao-tokens-identidade.md).

## Resultado e escopo

Aplicado o Prettier instalado aos diretórios `Button`, `Input`, `Select` e `FormField`, dentro de `apps/web/src/components/ui`. Oito dos 14 arquivos do escopo precisavam de formatação. Os 14 passaram no check restrito; a dívida global caiu de 254 para 246 arquivos, sem novos apontamentos de formatação.

Mudanças restritas à apresentação do código: indentação, espaçamento, quebras de linha e pontuação de formatação. Preservados contratos, valores literais, tokens, regras CSS, callbacks, referências, mensagens e atributos de acessibilidade. Não foram alteradas dependências, configurações ou regras de negócio. As alterações locais anteriores foram preservadas no retrato usado para comparação.

## Arquivos

Caminhos relativos a `apps/web/src/components/ui`:

- Formatados: `Button/Button.tsx`, `Input/Input.tsx`, `Input/Input.css`, `Select/Select.tsx`, `Select/AsyncSelect.tsx`, `Select/Select.css`, `FormField/FormField.tsx` e `FormField/FormField.css`.
- Conferidos e já padronizados: os quatro `index.ts`, `Button/Button.css` e `Select/AsyncSelect.test.tsx`.
- Documentação: este registro e continuidade nos documentos 29 e 54.

## Validações

| Verificação | Resultado |
|---|---|
| Prettier restrito aos quatro diretórios | Aprovado nos 14 arquivos |
| Comparação estrutural antes/depois | Estruturas equivalentes nos 14 arquivos |
| `npm.cmd run format:check` global | Reprovado: 246 arquivos pendentes; antes, 254; oito retirados e zero novos |
| Primeira execução de `npm.cmd run test:run` | 1.110 aprovados e uma falha em Cooperados |
| Reexecução isolada de `CooperadoCidade.test.tsx` | 24 aprovados, zero falhas |
| Reexecução completa de `npm.cmd run test:run` | 1.111 aprovados, zero falhas, 74 arquivos |
| `npm.cmd run typecheck` | Aprovado: zero erros, saída 0 |
| `npm.cmd run build` | Aprovado, saída 0 |
| `npm.cmd run lint` | Aprovado: zero erros e avisos |
| `npm.cmd run lint:design-system` | Aprovado: zero ocorrências |
| `git diff --check` | Aprovado |

A comparação auxiliar usou TypeScript e PostCSS já instalados. A primeira execução sinalizou a remoção de parênteses redundantes nos condicionais de estilo do `AsyncSelect`. O verificador foi ajustado para comparar as árvores das expressões sem esse invólucro, preservando operadores e operandos. A comparação final confirmou a equivalência de estrutura e conteúdo literal em TypeScript/TSX, além de ordem, seletores, propriedades, valores e comentários de CSS, com normalização de representação do formatador.

Não foram criados testes para a formatação. A suíte existente cobre componentes e consumidores, campos condicionais, máscaras, acessibilidade de formulários, temas e tokens. Os checks locais não substituem homologação visual/autenticada. Backend, campos persistidos e a política de Title Case não foram alterados; testes do backend não foram executados.

## Falha intermitente observada e continuidade

Na primeira execução completa, o cenário “mantém o estado vazio sem cidade inexistente quando a UF não retorna opções”, de `tests/forms/CooperadoCidade.test.tsx`, recebeu um envio contendo `cidadeId` quando esperava sua ausência. A execução isolada dos 24 cenários de cidade e a nova execução completa dos 1.111 testes passaram, sem alteração adicional de código ou teste. A origem da intermitência não foi determinada neste lote de formatação.

A investigação foi concluída no [documento 56](56-correcao-envio-cidade-cooperados.md): o envio podia capturar o ID antes do efeito de limpeza. A regressão foi reproduzida de forma determinística e corrigida no handler de envio, com 1.125 testes aprovados. Os números acima preservam o retrato deste lote de formatação. Retomar a formatação por `Checkbox`, `Textarea` e `components/fields`. A homologação integrada permanece no [plano de estabilização](29-estabilizacao-frontend.md).

Relatórios locais ignorados pelo Git: `scratch/lint-etapa27-*`, incluindo retrato anterior, comparação estrutural/de pendências, primeira execução completa com falha, reexecução isolada e suíte final aprovada.
