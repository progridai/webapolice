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

## Estado da Implementação — 23/10/2026

A decisão permanece em vigor. PostgreSQL/EF Core já estão implementados; Application ainda contém violações registradas na revisão 76. O recorte de Clientes foi corrigido para depender de portas e tipos de Domain.Clientes. As portas de operação auditada são específicas do módulo e seus adaptadores reutilizam OperacaoAuditada com uma conexão/transação física compartilhada. Isso não autoriza ler tabelas de outro módulo diretamente nem cria exceção ao limite de EF em Infrastructure. Testes arquiteturais agora carregam os módulos e falham nas dependências remanescentes. Atomicidade real requer executar as integrações PostgreSQL isoladas.


## Continuidade — 26/10/2026

Resolvidas as dependências detectadas em Application com portas específicas de Cadastro/Segurança/Seguro, consultas EF em Infrastructure e entidades POCO em Domain. O contrato neutro de transação substitui IDbContextTransaction nas portas; o adaptador envolve a mesma transação EF, sem nova conexão. ICadastroSeguroQueries remove o SQL direto de Cadastro dos handlers de vínculos de Seguro. Não houve novo projeto, biblioteca, schema ou alteração das propriedades das entidades. O teste adicional de portas impede expor IQueryable nos contratos. As regras existentes não foram excluídas ou relaxadas. Evidências e limites na revisão 79.
