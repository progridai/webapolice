# Estabilização do Tratamento de Erros de Apólices

Data: 21/10/2026. Continuação do [primeiro lote de lint](36-estabilizacao-lint-testes-apolices.md).

## Resultado

Removidos os 11 usos de `any` restantes no código de produção de Apólices: seis hooks de consulta e cinco consumidores de API. O lint global passou de 60 para 48 erros e de sete para quatro avisos. As ocorrências sinalizadas de `no-explicit-any` passaram de 32 para 21. Os testes de páginas que já possuíam supressões dessa regra não foram modificados neste lote; essas contagens não representam todos os usos de `any` do repositório.

Foram adicionados 96 cenários de regressão: os 485 testes passaram em 55 arquivos. TypeScript e build continuam aprovados. O lint global permanece reprovado pelas pendências registradas.

## Comportamento corrigido

- **Consultas:** os seis hooks tratam o valor capturado como `unknown`. Instâncias de `Error`, incluindo `HttpApiError`, são preservadas com mensagem e metadados. Valores sem esse contrato, como `null`, `undefined` e objetos brutos, geram um `Error` com mensagem de contingência específica da consulta. Antes, alguns hooks acessavam `err.name` ou `err.message` sem verificar o valor, ou armazenavam qualquer rejeição em um estado declarado como `Error`.
- **Cancelamento:** os cinco hooks que já ignoravam `AbortError` verificam explicitamente `Error` e `DOMException`. A listagem mantém também o reconhecimento de `CanceledError`. Os testes detectaram que `DOMException` não é uma instância de `Error` no jsdom; a guarda foi corrigida antes da validação final.
- **Ações e formulários:** inativação de Ramo, Subgrupo e Subestipulante, além da gravação dos vínculos de Ramo e Subestipulante, usam a mensagem do erro normalizado pelo cliente HTTP. Foram removidos os acessos a `response.data.message/detail`, próprios de outro formato de cliente. `HttpApiError` já herda de `Error`; não é necessário converter novamente a resposta ou perder seus metadados.
- **Falhas sem mensagem:** valores sem contrato de erro e mensagens vazias nas ações usam as mensagens de contingência existentes. Os diálogos e os dados dos formulários permanecem disponíveis para nova tentativa. Callbacks de sucesso, fechamento e atualização não são disparados em uma falha.
- **Limpeza local:** removidos um parâmetro de `catch` não utilizado na consulta de Ramos e três supressões de lint desnecessárias no hook de Histórico. As demais regras permanecem ativas, sem novas supressões.

As mensagens HTTP continuam sendo normalizadas no cliente central, conforme a ADR 010. Não foi criado um normalizador paralelo. Este lote não modifica endpoints, payloads, regras de autorização, estrutura do banco ou campos de negócio. A política de Title Case e a preservação das evidências de auditoria permanecem conforme a documentação existente.

## Testes e validações

| Verificação | Resultado |
|---|---|
| Testes direcionados de erros e Ramos | 102 aprovados, zero falhas |
| `npm.cmd run test:run` | 485 aprovados, zero falhas, 55 arquivos |
| `npm.cmd run typecheck` | Aprovado: zero erros, código de saída 0 |
| `npm.cmd run build` | Aprovado, código de saída 0 |
| `npm.cmd run lint` | 48 erros e 4 avisos; antes, 60 erros e 7 avisos; nenhuma ocorrência nova por arquivo/regra |
| `npm.cmd run lint:design-system` | Sete ocorrências preexistentes em `AsyncSelect.tsx` e `DesignSystemPage.tsx` |
| `npm.cmd run format:check` | 278 arquivos pendentes, sem aumento da dívida de formatação |
| `git diff --check` | Aprovado |

`useApolicesErrors.test.tsx` cobre 36 cenários nos seis hooks: preservação de `HttpApiError` e `Error`, rejeições sem contrato, carregamento, recuperação por retry/refetch ou paginação e cancelamento. A revisão do ciclo de vida dos efeitos e da concorrência entre requisições continua em etapa própria; os testes de cancelamento deste lote verificam a classificação do erro.

`ApoliceVinculosErrors.test.tsx` cobre 60 cenários nas cinco ações: mensagem HTTP, rejeição com `null`, mensagem vazia, conservação dos campos, ausência de callbacks em falha, liberação da ação para nova tentativa, recuperação e identificadores/payloads enviados. Executado com os componentes reais nos temas claro, escuro e sistema, incluindo sistema claro/escuro. Confere alertas semânticos e seleção do tema no DOM; não substitui homologação visual em navegador.

O teste existente de Ramos passou a esperar a mensagem efetiva do erro, em vez do texto genérico anterior. Os demais testes existentes foram preservados. Não foram adicionadas dependências, casts para ocultar contratos ou alterações de configuração. A formatação ficou restrita aos testes do lote e aos três hooks que já estavam conformes antes da alteração; os demais arquivos de produção receberam somente alterações locais no tratamento de erros.

Relatórios locais ignorados pelo Git em `scratch/lint-etapa9-*`. Comparativo de lint com `scratch/lint-etapa8-lint.json`. Validações locais com serviços simulados, sem operações no ambiente publicado. Backend não alterado; seus testes não foram executados.

## Arquivos alterados

Caminhos relativos a `apps/web`:

- Hooks: `src/features/apolices/hooks/useApolices.ts`, `useApoliceDetalhe.ts`, `useApoliceHistorico.ts`, `useApoliceModulos.ts`, `useApoliceSubestipulantes.ts` e `useApoliceSubgrupos.ts`.
- Abas: `src/features/apolices/components/tabs/RamosTab.tsx`, `SubgruposTab.tsx` e `SubestipulantesTab.tsx`.
- Formulários: `src/features/apolices/components/tabs/ApoliceRamoFormModal.tsx` e `src/features/apolices/components/modals/SubestipulanteApoliceModal.tsx`.
- Testes novos: `src/features/apolices/hooks/useApolicesErrors.test.tsx` e `src/features/apolices/components/ApoliceVinculosErrors.test.tsx`.
- Teste atualizado: `src/features/apolices/components/tabs/RamosTab.test.tsx`.

Documentação: este registro e continuidade nos documentos 29 e 36. As alterações locais anteriores foram preservadas.

## Próxima etapa

Continuar a tipagem do tratamento de erros nos consumidores de API dos demais cadastros, reutilizando `HttpApiError` e preservando as mensagens e os dados dos formulários. Os 21 apontamentos de `no-explicit-any` restantes incluem outros domínios e componentes de campos/seletores; devem ser tratados em lotes próprios.

Depois, revisar os efeitos, suas dependências e os estados de requisições concorrentes. Permanecem pendentes design system, formatação geral e homologação integrada, conforme o [plano de estabilização](29-estabilizacao-frontend.md).
