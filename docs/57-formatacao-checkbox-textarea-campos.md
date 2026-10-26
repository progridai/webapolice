# Formatação de Checkbox, Textarea e Campos de Entrada

Data: 22/10/2026. Retomada dos lotes de formatação após a [correção do envio de cidade em Cooperados](56-correcao-envio-cidade-cooperados.md).

## Resultado e escopo

Aplicado o Prettier instalado somente a `apps/web/src/components/ui/Checkbox`, `apps/web/src/components/ui/Textarea` e `apps/web/src/components/fields`. Todos os 14 arquivos do escopo passaram no check restrito. A dívida global caiu de 246 para 235 arquivos: 11 pendências removidas e nenhuma nova.

Dez arquivos tiveram mudanças de apresentação no conteúdo e `components/fields/index.ts` teve apenas normalização de fim de linha, sem diferença de conteúdo no diff do Git. Os três arquivos restantes já estavam padronizados.

Preservados contratos, máscaras, valores literais, eventos, referências, seleção indeterminada do checkbox, rótulos, tokens e regras CSS. Não houve mudança funcional, dependência, configuração de checks, regra de negócio, campo persistido ou política de Title Case. As alterações locais anteriores foram preservadas no retrato usado para comparação.

## Arquivos

Caminhos relativos a `apps/web/src/components`:

- Formatados: `ui/Checkbox/Checkbox.tsx`, `ui/Checkbox/Checkbox.css`, `ui/Textarea/Textarea.tsx`, `ui/Textarea/Textarea.css`, `fields/CepInput.tsx`, `fields/CnpjInput.tsx`, `fields/CpfInput.tsx`, `fields/DateInput.tsx`, `fields/EmailInput.tsx` e `fields/PhoneInput.tsx`.
- Fim de linha normalizado: `fields/index.ts`.
- Conferidos e já padronizados: `ui/Checkbox/index.ts`, `ui/Textarea/index.ts` e `fields/MaskedInputs.test.tsx`.
- Documentação: este registro e continuidade nos documentos 29 e 56.

## Validações

| Verificação | Resultado |
|---|---|
| Prettier restrito aos três diretórios | Aprovado nos 14 arquivos |
| Comparação estrutural antes/depois | Estruturas equivalentes nos 14 arquivos |
| `npm.cmd run format:check` global | Reprovado: 235 arquivos pendentes; antes, 246; 11 retirados e zero novos |
| `npm.cmd run test:run` | 1.125 aprovados, zero falhas, 74 arquivos |
| `npm.cmd run typecheck` | Aprovado: zero erros, saída 0 |
| `npm.cmd run build` | Aprovado, saída 0 |
| `npm.cmd run lint` | Aprovado: zero erros e avisos |
| `npm.cmd run lint:design-system` | Aprovado: zero ocorrências |
| `git diff --check` | Aprovado |

A comparação auxiliar usou os parsers de TypeScript e PostCSS já instalados, sobre o retrato imediatamente anterior ao lote. Conferiu estrutura e conteúdo literal de TypeScript/TSX, além de ordem, seletores, propriedades, valores e comentários de CSS, normalizando diferenças de representação do formatador e expressões entre parênteses equivalentes. O retrato textual normaliza fins de linha, portanto a alteração exclusiva de fim de linha do `fields/index.ts` foi contabilizada pela comparação dos relatórios de Prettier, não como mudança estrutural.

Os checks e a comparação passaram na primeira execução após a formatação. Não foram criados testes para mudanças de estilo de código. A suíte existente inclui máscaras, eventos, formulários consumidores, temas claro/escuro/sistema, tokens e a regressão de cidade de Cooperados, que permaneceu aprovada. Não foram acrescentadas supressões, casts ou relaxamentos de tipos/validações.

Os checks locais não substituem homologação visual/autenticada em navegador. Backend não alterado; seus testes não foram executados. Relatórios locais ignorados pelo Git: `scratch/lint-etapa29-*`, incluindo retrato anterior, comparação estrutural/de pendências e logs dos checks.

## Próxima etapa

O lote de `FormSection`, `FormGrid`, `FormActions` e `ReadOnlyField` foi concluído no [documento 58](58-formatacao-estrutura-formularios.md), com sete pendências removidas e 228 arquivos restantes no global. Os números acima preservam o retrato deste lote de campos. Próximo lote: `Alert`, `Badge` e `EmptyState`, sem mudanças funcionais. A homologação integrada continua no [plano de estabilização](29-estabilizacao-frontend.md).
