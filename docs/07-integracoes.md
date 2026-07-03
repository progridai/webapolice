# 07. Integrações Externas

## 1. Visão Arquitetural

As integrações com sistemas externos (Icatu, Vindi, Protheus, SIAPE, WhatsApp, n8n) deverão ocorrer exclusivamente na camada de **Infrastructure**. O núcleo da aplicação (`Domain` e `Application`) apenas conhecerá "portas" (interfaces) que abstraem o serviço, sem vazar detalhes de protocolos HTTP, SOAP, SDKs ou bibliotecas de terceiros.

A estrutura conceitual futura esperada será:
```
Infrastructure/
  └── Integracoes/
      ├── Icatu/
      ├── Vindi/
      ├── Protheus/
      ├── Siape/
      └── WhatsApp/
```

## 2. Princípios Obrigatórios

*   **Adaptadores (Adapters):** Cada integração será implementada através de uma classe adaptadora que implementa a interface do domínio/aplicação.
*   **Transações e Bloqueios:** Chamadas de rede HTTP externas não devem ocorrer mantendo bloqueios ou transações abertas de banco de dados locais. Operações locais atômicas e as integrações devem ser adequadamente particionadas, preferencialmente tratando falhas parciais.
*   **Resiliência (Não Antecipada):** Retry controlado, timeout, circuit breaker serão adicionados apenas sob justificativa de uso em momento posterior, porém as interfaces devem ser projetadas para aceitar configurações de resiliência e repasse do `CancellationToken`.
*   **Idempotência e Observabilidade:** Logs e "Correlation ID" são fundamentais em requisições de saída. Para endpoints expostos, Webhooks recebidos devem validar assinaturas ou escopos (Keycloak) antes da ação.
