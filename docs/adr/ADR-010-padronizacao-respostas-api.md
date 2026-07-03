# ADR 010: Padronização de Respostas da API e Tratamento de Exceções

## Data
2026-07-03

## Status
Aceito

## Contexto
Precisamos estabelecer um padrão global para as respostas da API, incluindo tratamento centralizado de exceções, formatação consistente de erros de validação e respostas semânticas, garantindo a previsibilidade para consumidores da API (ex: clientes React, integrações externas).
Ao mesmo tempo, as regras e exceções de negócio originadas nos módulos não podem depender diretamente de tecnologias HTTP ou de serialização nativa do ASP.NET Core, mantendo assim a integridade do isolamento das camadas de domínio e aplicação em um monólito modular.

## Decisão

Foram tomadas as seguintes decisões:

1. **Uso de Problem Details (RFC 7807/9457)**
   Todas as respostas de falha na API, sejam falhas de autorização (401/403), validação (400), erros de negócio (404, 409, 422) ou falhas não previstas (500), utilizarão a formatação `ProblemDetails` nativa do ASP.NET Core. 

2. **IExceptionHandler nativo do ASP.NET Core utilizado pelo projeto em .NET 10**
   A tradução de exceções para as respostas HTTP será realizada de forma centralizada através da implementação de um `IExceptionHandler` registrado via injenção de dependência (`builder.Services.AddExceptionHandler`). Isto substitui pipelines antigos ou custom middlewares.

3. **Inexistência de Envelope Global (Wrapper de Sucesso)**
   Não foi criado nenhum envelope global de resposta de sucesso (exemplo: `{ "data": ... }`). As respostas da API, quando bem-sucedidas, retornam apenas a entidade requisitada ou coleção bruta, seguindo um princípio puramente REST e pragmático para diminuir o tamanho dos payloads.

4. **Adiamento da Decisão de Result Pattern (Padrão Result)**
   Decidiu-se não criar uma infraestrutura pesada baseada no padrão Result (e.g. `Result<T>`) neste momento até que os fluxos de caso de uso se tornem suficientemente complexos. Exceções puras de C# (e.g., `ConflitoDeNegocioException` e `RegraDeNegocioException` situadas no `SharedKernel`) foram usadas como alternativa viável para falhas anômalas ou excepcionais sem referenciar tipos HTTP.

5. **Omissão Completa de Dados Sensíveis**
   Nenhuma *Stack Trace* ou erro de conectividade/criptografia será devolvido diretamente na interface pública da aplicação. Erros 500 exibirão mensagens de segurança (ex: "Consulte os logs para mais informações").

## Consequências

- **Positivas:**
  - Reduz drasticamente a complexidade nos `Controllers`, permitindo a implementação de endpoints limpos e focados no fluxo normal da requisição.
  - Consistência de contrato 100% aderente a RFCs padronizadas na indústria, favorecendo uso com Swagger/OpenAPI.
  - Garantia de que a lógica de domínio será agnóstica a detalhes HTTP, como `StatusCode` ou `Content-Type`.

- **Negativas/Limitações:**
  - Controlar fluxos com exceções pode ser um *anti-pattern* quando a taxa de falhas na validação aumenta intensamente. Será reavaliado o uso do padrão `Result` quando os primeiros *Commands* / *Queries* ganharem contornos de regras intensas.
