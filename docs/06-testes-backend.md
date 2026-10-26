# 06. Estratégia de Testes do Backend

O backend deve aderir à pirâmide de testes para garantir integridade, velocidade e validação comportamental.

## 1. Testes de Unidade (Domain e Application)

*   **Domain:** Devem ser exaustivamente testados. Garantem as invariantes das entidades e lógicas puras. Nenhuma dependência externa ou mock de IO (rede, banco) deve existir aqui.
*   **Application:** Testam o fluxo do caso de uso. Podem utilizar substitutos locais ("fakes" ou mocks controlados) para as portas definidas na própria camada (ex: repositórios in-memory simulados). 

## 2. Testes de Integração (Infrastructure)

*   Utilizam o banco de dados real via containers efêmeros (ex: Testcontainers) para testar os repositórios reais e garantir transações atômicas seguras no banco de dados físico PostgreSQL.
*   Atomicidade e concorrência devem ser comprovadas no PostgreSQL efêmero. A consolidação de 23/10/2026 não executou essa homologação: Docker Engine indisponível. Testes de falha de auditoria/cancelamento foram adicionados em ClienteAuditadoPostgresTests; compilar esses testes não comprova rollback físico.

## 3. Testes de Integração HTTP (API)

*   Servem como ponta-a-ponta interno.
*   Validam filtros de exceção, serialização JSON, mapeamento para `ProblemDetails` e fluxos de segurança e autorização (middlewares HTTP, políticas Keycloak integradas).

## 4. Testes de Arquitetura

O projeto `WebApolice.Architecture.Tests` utiliza reflexão/NetArchTest para verificar as regras abaixo. Desde 23/10/2026 carrega explicitamente os 14 assemblies de produção, exige seleções não vazias, confere o inventário e inclui regressão de uma violação conhecida. O workflow de CI foi versionado, mas ainda não foi executado remotamente; dependências remanescentes detectadas mantêm a suíte reprovada, sem allowlist ou supressão:
*   As camadas respeitem o fluxo unidirecional.
*   O *Domain* não referencie pacotes que deveriam estar na *Infrastructure* ou *API*.
*   O `SharedKernel` não acople a nenhum módulo de negócio.
*   A nomenclatura de classes/namespaces seja obedecida.

## 5. Execução e Limites

Executar `python backend/scripts/verificar-qualidade.py` da raiz; consultar [operação e validação](78-operacao-e-validacao-consolidacao.md). O gate inclui as seis verificações frontend, build/testes backend e auditorias. Não tratar testes ignorados de catálogos ou ausência de infraestrutura como aprovação. No Windows, o runner de testes desabilita apenas o provider EventLog no processo filho; não altera a política de logs da aplicação.

Testes HTTP de JWT preservam as regras reais do host e usam uma chave RSA artificial e ClockSkew zero; mantêm validações de assinatura, emissor, audiência e validade, sem discovery externo. O provisionamento e as portas de catálogo/permissões recebem substitutos explícitos para dispensar conexão do host; as regras do handler de autorização permanecem ativas. A matriz de catálogo executada com InMemory mantém os campos JSON com conversão específica desse provider e não comprova comportamento relacional. Integrações reais, Keycloak e paridade com o legado continuam necessárias.
