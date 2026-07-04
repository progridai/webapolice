# 04. Convenções do Backend

## 1. Nomes e Namespaces

*   **Projetos:** Seguem o padrão `WebApolice.[Modulo].[Camada]` (ex.: `WebApolice.Clientes.Domain`).
*   **Namespaces:** Devem corresponder exatamente à estrutura de pastas do projeto (ex.: `WebApolice.Clientes.Application.UseCases`).
*   **Idioma:** A linguagem ubíqua reflete o domínio brasileiro de seguros. Portanto, nomes de classes de domínio, casos de uso, variáveis de negócio e campos devem estar em **Português**. 
    *   Exemplo: `Apolice`, `Sinistro`, `Proposta`.
    *   *Exceção:* Infraestrutura técnica genérica, padrões de design e componentes padrão do .NET devem estar em Inglês (ex.: `Repository`, `Factory`, `Controller`).

## 2. Tipos de Dados

*   **Identificadores (IDs):** O tipo padrão para chaves primárias será `long`. Isto é justificado pela forte integração esperada com a base de dados legada (identity integers).
*   **Moeda / Valores Financeiros:** Sempre utilizar `decimal`. A moeda padrão implícita é BRL (Real).
*   **Datas e Horas:**
    *   Para datas puras (sem horário de negócio relevante), usar `DateOnly`.
    *   Para horários sem data, usar `TimeOnly`.
    *   Para carimbos de tempo técnicos (auditoria), usar `DateTimeOffset` armazenados em UTC.
    *   *Não espalhar `DateTime.Now` pelo código.*

## 3. Casos de Uso (Commands e Queries)

*   **Padrão:** O código deve priorizar a injeção de classes nomeadas explicitamente com o sufixo `Command`, `Query` e seus respectivos `Handlers`.
*   **Retorno de Erros:** Exceções não devem ser usadas para regras de negócios triviais ou validações conhecidas. Contudo, não adotaremos uma biblioteca de result monolítica genérica neste momento. Erros de domínio não conhecem HTTP; a API ficará responsável por converter respostas do application em `ProblemDetails` e respectivos status codes HTTP.
*   **Cancelamento:** Todos os métodos assíncronos devem aceitar e repassar um `CancellationToken`.

## 4. Validação

*   **API:** Valida formato, presença de campos essenciais de HTTP, limites de requisição e segurança de tokens.
*   **Application:** Valida pré-condições do caso de uso, disponibilidade e orquestra regras consistentes.
*   **Domain:** Valida regras de negócio estritas (invariantes) que a entidade nunca pode quebrar.

## 5. Segurança e Auditoria

*   **Identidade e Claims:** A identidade baseia-se na claim `sub` do Keycloak (External ID). Não há tabela de senhas local.
*   **Auditoria Persistida:** Operações do sistema são auditadas pelo módulo técnico `WebApolice.Auditoria`, que salva o ID do usuário externo, ações e metadados JSONB.
*   **Mascaramento:** Tokens, senhas e informações confidenciais são removidas da auditoria pelo `ProvedorMascaramento` antes da persistência.
