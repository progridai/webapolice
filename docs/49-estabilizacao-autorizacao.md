# Estabilização do Carregamento de Autorizações

Data: 22/10/2026. Continuação da [estabilização das rotas de Cooperados](48-estabilizacao-rotas-cooperados.md).

## Resultado

Revisado o ciclo de `AuthorizationProvider`: permissões anteriores deixam de alimentar decisões de acesso ao sair, trocar de usuário ou atualizar a consulta. Requisições substituídas são canceladas e respostas antigas são descartadas. A interface de erro permite tentar novamente, usando os componentes existentes do design system.

O lint global passou de cinco para quatro erros, mantendo três avisos. Removido o apontamento de `react-hooks/set-state-in-effect` do provider, sem novos apontamentos por arquivo/regra/severidade. Os dois arquivos TypeScript deste lote estão sem erros ou avisos de ESLint. A suíte completa passou de 1.032 para 1.065 testes aprovados, em 71 arquivos; TypeScript e build aprovados.

## Comportamento

- **Consulta por sessão:** reutilizado `useConsulta`, já existente. A operação só fica habilitada depois de a autenticação concluir. Sua identidade considera autenticação, carregamento da autenticação e `user.id`. Trocar de usuário autenticado inicia outra consulta; recriar o objeto do mesmo usuário não repete a chamada.
- **Logout e troca de usuário:** o resultado anterior deixa de ser exposto imediatamente. O sinal da operação anterior é abortado e uma resposta tardia, de sucesso ou falha, não modifica as permissões da sessão atual.
- **Atualização:** `refresh` conserva o contrato `() => Promise<void>`. Enquanto a consulta está pendente, flags de acesso são falsas e listas ficam vazias. Uma atualização concorrente substitui a anterior. Referências de atualização de uma sessão antiga, após logout ou desmontagem, não iniciam chamadas.
- **Resultado aplicável:** o hook compartilhado pode reter dados durante atualização para seus consumidores habituais. O provider usa esses dados para autorização somente após a conclusão bem-sucedida da consulta da sessão atual; carregamento e erro não revalidam permissões antigas.
- **Falha e cancelamento:** falhas da operação atual são convertidas em um erro com mensagem genérica e causa preservada. Isso inclui cancelamento reportado pelo cliente HTTP sem o sinal da operação ter sido abortado: ele não pode liberar o resultado anterior. Operações efetivamente abortadas pelo ciclo da consulta continuam descartadas pelo sinal.
- **Erro e recuperação:** `Alert` e `Button` substituem o painel de diagnóstico com cores fixas. O usuário recebe uma mensagem e a ação “Tentar novamente”, sem recarregar a janela. Como antes, o erro substitui o conteúdo filho. Removidos detalhes brutos da falha, URL da API, instruções de console e logs da resposta completa de permissões.

Preservados `/api/seguranca/me`, o contrato `UsuarioAutenticadoResponse`, o contexto público e seus métodos. Códigos de módulos, recursos e permissões continuam com comparação exata. `acessoTotal` continua sendo atalho de `possuiPermissao`, sem habilitar implicitamente módulos, recursos ou a função de operador. A função de operador continua vindo da resposta do backend. Usuário não encontrado/inativo e módulo desabilitado continuam bloqueados pelo guard existente, inclusive módulo desabilitado com acesso total.

Nenhuma política de autorização do backend, configuração do Keycloak, dependência, campo persistido ou regra de Title Case foi alterada. A supressão preexistente de Fast Refresh sobre o export de `useAuthorization` foi mantida; nenhuma supressão nova, `any`, cast para esconder contrato ou relaxamento de compilação foi introduzido. As alterações locais anteriores foram preservadas.

## Validações

| Verificação | Resultado |
|---|---|
| Testes direcionados do provider e guard | 44 aprovados, zero falhas, dois arquivos; 33 novos e 11 anteriores |
| `npm.cmd run test:run` | 1.065 aprovados, zero falhas, 71 arquivos |
| `npm.cmd run typecheck` | Aprovado: zero erros, saída 0 |
| `npm.cmd run build` | Aprovado, saída 0 |
| ESLint do provider e teste novo | Zero erros e avisos |
| `npm.cmd run lint` global | Quatro erros e três avisos; antes, cinco e três |
| `npm.cmd run lint:design-system` | Sete ocorrências preexistentes em `AsyncSelect.tsx` e `DesignSystemPage.tsx` |
| `npm.cmd run format:check` | 262 arquivos pendentes; antes, 263 |
| Prettier do provider e teste novo | Aprovado |
| Comparação das interfaces de resposta e contexto com o retrato inicial | Preservadas |
| `git diff --check` | Aprovado |

Os 33 testes novos usam `AuthContext`, `AuthorizationProvider` e, nos cenários de integração de acesso, `PermissionProtectedRoute` reais; apenas o cliente HTTP é simulado. Cobrem espera da autenticação, identidade estável, troca de usuário, logout/login, respostas antigas antes/depois da resposta atual, atualizações concorrentes, desmontagem, StrictMode, falhas HTTP e valores de erro desconhecidos, cancelamento atual e nova tentativa.

O guard real é exercitado com ausência de permissão, módulo desabilitado apesar de acesso total, usuário inativo e usuário não encontrado. O fluxo de conteúdo autorizado → atualização → falha → nova tentativa → permissão revogada é exercitado nos temas claro, escuro e sistema com preferência clara/escura. A mensagem de erro possui texto e usa os tokens e estados interativos dos componentes existentes. Essa verificação em DOM não equivale a uma auditoria visual de contraste ou acessibilidade em navegador.

Na primeira execução direcionada, 43 cenários passaram e um de StrictMode falhou: o teste colocava a fronteira abaixo da raiz e não provocava a repetição inicial de efeitos esperada. O teste foi corrigido para usar a opção `reactStrictMode` na raiz, conforme a API instalada do Testing Library. Os 44 testes direcionados e a suíte completa passaram depois da correção, sem ajuste de produção para contornar a falha.

As verificações são locais, com HTTP simulado; não substituem homologação autenticada com API/Keycloak reais. Backend não alterado; seus testes não foram executados. O scanner de design system não inclui a pasta `auth`, portanto a remoção das cores fixas do painel não altera suas sete ocorrências globais. Design system e formatação geral continuam pendentes.

Relatórios locais ignorados pelo Git: `scratch/lint-etapa21-*`, incluindo retrato inicial, comparativo do lint e execuções inicial/final dos testes.

## Arquivos alterados

Caminhos relativos a `apps/web`:

- Produção: `src/auth/AuthorizationProvider.tsx`.
- Teste novo: `tests/auth/AuthorizationProvider.test.tsx`.
- Documentação: este registro e continuidade nos documentos 29 e 48.

## Próxima etapa

Os quatro erros de `prefer-const` foram corrigidos no [documento 50](50-estabilizacao-constantes-validadores.md), preservando os cálculos e contratos existentes. O lint global passou com zero erros e três avisos; os números acima preservam o retrato desta etapa de autorização. Continuar pela revisão dos três avisos de lint em `ClienteForm.tsx` e `CooperadoForm.tsx`. Design system, formatação geral e homologação integrada permanecem no [plano de estabilização](29-estabilizacao-frontend.md).
