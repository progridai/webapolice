# Estratégia de Migração (03-estrategia-de-migracao.md)

Este documento descreve a estratégia de transição, coexistência e migração de dados do ERP legado (VB.NET / SQL Server) para o novo ERP **WebApolice** (ASP.NET Core / PostgreSQL).

---

## 1. Abordagem Geral da Migração

### A. Migração Progressiva
A substituição do ERP legado não ocorrerá em um único dia por meio de um corte total (*Big Bang*). Adotaremos uma estratégia de migração incremental e progressiva por módulos funcionais, minimizando os riscos operacionais e garantindo a continuidade do negócio.

### B. Convivência Temporária de Sistemas
O ERP legado e o novo ERP WebApolice coexistirão em produção por um período determinado. Durante essa fase, usuários utilizarão ambos os sistemas para tarefas distintas, dependendo do módulo migrado.

### C. Migração por Módulos
O escopo do sistema será dividido em fronteiras de domínio lógico. Cada módulo será migrado individualmente seguindo a estratégia:
1. Implementação do novo módulo no WebApolice.
2. Migração inicial dos dados históricos daquele domínio do SQL Server para o PostgreSQL.
3. Sincronização e transição de uso para o novo sistema.
4. Desativação da respectiva tela/função no sistema legado.

> [!IMPORTANT]
> **A ordem de priorização e sequência de migração de cada módulo ainda será definida em planejamentos estratégicos posteriores.**

---

## 2. Estratégia de Banco de Dados e Remodelagem

### A. Não Copiar o Modelo de Dados Antigo
O modelo de tabelas do banco SQL Server não deve ser clonado de forma idêntica para o PostgreSQL. O banco legado carrega decisões históricas obsoletas e desnormalizações ou normalizações ineficientes. A nova base PostgreSQL deve ser projetada do zero baseada na modelagem de domínios moderna da Clean Architecture.

### B. Remodelagem Orientada ao Domínio
As tabelas serão desenhadas em conformidade com as regras de negócio atuais e projeções de escala futuras, respeitando a separação física ou lógica de esquemas para cada módulo.

---

## 3. Coexistência de Dados e Sincronização

### A. Fonte Oficial do Dado (Single Source of Truth)
Para cada entidade e dado de negócio, deve ser definido de forma explícita qual sistema é o detentor oficial da escrita em cada momento da transição:
* **Módulo não migrado**: O ERP Legado (SQL Server) é o dono da escrita. O WebApolice apenas lê uma réplica ou consome via sync.
* **Módulo migrado**: O WebApolice (PostgreSQL) assume o controle oficial de escrita. O legado passa a ler dados sincronizados a partir dele.

### B. Evitar Escrita Concorrente Não Controlada
Para mitigar o risco de conflitos e corrupção de dados, a escrita concorrente na mesma tabela física por ambos os sistemas deve ser evitada. Caso um dado precise ser alterado por ambos os fluxos, a modificação deve ser centralizada em uma API explícita com tratamento de conciliação.

### C. Processos Explícitos de Sincronização
As sincronizações de dados históricos ou operacionais durante o período de transição devem utilizar processos bem delimitados:
* APIs REST estruturadas no backend do novo ERP.
* Workers .NET para cargas em lote e processamentos volumosos de dados com validação.
* Fluxos controlados no n8n apenas para sincronizações não críticas e notificações externas.

### D. Conciliação e Registro de Divergências
Os processos de migração de dados devem incluir auditoria automatizada pós-carga para verificar se os saldos, apólices e informações migradas batem perfeitamente com a origem. Qualquer divergência de dados detectada deve ser registrada em logs dedicados de reconciliação para rápida atuação da equipe técnica.

---

## 4. Mitigação de Falhas e Rollback

### A. Previsão de Rollback por Etapa
Cada implantação de novo módulo deve possuir um plano de contingência (*Rollback*) documentado e testado. Se o novo módulo apresentar falhas catastróficas em produção, deve ser possível reverter a operação das escritas para o sistema antigo sem perda de dados inseridos no período de testes.

### B. Isolamento de Falhas
Falhas em um novo módulo migrado no WebApolice não devem indisponibilizar funcionalidades críticas que ainda dependem do sistema legado e vice-versa.
