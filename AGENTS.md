# Regras permanentes do WebApolice

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
