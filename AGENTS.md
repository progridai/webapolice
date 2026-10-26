# Regras permanentes do WebApolice

## Padrões obrigatórios do frontend

Para criar ou alterar telas, componentes, formulários, hooks, serviços, contratos e testes, leia e siga [docs/guias/guia-padroes-frontend.md](docs/guias/guia-padroes-frontend.md). Preserve os padrões existentes e mantenha a documentação atualizada na mesma entrega.

Antes de concluir alterações do frontend, execute em `apps/web`: `npm.cmd run format:check`, `npm.cmd run typecheck`, `npm.cmd run lint`, `npm.cmd run lint:design-system`, `npm.cmd run test:run` e `npm.cmd run build`. Todos devem passar, com zero pendências de formatação, erros de tipos, erros/avisos de lint e ocorrências no scanner. Não flexibilize configurações ou verificações para obter aprovação.

## Padronização obrigatória dos dados

Leia [docs/26-padronizacao-title-case.md](docs/26-padronizacao-title-case.md).
Todo texto humano de negócio deve ser persistido em **Title Case**, independente da origem (formulário, API, importação, integração ou job). Exemplo: `JOÃO DA SILVA` → `João da Silva`.

- Preposições, artigos, conjunções e contrações da lista compartilhada ficam em minúsculas, inclusive no início do campo. Mantenha essa lista sincronizada em C#, TypeScript e SQL.
- Reutilize `PadronizacaoTexto` e `TextoPadronizadoDbContext` no backend e `normalizeHumanData` no cliente HTTP; não implemente versões locais nem use apenas CSS.
- Todo novo DbContext de negócio deve herdar de `TextoPadronizadoDbContext`.
- Ao adicionar um campo humano, atualize a classificação explícita em C#, TypeScript e SQL, incluindo testes. Campos novos precisam ter sua natureza avaliada; não basta serem strings.
- Preserve credenciais, e-mails, URLs, caminhos, códigos, documentos, UF, enums/status, chaves de busca normalizadas e evidências originais/auditoria.
- SQL direto, COPY e importadores devem usar os triggers instalados pelo script de padronização. Reaplique o script após criar novas tabelas/colunas humanas.
- Não reescreva dados de auditoria ou evidências da migração. Não transforme contratos técnicos, nomes de propriedades JSON ou identificadores de recursos.
