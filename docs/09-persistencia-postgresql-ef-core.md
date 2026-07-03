# Persistência com PostgreSQL e Entity Framework Core

Este documento descreve as diretrizes, padrões e comandos adotados para a infraestrutura de persistência de dados do sistema WebApólice.

## Arquitetura e Pacotes

- **Banco de Dados**: PostgreSQL 18.4 (Homologado tanto no ambiente local via Docker Compose quanto em testes automatizados com `Testcontainers`).
- **ORM**: Entity Framework Core 10 (`Microsoft.EntityFrameworkCore`)
- **Provider**: Npgsql (`Npgsql.EntityFrameworkCore.PostgreSQL`)
- **Localização do DbContext**: `Infrastructure` (exclusivamente).
- **Abstração e Limites**: O projeto `WebApolice.Shared.Infrastructure` contém somente infraestrutura técnica compartilhada.
  - O `InfraestruturaDbContext` é exclusivamente técnico.
  - Módulos de negócio **não herdam** desse contexto nem derivam dele.
  - Módulos de negócio **não adicionam entidades** nesse contexto.
  - Nenhum módulo acessa tabelas por meio do contexto técnico.
  - **Não crie classe base** de DbContext para reutilização indiscriminada. Caso existam configurações técnicas reutilizáveis, elas poderão ser compartilhadas por métodos de extensão pequenos, sem herança obrigatória.
  - O pacote de design (`Microsoft.EntityFrameworkCore.Design`) é importado na API como `PrivateAssets="all"`.

## Estratégia de DbContext e Schemas

- Adota-se **Um DbContext por Módulo**. O contexto não deve ser compartilhado de forma universal.
  - Cada módulo futuro possui seu próprio projeto Infrastructure.
  - Cada módulo futuro possui seu próprio DbContext.
- Cada módulo possui um **Schema** lógico (ex: `schema: clientes`, `schema: cobrancas`).
- Apenas a infraestrutura de controle (migrations e tabelas técnicas) utilizará o schema `infraestrutura`.

## Nomenclatura (Naming Conventions)

- Utiliza-se obrigatoriamente nomes em **Português** e **snake_case** para todas as estruturas físicas do banco de dados (tabelas, colunas, chaves primárias, estrangeiras, índices e restrições).
- A conversão de `PascalCase` (classes/propriedades C#) para `snake_case` (banco de dados) é feita de forma automática via biblioteca `EFCore.NamingConventions` utilizando `.UseSnakeCaseNamingConvention()`. O pacote apenas ajusta o casing; as classes devem ser nomeadas em português para o resultado correto. Nomes em inglês (`created_at`, `user_id`) são proibidos na aplicação.
- O histórico do EF Core está configurado como `__EFMigrationsHistory` no schema `infraestrutura`. Essa é a única estrutura permitida com a mescla técnica do framework. Não possuímos tabelas artificiais de infraestrutura além do histórico base.

## Gerenciamento de Migrations e Comandos (CLI)

O DbContext base `InfraestruturaDbContext` possui a sua implementação de tempo de design `InfraestruturaDbContextFactory` que garante a aplicação segura de dependências sem comprometer o fluxo de inicialização da API (API não executa migrações na inicialização).

Para criar uma migration:
```bash
$env:ConnectionStrings__PostgreSql = "Host=localhost;Port=5432;Database=webapolice;Username=webapolice_admin;Password=alterar_localmente"
dotnet ef migrations add <NomeDaMigration> --project src/WebApolice.Shared.Infrastructure --startup-project src/WebApolice.Api --context InfraestruturaDbContext
```

Para aplicar uma migration:
```bash
dotnet ef database update --project src/WebApolice.Shared.Infrastructure --startup-project src/WebApolice.Api --context InfraestruturaDbContext
```

Para gerar o script SQL:
```bash
dotnet ef migrations script --idempotent --project src/WebApolice.Shared.Infrastructure --startup-project src/WebApolice.Api --context InfraestruturaDbContext
```

## Health Checks e Resiliência

- A comunicação com o PostgreSQL está configurada para suportar falhas transientes por meio da configuração de **Retry** nativa do Entity Framework. A configuração exata é:
  - **Máximo de tentativas:** 3
  - **Atraso máximo:** 5 segundos
  - **Regras do Retry:**
    - O retry atende apenas falhas transitórias reconhecidas pelo provider.
    - O retry não substitui transações.
    - O retry não garante idempotência.
    - Operações externas não podem ser repetidas automaticamente.
    - Regras de negócio não são submetidas a retry.
    - A indisponibilidade persistente resulta em falha e readiness 503.
- A API expõe `/api/health` e `/api/health/live` como pontos de prova de processo (liveness). Retornam `200 OK` mesmo se o banco estiver indisponível.
- E `/api/health/ready` como prontidão e disponibilidade dos recursos associados, incluindo validação de conexão ativa com o PostgreSQL usando `AddDbContextCheck`. Se o banco cair, este endpoint retorna `503 Service Unavailable`, omitindo com segurança informações sensíveis (nenhuma connection string ou rastreamento de pilha é exportado).

## Testes

- A camada de `Domain` e `Application` estão banidas de possuírem referências do Entity Framework, validado pelo `ArchitectureTests`.
- A integração com o banco real é realizada via `Testcontainers.PostgreSql`, executando validações em banco limpo a cada nova bateria de testes de integração, impedindo a necessidade de usar providers EF Core InMemory.

## Segurança e Produção

- Credenciais não residem no repositório. Use o `.env` derivado de `.env.example` para senhas locais, injetadas via config global.
- `Database.Migrate()` automático na inicialização da aplicação é proibido; deploys de produção exigem a aplicação controlada dos scripts via pipeline e ferramentas de migração.
