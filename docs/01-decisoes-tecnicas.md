# Decisões Técnicas (01-decisoes-tecnicas.md)

Este documento registra as decisões de tecnologia e arquitetura de software adotadas para o novo ERP **WebApolice**, apresentando as justificativas, riscos e alternativas avaliadas para cada escolha.

> [!NOTE]
> Para manter a flexibilidade de evolução inicial, nenhuma versão de software ou biblioteca está fixada nesta etapa documental.

---

## 1. React com TypeScript no Frontend

* **Decisão**: Utilização do ecossistema React aliado à tipagem estática e segura do TypeScript para o desenvolvimento da interface web responsiva do ERP.
* **Justificativa**: React é amplamente adotado no mercado, possui um vasto ecossistema de componentes prontos e, em conjunto com o TypeScript, mitiga erros comuns em tempo de compilação, melhorando a manutenibilidade do código por times de desenvolvimento de médio e grande porte.
* **Benefícios**:
  * Alta componentização e reutilização de código de UI.
  * Autocompletação avançada e facilidade de refatoração garantida pelo compilador do TypeScript.
  * Grande disponibilidade de profissionais e documentação no mercado.
* **Riscos**:
  * Curva de aprendizado inicial para desenvolvedores acostumados apenas com o padrão de renderização do VB.NET/ASP.NET clássico.
  * Complexidade de gerenciamento de estado global se não for bem modelado.
* **Consequências**: Todo o código visual do ERP deve ser componentizado em React e estritamente tipado. O uso de tipagem `any` ou código JS vanilla sem tipagem é proibido.
* **Alternativas Consideradas**: Angular (rejeitado devido à curva de aprendizado mais íngreme e rigidez estrutural) e Vue.js (rejeitado pelo menor ecossistema corporativo local se comparado ao React).

---

## 2. Vite como Ferramenta de Construção (Build Tool)

* **Decisão**: Utilização do Vite para empacotamento, transpilação e servidor de desenvolvimento do frontend React.
* **Justificativa**: Vite oferece um ciclo de feedback extremamente rápido durante o desenvolvimento por meio de Hot Module Replacement (HMR) baseado em ES Modules nativos, além de compilações de produção altamente otimizadas via Rollup.
* **Benefícios**:
  * Inicialização e atualizações quase instantâneas no servidor local de desenvolvimento.
  * Configuração simples e direta se comparado ao Webpack manual.
* **Riscos**:
  * Menor compatibilidade imediata com plug-ins de legado absoluto do Webpack (embora o ecossistema do Vite possua equivalentes para a quase totalidade das necessidades).
* **Consequências**: Os desenvolvedores devem utilizar a CLI do Vite para rodar localmente e realizar o build de distribuição estática.
* **Alternativas Consideradas**: Create React App (rejeitado por estar obsoleto e ter performance de build inferior) e Webpack configurado manualmente (rejeitado pelo custo de manutenção de arquivos de configuração complexos).

---

## 3. ASP.NET Core no Backend

* **Decisão**: Utilizar o framework ASP.NET Core para a construção da API REST principal e lógica de negócios do ERP.
* **Justificativa**: A plataforma .NET moderna oferece excelente performance, suporte nativo a múltiplos sistemas operacionais, facilidade de integração em container Docker e simplifica o reaproveitamento de conceitos conhecidos pelo time que trabalhava com o legado VB.NET.
* **Benefícios**:
  * Altíssimo desempenho e baixo consumo de recursos de memória.
  * Forte ecossistema corporativo com suporte a injeção de dependência nativa e middlewares.
  * Facilidade de contratação e transição de desenvolvedores .NET.
* **Riscos**:
  * O time de desenvolvimento precisará se adaptar às mudanças do ecossistema .NET Core moderno se comparado ao antigo .NET Framework tradicional.
* **Consequências**: Toda a lógica de negócios centralizada e persistência do ERP pertencerá a uma API desenvolvida em C# utilizando ASP.NET Core.
* **Alternativas Consideradas**: Node.js/NestJS (rejeitado para aproveitar a familiaridade prévia do time com ecossistema Microsoft/C#) e Java/Spring Boot (rejeitado pela ausência de profissionais Java na equipe).

---

## 4. PostgreSQL como Banco de Dados

* **Decisão**: Adoção do PostgreSQL como o sistema gerenciador de banco de dados relacional (SGBD) oficial do novo ERP.
* **Justificativa**: O PostgreSQL é um banco de dados de código aberto altamente maduro, robusto, compatível com padrões SQL rigorosos, com suporte nativo avançado a dados JSONB (quando necessário) e sem os custos de licenciamento do SQL Server corporativo.
* **Benefícios**:
  * Gratuito, sem custos de licenças proprietárias.
  * Suporte a alta concorrência e integridade referencial.
  * Ampla gama de ferramentas de monitoramento e fácil execução em ambiente conteinerizado.
* **Riscos**:
  * Necessidade de adaptar queries complexas ou Stored Procedures escritas em T-SQL (SQL Server) para PL/pgSQL (PostgreSQL).
* **Consequências**: Toda a nova persistência do ERP deve ser modelada do zero pensando em PostgreSQL. O uso de recursos proprietários do SQL Server está banido do novo código.
* **Alternativas Consideradas**: Manutenção do SQL Server (rejeitado pelo alto custo de licenciamento em escala) ou bancos não-relacionais como MongoDB (rejeitados por não serem adequados como banco transacional principal de um ERP).

---

## 5. Keycloak para Gestão de Identidade

* **Decisão**: Utilização do Keycloak como servidor centralizado de gerenciamento de identidades e acesso (IAM) utilizando protocolos OpenID Connect (OIDC) e OAuth 2.0.
* **Justificativa**: Centralizar o controle de autenticação em uma ferramenta de mercado consagrada evita o desenvolvimento caseiro de segurança e expõe uma solução unificada de Single Sign-On (SSO) para o ERP, o Bravito e eventuais portais adicionais.
* **Benefícios**:
  * Redução no esforço de desenvolvimento de fluxos de login, recuperação de senha e MFA (autenticação de dois fatores).
  * Fácil integração de novos aplicativos parceiros no futuro.
  * Painel administrativo completo para gestão de usuários.
* **Riscos**:
  * Adição de mais um componente crítico na infraestrutura cuja indisponibilidade paralisa o login em todos os sistemas.
* **Consequências**: O ERP não armazenará hashes de senhas de usuários em seu banco de dados principal. As telas de login redirecionarão ou consumirão o fluxo do Keycloak.
* **Alternativas Consideradas**: Autenticação customizada em banco (rejeitada por riscos de segurança e retrabalho) e serviços SaaS como Auth0 (rejeitados por custos recorrentes indexados ao volume de usuários).

---

## 6. Monólito Modular como Arquitetura Inicial

* **Decisão**: Estruturar o backend em um monólito modular, onde os diferentes domínios de negócio são isolados em módulos lógicos bem delimitados de código, mas executados sob um único processo de aplicação.
* **Justificativa**: Evita a complexidade operacional, de rede e de consistência distribuída de microsserviços na fase inicial de reescrita, mantendo a simplicidade de implantação de um monólito enquanto garante separação de conceitos.
* **Benefícios**:
  * Facilidade de desenvolvimento, testes locais e publicação (deploy).
  * Baixa latência em chamadas inter-módulos (chamadas em memória).
  * Preparação nativa para uma futura separação em microsserviços, caso seja necessário, graças às fronteiras de domínio bem definidas.
* **Riscos**:
  * Possibilidade de desenvolvedores quebrarem as regras de isolamento de domínios via acoplamento de código direto se não houver revisão e linting rigorosos.
* **Consequências**: Comunicação direta entre bancos de módulos diferentes é proibida. Módulos devem interagir apenas por contratos públicos definidos (interfaces/APIs ou eventos em memória).
* **Alternativas Consideradas**: Microsserviços desde o início (rejeitado devido à altíssima complexidade de rede e gerência de infraestrutura prematura).

---

## 7. n8n para Orquestrações Flexíveis e Automulações Não Críticas

* **Decisão**: Adoção do n8n como ferramenta de orquestração de fluxos de trabalho (Workflows) de baixa criticidade ou integrações externas flexíveis.
* **Justificativa**: O n8n permite que integrações pontuais, envios de notificações, sincronizações periódicas simples e fluxos passíveis de alteração rápida por analistas de suporte sejam desenhados visualmente com baixo esforço de codificação.
* **Benefícios**:
  * Redução da carga de codificação no core do ERP para fluxos secundários.
  * Interface visual facilitando a depuração e monitoramento de fluxos integrados.
* **Riscos**:
  * Risco de fragmentação de lógica de negócios essencial fora do ERP se a ferramenta for usada de forma indisciplinada.
* **Consequências**: Regras de cálculo financeiro, emissão de apólices e auditorias críticas de estado nunca devem ser delegadas ao n8n. O n8n é estritamente um orquestrador complementar.
* **Alternativas Consideradas**: Codificação manual de todas as tarefas de segundo plano no core do backend (rejeitado pela perda de agilidade na manutenção diária de fluxos secundários).

---

## 8. Workers .NET para Tarefas Críticas, Longas ou Volumosas

* **Decisão**: Utilização de Background Services (.NET Workers) rodando de forma assíncrona para o processamento de tarefas pesadas de retaguarda que necessitem de integridade de transações.
* **Justificativa**: Tarefas críticas de lote (como importação massiva de apólices, conciliações bancárias robustas e faturamentos recorrentes) exigem a robustez transacional e a tipagem do C# direto no banco PostgreSQL.
* **Benefícios**:
  * Execução assíncrona, evitando gargalos de requisições HTTP na API principal do ERP.
  * Compartilhamento direto das entidades de domínio e lógicas de validação já escritas no monólito modular do backend.
* **Riscos**:
  * Necessidade de controle rígido de concorrência e conciliação de estados no banco de dados.
* **Consequências**: Processamentos pesados de escrita concorrente devem ser enfileirados ou processados via Workers .NET específicos.
* **Alternativas Consideradas**: Executar processamento pesado diretamente na Thread da requisição HTTP (rejeitado por degradar a experiência do usuário e estourar timeouts).

---

## 9. Docker para Execução e Publicação

* **Decisão**: Uso de containers Docker para empacotar, distribuir e executar o frontend, backend, Keycloak, n8n e demais componentes.
* **Justificativa**: Garante paridade absoluta entre o ambiente de desenvolvimento local dos programadores e o ambiente de homologação e produção final, mitigando problemas clássicos de configuração ambiental.
* **Benefícios**:
  * Padronização de infraestrutura como código.
  * Facilidade de provisionamento e escalabilidade horizontal dos containers.
* **Riscos**:
  * Necessidade de treinamento do time de infraestrutura local em conceitos de orquestração de containers.
* **Consequências**: Todos os serviços do projeto devem conter seus respectivos arquivos de especificação de ambiente (`Dockerfile` ou configurações de compose equivalentes), criados em fases posteriores.
* **Alternativas Consideradas**: Deploy manual em servidores Windows Server IIS antigos (rejeitado pela falta de escalabilidade e dificuldade de automação).

---

## 10. APIs como Única Forma de Comunicação entre Sistemas

* **Decisão**: Expor todas as funcionalidades e dados do ERP através de APIs estruturadas no backend. Qualquer cliente externo (incluindo o aplicativo Bravito) deve interagir exclusivamente através dessas portas de entrada.
* **Justificativa**: Garante o isolamento completo da camada de dados do ERP. Nenhuma aplicação externa poderá ler ou escrever diretamente no PostgreSQL, garantindo que as regras de integridade e auditoria sempre sejam executadas pelo backend.
* **Benefícios**:
  * Desacoplamento total entre o banco de dados e as interfaces visuais.
  * Possibilidade de alterar a estrutura interna do PostgreSQL sem quebrar o aplicativo Bravito, desde que mantida a assinatura dos contratos da API.
  * Segurança centralizada no nível da API.
* **Riscos**:
  * Latência de rede adicional em comparação com acessos diretos a banco de dados (gerenciável com boas práticas de cache e modelagem de payloads).
* **Consequências**: Toda e qualquer integração externa ou leitura de dados por terceiros passa obrigatoriamente por controllers/endpoints autenticados da API do ERP.
* **Alternativas Consideradas**: Acesso direto ao banco de dados por relatórios ou pelo Bravito via views compartilhadas (rejeitado por acoplamento excessivo e brechas de segurança).
