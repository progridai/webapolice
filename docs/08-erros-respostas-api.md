# Padronização de Erros e Respostas da API

Este documento descreve o padrão adotado para as respostas de sucesso e erro da API do `WebApolice`. O padrão visa garantir previsibilidade para todos os clientes (React, Flutter, integrações externas), isolar os detalhes de negócio das preocupações com HTTP, e aderir às RFCs de `ProblemDetails`.

## Princípios

- **Erros de domínio não conhecem HTTP**: As camadas de Domain e Application não dependem do ASP.NET Core e não retornam tipos HTTP (`IResult`, `ProblemDetails`). Eles levantam exceções de negócio (no SharedKernel) ou retornam resultados agnósticos.
- **A API é o limite de conversão**: Cabe exclusivamente à camada da API (`WebApolice.Api`) capturar as falhas internas e traduzi-las para respostas HTTP adequadas através de `IExceptionHandler`.
- **Respostas de erro usam Problem Details**: Todo erro devolve o Content-Type `application/problem+json` seguindo a RFC 7807/9457.
- **Respostas de sucesso não possuem envelope obrigatório**: Respostas `200 OK` ou `201 Created` retornam os dados diretamente sem envolver em um wrapper genérico como `{ "data": ... }`.
- **Traceabilidade**: Todas as respostas de erro contêm um `traceId` (rastreabilidade central).
- **Segurança**: Nunca se exibe *stack traces* no retorno aos clientes. Detalhes internos são registrados em log e o cliente recebe apenas mensagens amigáveis.

---

## Estrutura Base de Erros (`ProblemDetails`)

Toda resposta de erro deve, no mínimo, se parecer com:

```json
{
  "type": "https://webapolice/errors/recurso-nao-encontrado",
  "title": "Recurso não encontrado",
  "status": 404,
  "detail": "O cliente solicitado não existe.",
  "instance": "/api/clientes/123",
  "traceId": "0HN0L1..."
}
```

Para erros de validação (400), haverá a propriedade adicional `errors`:

```json
{
  "type": "https://webapolice/errors/requisicao-invalida",
  "title": "Requisição inválida",
  "status": 400,
  "detail": "Um ou mais erros de validação ocorreram.",
  "instance": "/api/clientes",
  "traceId": "0HN0L1...",
  "errors": {
    "Nome": [
      "O nome é obrigatório."
    ]
  }
}
```

---

## Mapeamento de Status HTTP

| Cenário | Tipo de Exceção Base | Status HTTP | URL em `type` | Nível de Log |
|---------|-----------------------|-------------|---------------|---------------|
| Validação Estrutural / Binding (JSON Incorreto) | `ModelStateInvalid` (Framework) | `400 Bad Request` | `https://webapolice/errors/requisicao-invalida` | Warning / Information |
| Sem token, ou Token Inválido / Expirado | Rejeição no Middleware (JWT) | `401 Unauthorized` | `https://webapolice/errors/nao-autenticado` | Information |
| Token válido, mas sem permissão/role | Rejeição de Política (Authorization) | `403 Forbidden` | `https://webapolice/errors/acesso-negado` | Information |
| Identificador inexistente | `RecursoNaoEncontradoException` | `404 Not Found` | `https://webapolice/errors/recurso-nao-encontrado` | Warning |
| Duplicidade ou conflito de estado | `ConflitoDeNegocioException` | `409 Conflict` | `https://webapolice/errors/conflito` | Warning |
| Quebra de regra de negócio (semântica) | `RegraDeNegocioException` | `422 Unprocessable Entity`| `https://webapolice/errors/regra-de-negocio` | Warning |
| Exceção não prevista / Falha Interna | `Exception` / Qualquer outra | `500 Internal Server Error`| `https://webapolice/errors/erro-interno` | Error (com stack trace) |

> A stack trace ou a mensagem original da `Exception` não prevista (500) é armazenada exclusivamente nos logs do servidor. Em produção, a mensagem será restrita a "Consulte os logs para obter mais informações."

---

## Logs e TraceId

O `traceId` utiliza `HttpContext.TraceIdentifier` do ASP.NET Core e é injetado automaticamente na coleção `Extensions` do `ProblemDetails`.

- As validações e erros previstos registram *Warning* nos logs, acompanhados do `traceId` para facilitar a busca do comportamento do cliente no agregador de logs.
- As falhas inesperadas (500) disparam log de erro (`Error`) detalhado com a `Exception` completa e o `traceId`. Não registramos dados sensíveis nos logs (tokens, CPFs sem mascaramento, dados bancários).

## Como Criar Novos Erros

Ao projetar um módulo de negócio (ex: Cadastro de Clientes):

1. **Na camada Application/Domain**: Avalie que tipo de falha a operação representa e lance a exceção base apropriada (ou extenda-a, se desejar um bloco catch específico no controller).
2. Não dependa de tipos HTTP no domínio.
3. Se você lançar um `RecursoNaoEncontradoException("Cliente não encontrado.")`, a camada de API através do `GlobalExceptionHandler` interceptará e devolverá automaticamente um 404 Problem Details com a respectiva mensagem.
