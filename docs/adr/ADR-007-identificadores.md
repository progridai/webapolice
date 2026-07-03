# ADR 007: Escolha de Identificadores (IDs)

## Status
Aceito

## Contexto
O WebApólice precisa definir um padrão consistente para identificadores das entidades de negócio no banco de dados. Temos um contexto de migração de um banco SQL Server legado onde predominantemente utilizam-se identificadores baseados em inteiros incrementais. Adicionalmente, as APIs e as integrações precisarão compartilhar esses identificadores. 

Opções principais analisadas:
- `Guid`: Facilita identificadores não sequenciais (segurança teórica de ofuscação), gerados na aplicação. Impacta performance em grandes chaves e indexação se não for ordenado (ex: uuid-v7), além do volume extra em bytes.
- `int`: Baixo custo de armazenamento, amigável. Limite de tamanho pode ser um risco futuro, porém improvável em muitas tabelas; 
- `long` (bigint): Custo um pouco superior ao `int`, mas sem limites práticos e alinhado ao histórico do sistema, facilitando a migração estruturada.

## Decisão
Decidimos que os **Identificadores das Entidades Padrão serão do tipo `long` (BigInt)**.
A geração do ID, a princípio, será delegada ao banco de dados no momento da inserção (identity).

## Consequências
*   A migração de dados e relacionamentos legados manterá fidelidade às chaves numéricas atuais, impedindo a necessidade de mapeamento em runtime complexo.
*   Em expostos públicos das APIs (ex.: IDs na URL), adotar-se-á o valor numérico. Caso seja necessário ofuscar no futuro para segurança, uma estratégia de Hashids poderá ser introduzida em nível de apresentação.
*   Simplifica índices no PostgreSQL.
