# Visão do Produto (00-visao-do-produto.md)

Este documento define os objetivos estratégicos, o público-alvo, os problemas a serem solucionados e as características essenciais do novo ERP **WebApolice**.

---

## 1. Problema a Resolver
O mercado exige alta disponibilidade, flexibilidade de integração e velocidade de adaptação a novos processos e canais digitais. O sistema atual enfrenta barreiras de infraestrutura que limitam o crescimento da organização, dificultando a manutenção rápida e o consumo externo de seus dados de forma segura. O novo ERP WebApolice visa eliminar essas barreiras tecnológicas, fornecendo uma base moderna e unificada.

## 2. Limitações Gerais do ERP Legado
O ERP legado apresenta restrições que motivam sua substituição progressiva:
* **Tecnologia Desatualizada**: Desenvolvido em VB.NET, o que limita a adoção de bibliotecas e padrões de desenvolvimento modernos, além de dificultar a atração de novos talentos de engenharia.
* **Dependência de Banco de Dados Específico**: Desenvolvido exclusivamente sobre SQL Server, gerando custos de licenciamento e limitando a portabilidade.
* **Interface e Acessibilidade**: Dificuldade em fornecer uma experiência de usuário rica, fluida e responsiva diretamente no navegador de dispositivos móveis.
* **Acoplamento e Integração**: Arquitetura que dificulta a criação de APIs limpas e isoladas para consumo por parceiros e aplicações periféricas (como o aplicativo Bravito).

## 3. Objetivo do Novo ERP
Estabelecer uma aplicação corporativa centralizada, executada inteiramente na web, projetada sob o paradigma de monólito modular de alta coesão e baixo acoplamento. O sistema deve centralizar e expor todas as capacidades de negócio por meio de APIs robustas e seguras.

## 4. Usuários Esperados
O ERP WebApolice será utilizado por diversos perfis de usuários:
* **Administradores do Sistema**: Responsáveis pelo controle de permissões, parametrizações gerais e auditoria de processos.
* **Operadores Internos**: Profissionais responsáveis pela gestão operacional diária de apólices, sinistros e faturamento.
* **Parceiros e Corretores**: Usuários externos que realizam consultas, cotações e acompanhamentos de processos.
* **Clientes Finais**: Consumidores que interagem com consultas rápidas e informações de suas apólices (seja via web ou através do Bravito).

## 5. Características Fundamentais

### A. Uso Responsivo e Multidispositivo
* O ERP será 100% acessível via navegadores web modernos.
* A interface (construída em React) se adaptará dinamicamente e de forma responsiva para computadores desktop, tablets e celulares.
* Não haverá obrigatoriedade de instalação de um aplicativo nativo móvel para acessar o ERP.

### B. Integração com o Bravito
* O aplicativo **Bravito** continuará existindo como canal complementar de mobilidade.
* O Bravito será um cliente puro de consumo das APIs do ERP WebApolice.
* O Bravito focará em canais assistidos de chat, inteligência artificial integrada, consultas ágeis de status e fluxos de atendimento simplificados.

### C. Crescimento Futuro por Módulos
* A arquitetura de monólito modular garante que novos módulos de negócio possam ser desenvolvidos e acoplados ao ERP com o mínimo de interferência nos domínios existentes.
* Cada domínio deve ser autocontido em termos de regras de negócio e persistência conceitual.

### D. Auditoria, Segurança e Rastreabilidade
* **Auditoria de Operações**: Todas as ações de criação, modificação ou exclusão de dados devem ser obrigatoriamente registradas em logs estruturados de auditoria.
* **Segurança Centralizada**: Autenticação unificada via Keycloak, com controle fino de permissões de acesso (RBAC/ABAC) gerenciado e validado pelo ERP.
* **Rastreabilidade**: Garantia de que cada transação de negócio possa ser auditada retrospectivamente, identificando o autor, a data/hora e o estado anterior da informação.

### E. Uso Assistido de Inteligência Artificial
* O sistema deve ser preparado arquiteturalmente para fornecer endpoints e dados limpos que sirvam de contexto para integrações de inteligência artificial assistida.
* A IA atuará como facilitadora em consultas, análises preliminares de dados e automação de processos não críticos, sempre sob supervisão ou acionamento explícito do usuário.
