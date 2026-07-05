# Fundacao Frontend

## Ambiente local

O frontend Vite usa variaveis centralizadas em `apps/web/src/app/config/env.ts`.
Nao use `import.meta.env` diretamente fora desse modulo.

Variaveis esperadas:

```env
VITE_API_BASE_URL=http://127.0.0.1:5007
VITE_KEYCLOAK_URL=http://127.0.0.1:8080
VITE_KEYCLOAK_REALM=webapolice
VITE_KEYCLOAK_CLIENT_ID=webapolice-web
VITE_ENABLE_DESIGN_SYSTEM=true
```

`VITE_API_BASE_URL` deve conter apenas a base da API, sem rota de recurso. A
listagem de clientes compoe a URL final como:

```text
http://127.0.0.1:5007/api/clientes
```

Variaveis `VITE_*` sao lidas na inicializacao do Vite. Reinicie `npm run dev`
apos alterar `.env`.

## CORS

A API permite as origens locais configuradas em
`Cors:FrontendOrigins`:

```json
[
  "http://127.0.0.1:5173",
  "http://localhost:5173"
]
```

Em producao, configure explicitamente as origens permitidas. Nao use
`AllowAnyOrigin`.

## HTTP/HTTPS local

O ambiente de desenvolvimento roda em HTTP:

- Frontend: `http://127.0.0.1:5173`
- API: `http://127.0.0.1:5007`
- Keycloak: `http://127.0.0.1:8080`

Se a API for executada por perfil HTTPS, confie no certificado local com:

```bash
dotnet dev-certs https --trust
```

## Troubleshooting: Failed to fetch

`Failed to fetch` no navegador costuma indicar falha antes da API responder ao
JavaScript: CORS, servidor fora do ar, URL incorreta, protocolo/porta errados ou
certificado local nao confiavel.

Checklist:

- confirmar `VITE_API_BASE_URL`;
- reiniciar Vite apos mudar `.env`;
- testar `GET /api/health/live`;
- testar preflight `OPTIONS /api/clientes`;
- confirmar que a API retorna CORS tambem em 401/403;
- confirmar que o token Bearer esta sendo enviado sem registra-lo em logs.
