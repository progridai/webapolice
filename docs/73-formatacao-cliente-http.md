# Formatação de services/http

Data: 23/10/2026. Continuação da [formatação dos componentes de aplicação e tema](72-formatacao-aplicacao-tema.md).

Aplicado o Prettier instalado aos sete arquivos de `apps/web/src/services/http`. Foram formatados `http.types.ts`, `httpClient.ts`, `httpError.ts`, `index.ts`, `titleCase.ts` e `titleCase.test.ts`; no arquivo de tipos, a única alteração foi a padronização dos finais de linha para LF. `apiConfig.ts` já estava padronizado.

Preservados contratos, métodos HTTP, composição de URLs e parâmetros, headers, obtenção do token, timeout, cancelamento, serialização, resposta 204 e normalização de erros. A comparação estrutural e a comparação do JavaScript emitido pelo TypeScript instalado confirmaram equivalência nos sete arquivos, incluindo literais, expressões regulares e listas de classificação textual.

As listas de campos humanos e palavras em minúsculas, a cultura pt-BR, o tratamento recursivo e a preservação de campos técnicos permaneceram iguais. A formatação não introduziu mudança na política compartilhada de Title Case nem exigiu alteração das listas equivalentes em C# e SQL.

| Verificação | Resultado |
|---|---|
| Prettier de todo `services/http` | Aprovado nos sete arquivos |
| Comparação estrutural e do JavaScript emitido | Aprovada nos sete arquivos |
| `npm.cmd run format:check` global | Reprovado: 172 arquivos pendentes; antes, 178; seis retirados e zero novos |
| `npm.cmd run test:run` | 1.125 aprovados, zero falhas, 74 arquivos; inclui 11 cenários de Title Case |
| `npm.cmd run typecheck` | Aprovado: zero erros, saída 0 |
| `npm.cmd run build` | Aprovado, saída 0 |
| `npm.cmd run lint` | Aprovado: zero erros e avisos |
| `npm.cmd run lint:design-system` | Aprovado: zero ocorrências |
| `git diff --check` | Aprovado |

Não foram criados testes para mudanças de formatação; foi formatado o teste existente e executada a suíte completa do frontend. A formatação restrita e as comparações passaram na primeira execução após a formatação. Os checks locais não substituem homologação dos fluxos autenticados em navegador.

Não houve mudança funcional, dependência, configuração, contrato de API ou campo persistido. As alterações locais anteriores foram preservadas. Backend não alterado; seus testes não foram executados. Relatórios locais ignorados pelo Git: `scratch/lint-etapa45-*`, incluindo retrato anterior, comparações, estado inicial do Git e logs dos checks.

Atualizados este registro e a continuidade dos documentos 29 e 72. O lote de API, configuração, hooks e utilitários compartilhados foi concluído no [registro 74](74-formatacao-utilitarios-compartilhados.md), com quatro arquivos formatados e dívida reduzida para 168. Próximo lote: os sete arquivos das páginas `Login`, `Unauthorized` e `Forbidden`. A homologação integrada continua no [plano de estabilização](29-estabilizacao-frontend.md).
