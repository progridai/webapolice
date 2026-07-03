# ADR 008: Estratégia de Persistência com PostgreSQL e EF Core no Monólito Modular

## Status
Aceito

## Contexto
O projeto WebApólice adotará um modelo arquitetural de **Monólito Modular** utilizando ASP.NET Core. No futuro, isso exigirá suporte para acesso a banco de dados. Os pontos críticos de decisão incluem qual tecnologia de acesso de banco usar, qual motor de banco de dados e o nível de isolamento entre os módulos.

## Decisão
- O banco de dados a ser utilizado, na etapa posterior (não presente ainda), será o **PostgreSQL**.
- A biblioteca de acesso será o **Entity Framework Core (EF Core)** restrita à camada de Infrastructure.
- A nível de estrutura, utilizaremos um banco de dados unificado na instância, porém **separaremos os módulos logicamente através de Schemas no PostgreSQL** (por ex.: `schema: Clientes`). 
- Será utilizado um **DbContext por Módulo Lógico**, o que impede dependências diretas, forçando o acesso a outras tabelas de outros módulos a passar exclusivamente pelos contratos de comunicação.

## Consequências
- A adoção do schema no PostgreSQL fornecerá organização visual, com facilidade na separação, além de facilitar a extração do schema inteiro com suas respectivas tabelas em caso de desacoplamento completo futuro.
- A divisão de um DbContext por módulo (e não um DbContext gigante, nem o contexto compartilhado) fará a modelagem de cada módulo manter-se limpa. O isolamento transacional local necessitará planejamento se uma operação disparar ações em dois módulos distintos ao mesmo tempo (serão tratadas transacionalmente pelo fluxo).
