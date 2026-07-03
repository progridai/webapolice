# Infraestrutura Local de Desenvolvimento (04-infraestrutura-local.md)

Este documento detalha a topologia, fluxo de inicialização e responsabilidades da infraestrutura de desenvolvimento local do ERP **WebApolice**, baseada em Docker Compose.

---

## 1. Topologia e Fluxo de Dados

A arquitetura local foi desenhada para isolar os dados lógicos do ERP e da segurança, mantendo a simplicidade de operação em um único servidor físico de banco. A API e a aplicação Web (Frontend) comunicam-se com a infraestrutura local através das portas expostas em localhost (`127.0.0.1`).

### Diagrama da Arquitetura Local

```text
Navegador
   |
   +--> Keycloak :8080 (Container docker)
   |
Ferramenta de banco / API Backend (futuro)
   |
   +--> PostgreSQL :5432 (Container docker)
            |
            +--> banco: webapolice (ERP)
            |
            +--> banco: keycloak (Identity provider)
```

> [!IMPORTANT]
> **API Desconectada**: Nesta etapa técnica de infraestrutura, a API backend e o frontend React continuam rodando localmente na máquina (fora do Docker) e **ainda não estão conectados** ao banco PostgreSQL nem ao Keycloak. A configuração de conexão e a autenticação funcional serão abordadas em etapas posteriores.

---

## 2. Responsabilidades dos Serviços

### A. PostgreSQL (`postgres`)
* **Versão Oficial**: `postgres:18.4`.
* **Escopo**: Armazenamento relacional de todas as entidades de negócio do ERP e persistência do esquema interno do Keycloak.
* **Isolamento**: Utiliza o script de inicialização `01-create-databases.sh` para provisionar dois bancos e usuários independentes:
  1. **Banco `webapolice`**: Proprietário: `webapolice_app`. Utilizado pela API do ERP.
  2. **Banco `keycloak`**: Proprietário: `keycloak_app`. Utilizado pelo Keycloak.
  * O usuário `webapolice_app` não possui acesso de leitura ou escrita ao banco `keycloak`.
  * O usuário `keycloak_app` não possui acesso de leitura ou escrita ao banco `webapolice`.
  * Apenas o superusuário de inicialização (`webapolice_admin`) possui privilégios administrativos totais.

### B. Keycloak (`keycloak`)
* **Versão Oficial**: `quay.io/keycloak/keycloak:26.6.4`.
* **Escopo**: Gestão de identidades e acessos (Identity and Access Management - IAM).
* **Conexão**: Comunica-se internamente com o PostgreSQL usando a rede virtual do Docker, resolvendo o host de banco pelo nome de serviço `postgres`.
* **Modo de Execução**: `start-dev`. Projetado para o ciclo ágil de desenvolvimento, dispensando pré-requisitos complexos de produção (como HTTPS obrigatório na camada do container).

---

## 3. Rede e Persistência

* **Rede (`webapolice_network`)**: Rede privada do tipo *bridge* que permite a comunicação direta e segura entre o Keycloak e o PostgreSQL via nomes lógicos dos serviços Docker, sem expor os canais de rede internos da rede física do host.
* **Volume (`postgres_data`)**: Volume nomeado utilizado para persistir o diretório `/var/lib/postgresql/data`. Isso garante que a exclusão dos containers (ex: `docker compose down`) não delete os dados gerados, simulando o comportamento de um banco corporativo.

---

## 4. Fluxo de Inicialização Seguro

1. **Leitura de Variáveis**: O Docker Compose lê o arquivo local `.env` (gerado a partir de `.env.example`).
2. **Subida do PostgreSQL**: O container `postgres` é instanciado.
3. **Provisionamento Inicial**:
   * Se o diretório de dados estiver vazio (primeira execução), o script `01-create-databases.sh` montado sob `/docker-entrypoint-initdb.d` é acionado.
   * O script cria as roles, bancos, atribui os donos correspondentes e revoga os privilégios públicos para evitar acessos cruzados.
4. **Verificação de Saúde (Health Check) do PostgreSQL**:
   * O comando `pg_isready` monitora a saúde do banco administrativo a cada 10 segundos.
5. **Bloqueio do Keycloak**:
   * O serviço `keycloak` aguarda o PostgreSQL atingir o estado `healthy` devido à declaração `depends_on`.
6. **Subida do Keycloak**:
   * O container `keycloak` inicia em modo `start-dev`, aplica as credenciais administrativas e conecta-se ao banco `keycloak` no PostgreSQL.
7. **Verificação de Saúde do Keycloak**:
   * O health check via sockets TCP (`/dev/tcp`) monitora a resposta `200 OK` do endpoint `/health/live` do Keycloak na porta `9000` (porta de gerenciamento interna do Keycloak 26).

---

## 5. Diferenças entre Desenvolvimento (Local) e Produção

| Característica | Ambiente Local (Docker Compose) | Ambiente de Produção |
| :--- | :--- | :--- |
| **Topologia de Banco** | Único servidor PostgreSQL contendo múltiplos bancos. | Instâncias gerenciadas isoladas ou clusters separados. |
| **Modo Keycloak** | `start-dev` (HTTP na porta 8080). | `start` (HTTPS obrigatório, certificados válidos). |
| **Proxy Reverso** | Não utilizado localmente. | Nginx/Traefik/Cloud Load Balancer gerenciando TLS. |
| **Segredos** | Armazenados localmente no `.env` (não versionado). | Injetados por cofre de segredos (ex: Azure Key Vault). |

---

## 6. O que NÃO foi implementado nesta etapa

* **Tabelas de Negócio / Migrations**: O banco de dados está vazio. Nenhuma tabela operacional foi gerada.
* **Conexões da Aplicação**: A API backend e o frontend React continuam desconectados de banco e autenticação.
* **Configuração de Realms e Clientes**: O Keycloak possui apenas a conta padrão de administrador e o realm inicial básico do sistema. Nenhum cliente do ERP foi cadastrado.
* **Outros Serviços de Apoio**: Redis, n8n, mensageria e painéis adicionais de gerência (como pgAdmin ou observabilidade) não fazem parte desta entrega técnica de infraestrutura.
