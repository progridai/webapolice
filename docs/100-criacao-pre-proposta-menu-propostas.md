# Criação de Pré Proposta pelo Menu Propostas

Data: 18/11/2026. Implementação solicitada após identificar que a criação da primeira pré proposta estava disponível somente pela aba Vidas da Apólice. Implementado e validado localmente.

## Comportamento

A página **Propostas** passa a oferecer **Nova pré proposta**. O usuário seleciona primeiro uma Apólice ativa e depois a Vida. Ambas as listas possuem busca, paginação de 20 registros, carregamento, erro com nova tentativa e estado vazio. A busca de Vida usa nome ou documento, conforme o contrato existente da API; matrícula é exibida para identificação e permanece com sua caixa original.

A seleção de Vida exibe Cliente, matrícula, Subgrupo, Módulo e impedimentos. Vida inativa, sem vínculo de Cliente ou com propostas anteriores não pode iniciar uma nova primeira proposta. Para histórico existente, as operações de nova versão e reinclusão continuam no detalhe da proposta. A decisão final de elegibilidade é revalidada pelo backend na criação, incluindo alterações concorrentes de Apólice, Vida e condições.

Após escolher uma Vida disponível, reutiliza-se `CriarPropostaDialog` e `useCriarProposta`. O diálogo permite conferir o contexto antes de gravar. O POST existente envia os UUIDs da Apólice e da Vida na rota, sem inventar origem ou alterar os vínculos da Vida. Ao concluir, o UUID retornado pela API é aberto diretamente em `PropostasConsulta` para preenchimento. Em erro, o contexto permanece disponível para conferência, cancelamento ou nova tentativa. O hook preserva a proteção contra duplo envio; o diálogo impede fechamento durante a gravação e a saída do seletor fica bloqueada enquanto a confirmação estiver aberta.

Cancelar retorna à consulta anterior. Trocar a Apólice reinicia a busca/página de Vidas e cancela a consulta pendente. As consultas usam os serviços HTTP existentes com AbortSignal e `useConsulta`; respostas antigas não substituem a seleção atual. Títulos das etapas recebem foco na entrada; tabelas, botões, modal e paginação reutilizam o design system e seus tokens nos três temas.

O texto antigo de geração financeira indisponível foi atualizado no diálogo compartilhado: a criação continua sem vigência e sem parcela, e o tratamento financeiro ocorre no preenchimento/confirmação/ativação já implementado. Não muda as regras aprovadas do ciclo financeiro.

## Autorização e Dados

A ação exige módulo APOLICES, recurso PROPOSTAS e permissões `propostas.visualizar` e `propostas.inserir`. As seleções reutilizam GETs de Apólices/Vidas protegidos por `apolices.visualizar`; caso essa consulta não esteja permitida, o fluxo explica a necessidade e não faz as requisições. Não há nova permissão, concessão automática ou exigência de alterar Cliente/Vida. A criação permanece no endpoint contextual protegido por `propostas.inserir`.

Não há endpoint, contrato de API, coluna, migration ou texto humano persistido novo. UUIDs, paginação e busca são técnicos; os nomes apresentados vêm dos cadastros e as matrículas são preservadas. O POST continua usando o cliente HTTP e a padronização compartilhada, sem alterar classificações C#/TypeScript/SQL nem reprocessar registros anteriores.

## Arquivos e Verificações

- `apps/web/src/features/propostas/pages/PropostasPage.tsx`: ação de criação, retorno à consulta e abertura do UUID criado.
- `apps/web/src/features/propostas/components/NovaPreProposta.tsx`: seleção da Apólice/Vida com consultas canceláveis, busca, paginação e impedimentos.
- `apps/web/src/features/propostas/components/CriarPropostaDialog.tsx`: mensagem de continuidade financeira atualizada, compartilhada com a criação pela Apólice.
- `apps/web/src/features/propostas/pages/PropostasPage.test.tsx`: 13 cenários, incluindo três temas, permissões/catálogo, seleção/paginação, contexto de UUIDs, cancelamento pendente/resposta antiga, erro/nova tentativa/vazio e prevenção de reenvio/saída durante a gravação.

Os 13 cenários direcionados passaram. Os seis checks obrigatórios do frontend foram concluídos com sucesso, sem flexibilizar configurações. Nenhuma alteração de backend ou de dados reais nesta etapa. O frontend local usa Vite e recebe a alteração sem reiniciar a API; a homologação visual autenticada pelo usuário permanece separada dos testes automatizados.


## Resultado Final

| Verificação | Resultado |
| --- | --- |
| `npm.cmd run format:check` | Aprovado; nenhum arquivo pendente |
| `npm.cmd run typecheck` | Aprovado; zero erros |
| `npm.cmd run lint` | Aprovado; zero erros e avisos |
| `npm.cmd run lint:design-system` | Aprovado; zero ocorrências |
| `npm.cmd run test:run -- --maxWorkers=2` | 1.423 testes aprovados em 100 arquivos |
| `npm.cmd run build` | Aprovado; permanece o aviso de chunk acima de 500 kB, sem alterar o limite |
| `git diff --check` | Aprovado |
| Verificação local de padrões reconhecidos de credenciais nos arquivos novos/alterados desta etapa | Zero ocorrências |

Log completo: `scratch/nova-pre-proposta-frontend-validation.log`; testes direcionados: `scratch/nova-pre-proposta-testes.log`. O Vite local respondeu na porta 5173 e os sourcemaps da página Propostas e do seletor corresponderam aos arquivos atuais. A configuração aponta para a API na porta 5007, que permaneceu Healthy com PostgreSQL Healthy. O usuário pode atualizar `http://127.0.0.1:5173/propostas` e clicar em “Nova pré proposta”. Não foi feito login no navegador para homologação visual autenticada, nem criado registro real para testar a ação. Não houve nova migration, reinício de API, commit ou push nesta alteração de interface.
