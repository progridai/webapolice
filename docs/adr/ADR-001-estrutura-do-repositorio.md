# ADR-001: Estrutura do Repositório (ADR-001-estrutura-do-repositorio.md)

* **Status**: Aceito
* **Data**: 2026-07-02
* **Autor**: Equipe de Arquitetura

---

## 1. Decisão
Adotar um único repositório compartilhado (Monorepo lógico) para conter tanto o código da aplicação frontend (`apps/web`) quanto o ecossistema do backend (`backend/` contendo a solução `.sln` e projetos .NET), além de documentação centralizada.

## 2. Contexto e Motivação
A separação física de repositórios para front e back no início de uma reescrita impõe atritos de versionamento, dificulta o rastreio de alterações atômicas de contrato de APIs e exige a manutenção de múltiplos fluxos de integração contínua (CI). Como o time é concentrado e o escopo do ERP WebApolice está sendo reescrito de forma unificada, centralizar a fundação técnica simplifica o desenvolvimento local e a governança inicial.

## 3. Benefícios
* **Atomicidade**: Alterações que modificam simultaneamente contratos da API no backend e o consumo de dados no frontend podem ser enviadas em um único Pull Request.
* **Governança de Documentação**: Documentos estratégicos (`docs/`), ADRs e prompts padronizados residem ao lado do código de produção de ambas as stacks.
* **Ambiente Único**: Simplifica o checkout do projeto e a inicialização de ferramentas locais de desenvolvimento para o time.

## 4. Riscos e Mitigações
* **Acoplamento Físico de Pastas**: Riscos de misturar ferramentas de build ou scripts. 
  * *Mitigação*: Delimitar pastas raízes independentes (`apps/web` com ferramentas Node e `backend` com a CLI .NET) sem cruzamento de ferramentas de empacotamento.
* **Tamanho do Repositório**: O repositório pode crescer muito.
  * *Mitigação*: Um arquivo `.gitignore` rígido garante que lixos de build (`node_modules`, `bin/`, `obj/`) nunca sejam commitados.

## 5. Consequências
* O fluxo de trabalho de CI/CD deve ser configurado futuramente para monitorar alterações em subpastas de forma isolada (evitando builds desnecessários).
* O desenvolvedor local precisará manter dependências do Node e do .NET SDK ativas em sua máquina.

## 6. Alternativas Consideradas
* **Repositórios Separados (Multi-repo)**: Rejeitado nesta etapa pela complexidade adicional de manter sincronização síncrona de branches correspondentes para alterações de contratos de API.
