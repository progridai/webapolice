# Diretrizes de Contribuição (CONTRIBUTING.md)

Este documento estabelece as regras e o fluxo de trabalho obrigatórios para todos os desenvolvedores que contribuem para o repositório do **WebApolice**.

---

## 1. Fluxo de Trabalho e Branches
* **Criação de Branches**: Todas as alterações devem ser feitas a partir de branches criadas a partir da branch principal (ex: `main` ou `develop`).
  * > [!NOTE]
    > **[PENDENTE]**: A convenção definitiva de nomenclatura de branches (ex: `feature/`, `bugfix/`, `hotfix/`) será acordada e documentada nesta seção futuramente.
* **Isolamento de Alterações**: Crie uma branch específica para cada tarefa. Evite trabalhar em múltiplos problemas não relacionados sob uma única branch.

## 2. Padrões de Commit
* **Commits Objetivos**: Cada commit deve representar uma única alteração lógica e conter uma mensagem concisa e explicativa do que foi alterado e o porquê.
  * > [!NOTE]
    > **[PENDENTE]**: A convenção formal de formatação de mensagens de commit (ex: *Conventional Commits*) será definida e estabelecida em fases futuras.

## 3. Pull Requests (PRs) e Revisão de Código
* **PRs Pequenos e Focados**: Mantenha os Pull Requests com o menor escopo possível. PRs menores facilitam a revisão de código (*code review*), reduzem conflitos de mesclagem e aceleram a integração.
* **Revisão de Código Obrigatória**: Nenhum código deve ser mesclado na branch principal sem que passe pelo fluxo de aprovação padrão estabelecido pelo time de engenharia.
* **Sem Mistura de Escopos**: Nunca misture refatorações de código legadas ou formatações estéticas amplas com a implementação de novas regras de negócio ou correções de bugs em um mesmo Pull Requests. Separe-os em tarefas e PRs distintos.
* **Checklist Obrigatório para Pull Requests de Frontend**:
  - [ ] Utiliza apenas tokens semânticos
  - [ ] Funciona no tema claro
  - [ ] Funciona no tema escuro
  - [ ] Funciona no modo sistema
  - [ ] Não introduz cores fixas nos componentes
  - [ ] Mantém contraste acessível
  - [ ] Usa cores funcionais somente para estados
  - [ ] Não cria paleta paralela
  - [ ] Possui testes dos estados visuais relevantes

## 4. Gerenciamento de Dependências
* **Aprovação Prévia**: A adição de qualquer biblioteca externa ou alteração de pacotes no frontend (`package.json`) ou backend (`.csproj`) exige justificativa formal técnica e aprovação da equipe antes de ser integrada ao projeto.

## 5. Qualidade, Testes e Segurança
* **Necessidade de Testes**: Qualquer nova funcionalidade ou correção de defeito crítico deve ser acompanhada por testes automatizados equivalentes (unitários, de integração ou ponta a ponta), assim que o framework de testes estiver estabelecido.
* **Proibição de Segredos**: É terminantemente proibido versionar chaves privadas, senhas, tokens de autenticação, strings de conexão ou quaisquer outros dados sensíveis. Utilize variáveis de ambiente ou arquivos de configuração ignorados pelo Git (`.gitignore`).

## 6. Atualização de Documentação
* Se uma alteração alterar o comportamento de uma API, alterar o esquema do banco de dados PostgreSQL ou impactar uma decisão de arquitetura, os arquivos de documentação correlacionados na pasta `docs/` devem ser obrigatoriamente atualizados no mesmo Pull Request.
  * Em especial, caso haja alterações no banco de dados, certifique-se de atualizar os documentos estruturais como [17-modelagem-banco-dados-webapolice.md](file:///c:/PROJETOS/Rsul%20Automacoes/Projetos/webapolice/docs/17-modelagem-banco-dados-webapolice.md) e [18-modelagem-clientes-core-cadastro.md](file:///c:/PROJETOS/Rsul%20Automacoes/Projetos/webapolice/docs/18-modelagem-clientes-core-cadastro.md).
