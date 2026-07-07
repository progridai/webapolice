# Regras de Projeto para Inteligência Artificial (AI_PROJECT_RULES.md)

Este documento é de leitura **obrigatória e prioritária** para qualquer agente de Inteligência Artificial (IA) que realize análises, planejamentos ou alterações neste repositório.

---

## Estrutura do Repositório
* **Frontend**: Localizado em `apps/web` (React 19 / TypeScript / Vite 8).
* **Backend**: Localizado em `backend` (Solução `.sln` em .NET 10 LTS).

---

## Diretrizes Gerais e Restrições

1. **Ler a documentação** contida no diretório `docs/` e consultar os **ADRs** (`docs/adr/`) antes de propor ou realizar qualquer implementação.
2. **Não adicionar bibliotecas ou dependências** ao projeto sem justificar detalhadamente o benefício e obter aprovação formal do usuário.
3. **Não alterar ou substituir as tecnologias estruturais definidas** (React, TypeScript, Vite, ASP.NET Core, PostgreSQL, Keycloak, n8n, Workers .NET) sem uma decisão de arquitetura documentada.
4. **Não criar código ou arquivos fora da estrutura oficial de pastas** estabelecida para o frontend (`apps/web`), backend (`backend/src`) ou testes.
5. **Não executar integrações com sistemas externos diretamente pelo frontend**. Qualquer chamada a APIs externas (exceto o próprio backend do ERP e o Keycloak para autenticação) deve passar obrigatoriamente pelo backend.
6. **Não colocar regras de negócio complexas ou validações de domínio dentro de componentes React**. Componentes devem focar na apresentação, estado local e interação visual.
7. **Não utilizar o tipo `any` no TypeScript** sem uma justificativa explícita e documentada em código.
8. **Não criar abstrações genéricas ou padrões complexos de design** (como múltiplos níveis de herança ou repositórios redundantes) antes que exista uma necessidade real e concreta em produção.
9. **Não propor ou criar microsserviços**. A arquitetura inicial do sistema é um monólito modular.
10. **Não duplicar componentes de UI, esquemas de validação ou contratos de dados**. Reutilize e centralize conforme os padrões de domínio.
11. **Não alterar contratos de APIs publicadas (endpoints, payloads, schemas) de forma silenciosa**. Qualquer quebra de retrocompatibilidade deve ser sinalizada.
12. **Não criar tabelas, colunas ou relacionamentos no banco de dados** sem antes documentar as regras de negócio e a modelagem do domínio no diretório `docs/` (Veja as referências em [docs/17-modelagem-banco-dados-webapolice.md](file:///c:/PROJETOS/Rsul%20Automacoes/Projetos/webapolice/docs/17-modelagem-banco-dados-webapolice.md) e [docs/18-modelagem-clientes-core-cadastro.md](file:///c:/PROJETOS/Rsul%20Automacoes/Projetos/webapolice/docs/18-modelagem-clientes-core-cadastro.md)).
13. **Não usar campos JSON ou tabelas não relacionais de forma indiscriminada** como substitutos para uma modelagem relacional apropriada no PostgreSQL.
14. **Não armazenar segredos**, chaves de API, senhas, certificados ou credenciais em arquivos do repositório. Utilize variáveis de ambiente ou gerenciadores de segredos.
15. **Não realizar ações financeiras autônomas** (como disparar pagamentos ou liquidações reais) sob o controle direto de agentes de IA.
16. **Sempre criar estados visuais apropriados** de carregamento (*loading*), erro (*error handling*) e estados vazios (*empty states*) para todas as telas e interações do usuário.
17. **Sempre validar autorizações e permissões no backend**, mesmo que a interface do frontend oculte ou desabilite determinados botões ou páginas.
18. **Sempre manter a documentação interna e os testes automatizados atualizados** de forma síncrona com qualquer alteração realizada no código de produção.
19. **Sempre apresentar um resumo claro dos arquivos criados ou alterados** ao final de cada iteração de trabalho.
20. **Sempre executar todas as validações e testes disponíveis** (build, tests, lint, typecheck) localmente antes de sinalizar a conclusão de uma tarefa.
21. **Não criar módulos de negócio** ou projetos adicionais na solução .NET sem que haja uma definição de domínio prévia e documentada.
22. **Identidade visual e temas**:
    * Toda tela nova ou componente implementado no frontend deve obrigatoriamente funcionar e ser testado nos temas claro (`light`), escuro (`dark`) e modo sistema (`sistema`).
    * Componentes comuns e específicos devem consumir exclusivamente tokens semânticos de estilo. É proibido utilizar cores hexadecimais ou rgb diretamente na estilização de componentes.
    * O dourado `#D4AF37` é a cor principal e de destaque da marca. Branco, preto suave e cinza claro compõem a paleta principal e neutra da aplicação.
    * As cores verde, vermelha e azul são exclusivamente funcionais (para estados de sucesso, erro/perigo e informação, respectivamente).
    * O azul não pode ser utilizado como cor dominante ou primária da navegação/aplicação.
    * O verde não pode ser utilizado como cor padrão de botões principais.
    * O vermelho deve ser reservado exclusivamente para sinalização de erros críticos ou ações altamente destrutivas.
    * Nenhuma nova paleta paralela de cores ou estilos ad-hoc pode ser criada fora do design system.
    * Qualquer novo token semântico necessário deve ser cadastrado e formalmente documentado em `docs/12-identidade-visual-design-system.md`.
    * A conformidade de acessibilidade e contraste (WCAG) deve ser validada e mantida em todas as interações.
    * Os estados de erro, sucesso e informação nunca podem depender unicamente da cor para comunicar sua finalidade (devem conter ícones, textos ou labels).
    * Todo componente interativo novo deve possuir estados de hover, focus-visible, active e disabled definidos nos dois temas.
    * Nenhuma tela ou componente pode assumir que o fundo da aplicação será sempre branco ou claro.
    * Nenhum ativo visual ou logotipo original da marca deve ser modificado em suas cores ou proporções sem autorização.
    * Toda revisão de frontend deve validar ativamente o suporte a temas e o consumo correto dos tokens semânticos.

---

## Comportamento Esperado da IA

Ao interagir com este repositório, a IA deve agir de acordo com os seguintes comportamentos profissionais:

* **Analise antes de alterar**: Realize uma varredura completa nas dependências, arquivos correlacionados, ADRs e documentação do domínio antes de propor alterações de código.
* **Reutilize padrões existentes**: Siga a arquitetura de arquivos e convenções de nomenclatura que já estão estabelecidas no projeto.
* **Faça alterações pequenas e rastreáveis**: Prefira commits ou modificações incrementais que facilitem a revisão de código (*code review*) por humanos.
* **Não refatore partes não relacionadas**: Não altere arquivos ou linhas de código que estejam fora do escopo direto da tarefa designada, exceto se solicitado ou para corrigir lints críticos no mesmo arquivo.
* **Informe riscos e decisões**: Se uma implementação trouxer riscos à segurança, concorrência de dados, quebras de builds ou retrocompatibilidade, apresente-os de forma transparente.
* **Não esconda erros ou falhas de ambiente**: Se ocorrer um erro durante a execução de scripts, builds ou testes, relate imediatamente ao invés de contornar de forma silenciosa ou apagar validações.
* **Não diga que algo foi validado sem realmente validar**: É proibido afirmar que um comando funcionou ou que o build está limpo sem de fato ter executado com sucesso no terminal. Registre qualquer falha de ambiente ou dependência que impeça o sucesso de comandos.

---

## Regras de Infraestrutura e Banco de Dados

* **Gerenciamento de Imagens e Tags**: Utilizar apenas imagens Docker oficiais e com tags de versões explícitas (ex: `postgres:18.4`). É terminantemente proibido utilizar `latest`, tags flutuantes ou imagens de terceiros não homologadas.
* **Segurança de Segredos**: Nunca expor credenciais, chaves ou senhas em arquivos de configuração do Docker Compose ou em código-fonte. Utilize referências de variáveis de ambiente obtidas de arquivos `.env` locais (os quais nunca devem ser versionados).
* **Segurança de Rede Local**: Mapear portas de serviços (como banco de dados PostgreSQL e console do Keycloak) vinculadas estritamente ao localhost (`127.0.0.1`) no ambiente de desenvolvimento local, evitando exposição pública inadvertida.
* **Gerenciamento de Dados**: Não automatizar a destruição de volumes ou dados de forma silenciosa ou irrecuperável. A remoção de volumes nomeados do Docker deve ser uma ação consciente e documentada.
* **Isolamento de Inicialização e Migrações**: A inicialização da infraestrutura (criação de bancos de dados vazios, usuários e permissões de sistema) deve ser completamente separada das migrations de aplicação ou regras de negócio.
* **Configuração Oficial e Padronizada**: A configuração de ferramentas de infraestrutura (como Keycloak) deve seguir estritamente as diretrizes da documentação oficial do fabricante, sem customizações ad-hoc de produção aplicadas ao modo de desenvolvimento.
* **Separação de Etapas**: Não criar realms, clients, roles ou usuários de teste sem que haja uma etapa de entrega técnica ou planejamento de segurança dedicados a esse fim.
* **Distinção de Ambientes**: Nunca tratar configurações ou facilidades de ambiente de desenvolvimento local (como o modo `start-dev` do Keycloak ou múltiplos bancos em uma única instância PostgreSQL) como padrões ou topologias viáveis para produção.

---

## Regras de Autenticação, Autorização e Identidade (Keycloak)

* **Segurança de Identity Provider (IdP)**: O Keycloak deve atuar como provedor central de identidade. Configurações de Realms, Clients e Roles devem ser versionadas em arquivo declarativo (`webapolice-realm.json`) e aplicadas via script automatizado.
* **Segurança de Clients OIDC**:
  * **Client Frontend (`webapolice-web`)**: Deve ser público (`publicClient: true`), com Standard Flow (Authorization Code) ativado e fluxos inseguros (Implicit, Direct Access Grants) desativados. O uso de PKCE com método de desafio `S256` e Proof Key obrigatório é estritamente mandatório. Redirecionamentos devem ser estritamente restritos ao localhost (`http://127.0.0.1:5173/*`).
  * **Client API (`webapolice-api`)**: Deve ser confidencial (`publicClient: false`), com Standard Flow e Direct Access Grants desativados. Segredos de client devem ser injetados em tempo de execução via variáveis de ambiente e nunca salvos no JSON declarativo.
* **Usuário Administrativo de Desenvolvimento**: O usuário de desenvolvimento `dev.admin` e suas permissões associadas (como a role global `admin`) devem ser criados por meio de script idempotente (`configure-realm.sh`). Nenhuma senha em texto puro ou credencial real de usuário deve constar em repositório Git ou documentação pública.
* **Auditoria de Conformidade**: Qualquer alteração de infraestrutura ou configuração de Realm/Client OIDC deve ser auditada e validada pelo script `validate-realm.sh`, garantindo que os requisitos de conformidade com PKCE e segredos não expostos permaneçam intactos.
