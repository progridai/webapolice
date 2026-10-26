# Estabilização dos Modais de Vínculos de Apólices

Data: 22/10/2026. Continuação do [lote de campos de busca](44-estabilizacao-campos-busca-apolices.md).

## Resultado

Revisados `SubestipulanteApoliceModal` e `ApoliceRamoFormModal`. O lint global passou de 18 para 14 erros, mantendo quatro avisos. Removidos dois erros de `set-state-in-effect` e dois de `immutability`, sem novos apontamentos por arquivo/regra/severidade. Os cinco arquivos TypeScript deste lote estão sem erros ou avisos de ESLint.

A suíte completa passou de 851 para 903 testes aprovados, em 66 arquivos. TypeScript e build aprovados. Acrescentados 50 testes de ciclo dos modais e dois de contrato HTTP para a listagem de Ramos.

## Comportamento e decisões

- **Abertura e contexto:** o componente público monta o formulário interno somente quando aberto. A chave do formulário combina apólice e vínculo, ou modo de inclusão. Fechamento, troca de vínculo ou troca de apólice descartam a instância anterior; a nova instância recebe seus valores iniciais por `defaultValues`. Não há efeito chamando `reset` e alterando estado para reinicializar o formulário.
- **Edição em andamento:** renderizar novamente com um objeto equivalente do mesmo vínculo preserva alterações e mensagens de erro. Atualizações de props do mesmo identificador não sobrescrevem automaticamente o rascunho. Fechar/reabrir ou mudar o contexto reinicializa os campos. A definição do esquema, IOF decimal/zero, identificadores e payloads foram preservados.
- **Catálogos:** os dois modais reutilizam `useConsultaApolice`, consultando somente na inclusão. Mantidos os filtros de ativos e o limite de 100 registros. A edição usa a referência recebida e não depende da lista de ativos. Fechamento, mudança de contexto e desmontagem abortam a consulta; resultados e erros tardios não alteram outra abertura. Não foram adicionados timers nem outra infraestrutura de consulta.
- **Cancelamento até o cliente HTTP:** `ramosApi.listar` recebeu o segundo argumento opcional `signal?: AbortSignal`; o método de Subestipulantes já o suportava. Os sinais são encaminhados ao cliente HTTP. Chamadas existentes sem sinal continuam válidas, com o mesmo endpoint, filtros e resposta.
- **Carregamento, falha e vazio:** os catálogos têm estados separados das falhas de gravação. A lista fica desabilitada enquanto carrega ou apresenta falha. Falhas exibem texto e ação de tentar novamente; catálogo vazio exibe mensagem específica. A inclusão fica indisponível enquanto o catálogo está carregando, com erro ou vazio. A nova tentativa não submete o formulário.
- **Gravação única:** cada formulário mantém uma trava de operação em andamento. Submits repetidos, inclusive antes da próxima renderização, não iniciam outra gravação. Validação que termina após a desmontagem também não inicia chamada. Campos editáveis e ações ficam desabilitados durante o envio.
- **Fechamento:** Cancelar, botão de fechar, Escape e clique no fundo ficam bloqueados enquanto a gravação está pendente. Todos continuam disponíveis quando o formulário está ocioso. O fechamento usa callback estável, consultando a trava no evento.
- **Conclusões antigas:** sucesso, erro e término de uma gravação só atualizam a instância ainda montada. Se o contexto for trocado externamente durante a operação, sua conclusão não fecha o novo modal, não mostra erro nele e não dispara seus callbacks de atualização. Uma gravação já enviada pode concluir no backend; desmontar a interface não cancela nem desfaz essa escrita. Os contratos de gravação não receberam sinal de cancelamento neste lote.
- **Subestipulantes e temas:** substituídas classes de cinza/vermelho fixas por componentes/tokens semânticos existentes. Campos usam `FormField`, incluindo a seleção dentro do `Controller`, e a entidade em edição usa `ReadOnlyField`. Erros ficam associados ao campo por atributos de acessibilidade. Nome e documento/código continuam disponíveis na edição.

Não houve alteração de banco, permissões, dependências, campos persistidos, esquemas de negócio ou contratos REST. A regra de Title Case e a preservação de evidências permanecem conforme o documento 26. As alterações locais anteriores foram preservadas.

## Validações

| Verificação | Resultado |
|---|---|
| Testes existentes de erros de vínculos e esquemas | 90 aprovados, zero falhas, dois arquivos |
| Novos testes de ciclo e contratos HTTP | 66 aprovados, zero falhas, dois arquivos; 50 de ciclo e 16 de HTTP no total |
| `npm.cmd run test:run` | 903 aprovados, zero falhas, 66 arquivos |
| `npm.cmd run typecheck` | Aprovado: zero erros, saída 0 |
| `npm.cmd run build` | Aprovado, saída 0 |
| ESLint dos três arquivos de produção e dois testes | Zero erros e avisos |
| `npm.cmd run lint` global | 14 erros e quatro avisos; antes, 18 e quatro |
| `npm.cmd run lint:design-system` | Sete ocorrências preexistentes em `AsyncSelect.tsx` e `DesignSystemPage.tsx` |
| `npm.cmd run format:check` | 269 arquivos pendentes; antes, 271 |
| Prettier dos dois modais e dois testes | Aprovado; API de Ramos recebeu somente a alteração necessária |
| `git diff --check` | Aprovado |

`ApoliceVinculosLifecycle.test.tsx` cobre os dois modais em inclusão e edição: abertura/fechamento, props equivalentes, troca de vínculo/apólice, reset, catálogo pendente, erro/vazio/retry, cancelamento, conclusões tardias, payloads, IOF zero, envio duplicado, bloqueio de fechamento, validação após desmontagem, erros acessíveis e `StrictMode`. Os quatro temas exercitados são claro, escuro e sistema com preferência clara/escura. Os 60 testes anteriores de erros de vínculos também exercitam a edição nos temas suportados e continuaram aprovados.

`FeatureContracts.test.ts` passou de 14 para 16 cenários. Os dois novos verificam filtros e propagação do sinal de cancelamento na listagem de Ramos, com e sem sinal externo. Os 30 testes de esquemas de formulários, incluindo coerção/limites de IOF, continuaram aprovados.

A primeira passagem de ESLint apontou `react-hooks/refs` ao passar o callback protegido por ref a `handleSubmit` durante a renderização. A construção e execução do handler foram movidas para o evento de submit, onde a validação e o acesso às travas pertencem. A passagem seguinte e a verificação final ficaram limpas. Não houve falhas nos testes executados ou no TypeScript. Não foram adicionados `any`, casts para esconder contratos, supressões ou relaxamentos de compilação.

As verificações são locais, em DOM e com APIs/fetch simulados. Não substituem homologação visual e integrada com autenticação, inclusive foco e apresentação dos modais em navegador. Não foram introduzidos tokens novos ou cores fixas. Permanecem pendentes as sete ocorrências anteriores do design system e a formatação geral. Backend não alterado; seus testes não foram executados.

Relatórios locais ignorados pelo Git: `scratch/lint-etapa17-*`, incluindo retrato inicial, testes, build, tipos e comparativo de lint.

## Arquivos alterados

Caminhos relativos a `apps/web`:

- Produção: `src/features/apolices/components/modals/SubestipulanteApoliceModal.tsx`, `src/features/apolices/components/tabs/ApoliceRamoFormModal.tsx` e `src/features/ramos/api/ramos.api.ts`.
- Teste novo: `src/features/apolices/components/ApoliceVinculosLifecycle.test.tsx`.
- Teste ampliado: `tests/http/FeatureContracts.test.ts`.
- Documentação: este registro e continuidade nos documentos 29 e 44.

## Próxima etapa

A revisão das listagens de Corretoras, Módulos, Ramos e Seguradoras foi concluída no [lote de listagens de cadastros](46-estabilizacao-listagens-cadastros.md), com 984 testes aprovados e lint global em 10 erros e quatro avisos. Os resultados acima preservam o retrato desta entrega. A próxima etapa é revisar o hook `useCooperados`.

Depois, continuar por Cooperados, autorização e demais regras de lint. Design system, formatação geral e homologação integrada permanecem no [plano de estabilização](29-estabilizacao-frontend.md).
