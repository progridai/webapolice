# Estabilização das Constantes dos Validadores

Data: 22/10/2026. Continuação da [revisão do carregamento de autorizações](49-estabilizacao-autorizacao.md).

## Resultado

Corrigidos os quatro erros restantes de `prefer-const`. O lint global passou de quatro para zero erros, mantendo os três avisos anteriores e sem novos apontamentos por arquivo/regra/severidade. O comando de lint passou com saída 0; a estabilização geral ainda tem pendências de avisos, design system, formatação e homologação.

As alterações de código se limitam a quatro declarações, trocando `let` por `const`:

- `apps/web/src/shared/utils/validators.ts`: os dois dígitos verificadores de CPF e a string dos dígitos de CNPJ.
- `apps/web/testZod.ts`: a string dos dígitos de CNPJ no script de diagnóstico existente.

Essas variáveis não recebem novos valores. Acumuladores, índices e demais variáveis reatribuídas continuam com `let`. Cálculos, regras de validação, mensagens, schemas e exports foram preservados. A comparação com o retrato anterior confirmou que não houve outra alteração nos dois arquivos.

## Validações

| Verificação | Resultado |
|---|---|
| `npm.cmd run lint` global | Aprovado, saída 0: zero erros e três avisos; antes, quatro erros e três avisos |
| ESLint dos dois arquivos alterados | Zero erros e avisos |
| `npm.cmd run typecheck` | Aprovado: zero erros, saída 0 |
| `npm.cmd run test:run` | 1.065 aprovados, zero falhas, 71 arquivos |
| `npm.cmd run build` | Aprovado, saída 0 |
| `node testZod.ts` | Script existente executado, saída 0 |
| Comparação com o retrato anterior dos arquivos | Somente as quatro substituições de `let` por `const` |
| `npm.cmd run lint:design-system` | Reprovado: sete ocorrências preexistentes em `AsyncSelect.tsx` e `DesignSystemPage.tsx` |
| `npm.cmd run format:check` | Reprovado: 262 arquivos pendentes, mesma contagem anterior |
| `git diff --check` | Aprovado |

Não foram acrescentados testes para as substituições de declaração; foi executada a suíte existente, incluindo os consumidores dos validadores. `testZod.ts` é um script de diagnóstico fora dos diretórios incluídos no typecheck da aplicação (`src` e `tests`): sua execução direta foi conferida separadamente, sem tratá-la como teste automatizado com asserções.

A formatação preexistente dos dois arquivos foi preservada para manter este lote restrito às quatro declarações. Nenhuma dependência, configuração de lint, supressão, contrato de API, campo persistido, regra de Title Case ou apresentação visual foi alterada. A suíte existente de temas também permaneceu aprovada; esta etapa não acrescenta interface. Backend não alterado; seus testes não foram executados. As alterações locais anteriores foram preservadas.

Relatórios locais ignorados pelo Git: `scratch/lint-etapa22-*`, incluindo retrato inicial, comparativo de lint, testes e logs das verificações.

## Documentação e continuidade

Atualizados este registro e os documentos 29 e 49. Os números dos lotes anteriores permanecem como retratos históricos.

A revisão dos três avisos foi concluída no [documento 51](51-estabilizacao-assinaturas-formularios.md), com assinaturas de `useWatch` e zero erros/avisos no lint global. Os números acima preservam o retrato desta etapa de constantes. O próximo passo é corrigir a seleção automática de cidade após CEP em Cooperados, cuja falha preexistente foi reproduzida durante aquela revisão. Design system, formatação geral e homologação integrada permanecem no [plano de estabilização](29-estabilizacao-frontend.md).
