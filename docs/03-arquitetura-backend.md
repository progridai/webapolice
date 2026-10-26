# 03. Arquitetura do Backend

## 1. Visão Geral

Estado em 26/10/2026: a norma abaixo permanece. Os casos de uso de Cadastro, Segurança e Seguro passaram a usar portas de persistência sem EF/DbContext/IQueryable; consultas e adaptadores ficam em Infrastructure. Entidades POCO compartilhadas pelas portas ficam em Domain, mantendo os mapeamentos relacionais. TransacaoEf adapta a transação existente ao contrato neutro ITransacaoPersistencia, preservando commit/rollback/dispose na mesma conexão. Consultas entre os handlers de vínculos de Seguro e Cadastro usam ICadastroSeguroQueries. O serviço de provisionamento recebe uma exceção de concorrência neutra; somente o adaptador interpreta SQLSTATE/constraint. As regras arquiteturais passam localmente; isso não constitui revisão exaustiva de todo SQL, auditoria ou regra de negócio. Ver revisões [76](76-revisao-arquitetura-seguranca-qualidade.md) e [79](79-preparacao-integracao-main.md).

O backend do WebApólice é projetado como um **Monólito Modular** em ASP.NET Core (.NET 10).
Esta abordagem foi escolhida para balancear a simplicidade de implantação e operação com o rigor da separação lógica necessária para um ERP de seguros em crescimento.

## 2. Princípios Fundamentais

*   **Implantação Única:** Todo o backend roda como um processo unificado (uma única unidade de implantação).
*   **Módulos Lógicos Isolados:** O código é estritamente separado por contextos (ex.: Clientes, Propostas, Sinistros).
*   **Persistência Definitiva (PostgreSQL):** A persistência futura e principal da aplicação será realizada em PostgreSQL. O armazenamento em memória **não** é uma decisão arquitetural do produto, sendo restrito apenas a simulações durante a execução de testes automatizados.
*   **Pronto para Extração:** Se houver necessidade concreta de escalabilidade isolada ou fronteira organizacional, módulos podem ser extraídos para microsserviços posteriormente.

## 3. Direção das Dependências

As dependências seguem um fluxo estritamente unidirecional para o domínio:

```
Domain
  ↑
Application
  ↑
Infrastructure
  ↑
API / Bootstrapper
```

### Regras de Restrição
1.  **Domain** não referencia `Application`, `Infrastructure`, `API`, ou bibliotecas externas como `EF Core` e `ASP.NET Core`.
2.  **Application** não referencia `Infrastructure` ou `API`. Ele orquestra casos de uso e define interfaces (portas).
3.  **Infrastructure** implementa as interfaces definidas por `Application` (ou `Domain`).
4.  **API (WebApolice.Api)** atua apenas como ponto de entrada HTTP, segurança e composição de dependências (Bootstrapper).
5.  **Um módulo não acessa a Infrastructure de outro módulo.** Toda comunicação inter-módulo é feita via interfaces/contratos públicos.

## 4. Responsabilidades das Camadas

*   **Domain:** Entidades, Value Objects, invariantes e regras críticas. Desconhece a tecnologia subjacente.
*   **Application:** Casos de uso (Commands/Queries), DTOs internos de orquestração, coordenação de transações lógicas e autorização contextual.
*   **Infrastructure:** DbContext, repositórios, integrações HTTP/SMTP e mensageria técnica.
*   **Contracts:** Contratos de integração públicos, eventos e interfaces explicitamente desenhadas para compartilhar dados (evite que se torne um "dump" de DTOs).
*   **API:** Middlewares HTTP, filtros, roteamento, OpenTelemetry/logs estruturados iniciais, conversão de domínios para `ProblemDetails` e integração de autenticação (Keycloak).
- **WebApolice.Api**: Ponto de entrada (Host). Responsável por roteamento, injeção de dependência e endpoints.
- **WebApolice.SharedKernel**: Tipos comuns, exceções base e primitivas de domínio compartilhadas entre todos os módulos. Não pode referenciar nenhum módulo específico nem conceitos HTTP/políticas de segurança.
- **WebApolice.Shared.Infrastructure**: Infraestrutura transversal compartilhada. Contém também o contrato neutro de autorização (`WebApolice.Shared.Infrastructure.Security`), centralizando PerfisAcesso e PoliticasAutorizacao para os controladores dos módulos, e a infraestrutura de conexão compartilhada (`DbConnection` no escopo do DI) para coordenação atômica de transações locais (sem MSDTC).
- **WebApolice.Auditoria**: Módulo técnico para gravação persistente de eventos de auditoria com JSONB e isolamento de dependências.

Os módulos já presentes na solução têm maturidades distintas; o inventário e as violações remanescentes estão na revisão 76. Novos módulos devem seguir o guia específico e as mesmas restrições.

## 5. Comunicação Entre Módulos

Por ser um monólito modular:
*   A comunicação primária entre módulos é **síncrona (in-process)**, invocando serviços de aplicação ou consultas públicas.
*   Não é permitido realizar join direto entre tabelas de módulos distintos.
*   Para integrações complexas (quando necessário futuramente), será adotado o uso de eventos em memória ou um barramento simples. RabbitMQ/Kafka não devem ser adotados sem justificativa extrema.


Na consolidação de 23/10/2026, `AuthorizePermissaoAttribute` passou a ser contrato público em `WebApolice.Modulos.Seguranca.Contracts.Authorization`; controllers de outros módulos não devem importar Infrastructure de Segurança para declarar permissões. As políticas e a matriz de negação permanecem as mesmas.
