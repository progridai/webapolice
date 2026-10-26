# Formatação de API, configuração, hooks e utilitários compartilhados

Data: 23/10/2026. Continuação da [formatação do cliente HTTP](73-formatacao-cliente-http.md).

Aplicado o Prettier instalado aos seis arquivos de `shared/api`, `shared/config`, `shared/hooks` e `shared/utils`, dentro de `apps/web/src`. Foram formatados `api/enderecosApi.ts`, `utils/formatters.ts`, `utils/normalizers.ts` e `utils/validators.ts`; em `normalizers.ts`, a única alteração foi a padronização dos finais de linha para LF. `config/app-config.ts` e `hooks/useConsulta.ts` já estavam padronizados.

Preservados contratos da consulta de CEP, URL e cancelamento, configuração da aplicação, ciclo de consultas e nova tentativa, descarte de respostas canceladas, normalização de documentos e contatos, validações e máscaras, datas e valores monetários em pt-BR. A comparação estrutural e a comparação do JavaScript emitido pelo TypeScript instalado confirmaram equivalência nos seis arquivos, incluindo literais, expressões regulares e operações matemáticas.

| Verificação | Resultado |
|---|---|
| Prettier restrito aos quatro diretórios | Aprovado nos seis arquivos |
| Comparação estrutural e do JavaScript emitido | Aprovada nos seis arquivos |
| `npm.cmd run format:check` global | Reprovado: 168 arquivos pendentes; antes, 172; quatro retirados e zero novos |
| `npm.cmd run test:run` | 1.125 aprovados, zero falhas, 74 arquivos |
| `npm.cmd run typecheck` | Aprovado: zero erros, saída 0 |
| `npm.cmd run build` | Aprovado, saída 0 |
| `npm.cmd run lint` | Aprovado: zero erros e avisos |
| `npm.cmd run lint:design-system` | Aprovado: zero ocorrências |
| `git diff --check` | Aprovado |

Não foram criados testes para mudanças de formatação; foi executada a suíte existente. A formatação restrita e as comparações passaram na primeira execução após a formatação. Os checks locais não substituem homologação visual e dos fluxos autenticados em navegador.

Não houve mudança funcional, dependência, configuração, contrato de API, campo persistido ou política de Title Case. As alterações locais anteriores foram preservadas, inclusive as correções anteriores dos validadores. Backend não alterado; seus testes não foram executados. Relatórios locais ignorados pelo Git: `scratch/lint-etapa46-*`, incluindo retrato anterior, comparações, estado inicial do Git e logs dos checks.

Atualizados este registro e a continuidade dos documentos 29 e 73. As páginas `Login`, `Unauthorized` e `Forbidden` e todas as demais pendências foram concluídas na [etapa consolidada 75](75-conclusao-formatacao-frontend.md), por solicitação do usuário. O Prettier global passou com zero pendências; não restam lotes de formatação nesta sequência. A homologação integrada continua no [plano de estabilização](29-estabilizacao-frontend.md).
