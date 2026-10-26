# Estabilização dos Erros dos Formulários de Cadastros

Data: 22/10/2026. Continuação do [tratamento de erros de Apólices](37-estabilizacao-erros-apolices.md).

## Resultado

Corrigido o tratamento de erros das páginas de Módulos, Ramos, Seguradoras e Subestipulantes. Foram removidos seis usos de `any` e dois casts que presumiam respostas no formato de outro cliente HTTP. O lint global passou de 48 para 42 erros, mantendo quatro avisos. As ocorrências sinalizadas de `no-explicit-any` passaram de 21 para 15, sem novos apontamentos por arquivo/regra.

Adicionados 64 cenários de regressão: a suíte completa passou com 549 testes em 56 arquivos. TypeScript e build continuam aprovados. Lint, design system, formatação geral e homologação integrada permanecem pendentes.

## Correções e comportamento

- Os valores capturados nos quatro formulários usam `unknown`, com verificação de `Error` antes de acessar a mensagem. `HttpApiError` já herda de `Error` e continua sendo produzido pelo cliente HTTP central, conforme a ADR 010.
- Na criação e alteração, as quatro páginas apresentam a mensagem normalizada da API, preservando os dados digitados e permitindo nova tentativa. Foram removidos os acessos a `response.data.message/detail`, que não representam o contrato do cliente atual.
- Rejeições sem contrato de erro e erros com mensagem vazia usam os textos de contingência existentes. As páginas não navegam para a listagem após falha; a navegação ocorre quando a nova tentativa termina com sucesso.
- Seguradoras e Subestipulantes preservam também a mensagem normalizada na falha de carregamento, com texto de contingência para valores inesperados. Módulos e Ramos mantêm seus textos genéricos atuais de falha de carregamento.
- O teste existente de conflito de Seguradora usa `HttpApiError` com status 409, em lugar de um objeto simulando outro cliente HTTP. A mensagem e a asserção do cenário foram mantidas.

Os payloads e identificadores de criação/alteração foram preservados, incluindo o status inativo de Módulo na edição e os opcionais omitidos pelas páginas. Não houve mudanças em componentes de formulário, validações de negócio, endpoints, banco, campos humanos, permissões ou normalização Title Case. Nenhuma dependência, supressão de lint ou flexibilização de compilação foi adicionada.

## Validações

| Verificação | Resultado |
|---|---|
| Testes direcionados dos formulários | 73 aprovados, zero falhas, três arquivos |
| `npm.cmd run test:run` | 549 aprovados, zero falhas, 56 arquivos |
| `npm.cmd run typecheck` | Aprovado: zero erros, código de saída 0 |
| `npm.cmd run build` | Aprovado, código de saída 0 |
| `npm.cmd run lint` | 42 erros e 4 avisos; antes, 48 erros e 4 avisos |
| `npm.cmd run lint:design-system` | Sete ocorrências preexistentes em `AsyncSelect.tsx` e `DesignSystemPage.tsx` |
| `npm.cmd run format:check` | 278 arquivos pendentes, sem alteração da contagem |
| `git diff --check` | Aprovado |

O novo `CadastroFormErrors.test.tsx` usa as páginas, formulários e roteador reais, com APIs simuladas e fixtures verificadas pelos tipos de retorno existentes. Os 64 cenários cobrem:

- 32 casos de conflito HTTP: quatro cadastros, criação/edição e quatro configurações de tema (claro, escuro, sistema claro e sistema escuro).
- 24 casos de contingência: `null`, mensagem vazia e objeto fora do contrato, nos quatro cadastros em criação/edição.
- Oito falhas de carregamento: erro HTTP ou `null` nos quatro cadastros.

Os testes verificam mensagem e papel semântico do alerta, conservação dos campos, disponibilidade da ação após erro, ausência de navegação indevida, sucesso da nova tentativa e payloads/identificadores enviados. Os cenários de tema conferem o atributo de tema no DOM. A primeira execução detectou que o reset dos mocks apagava a simulação global de `matchMedia`; corrigida a configuração local do teste, a execução final passou integralmente.

Não houve reformatação geral: o arquivo novo foi formatado e as mudanças nos arquivos existentes ficaram restritas ao tratamento de erros e à fixture de conflito. Os testes antigos com supressões preexistentes de `any` continuam como dívida separada; a contagem de lint não mede os usos ocultados por essas diretivas.

Relatórios locais ignorados pelo Git em `scratch/lint-etapa10-*`, incluindo o lint inicial e o final. Validações locais com APIs simuladas, sem operações no ambiente publicado. Não substituem homologação integrada ou visual em navegador. Backend não alterado; seus testes não foram executados.

## Arquivos alterados

Caminhos relativos a `apps/web`:

- `src/features/modulos/pages/ModuloFormPage.tsx`.
- `src/features/ramos/pages/RamoFormPage.tsx`.
- `src/features/seguradoras/pages/SeguradoraFormPage.tsx`.
- `src/features/subestipulantes/pages/SubestipulanteFormPage.tsx`.
- `src/features/seguradoras/pages/SeguradoraFormPage.test.tsx` (fixture de conflito).
- `tests/pages/CadastroFormErrors.test.tsx` (novo).

Documentação: este registro e continuidade nos documentos 29 e 37. A branch estava limpa no início deste lote.

## Próxima etapa

Atualização de 22/10/2026: o [lote de Estipulantes e Cooperados](39-estabilizacao-erros-estipulantes-cooperados.md) corrigiu consultas, conflitos de cadastro e consulta de CEP. A suíte passou a 598 testes aprovados, com TypeScript e build aprovados; restam 33 erros e quatro avisos no lint global. Os resultados acima preservam o encerramento do lote dos formulários de cadastros.

Continuar pelo tratamento de erros de Estipulantes e Cooperados, incluindo consultas, criação e alteração. Avaliar tanto os `any` quanto os casts e acessos ao formato antigo de erro, preservando os conflitos de cadastro e mensagens normalizadas. As páginas de Segurança com esses acessos também precisam de revisão em lote próprio.

Depois, concluir a tipagem dos campos/seletores e revisar efeitos, dependências e concorrência de requisições. Design system, formatação geral e homologação continuam conforme o [plano de estabilização](29-estabilizacao-frontend.md).
