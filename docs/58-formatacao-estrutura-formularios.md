# Formatação da Estrutura dos Formulários

Data: 22/10/2026. Continuação da [formatação de Checkbox, Textarea e campos](57-formatacao-checkbox-textarea-campos.md).

## Resultado e escopo

Aplicado o Prettier instalado aos diretórios `FormSection`, `FormGrid`, `FormActions` e `ReadOnlyField`, dentro de `apps/web/src/components/ui`. Os 12 arquivos do escopo passaram no check restrito. Sete arquivos foram padronizados e a dívida global caiu de 235 para 228 arquivos, sem novos apontamentos.

Preservados contratos, textos, classes, atributos, conteúdo filho, comportamento das ações fixas, campo de leitura, valores dos tokens, seletores e ordem das regras responsivas. Não houve mudança funcional, dependência, configuração de checks, regra de negócio, campo persistido ou política de Title Case. As alterações locais anteriores foram preservadas no retrato imediatamente anterior ao lote.

## Arquivos

Caminhos relativos a `apps/web/src/components/ui`:

- Formatados: `FormSection/FormSection.tsx`, `FormGrid/FormGrid.tsx`, `FormGrid/FormGrid.css`, `FormActions/FormActions.tsx`, `ReadOnlyField/ReadOnlyField.tsx` e `ReadOnlyField/ReadOnlyField.css`.
- Apenas fim de linha normalizado: `FormSection/FormSection.css`.
- Conferidos e já padronizados: os quatro `index.ts` e `FormActions/FormActions.css`.
- Documentação: este registro e continuidade nos documentos 29 e 57.

## Validações

| Verificação | Resultado |
|---|---|
| Prettier restrito aos quatro diretórios | Aprovado nos 12 arquivos |
| Comparação estrutural antes/depois | Estruturas equivalentes nos 12 arquivos |
| `npm.cmd run format:check` global | Reprovado: 228 arquivos pendentes; antes, 235; sete retirados e zero novos |
| `npm.cmd run test:run` | 1.125 aprovados, zero falhas, 74 arquivos |
| `npm.cmd run typecheck` | Aprovado: zero erros, saída 0 |
| `npm.cmd run build` | Aprovado, saída 0 |
| `npm.cmd run lint` | Aprovado: zero erros e avisos |
| `npm.cmd run lint:design-system` | Aprovado: zero ocorrências |
| `git diff --check` | Aprovado |

A comparação auxiliar usou TypeScript e PostCSS já instalados, conferindo estrutura e conteúdo literal do código, além de ordem, seletores, propriedades, valores e comentários de CSS, com normalização das diferenças de representação do formatador. O retrato preservou também os fins de linha, permitindo identificar que a alteração do CSS de `FormSection` se limita a essa normalização. A comparação e os checks restritos passaram na primeira execução.

Não foram criados testes para mudanças de estilo de código. A suíte existente inclui consumidores desses componentes, formulários, temas claro/escuro/sistema, tokens e a regressão de cidade de Cooperados, que permaneceu aprovada. Não foram acrescentadas supressões, casts ou relaxamentos de tipos/validações.

Os checks locais não substituem homologação visual/autenticada em navegador. Backend não alterado; seus testes não foram executados. Relatórios locais ignorados pelo Git: `scratch/lint-etapa30-*`, incluindo retrato, comparação estrutural/de pendências e logs dos checks.

## Próxima etapa

O lote de `Alert`, `Badge` e `EmptyState` foi concluído no [documento 59](59-formatacao-alertas-indicadores.md): nove arquivos conferidos, dois formatados e 226 pendentes no global. Os números acima preservam o retrato deste lote de estrutura. Próximo lote: `Card`, `Spinner` e `Skeleton`, sem mudanças funcionais. A homologação integrada continua no [plano de estabilização](29-estabilizacao-frontend.md).
