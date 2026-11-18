# Revisão Explícita das Condições da Pré Proposta

Início: 17/11/2026. Conferência: 18/11/2026. Status: implementada e validada automaticamente; homologação visual autenticada pendente. Nenhuma alteração aplicada a bancos persistentes.

Continuidade dos documentos [83](83-fluxo-operacional-propostas.md), [84](84-preenchimento-rascunho-propostas.md), [92](92-reinclusao-propostas.md) e [93](93-nomenclatura-pre-proposta.md). Alteração/retirada de cancelamento, reinclusão e nomenclatura já foram entregues; a próxima frente da lista original é revisar explicitamente condições registradas.

## Decisões Aprovadas

O responsável definiu:

1. Permitir revisão na Pré Proposta e na proposta confirmada com início futuro, antes da efetivação.
2. Permitir escolher outro Subgrupo ou Módulo diretamente na revisão da pré proposta.
3. Após comparar, o usuário escolhe entre manter ajustes nas coberturas correspondentes e usar os prêmios atuais do Módulo.

Os beneficiários continuam preservados para conferência; a confirmação mantém suas validações obrigatórias. Não há autorização para gerar financeiro, alterar a situação da Vida ou reescrever versões anteriores.

## Contexto Próprio Aprovado e Modelagem

A escolha vale somente para a proposta, dentro da mesma Apólice. A Vida e as versões anteriores permanecem preservadas. A revisão também será permitida nas confirmadas cujo início ainda seja futuro no calendário de São Paulo; data alcançada, vigência e encerramento bloqueiam a operação.

A migration `20261117180000_CondicoesPropriasProposta` acrescenta `seguro.proposta.condicoes_proprias`, booleano técnico obrigatório com padrão `false`. Somente a revisão explícita define `true`. Confirmação/reagendamento/efetivação validam os vínculos próprios ativos na mesma Apólice para essas propostas; as demais continuam exigindo coincidência com a Vida. O campo não altera identidade, permissões ou a situação da Vida e não é um texto humano. Não há novas colunas humanas nem mudança de triggers de Title Case. O rollback é recusado quando existem condições próprias em uso.

Preservar itens/coberturas anteriores como inativos, sem remover prêmios, bases ou justificativas; registrar novos snapshots com novos UUIDs. Beneficiários e observação permanecem intactos. Correspondência de coberturas usa a identidade do cadastro de Cobertura, inclusive ao trocar Módulo, nunca nomes. Ao manter ajustes, copiar valor ajustado, bases originais e justificativa somente para correspondências únicas; coberturas novas recebem valores atuais. Ao usar o Módulo, os novos snapshots começam sem ajuste e os registros anteriores continuam consultáveis.

## Diagnóstico Anterior à Implementação

- `CriarPropostaHandler` captura Subgrupo/Módulo, Plano, Coberturas e prêmios na criação. `ReinclusaoPropostas` permite escolher condições anteriores ou atuais na preparação da reinclusão. Nenhum desses fluxos revisa uma pré proposta existente.
- `PreencherRascunhoHandler` edita beneficiários, observação e prêmios das coberturas já registradas. Preserva bases e justificativas dos ajustes; não inclui/remove condições por alteração do Módulo.
- `ValidacaoCondicoesProposta` e `VigenciaPropostas` revalidam elegibilidade e compatibilidade na confirmação/efetivação; não atualizam silenciosamente os snapshots.
- `IPropostasRepository` já oferece obtenção da proposta para preenchimento e consulta do Plano do Módulo. O adaptador bloqueia Apólice, Proposta e referências; a consulta do Plano protege Plano/Coberturas quando executada dentro de uma transação.
- `RevisaoPreenchimento` protege mudanças na proposta. Ela não identifica sozinha uma alteração de configuração do Módulo depois de uma comparação apresentada na tela.
- `PropostasContextoSql` valida os vínculos também ao atualizar itens e coberturas. Se houver mudança de contexto, preservar condições antigas exige planejar a ordem das gravações dentro da mesma transação; não desligar triggers nem excluir registros para contornar essa validação.

## Implementação e Contratos

A comparação não grava dados e apresenta contexto, Plano, Coberturas incluídas/removidas/mantidas, condições registradas, valores atuais e destino dos ajustes. Identifica correspondências pela identidade de Cobertura, nunca pelo nome ou posição. Endpoints protegidos por `propostas.alterar` e gates APOLICES/PROPOSTAS, com UUID da proposta na rota:

- `GET /api/propostas/{publicId}/condicoes/opcoes`: vínculos ativos da mesma Apólice, com UUIDs e nomes. Ausência de Subgrupo é opção explícita.
- `POST /api/propostas/{publicId}/condicoes/comparacao`: `revisaoEsperada`, `moduloPublicId` e `subgrupoPublicId` opcional. Retorna snapshots atuais, destino com valores do Módulo, destino mantendo ajustes, indicadores de diferenças e identificação da comparação.
- `POST /api/propostas/{publicId}/condicoes/aplicar`: vínculos/revisão, `comparacao`, `tratamentoAjustes` (`MANTER` ou `MODULO`) e `observacao` obrigatória de até 1.000 caracteres. Retorna nova revisão. Não recebe prêmios editáveis do cliente.
- O detalhe acrescenta `podeRevisarCondicoes` e `condicoesProprias`, sem remover contratos anteriores.

A comparação usa SHA-256 dos snapshots relevantes, revisão, identidade e data agendada. A aplicação reobtém e valida dados sob bloqueios de Apólice/Proposta, Vida, vínculos, Plano, Coberturas e cadastro de Módulo, comparando a identificação recebida. Configuração ou revisão alterada exige nova comparação. A identificação não substitui autorização nem é credencial. As escritas normais de configuração também serializam pela Apólice. Ausência de diferenças para a escolha é recusada.

A operação auditada confirma ou reverte alterações, revisão, histórico e auditoria juntos. Primeiro encerra snapshots ativos no contexto anterior, depois troca o contexto e grava novos snapshots; não desliga triggers. A revisão agendada repete validações de beneficiários, condições, datas e origem, preservando confirmação e início futuro. O histórico recebe **Revisão de Condições**, justificativa e nova revisão técnica; a auditoria registra snapshots antes/depois e escolha, sem duplicar nomes de beneficiários/documentos. Revisões sucessivas preservam todos os snapshots anteriores.

No detalhe, **Revisar condições** abre a seleção dos vínculos. **Comparar condições** apresenta diferenças de vínculo, coberturas incluídas/removidas e atributos/prêmios completos. O tratamento dos ajustes começa sem seleção; o resultado é mostrado antes de **Aplicar condições comparadas**. Abrir/voltar não grava. A revisão bloqueia navegação, confirmação, preenchimento e ações incompatíveis. Erros preservam entradas, com nova comparação ou recarga explícita. Consultas usam cancelamento; aplicação impede saída e envios duplicados. Componentes/tokens seguem o design system.

## Implantação e Limites

Aplicar a migration do contexto Seguro antes de publicar a API que consulta a nova coluna, pelo procedimento do ambiente e após conferir conexão, baseline e backup. A migration de nomenclatura do documento 93 também permanece preparada e deve respeitar a ordem do contexto. Não atualizar indiscriminadamente todos os contextos para instalar esta funcionalidade. Esta entrega não publicou a API, reiniciou serviços ou alterou bases persistentes. Não há nova coluna humana e não é necessário reinstalar triggers de Title Case por esta coluna técnica.

Flags, UUIDs, revisão, hash e códigos de tratamento são técnicos. Nomes, observações e justificativas reutilizam campos humanos/normalizadores existentes nas três camadas. A troca não altera numeração, situação da Vida, versões anteriores ou valores financeiros de cobrança.

Arquivos principais: `RevisarCondicoes/RevisaoCondicoes.cs`, domínio/configuração/migration de `CondicoesProprias`, porta/repositório, validação de confirmação e controlador; no frontend, `RevisaoCondicoesProposta.tsx`, contratos, schema e API próprios, integrados em `PropostaDetalhes.tsx`. Testes novos: `PropostasRevisaoCondicoesTests.cs`, `CondicoesPropriasMigrationTests.cs`, autorização e testes do componente/cliente HTTP.

## Validação Executada em 18/11/2026

Os testes PostgreSQL cobrem comparação sem escrita, troca de contexto somente na proposta, ajustes e bases, preservação de beneficiários/condições/Vida, revisão antiga, configuração alterada, hash adulterado, situações não permitidas, contexto de outra Apólice, revisões sucessivas, confirmação/ativação futura com condições próprias e rollback por falha de auditoria. Duas aplicações concorrentes da mesma comparação são serializadas com a primeira transação aberta na auditoria; a segunda é recusada sem duplicar snapshots/histórico. A migration preserva a imagem dos campos anteriores, aplica padrão falso, mantém o modelo sincronizado e bloqueia rollback após uso.

Todos os testes de integração usaram PostgreSQL efêmero Docker e dados isolados. Os testes de interface cobrem os três temas, escolha explícita, resultado comparado, justificativa, preservação de entradas após erro, recarga, ausência de diferenças, carregamento/erro/vazio, cancelamento de consultas, resposta tardia, duplicidade e bloqueio de navegação/ações. O cliente HTTP preserva hash/códigos técnicos e normaliza a justificativa. A autorização verifica 401, permissão errada, catálogo indisponível, UUID da rota e payloads.

| Verificação | Resultado desta execução |
| --- | --- |
| Suíte completa do backend | 751 aprovados: 483 Integration, 162 API, 28 Arquitetura, 70 Cadastro, 6 Auditoria Unit e 2 Auditoria Integration |
| `dotnet build backend/WebApolice.slnx --no-restore --verbosity quiet` | Aprovado; zero erros/avisos |
| `npm.cmd run format:check` | Aprovado; nenhum arquivo pendente |
| `npm.cmd run typecheck` | Aprovado; zero erros |
| `npm.cmd run lint` | Aprovado; zero erros/avisos |
| `npm.cmd run lint:design-system` | Aprovado; zero ocorrências |
| `npm.cmd run test:run -- --maxWorkers=2` | 1.368 aprovados em 92 arquivos |
| `npm.cmd run build` | Aprovado |
| Scanner local de segredos e `git diff --check` | Aprovados |

Acrescentados 18 cenários PostgreSQL/migration, 3 de autorização e 12 de frontend. As contagens anteriores dos documentos 92/93 são históricas. O build mantém os avisos preexistentes de chunk acima de 500 kB e tempo dos plugins; nenhuma configuração foi flexibilizada.

As primeiras execuções detectaram conflito de rastreamento EF de Módulo, necessidade de persistir chaves dos snapshots dentro da transação antes de revalidar agendamentos, organização de tipo aninhado incompatível com o inventário arquitetural e verificações de migration que precisavam considerar a nova coluna técnica. As correções mantiveram as proteções existentes. No frontend, o teste de confirmação passou a fornecer uma proposta elegível, e a assinatura do provedor de token foi corrigida para o contrato real. As verificações completas passaram após as correções.

O inventário de superfícies retornou `apps: []` e `browsers: []`; a homologação visual autenticada não foi executada e os testes DOM não a substituem. Nenhuma alteração de JWT, Keycloak, perfis ou permissões. Não houve publicação, reinício da API, commit ou push. Próximas frentes: regras financeiras/movimentos e homologação visual, além da implantação das migrations preparadas.
