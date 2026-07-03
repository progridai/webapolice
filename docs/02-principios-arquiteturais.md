# Princípios Arquiteturais (02-principios-arquiteturais.md)

Este documento define os princípios arquiteturais obrigatórios que devem reger toda a concepção, codificação, evolução e manutenção do ERP **WebApolice**.

---

## 1. Organização e Acoplamento

### A. Módulos Organizados por Domínio
O sistema backend deve ser desenvolvido como um monólito modular. Isso significa que a organização física e lógica de pastas e namespaces deve refletir os domínios de negócio reais, e não divisões puramente técnicas (como ter todas as controllers do sistema em um único projeto e todas as entidades em outro). Cada módulo do ERP representa um domínio autocontido de negócio.

### B. Baixo Acoplamento entre Módulos
Módulos não devem acessar diretamente as tabelas do banco de dados ou classes internas de outros módulos. Qualquer dependência entre domínios deve ser resolvida por meio de contratos de comunicação explícitos (interfaces públicas, APIs internas ou eventos em memória).

### C. Nenhuma Adoção Prematura de Microsserviços
A arquitetura inicial permanecerá como um monólito modular executando em um único processo lógico do backend. A quebra em múltiplos serviços físicos de rede (microsserviços) só será avaliada em caso de gargalos extremos de escalabilidade técnica, devido à alta complexidade de infraestrutura e latência associadas.

---

## 2. Separação de Responsabilidades e Regras de Negócio

### A. Nenhuma Regra de Negócio Relevante no Frontend
O frontend baseado em React e Vite serve puramente como uma camada de interação com o usuário, apresentação visual e gerenciamento de estado local da UI. Cálculos complexos, lógica fiscal, apuração de contratos ou qualquer tipo de validação de regras de apólices pertencem exclusivamente ao backend.

### B. Nenhuma Integração Externa Executada pelo Frontend
O frontend do ERP nunca deve realizar requisições diretas a Web Services de seguradoras, gateways de pagamento, APIs de bancos ou quaisquer sistemas terceiros. Todas as integrações externas pertencem ao backend do ERP, que centraliza chamadas, valida credenciais e protege tokens de acesso.

### C. Regras Financeiras e Críticas pertencem ao Backend
Toda a lógica financeira, conciliação, geração de parcelas e regras regulatórias críticas devem ser centralizadas e processadas pelo backend (API principal e Workers .NET) garantindo o uso de transações de banco de dados apropriadas.

### D. Nenhuma Consulta Direta do Bravito ao Banco de Dados do ERP
O aplicativo complementar **Bravito** jamais lerá ou gravará dados diretamente no PostgreSQL. O acesso a informações do ERP ocorrerá unicamente por meio do consumo de endpoints de API expostos e protegidos pelo backend do WebApolice.

---

## 3. Simplicidade e Legibilidade de Código

### A. Abstrações apenas com Benefício Concreto
Evite a criação de padrões de design desnecessários (como repositórios genéricos vazios sobre o Entity Framework, ou interfaces que possuem apenas uma única implementação concreta sem finalidade de teste ou desacoplamento). O excesso de abstração dificulta a leitura direta do código.

### B. Preferência por Código Simples, Explícito e Testável
O código escrito deve priorizar a clareza sobre o preciosismo sintático. Estruturas simples, fáceis de ler, manter e depurar são preferíveis a truques complexos de programação. Toda lógica de domínio deve ser facilmente isolável para a escrita de testes unitários rápidos.

---

## 4. Confiabilidade, Auditoria e Segurança

### A. Logs, Auditoria e Rastreabilidade desde o Início
Nenhum fluxo de alteração de dados deve subir para produção sem log de auditoria associado. O sistema deve rastrear quem alterou, quando alterou e qual foi a alteração em todas as operações de escrita no banco de dados.

### B. Segurança por Padrão (Secure by Design)
Toda funcionalidade exposta deve assumir o princípio do menor privilégio. A validação de identidade e verificação de regras de permissão (Roles/Policies) devem ser sempre processadas na borda do backend para cada requisição recebida, não confiando apenas na ocultação de elementos visuais no frontend.

### C. Tratamento de Idempotência em Integrações
Qualquer comunicação de integração externa ou processamento de lotes financeiros pelos Workers deve ser projetada para suportar reexecuções seguras em caso de falha (idempotência), evitando duplicações indesejadas de cobranças ou apólices.

---

## 5. Manutenibilidade e Evolução do Projeto

### A. Migrations Versionadas
Qualquer alteração na estrutura do PostgreSQL deve ser efetuada exclusivamente por meio de arquivos de Migration versionados via ferramenta de linha de comando no backend (ex: Entity Framework Core Migrations). Alterações manuais diretamente na base de dados de homologação ou produção são terminantemente proibidas.

### B. Documentação Atualizada junto com o Código
Se uma alteração de código modifica um comportamento arquitetural, remove um domínio ou substitui uma API essencial, o desenvolvedor é responsável por atualizar os documentos na pasta `docs/` e os templates do diretório `prompts/` no mesmo commit.

### C. Validação Arquitetural via Testes
* O acoplamento e o direcionamento correto de dependências (como a proibição de dependências do SharedKernel para a API) serão monitorados via testes arquiteturais baseados em reflexão (`WebApolice.Architecture.Tests`).
* Esses testes não impedem fisicamente a compilação isolada dos assemblies de forma nativa. O seu papel é de detecção e relatório de violações no momento da execução da suíte de testes.
* O bloqueio efetivo de alterações que violem a arquitetura será garantido pelo pipeline de integração contínua (CI), que rejeitará as alterações se os testes falharem.
* Além disso, referências indevidas devem ser evitadas ativamente pela própria estrutura e configuração de referências de projeto dos arquivos `.csproj`.
