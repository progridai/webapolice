# ADR 011: Fundação de Auditoria Persistida e Rastreabilidade

**Data:** 03 de Julho de 2026
**Status:** Aceito

## Contexto
O sistema `WebApolice` necessita de uma infraestrutura para auditar e rastrear ações realizadas pelos usuários, de forma persistente, em conformidade com políticas de segurança e minimização de dados (LGPD). 
A auditoria deve estar separada dos logs técnicos e da infraestrutura de negócio, e não deve possuir dependência direta de módulos de domínio para evitar acoplamento genérico.

## Decisão
Decidimos criar um módulo técnico isolado chamado `WebApolice.Auditoria`, contendo sua própria infraestrutura de persistência (DbContext, schemas, migrations) e sua entidade técnica de auditoria (`RegistroAuditoria`).

As principais decisões técnicas incluem:
1. **JSONB para flexibilidade estruturada**: Campos como `DadosAnteriores`, `DadosPosteriores` e `Metadados` serão salvos no PostgreSQL utilizando a coluna `jsonb`, em vez de criar dezenas de colunas esparsas ou tabelas relacionais em EAV (Entity-Attribute-Value).
2. **Contexto EF Core Isolado**: A auditoria terá um contexto específico, `AuditoriaDbContext`, sob o schema `auditoria`. O acesso a ele pela API ocorrerá via injeção de dependência dos contratos (`IRegistradorAuditoria`), garantindo que módulos de domínio não interajam com tabelas técnicas diretamente.
3. **Mascaramento e Minimização**: Desenvolvemos um `ProvedorMascaramento` customizado que limpa preventivamente propriedades sensíveis (senhas, tokens, dados bancários) do JsonNode antes da persistência, agindo como segunda barreira (a primeira sendo o envio consciente de dados permitidos pelo domínio).
4. **Contexto Identificado pelo Keycloak**: A entidade salvará o `sub` extraído do Keycloak (`UsuarioIdExterno`), em vez do ID de banco de dados do usuário, visto que a aplicação funciona como um Resource Server autônomo sem tabela de usuários locais obrigatória.

## Consequências
- A auditoria está contida e não "suja" o banco de dados principal de negócio, facilitando futuros off-loads, purges (retenção temporal) ou mesmo o particionamento da tabela.
- As consultas em `jsonb` permitem buscar facilmente recursos auditados por chaves internas sem overhead de junções relacionais massivas.
- Módulos futuros não precisam aprender tecnologias específicas de banco, bastando criar a classe que implementa seus contratos de auditoria para chamar a interface comum de injeção.
