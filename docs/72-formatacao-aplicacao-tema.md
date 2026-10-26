# Formatação dos componentes de aplicação e ThemeContext

Data: 23/10/2026. Continuação da [formatação de autenticação](71-formatacao-autenticacao.md).

Aplicado o Prettier instalado aos seis arquivos de `apps/web/src/components/application` e a `apps/web/src/shared/theme/ThemeContext.tsx`. Os sete arquivos passaram no check restrito. Foram formatados `ErrorBoundary.tsx` e `ThemeContext.tsx`; os componentes de carregamento e os três estilos CSS já estavam padronizados.

Preservados contratos, captura e recuperação de erros, fallback, retorno ao início, mensagens e atributos de acessibilidade. No tema, permaneceram iguais a preferência claro/escuro/sistema, chave e valores de armazenamento, resolução via `matchMedia`, aplicação de `data-theme`, listeners e sua remoção, aliases e exports. A comparação estrutural confirmou equivalência nos sete arquivos; o JavaScript emitido pelo TypeScript instalado permaneceu equivalente nos quatro arquivos TSX. Nenhuma regra CSS foi alterada.

| Verificação | Resultado |
|---|---|
| Prettier restrito ao lote | Aprovado nos sete arquivos |
| Comparação estrutural antes/depois | Aprovada nos sete arquivos |
| `npm.cmd run format:check` global | Reprovado: 178 arquivos pendentes; antes, 180; dois retirados e zero novos |
| `npm.cmd run test:run` | 1.125 aprovados, zero falhas, 74 arquivos; inclui cinco cenários de ThemeContext |
| `npm.cmd run typecheck` | Aprovado: zero erros, saída 0 |
| `npm.cmd run build` | Aprovado, saída 0 |
| `npm.cmd run lint` | Aprovado: zero erros e avisos |
| `npm.cmd run lint:design-system` | Aprovado: zero ocorrências |
| `git diff --check` | Aprovado |

Não foram criados testes para mudanças de formatação; foi executada a suíte existente. A formatação restrita e as comparações passaram na primeira execução após a formatação. Os checks locais não substituem homologação visual e dos fluxos autenticados em navegador.

Não houve mudança funcional, dependência, configuração, contrato de API, campo persistido ou política de Title Case. As alterações locais anteriores foram preservadas. Backend não alterado; seus testes não foram executados. Relatórios locais ignorados pelo Git: `scratch/lint-etapa44-*`, incluindo retrato anterior, comparações, estado inicial do Git e logs dos checks.

Atualizados este registro e a continuidade dos documentos 29 e 71. O lote de `services/http` foi concluído no [registro 73](73-formatacao-cliente-http.md), com seis arquivos formatados e dívida reduzida para 172. Próximo lote: os seis arquivos de `shared/api`, `shared/config`, `shared/hooks` e `shared/utils`. A homologação integrada continua no [plano de estabilização](29-estabilizacao-frontend.md).
