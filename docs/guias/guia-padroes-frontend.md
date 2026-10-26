# Padrões obrigatórios para desenvolvimento do frontend

Status: em vigor. Revisão: 23/10/2026.

Aplica-se a toda criação ou alteração de telas, componentes, formulários, hooks, serviços, contratos e testes em `apps/web`. Complementa as [regras do projeto](../../AI_PROJECT_RULES.md), o [guia de telas](guia-uso-design-system.md) e o [guia de integração de novos módulos à segurança](guia-implementacao-novos-modulos.md).

## Implementação

- Seguir a arquitetura existente e documentar as regras funcionais da entrega. Reutilizar componentes de `src/components/ui`, campos, validadores, máscaras, formatos e contratos compartilhados.
- Consumir a API pelo cliente de `src/services/http`. Nas consultas, reutilizar `useConsulta` quando aplicável, com cancelamento por contexto e descarte de respostas antigas. Definir estados de carregamento, erro, nova tentativa e ausência de dados.
- Cancelamento deve funcionar com signal já abortado, durante a obtenção do token, fetch e leitura do corpo. Liberar timers/listeners ao concluir; manter AbortError distinto de timeout/erro de rede. Nos testes, cancelar enquanto a operação está pendente e verificar a liberação após a conclusão.
- Preservar os tipos reais da API, UUIDs públicos e códigos técnicos. Corrigir a origem dos erros de tipos ou lint; não acrescentar `any`, casts, supressões ou exclusões para esconder problemas.
- Usar os tokens semânticos e as regras do [design system](../12-identidade-visual-design-system.md). Verificar temas claro, escuro e sistema, responsividade, rótulos, mensagens, foco e operação por teclado.
- Aplicar a [política obrigatória de Title Case](../26-padronizacao-title-case.md) aos dados humanos, reutilizando os mecanismos compartilhados. Avaliar a natureza de cada campo; atualizar classificações e testes em C#, TypeScript e SQL quando necessário. Preservar documentos, credenciais, códigos, e-mails, URLs, status e evidências históricas.
- Seguir o catálogo e os mecanismos de autorização existentes. Proteger menus e rotas conforme as permissões, mantendo a validação definitiva e a auditoria no backend.

## Formatação

Usar o Prettier instalado no projeto e a configuração existente, respeitando `.editorconfig`, UTF-8 e finais de linha LF. Os scripts oficiais são `format` e `format:check` de `apps/web/package.json`.

Durante o desenvolvimento, formatar os arquivos alterados. No Windows, a partir de `apps/web`, por exemplo:

```powershell
npx.cmd --no-install prettier --write src/features/exemplo/pages/ExemploPage.tsx
```

Substituir o caminho pelo arquivo real. Não instalar outro formatador ou alterar configurações, ignores, abrangência dos checks ou contratos para obter aprovação. Não reaplicar formatação em arquivos sem relação com a entrega. Mudanças exclusivamente de formatação devem preservar comportamento e conteúdo, inclusive textos JSX, expressões regulares e valores de configuração.

## Critério de conclusão

Antes de concluir uma entrega do frontend, executar em `apps/web`:

```powershell
npm.cmd run format:check
npm.cmd run typecheck
npm.cmd run lint
npm.cmd run lint:design-system
npm.cmd run test:run
npm.cmd run build
```

Todos devem terminar com sucesso: zero arquivos pendentes no Prettier global, zero erros de TypeScript, zero erros e avisos de ESLint, zero ocorrências no scanner e nenhuma falha de testes ou build. Build não substitui a verificação de tipos. No Windows, os comandos acima usam os lançadores `.cmd`; em outros ambientes, usar `npm` e `npx`.

Criar ou atualizar testes que comprovem o comportamento funcional alterado. Em mudanças apenas de formatação, executar a suíte existente e conferir a preservação do conteúdo e da estrutura, sem criar testes que apenas reproduzam o estilo de código. Se houver alteração de backend, executar também seus checks e testes pertinentes.

O runner foi atualizado corretivamente para Vitest 5. Mocks de classes devem ser construtíveis e mocks de funções devem declarar a assinatura correspondente, sem usar any ou casts para esconder incompatibilidades. As razões e os limites dessa atualização constam no documento 78.

Atualizar a documentação funcional e técnica na mesma entrega, informando o que mudou, validações executadas e limitações. Conferir o diff para preservar alterações locais anteriores. Uma falha de ferramenta ou ambiente deve ser registrada e resolvida, ou apresentada como impedimento; não declarar aprovação sem executar o check.

Os testes locais não substituem a homologação visual e dos fluxos autenticados em navegador. A obrigatoriedade deste guia e os scripts locais não equivalem a um bloqueio automático no CI; automações de repositório precisam estar efetivamente configuradas para isso.

O workflow de qualidade e o lançador local usam valores públicos fictícios para as quatro entradas `VITE_*` obrigatórias, conforme o [documento 80](../80-correcao-configuracao-ci-frontend.md). Validar também sem o `.env` local ao alterar a configuração do gate. Preservar a recusa de configuração obrigatória ausente; builds de implantação precisam da configuração real do ambiente. Nunca colocar segredos em variáveis `VITE_*`.

O [fechamento da formatação](../75-conclusao-formatacao-frontend.md) registra a base aprovada em 23/10/2026. Novas entregas devem manter os mesmos critérios de aprovação.
