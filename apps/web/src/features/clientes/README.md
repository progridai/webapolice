# Feature: Clientes

## Escopo Futuro

Este módulo implementará a gestão de clientes da plataforma WebApólice.

### Funcionalidades Previstas

- Listagem de clientes com paginação e filtros
- Cadastro de novo cliente
- Edição de cliente existente
- Visualização de detalhes do cliente
- Inativação de cliente

### Estrutura Esperada

```
features/clientes/
├── api/
│   ├── clientesApi.ts         # Chamadas HTTP ao backend
│   └── index.ts
├── components/
│   ├── ClienteForm/           # Formulário de cadastro/edição
│   ├── ClienteTable/          # Tabela com lista de clientes
│   └── index.ts
├── hooks/
│   ├── useClientes.ts         # Hook de listagem
│   ├── useCliente.ts          # Hook de detalhe
│   └── index.ts
├── pages/
│   ├── ClientesListPage.tsx   # Página: /app/clientes
│   ├── ClienteDetailPage.tsx  # Página: /app/clientes/:id
│   └── index.ts
├── routes/
│   ├── ClientesRoutes.tsx     # Rotas do módulo
│   └── index.ts
├── schemas/
│   ├── clienteSchema.ts       # Validação Zod (ou similar)
│   └── index.ts
├── types/
│   ├── cliente.types.ts       # Tipos do domínio
│   └── index.ts
├── utils/
│   └── index.ts
├── README.md                  # Este arquivo
└── index.ts
```

### Regras de Organização

1. **Sem dependências cruzadas**: esta feature não pode importar de outras features.
2. **Consome o Design System**: todos os componentes devem usar `src/components/ui`.
3. **Sem cores fixas**: apenas tokens semânticos via CSS.
4. **Chamadas HTTP**: sempre via `src/services/http`, nunca direto em componentes.
5. **Rotas integradas**: as rotas do módulo são registradas em `AppRoutes.tsx` com política de acesso declarada.
6. **Formulários**: erros de validação do backend normalizados via `flattenValidationErrors`.

### Política de Acesso (prévia)

```
/app/clientes       → roles: [admin, gestor, operador]
/app/clientes/:id   → roles: [admin, gestor, operador]
```

### Próximos Passos

- [ ] Definir contrato da API com o backend (endpoints, payloads)
- [ ] Mapear tipos do domínio (`Cliente`, `ClienteResumo`)
- [ ] Implementar `clientesApi.ts`
- [ ] Implementar `ClienteTable`
- [ ] Implementar `ClientesListPage`
- [ ] Criar rotas e registrar em `AppRoutes.tsx`
- [ ] Criar testes

### Referências

- Backend: `backend/src/WebApolice.Modulos.Clientes/`
- Documentação: `docs/11-modulo-clientes-backend.md`
- Fundação Frontend: `docs/14-fundacao-frontend.md`
