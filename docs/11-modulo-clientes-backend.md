# 11. Módulo Clientes - Backend

## 1. Localização e Definição das Políticas de Autorização
Decidimos utilizar a **Opção B** (contrato neutro de autorização) para evitar dependências circulares de apresentação. Como a `ClientesController` pertence ao próprio módulo de negócio e a API hospeda o bootstrapper, centralizamos as definições no projeto `WebApolice.Shared.Infrastructure.Security`:

*   **Arquivo**: [PoliticasAutorizacao.cs](file:///C:/PROJETOS/Rsul%20Automacoes/Projetos/webapolice/backend/src/WebApolice.Shared.Infrastructure/Security/PoliticasAutorizacao.cs)
*   **Perfis de Acesso (Roles)**:
    *   `admin`: Administrador global.
    *   `gestor`: Gestor de negócios (pode cadastrar, alterar e gerenciar status).
    *   `operador`: Operador operacional (visualização e listagem).
*   **Políticas de Autorização (Capacidades)**:
    *   `Administracao` -> Exclusivo para role `admin`.
    *   `GestaoClientes` -> Mapeia para roles `gestor` e `admin`.
    *   `ConsultaClientes` -> Mapeia para roles `operador`, `gestor` e `admin`.

### Matriz de Autorização por Endpoint
| Rota HTTP | Método | Capacidade Requerida | Operador | Gestor | Admin | Não Autenticado |
| :--- | :--- | :--- | :---: | :---: | :---: | :---: |
| `/api/clientes` | `POST` | `GestaoClientes` | 403 | 201 | 201 | 401 |
| `/api/clientes/{id}` | `PUT` | `GestaoClientes` | 403 | 204 | 204 | 401 |
| `/api/clientes/{id}/ativar` | `POST` | `GestaoClientes` | 403 | 204 | 204 | 401 |
| `/api/clientes/{id}/inativar` | `POST` | `GestaoClientes` | 403 | 204 | 204 | 401 |
| `/api/clientes/{id}` | `GET` | `ConsultaClientes`| 200 | 200 | 200 | 401 |
| `/api/clientes` | `GET` | `ConsultaClientes`| 200 | 200 | 200 | 401 |
| `/api/clientes/{id}` | `DELETE`| *(Proibido)* | 405 | 405 | 405 | 401 |

---

## 2. Estratégia Transacional e Atomicidade em Linux (Docker)
Anteriormente, utilizávamos `TransactionScope` que induzia à promoção de transações distribuídas (2PC) e exigia suporte ao MSDTC, impossibilitando a execução em containers Linux e ambientes Docker/VPS.

Substituímos o `TransactionScope` por uma **coordenação explícita de transação de banco com conexão compartilhada**:
1.  **Shared Connection**: Registramos a `DbConnection` (do Npgsql) com ciclo de vida `Scoped` no contêiner de DI.
2.  **Shared Contexts**: Ambos os contextos de banco (`ClientesDbContext` e `AuditoriaDbContext`) são injetados e configurados para utilizar a mesma conexão compartilhada na requisição HTTP.
3.  **Coordenação**: Criamos o [ClientesTransactionManager](file:///C:/PROJETOS/Rsul%20Automacoes/Projetos/webapolice/backend/src/WebApolice.Modulos.Clientes/Infrastructure/Persistence/ClientesTransactionManager.cs) implementando a interface `IClientesTransactionManager` na camada Application Ports. Ele abre a conexão de forma assíncrona, inicia a transação no driver Npgsql, e a atribui a ambos os DB Contexts usando:
    ```csharp
    await _clientesDbContext.Database.UseTransactionAsync(transaction, cancellationToken);
    await _auditoriaDbContext.Database.UseTransactionAsync(transaction, cancellationToken);
    ```
4.  **Rollback Garantido**: Se houver qualquer falha (no salvamento do cliente ou no envio da auditoria), um bloco try/catch assegura a chamada de `transaction.RollbackAsync()` desfazendo quaisquer alterações parciais de forma atômica no banco físico PostgreSQL.

### Prova em Linux
A validação de atomicidade foi executada com sucesso em container oficial do SDK .NET (`mcr.microsoft.com/dotnet/sdk:10.0`) rodando no Docker local, desabilitando o recurso de Ryuk reaper (`TESTCONTAINERS_RYUK_DISABLED=true`) e direcionando o tráfego da rede para o host (`TESTCONTAINERS_HOST_OVERRIDE=host.docker.internal`).
Todos os 10 testes de integração atômica passaram com sucesso, confirmando que transações cruzadas são 100% seguras no Linux sem depender do MSDTC.

---

## 3. Modelo de Banco de Dados e Migration Completa
A migration `InicialClientes` foi aplicada ao banco sob as seguintes especificações físicas:

*   **Schema**: `clientes`
*   **Tabela**: `clientes.clientes`
*   **Colunas e Tipos**:
    *   `id`: `bigint GENERATED ALWAYS AS IDENTITY` (Chave Primária)
    *   `nome`: `varchar(150) NOT NULL` (Normalizado, sem múltiplos espaços)
    *   `cpf`: `varchar(11) NOT NULL` (Armazena apenas os 11 dígitos numéricos limpos)
    *   `data_nascimento`: `date` (Opcional, mapeada de `DateOnly?`)
    *   `email`: `varchar(254)` (Opcional)
    *   `telefone`: `varchar(20)` (Opcional)
    *   `status`: `varchar(20) NOT NULL` (Valores permitidos: `Ativo`, `Inativo`)
    *   `data_cadastro_utc`: `timestamp with time zone NOT NULL`
    *   `data_atualizacao_utc`: `timestamp with time zone NOT NULL`
    *   `codigo_legado`: `bigint` (Opcional)
*   **Constraints**:
    *   `pk_clientes`: Chave Primária sobre `id`.
    *   `ck_clientes_status`: Validação de check no banco: `CHECK (status IN ('Ativo', 'Inativo'))`.
    *   `uk_clientes_cpf`: Índice Único para impedir CPFs duplicados no banco.
*   **Índices e Justificativa de Unicidade**:
    *   `ix_clientes_nome`: Índice simples no campo `nome` para acelerar buscas paginadas e filtragem de listagens.
    *   `ix_clientes_codigo_legado`: Índice Único Parcial criado usando a cláusula `WHERE codigo_legado IS NOT NULL`. Isso permite que múltiplos registros tenham valor `NULL` (clientes novos cadastrados sem código legado) enquanto garante a unicidade absoluta para códigos preenchidos.

---

## 4. Paginação e Filtros
A paginação de dados e aplicação de filtros na listagem foram desenhadas para serem processadas em nível de banco de dados:
*   Os métodos `.Skip()` e `.Take()` são aplicados no banco antes de materializar a lista (`IQueryable`).
*   A query aceita filtros opcionais de `nome` (usando `LIKE`), `cpf` (formatado ou limpo, com normalização automática para dígitos) e `status`.
*   A ordenação utiliza ordenação dinâmica segura baseada em padrão restrito de colunas (`id`, `nome`, `data_cadastro_utc`) com direção ascendente/descendente mapeada, evitando injeções de SQL. O desempate padrão é sempre feito pelo `id`.

---

## 5. Privacidade e Segurança de Dados
Adotamos políticas rígidas de proteção de dados sensíveis em conformidade com as boas práticas de privacidade:
*   **Exposição de CPF**: O CPF real nunca é impresso em mensagens de erro de validação, logs de sistema ou auditoria técnica. Em todas as DTOs e retornos públicos da API (como `CadastrarClienteResult`, `ConsultarClienteResult` e `ClienteListagemItemResult`), o CPF é retornado mascarado no formato `***.***.***-XX` (exibindo apenas os 2 últimos dígitos).
*   **Email e Telefone**: Os logs de auditoria e tabelas de log guardam metadados enxutos das alterações sem expor os valores completos de e-mail e telefone, preservando a intimidade dos dados de contato do cliente.
*   **Exibição de CPF Completo**: Fica declarado que qualquer futura necessidade de visualização do CPF completo por parte de um usuário exigirá um endpoint dedicado, permissão específica de negócio, justificativa de auditoria e controle de logs de acesso individualizados.

---

## 6. Proibição de Deleção Física (DELETE)
Garantimos deterministicamente que a API não expõe ações de deleção física:
*   Não existe nenhum método `DELETE`, `HttpDelete` ou handler para remoção física de clientes.
*   O teste `Delete_MetodoProibido_DeveRetornar405` na API realiza uma chamada `DELETE` na rota `/api/clientes/1` e assinala que o status de retorno é exatamente `405 Method Not Allowed`.

---

## 7. Inventário Geral de Testes (147 Testes)
A suíte completa de testes contém **147 testes** (100% aprovados):

*   `WebApolice.Modulos.Clientes.Tests` (44 testes)
    *   **Domínio**: Validações complexas de Nome (espaços, tamanho, acentuação), CPF (DV inválido, repetidos, formato), Data de Nascimento (futuro, nulo), Email/Telefone (tamanho, formato), Status e Código Legado.
    *   **Aplicação**: Cadastro (sucesso, duplicado prévio, erro de banco), Alteração (restrição a CPF, rollback, inexistente) e Listagem (filtros combinados, ordenação, paginação).
*   `WebApolice.Integration.Tests` (10 testes)
    *   **PostgreSQL**: Mapeamento de constraints físicos, comportamento de concorrência com threads reais.
    *   **Atomicidade**: Rollbacks comprovados no PostgreSQL real quando o registro de auditoria falha ou a inserção no módulo de clientes falha.
*   `WebApolice.Api.Tests` (65 testes)
    *   **Autorização**: Respostas `401 Unauthorized` para não-autenticados, `403 Forbidden` para operadores tentando ações restritas a gestores, e sucessos por perfil (Operador, Gestor, Admin).
    *   **Endpoints**: DELETE determinístico retornando 405.
*   `WebApolice.Architecture.Tests` (20 testes)
    *   **Arquitetura**: Proteção das dependências do Domain e Application (sem EF Core, sem ASP.NET Core, sem Npgsql). Garantia de que `SharedKernel` não contém nenhuma política ou dependência de autorização HTTP.
*   `WebApolice.Auditoria.Tests` (8 testes)
    *   Validação de regras de negócio de auditoria e persistência.

---

## 8. Segurança e Vulnerabilidades do Backend
A auditoria de pacotes executada via CLI `dotnet list package --vulnerable --include-transitive` retornou com **exit code 0** (sucesso) e reportou **Zero** vulnerabilidades conhecidas diretas ou transitivas em todos os projetos da solução backend `.slnx`.

---

## 9. Validação do Frontend
O ecossistema frontend em `apps/web` foi validado com êxito:
*   `npm ci`: Instalou 242 dependências sem erros.
*   `npm run lint`: Verificação com ESLint executada com sucesso (zero erros/warnings).
*   `npm run test -- --run`: Vitest aprovou os testes com sucesso.
*   `npm run build`: O compilador `tsc` e a ferramenta de build `vite` geraram o pacote de produção sem falhas (`dist/assets/index-*.js` criado).
