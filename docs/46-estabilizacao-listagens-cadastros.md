# Estabilização das Listagens de Cadastros

Data: 22/10/2026. Continuação da [revisão dos modais de vínculos](45-estabilizacao-modais-vinculos-apolices.md).

## Resultado

Revisadas as listagens de Corretoras, Módulos, Ramos e Seguradoras. O lint global passou de 14 para 10 erros, mantendo quatro avisos. Removidos os quatro apontamentos de `set-state-in-effect` dessas páginas, sem novos apontamentos por arquivo/regra/severidade. Os nove arquivos TypeScript deste lote estão sem erros ou avisos de ESLint.

A suíte completa passou de 903 para 984 testes aprovados, em 67 arquivos. TypeScript e build aprovados. Foram acrescentados 79 testes de ciclo das listagens e dois de contrato HTTP para Módulos.

## Comportamento e decisões

- **Reutilização concreta:** o controle de consultas anteriormente localizado em Apólices foi movido para `src/shared/hooks/useConsulta.ts`. As quatro listagens precisam do mesmo ciclo de carregamento, erro, cancelamento, refetch e atualização. O arquivo anterior mantém o nome `useConsultaApolice` por reexportação, preservando seus consumidores. A comparação com o retrato inicial confirmou a mesma implementação, alterando apenas nome e comentário. Não foram criadas duas implementações ou dependências entre módulos de negócio.
- **Consultas e estados:** cada página fornece uma função estável com página, tamanho de página, busca e status. O carregamento inicial é derivado da consulta; o efeito compartilhado registra resultados assíncronos. Ao trocar filtros ou página, dados e total anteriores ficam ocultos. Na atualização da mesma consulta, os dados existentes permanecem disponíveis enquanto carrega.
- **Cancelamento e concorrência:** nova consulta, refetch ou desmontagem aborta a operação anterior. Resultados e falhas tardios não alteram a consulta atual, mesmo quando o serviço simulado ignora o sinal. O carregamento da operação atual não é encerrado por uma resposta antiga. `StrictMode` continua suportado.
- **Filtros preservados:** mantidos tamanho de página 10, paginação, debounce de 500 ms do `SearchField`, reinício na página 1 ao buscar/alterar status e limpeza dos filtros aplicados. `false`, `true` e ausência de status continuam distintos. Corretoras e Seguradoras conservam `trim()` da busca; Módulos e Ramos preservam o texto como recebido. Busca não recebe Title Case. O componente `SearchField` não foi alterado.
- **Cancelamento até a API:** as quatro páginas encaminham o sinal à listagem. `modulosApi.listar` recebeu o segundo argumento opcional `signal?: AbortSignal`, encaminhado ao cliente HTTP. As outras três APIs já o suportavam. Chamadas existentes sem sinal continuam válidas, com o mesmo endpoint, filtros e resposta.
- **Erro e recuperação:** as quatro páginas exibem falha de consulta com ação de tentar novamente usando os filtros atuais. Módulos e Ramos deixam de apresentar essas falhas apenas no console. Quando a consulta falha sem dados, a página não apresenta um estado vazio enganoso. Quando falha uma atualização com dados anteriores, eles permanecem visíveis junto ao alerta. Vazio sem falha mantém as mensagens existentes.
- **Consulta e alteração de status:** em Corretoras e Seguradoras, as falhas de consulta foram separadas das falhas de inativação/reativação. A tentativa de uma nova operação limpa sua mensagem anterior. O alerta de operação pode ser dispensado, e a confirmação permite repetir a operação; tentar consultar novamente não se apresenta como se repetisse a escrita. O alerta de consulta permanece até a recuperação ou mudança de consulta, com sua ação própria.
- **Atualização após escrita:** o sucesso de inativação/reativação dispara atualização por contador de evento. Se filtros mudaram enquanto a escrita estava pendente, a atualização usa os filtros atuais em vez de uma função que capturou parâmetros antigos. O mecanismo de consulta cancela a leitura anterior e mantém o total coerente com a resposta atual.
- **Escopo das ações:** preservadas permissões, criação, edição, identificação das linhas e ações de status existentes. Módulos conserva apenas a inativação disponível; Ramos conserva sua confirmação nativa; Corretoras e Seguradoras conservam seus diálogos de confirmação. Não foram acrescentadas telas de visualização ou regras de negócio. Escritas já enviadas não são canceladas por este mecanismo de consultas.

Não houve alteração de banco, permissões, dependências, campos persistidos ou contratos REST. A regra de Title Case e a preservação de evidências continuam conforme o documento 26. As alterações locais anteriores foram preservadas.

## Validações

| Verificação | Resultado |
|---|---|
| Testes existentes de consultas de Apólices e listagem de Seguradoras | 168 aprovados, zero falhas, três arquivos |
| Testes de ciclo das listagens e contratos HTTP | 97 aprovados, zero falhas, dois arquivos; 79 de ciclo e 18 de HTTP no total |
| `npm.cmd run test:run` | 984 aprovados, zero falhas, 67 arquivos |
| `npm.cmd run typecheck` | Aprovado: zero erros, saída 0 |
| `npm.cmd run build` | Aprovado, saída 0 |
| ESLint dos sete arquivos de produção e dois testes | Zero erros e avisos |
| `npm.cmd run lint` global | 10 erros e quatro avisos; antes, 14 e quatro |
| `npm.cmd run lint:design-system` | Sete ocorrências preexistentes em `AsyncSelect.tsx` e `DesignSystemPage.tsx` |
| `npm.cmd run format:check` | 265 arquivos pendentes; antes, 269 |
| Prettier das quatro páginas, dois arquivos do hook e dois testes | Aprovado; API de Módulos recebeu somente a alteração necessária |
| `git diff --check` | Aprovado |

`CadastrosListLifecycle.test.tsx` cobre as quatro páginas com APIs tipadas simuladas: consulta inicial, filtros/status, paginação, busca e debounce, limpeza dos filtros aplicados, respostas/falhas antigas, carregamento atual, refetch, erro/vazio, desmontagem, inativação, reativação onde existente, falha de operação, atualização com filtros atuais, permissões e `StrictMode`. Inclui os temas claro, escuro e sistema com preferência clara/escura. A busca com relógio simulado usa o componente `SearchField` real.

`FeatureContracts.test.ts` passou de 16 para 18 cenários. Os dois novos verificam filtros de Módulos, incluindo `ativo: false`, espaços e acentos na busca, e propagação de cancelamento com e sem sinal externo. Os 162 testes anteriores de ciclo/erros de Apólices e os seis de Seguradoras continuaram aprovados após a extração do hook. A suíte completa também cobre todos os demais consumidores de Apólices.

A primeira verificação de tipos do teste novo apontou a opção `exact` em dois seletores `getByRole`, cuja tipagem não oferece essa opção. As chamadas passaram a usar a correspondência por nome já fornecida pela biblioteca; a verificação final passou. Uma comparação local do hook precisou ser reexecutada com a codificação UTF-8 explícita no script PowerShell/Python; a comparação final confirmou a preservação da implementação. Não houve falhas nos testes executados. Não foram adicionados `any`, casts para esconder contratos, supressões, timers artificiais ou relaxamentos de compilação.

As verificações são locais, em DOM e com APIs/fetch simulados. Não substituem homologação visual e integrada com autenticação. Mantidos os componentes e tokens do design system, sem cores fixas novas. Continuam pendentes as sete ocorrências anteriores do design system e a formatação geral. Backend não alterado; seus testes não foram executados.

Relatórios locais ignorados pelo Git: `scratch/lint-etapa18-*`, incluindo retrato inicial, testes, build, tipos e comparativo de lint.

## Arquivos alterados

Caminhos relativos a `apps/web`:

- Páginas: `src/features/corretoras/pages/CorretorasListPage.tsx`, `src/features/modulos/pages/ModulosListPage.tsx`, `src/features/ramos/pages/RamosListPage.tsx` e `src/features/seguradoras/pages/SeguradorasListPage.tsx`.
- Hook compartilhado: `src/shared/hooks/useConsulta.ts` (movido) e reexportação em `src/features/apolices/hooks/useConsultaApolice.ts`.
- API: `src/features/modulos/api/modulos.api.ts`.
- Teste novo: `tests/pages/CadastrosListLifecycle.test.tsx`.
- Teste ampliado: `tests/http/FeatureContracts.test.ts`.
- Documentação: este registro e continuidade nos documentos 29 e 45.

## Próxima etapa

A revisão de `useCooperados` foi concluída no [lote de consultas de Cooperados](47-estabilizacao-consultas-cooperados.md), com 1.028 testes aprovados e lint global em nove erros e três avisos. Os resultados acima preservam o retrato desta entrega. A próxima etapa é revisar as exportações de `cooperados.routes.tsx`.

Depois, continuar pelas exportações das rotas de Cooperados, autorização e demais regras de lint. Design system, formatação geral e homologação integrada permanecem no [plano de estabilização](29-estabilizacao-frontend.md).
