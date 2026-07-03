# Repositório de Prompts Padronizados (prompts/README.md)

Este diretório é reservado para centralizar e versionar todos os prompts oficiais recomendados e validados para guiar ferramentas de Inteligência Artificial Generativa durante as etapas de desenvolvimento do ERP **WebApolice**.

---

## 1. Propósito do Diretório
O uso de Inteligência Artificial no desenvolvimento de software traz agilidade, contanto que seja feito sob diretrizes rígidas que previnam a dispersão arquitetural ou a introdução de anti-padrões. Este diretório servirá para armazenar modelos (*templates*) de prompts estruturados, garantindo consistência técnica em diferentes tipos de tarefas.

Neste diretório, serão armazenados prompts oficiais para as seguintes atividades:
* **Criação de Módulos**: Instruções para iniciar um novo domínio com isolamento lógico adequado e sem acoplamento indevido.
* **Criação de Telas**: Diretrizes visuais, de acessibilidade, responsividade, componentes React reutilizáveis e controle de estado local.
* **Criação de Endpoints**: Padrões de design de APIs REST, assinaturas de métodos no ASP.NET Core, uso de DTOs e validação de contratos.
* **Migrations**: Padrões de criação de scripts de migração do Entity Framework para modelagem no PostgreSQL.
* **Integrações**: Diretrizes para criação de conectores no backend e orquestrações seguras e idempotentes.
* **Testes**: Modelos para geração de baterias de testes unitários ou de integração para o backend e frontend.
* **Revisão Arquitetural**: Critérios e checklists para submeter código pré-existente à avaliação da IA buscando gargalos de design de software.
* **Atualização de Documentação**: Padrões para atualizar de forma síncrona os arquivos em `docs/` e regras operacionais de desenvolvimento.

---

## 2. Regra Mandatória de Uso

> [!WARNING]
> **Prompts improvisados, ad-hoc ou sem validação técnica prévia não devem sob nenhuma circunstância substituir os prompts oficiais definidos neste diretório.**

Ao solicitar que uma IA implemente uma nova tela, módulo ou endpoint, o desenvolvedor deve obrigatoriamente copiar e preencher o template correspondente presente neste diretório para guiar o agente de IA, assegurando que o código gerado esteja em perfeita sintonia com a Clean Architecture, as decisões técnicas e os princípios arquiteturais do projeto.
