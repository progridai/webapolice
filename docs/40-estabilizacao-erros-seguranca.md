# Estabilização dos Erros de Usuários e Perfis

Data: 22/10/2026. Continuação do [lote de Estipulantes e Cooperados](39-estabilizacao-erros-estipulantes-cooperados.md).

## Resultado

Corrigidas as mensagens de falha de gravação nas quatro páginas de cadastro/edição de Usuários e Perfis. Foram removidos quatro casts que presumiam erros no formato de outro cliente HTTP. As páginas agora verificam `Error` antes de ler sua mensagem, reconhecendo também `HttpApiError`, que herda dessa classe.

Adicionados 33 cenários de regressão. A suíte completa passou com 631 testes em 60 arquivos; TypeScript e build aprovados. Os cinco arquivos de código/teste deste lote passaram no ESLint sem erros ou avisos.

O lint global continua com 33 erros e quatro avisos, sem novas ocorrências por arquivo/regra. Os casts corrigidos não eram apontamentos de `no-explicit-any`; a contagem global não diminui neste lote. Restam seis ocorrências dessa regra nos campos/seletores, além das demais dívidas registradas.

## Comportamento corrigido

- Cadastro e edição de Usuários e Perfis exibem a mensagem normalizada da API em vez de procurar `response.data.message`. A normalização continua no cliente HTTP central, conforme a ADR 010.
- Rejeições como `null`, objetos fora do contrato e erros com mensagem vazia usam as mensagens de contingência existentes, sem falha ao acessar propriedades de um valor inesperado.
- Após erro de gravação, os dados e seleções de perfis/permissões permanecem no formulário, a ação fica disponível para nova tentativa e a página navega para os detalhes somente após sucesso.
- Permanecem os payloads atuais, incluindo omissão de username/código na edição, IDs públicos nas seleções e envio de senha temporária somente na criação de Usuário. A confirmação da senha continua fora do payload da API; os campos de senha saem da tela com a navegação após sucesso.
- A proteção de edição do perfil `ADMINISTRADOR`, quando marcado como perfil de sistema com acesso total, foi preservada e exercitada pelos testes. As mensagens genéricas existentes das falhas de carregamento também foram mantidas.

As alterações de produção ficaram restritas aos quatro blocos de tratamento de erro de gravação. Não houve alterações de autenticação, autorização, permissões reais, contratos, endpoints, banco, componentes de formulário ou regras de Title Case. Não foram criados campos de negócio nem alteradas credenciais reais. As fixtures de usuários/senhas são sintéticas e as APIs são simuladas nos testes.

## Validações

| Verificação | Resultado |
|---|---|
| Testes direcionados de erros e contratos de Usuários | 47 aprovados, zero falhas, dois arquivos |
| `npm.cmd run test:run` | 631 aprovados, zero falhas, 60 arquivos |
| `npm.cmd run typecheck` | Aprovado: zero erros, código de saída 0 |
| `npm.cmd run build` | Aprovado, código de saída 0 |
| ESLint dos quatro arquivos de produção e teste novo | Zero erros e avisos |
| `npm.cmd run lint` global | 33 erros e 4 avisos, sem alteração da contagem e sem novos apontamentos por arquivo/regra |
| `npm.cmd run lint:design-system` | Sete ocorrências preexistentes em `AsyncSelect.tsx` e `DesignSystemPage.tsx` |
| `npm.cmd run format:check` | 278 arquivos pendentes, sem alteração da contagem |
| `git diff --check` | Aprovado |

`SegurancaFormErrors.test.tsx` usa páginas, formulários, seletores e roteador reais, com fixtures verificadas pelos tipos de retorno das APIs:

- 16 cenários de mensagem HTTP e recuperação: quatro páginas nos temas claro, escuro e sistema (claro/escuro).
- 12 cenários de contingência: `null`, mensagem vazia e objeto fora do contrato em cada página.
- Quatro cenários de falha de carregamento de perfis/catálogo.
- Um cenário de bloqueio da edição do perfil `ADMINISTRADOR` protegido.

Os testes verificam alertas semânticos, conservação dos campos e seleções, liberação das ações, navegação, payloads e preservação dos identificadores técnicos. O teste existente de contratos de Usuários também foi executado. A verificação estática inicial encontrou um import sem uso no teste novo; ele foi removido e as verificações estáticas foram repetidas com os resultados finais acima.

A validação de tema confere o DOM e não substitui homologação visual em navegador. Nenhum usuário, perfil ou permissão foi gravado em serviços reais. Backend não alterado; seus testes não foram executados. Não foram adicionadas dependências ou supressões. O arquivo de teste novo foi formatado, sem reformatação geral dos arquivos existentes.

Relatórios locais ignorados pelo Git em `scratch/lint-etapa12-*`, com retrato inicial dos quatro arquivos e comparativo inicial/final de lint. As alterações locais dos lotes anteriores foram preservadas.

## Arquivos alterados

Caminhos relativos a `apps/web`:

- `src/features/seguranca/pages/CadastrarUsuarioPage.tsx`.
- `src/features/seguranca/pages/EditarUsuarioPage.tsx`.
- `src/features/seguranca/pages/CadastrarPerfilPage.tsx`.
- `src/features/seguranca/pages/EditarPerfilPage.tsx`.
- `tests/pages/SegurancaFormErrors.test.tsx` (novo).

Documentação: este registro e continuidade nos documentos 29 e 39.

## Próxima etapa

Atualização de 22/10/2026: o [lote de campos e seletor](41-estabilizacao-tipos-campos-seletor.md) removeu os seis apontamentos de `no-explicit-any`. A suíte passou a 662 testes aprovados, com TypeScript e build aprovados; restam 27 erros e quatro avisos no lint global. Os resultados acima preservam o encerramento do lote de Segurança.

Concluir a tipagem dos seis usos de `any` sinalizados nos campos de CEP, CNPJ, CPF, telefone e no `AsyncSelect`. Reutilizar os contratos das bibliotecas instaladas e preservar máscaras, callbacks, carregamento e seleção. Depois, revisar efeitos, dependências e concorrência de requisições em lote próprio.

Permanecem pendentes design system, formatação geral e homologação integrada, conforme o [plano de estabilização](29-estabilizacao-frontend.md).
