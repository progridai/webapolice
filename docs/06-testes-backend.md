# 06. Estratégia de Testes do Backend

O backend deve aderir à pirâmide de testes para garantir integridade, velocidade e validação comportamental.

## 1. Testes de Unidade (Domain e Application)

*   **Domain:** Devem ser exaustivamente testados. Garantem as invariantes das entidades e lógicas puras. Nenhuma dependência externa ou mock de IO (rede, banco) deve existir aqui.
*   **Application:** Testam o fluxo do caso de uso. Podem utilizar substitutos locais ("fakes" ou mocks controlados) para as portas definidas na própria camada (ex: repositórios in-memory simulados). 

## 2. Testes de Integração (Infrastructure)

*   **Infrastructure:** Responsável por validar as integrações técnicas, como queries no EF Core, mapeamentos objeto-relacional (ORM), e persistência correta no banco de dados, bem como integração com APIs REST externas.
*   Futuramente, envolverão o banco de dados real via containers efêmeros (ex: Testcontainers) para testar os repositórios reais sem depender do ambiente do desenvolvedor.

## 3. Testes de Integração HTTP (API)

*   Servem como ponta-a-ponta interno.
*   Validam filtros de exceção, serialização JSON, mapeamento para `ProblemDetails` e fluxos de segurança e autorização (middlewares HTTP, políticas Keycloak integradas).

## 4. Testes de Arquitetura

O projeto `WebApolice.Architecture.Tests` utiliza reflexão (ex: `NetArchTest.Rules`) para garantir, de forma automatizada no pipeline de CI, que:
*   As camadas respeitem o fluxo unidirecional.
*   O *Domain* não referencie pacotes que deveriam estar na *Infrastructure* ou *API*.
*   O `SharedKernel` não acople a nenhum módulo de negócio.
*   A nomenclatura de classes/namespaces seja obedecida.
