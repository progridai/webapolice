# ADR-005: Infraestrutura Local com Docker Compose (ADR-005-infraestrutura-local-docker.md)

* **Status**: Aceito
* **Data**: 2026-07-03
* **Autor**: Equipe de Desenvolvimento / Antigravity

---

## 1. Decisão
Adotar o **Docker Compose** como o mecanismo oficial para provisionar, gerenciar e executar a infraestrutura de suporte local de desenvolvimento. A infraestrutura inicial consiste de:
* Um container de banco de dados rodando **PostgreSQL 18.4**.
* Dois bancos de dados separados rodando no mesmo container PostgreSQL (um para a aplicação ERP `webapolice` e outro para o `keycloak`).
* Dois usuários de banco distintos (com privilégios limitados a seu respectivo banco, sem permissões cruzadas).
* Um container **Keycloak 26.6.4** configurado em modo de desenvolvimento (`start-dev`) e conectado a seu banco de dados próprio no PostgreSQL local.
* Persistência de dados local baseada em volumes nomeados do Docker (`postgres_data`).
* Comunicação entre containers efetuada sob uma rede privada do Docker (`webapolice_network`).
* Vinculação de portas de acesso exclusivamente para o host local (`127.0.0.1`), garantindo que os serviços não fiquem expostos na rede externa.

## 2. Contexto e Motivação
A fundação técnica inicial foi criada e homologada, e a próxima etapa exige persistência relacional (PostgreSQL) e gestão de identidades (Keycloak). Para evitar que os desenvolvedores precisem instalar manualmente múltiplos SGBDs ou servidores locais que possam gerar conflitos de versão e sistema operacional, o uso de containers Docker garante a paridade de ambiente e reprodutibilidade imediata em qualquer máquina de desenvolvimento.

A decisão de executar um único servidor PostgreSQL local contendo dois bancos independentes foi tomada para simplificar o consumo de memória no ambiente de desenvolvimento local, sem acoplar a arquitetura lógica, uma vez que cada serviço consome credenciais e esquemas estritamente separados.

## 3. Justificativas
* **Paridade de Ambiente**: Evita o clássico problema de "funciona na minha máquina".
* **Isolamento de Credenciais**: O ERP e o Keycloak utilizam usuários lógicos específicos com privilégios restritos (o ERP não acessa o banco do Keycloak e vice-versa).
* **Segurança na Rede Local**: A vinculação exclusiva a `127.0.0.1` impede conexões indesejadas de outras máquinas da mesma rede local.
* **Persistência Segura**: Os dados persistem no volume nomeado `postgres_data`, permitindo parar e iniciar containers sem perda de dados locais, simulando o comportamento de um banco de dados real.

## 4. Diferença entre Topologia Local e de Produção
* **Topologia Local**: Um único container PostgreSQL serve os dois bancos (`webapolice` e `keycloak`) para economizar memória e simplificar o provisionamento local. Keycloak executa em modo `start-dev` sem HTTPS habilitado na camada do container.
* **Produção**: O banco do ERP e o banco do Keycloak devem rodar em servidores (ou instâncias de banco gerenciadas como RDS/Cloud SQL) fisicamente isolados e altamente disponíveis. O Keycloak deve rodar em modo de produção (`start`), atrás de um proxy reverso com HTTPS ativo e certificados válidos.

## 5. Riscos e Mitigações
* **Conflitos de Portas**: Portas `5432` ou `8080` podem já estar em uso por serviços locais nativos do desenvolvedor.
  * *Mitigação*: Documentar no README como alterar as portas locais via arquivo `.env` sem alterar a especificação do `docker-compose.yml`.
* **Inicialização Assíncrona (Race Conditions)**: O Keycloak pode tentar se conectar ao PostgreSQL antes de o banco estar pronto para receber requisições.
  * *Mitigação*: Uso de `healthcheck` oficial via `pg_isready` no container do PostgreSQL e dependência condicional `service_healthy` no Keycloak.

## 6. Consequências
* O desenvolvedor local precisará ter o Docker e Docker Compose instalados na máquina de trabalho para executar a infraestrutura de apoio.
* A API e o frontend continuam rodando localmente (fora de containers) nesta fase técnica, facilitando a depuração imediata e o ciclo de feedback rápido (*hot reload*), consumindo o banco e o Keycloak via `127.0.0.1`.

## 7. Alternativas Consideradas
* **Provisionamento Nativo**: Instalar PostgreSQL e Keycloak diretamente no Windows/Linux/macOS de cada programador. Rejeitado pelo alto custo de configuração e conflitos frequentes de versão.
* **Containers PostgreSQL Separados**: Subir dois containers PostgreSQL distintos locais. Rejeitado pelo consumo desnecessário de memória e processamento na máquina local dos desenvolvedores.

## 8. Critérios para Futura Revisão
Esta topologia será revisada quando:
1. Houver necessidade de testar cenários complexos de failover ou alta disponibilidade locais.
2. For iniciada a especificação de pipelines de CI/CD baseados em containers temporários.
