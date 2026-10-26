# Formatação de Tokens e Identidade Visual

Data: 22/10/2026. Primeiro lote de formatação, após a [estabilização dos tokens do design system](53-estabilizacao-tokens-design-system.md).

## Resultado e escopo

Aplicado o Prettier instalado somente a `apps/web/src/styles/tokens` e `apps/web/src/shared/identidade`. Dos 12 arquivos do escopo, oito precisavam de formatação e quatro já estavam padronizados. Todos os 12 passaram no check restrito. A dívida global caiu de 262 para 254 arquivos, sem novos arquivos apontados.

Mudanças restritas à apresentação do código: indentação, quebras de linha, espaçamento, aspas e representação das cores hexadecimais em CSS. Cores e demais valores efetivos, ordem das regras, seletores, contratos, cálculos de contraste, cache e comportamento dos temas foram preservados. Não foram instaladas dependências nem alteradas configurações de formatação, lint ou compilação. As alterações locais anteriores foram preservadas no retrato usado para comparação.

## Arquivos

Caminhos relativos a `apps/web/src`:

- Tokens formatados: `styles/tokens/cores.css`, `espacamentos.css`, `sombras.css`, `temas.css` e `tipografia.css`.
- Identidade formatada: `shared/identidade/IdentidadeVisualContext.tsx`, `IdentidadeVisualService.ts` e `IdentidadeVisualService.spec.ts`.
- Conferidos e já padronizados: `styles/tokens/bordas.css`, `shared/identidade/index.ts`, `marcasDemonstracao.ts` e `types.ts`.
- Documentação: este registro e continuidade nos documentos 29 e 53.

## Validações

| Verificação | Resultado |
|---|---|
| Prettier restrito aos dois diretórios | Aprovado nos 12 arquivos |
| Comparação estrutural antes/depois | Aprovada nos 12 arquivos |
| `npm.cmd run format:check` global | Reprovado: 254 arquivos pendentes; antes, 262; oito retirados e zero novos |
| `npm.cmd run test:run` | 1.111 aprovados, zero falhas, 74 arquivos |
| `npm.cmd run typecheck` | Aprovado: zero erros, saída 0 |
| `npm.cmd run build` | Aprovado, saída 0 |
| `npm.cmd run lint` | Aprovado: zero erros e avisos |
| `npm.cmd run lint:design-system` | Aprovado: zero ocorrências |
| `git diff --check` | Aprovado |

A comparação auxiliar usou os parsers de TypeScript e PostCSS já instalados, sobre o retrato imediatamente anterior a este lote. Conferiu estrutura sintática e conteúdo literal de TypeScript/TSX, além de ordem, seletores, propriedades, valores e comentários de CSS, normalizando as diferenças de representação do formatador. As primeiras execuções desse verificador precisaram de ajustes para excluir o texto bruto da raiz TypeScript, tratar nós sem valor e reconhecer equivalência de caixa hexadecimal, aspas e espaços em funções CSS. A versão final confirmou os 12 arquivos sem exigir mudança funcional na aplicação.

Não foram criados testes novos para a formatação; foi executada a suíte existente, incluindo os cenários de temas claro, escuro e sistema, identidade, contraste e tokens. Os checks locais não substituem homologação visual/autenticada em navegador. Backend, campos persistidos e a política de Title Case não foram alterados; testes do backend não foram executados.

Relatórios locais ignorados pelo Git: `scratch/lint-etapa26-*`, incluindo retrato anterior, lista de estruturas comparadas, comparação de pendências, resultados de testes e logs dos checks.

## Próxima etapa

O lote de `Button`, `Input`, `Select` e `FormField` foi concluído no [documento 55](55-formatacao-componentes-formulario.md), com oito arquivos formatados e 246 pendentes no global. Os números acima preservam o retrato deste primeiro lote. Priorizar agora a investigação da falha intermitente observada no teste de cidade vazia de Cooperados, registrada no documento 55; depois, continuar por `Checkbox`, `Textarea` e `components/fields`. A homologação integrada continua no [plano de estabilização](29-estabilizacao-frontend.md).
