# Infraestrutura Local de Desenvolvimento

Este diretório contém a especificação da infraestrutura local de apoio do **WebApolice** utilizando Docker Compose. Ela engloba os serviços do PostgreSQL e do Keycloak necessários para a execução local do ERP.

---

## 1. Pré-requisitos

Para executar os containers locais, certifique-se de ter instalado em sua máquina:
1. **Docker Engine / Docker Desktop** (versão recomendada v24+ ou v25+).
2. **Docker Compose CLI** (incluso nas instalações modernas do Docker).

---

## 2. Inicialização Rápida

### Passo 1: Configurar Variáveis de Ambiente
Crie uma cópia do arquivo `.env.example` (localizado na raiz do repositório) com o nome `.env`:
```bash
cp ../.env.example ../.env
```
Abra o arquivo `../.env` recém-criado e altere os valores padrão de senhas (`alterar_localmente`) para credenciais seguras da sua escolha. **Nunca compartilhe ou versione o arquivo `.env`.**

### Passo 2: Subir os Serviços
Dentro deste diretório (`infrastructure/`), execute:
```bash
docker compose --env-file ../.env up -d
```

---

## 3. Comandos Úteis de Operação

### Consultar Status dos Containers
```bash
docker compose --env-file ../.env ps
```

### Visualizar Logs em Tempo Real
* **PostgreSQL**:
  ```bash
  docker compose --env-file ../.env logs -f postgres
  ```
* **Keycloak**:
  ```bash
  docker compose --env-file ../.env logs -f keycloak
  ```

### Parar os Serviços (Mantendo os Dados)
```bash
docker compose --env-file ../.env down
```
> [!NOTE]
> Este comando desliga os containers, mas preserva todos os dados salvos nos bancos de dados, pois utiliza o volume persistente nomeado `postgres_data`.

### Parar os Serviços e Remover os Dados (Aviso de Perda de Dados)
```bash
docker compose --env-file ../.env down -v
```
> [!WARNING]
> O uso da flag `-v` remove o volume persistente `postgres_data` associado. Todos os dados inseridos localmente no PostgreSQL e no Keycloak serão permanentemente perdidos.

---

## 4. Detalhes de Conectividade e URLs

| Serviço | Porta no Host | Interface de Bind | URL / Acesso |
| :--- | :--- | :--- | :--- |
| **PostgreSQL** | `5432` | `127.0.0.1` | `localhost:5432` (Ferramentas de banco) |
| **Keycloak** | `8080` | `127.0.0.1` | [http://localhost:8080](http://localhost:8080) (Console Admin) |

### Bancos de Dados e Usuários Criados

O script de inicialização cria as seguintes credenciais lógicas isoladas no PostgreSQL:
1. **ERP Database**:
   * Banco: `webapolice`
   * Proprietário: `webapolice_app` (Senha configurada no seu `.env`)
2. **Keycloak Database**:
   * Banco: `keycloak`
   * Proprietário: `keycloak_app` (Senha configurada no seu `.env`)
3. **Superusuário Administrativo** (Usado apenas na inicialização / manutenção):
   * Usuário: `webapolice_admin` (Senha configurada no seu `.env`)

---

## 5. Funcionamento dos Scripts de Inicialização

* O script [01-create-databases.sh](file:///c:/PROJETOS/Rsul%20Automacoes/Projetos/webapolice/infrastructure/postgres/init/01-create-databases.sh) é montado como **somente leitura** em `/docker-entrypoint-initdb.d` do PostgreSQL.
* Este script é executado **apenas na primeira inicialização do volume** (quando o volume `postgres_data` está vazio). Alterações posteriores no script não serão aplicadas a volumes já existentes.
* Se precisar re-executar a inicialização completa do script de banco, pare o ambiente com `docker compose --env-file ../.env down -v` e suba novamente.

---

## 6. Diagnóstico e Resolução de Problemas

### Diagnóstico de Portas Ocupadas
Se o Docker Compose falhar ao iniciar relatando que a porta `5432` ou `8080` já está alocada, execute no terminal do Windows para identificar o processo causador:
```powershell
Get-NetTCPConnection -LocalPort 5432
Get-NetTCPConnection -LocalPort 8080
```
* **Solução**: Você pode parar o serviço local que está alocando a porta ou alterar o mapeamento da porta local no arquivo `../.env` (ex: `POSTGRES_PORT=5433` ou `KEYCLOAK_PORT=8081`). O Compose redirecionará dinamicamente sem quebrar a rede interna.

### Diagnóstico de Containers `unhealthy`
Se um container apresentar estado de saúde instável ou permanentemente `unhealthy`, verifique os logs para detalhes específicos:
1. Verifique se o PostgreSQL inicializou as permissões do usuário do Keycloak corretamente.
2. Certifique-se de que o Keycloak não está em loop de reconexão de banco.
3. Se o Keycloak falhar repetidamente no health check de rede interna: confirme se o parâmetro `KC_HEALTH_ENABLED: "true"` está presente nas configurações do serviço.

---

## 7. Alerta de Produção

> [!CAUTION]
> **Esta configuração de Docker Compose e variáveis de ambiente é destinada única e exclusivamente para o ambiente local de desenvolvimento (start-dev). Ela não é segura para implantações de produção, pois carece de configurações de criptografia de rede (TLS/HTTPS), redundância de dados e restrições rígidas adicionais de firewall.**
