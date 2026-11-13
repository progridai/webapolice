# Exclusão Hierárquica de Vínculos da Apólice

Data: 11/11/2026. Complementa o documento 81.

Complemento de Propostas, conferido em 12/11/2026: novas propostas guardam FKs contextuais de Subgrupo/Módulo, seus itens referenciam o Plano do Módulo e suas coberturas referenciam a associação utilizada. Esses snapshots também bloqueiam exclusão, mesmo depois de alterar o contexto atual da Vida. A proteção conservadora descrita abaixo continua necessária para os registros anteriores sem essas referências. Sete triggers adicionais e testes de ancestralidade/SQL direto complementam as proteções existentes; ver [documento 83](83-fluxo-operacional-propostas.md).

## Escopo e Comportamento

Disponibilizar Excluir em Ramos, Subgrupos, Subestipulantes, Módulos, Plano do Módulo e suas Coberturas, além dos Produtos, Planos e Coberturas do Universo Permitido anterior. A exclusão é lógica, confirmada, auditada e restrita ao registro contextual da apólice. Cadastros globais não são excluídos. Encerrar/Inativar permanecem ações distintas. Registros ativos ou inativos podem ser excluídos se não tiverem dependências.

Não há cascata: excluir os filhos antes dos pais. Filhos inativos continuam bloqueando; filhos já excluídos logicamente deixam de bloquear a apresentação operacional, mantendo a referência física para histórico. Propostas, inclusive excluídas/inativas, continuam evidências de uso e bloqueiam a exclusão do vínculo utilizado. Participações de Vida não excluídas, mesmo encerradas, bloqueiam seus Subgrupos/Módulos e o Plano/Coberturas aplicáveis.

## Dependências

| Registro | Impedimentos |
| --- | --- |
| Ramo | Vidas, propostas ou configuração contextual (Produtos/Planos de Módulos) da apólice; sem FK contextual de Ramo em Vidas/Propostas, a proteção é conservadora, sem inferir igualdade entre textos/IDs globais. |
| Subgrupo | Vidas não excluídas ou qualquer proposta das Vidas que o referenciam. `proposta.subgrupo_id` legado não é confundido com o Subgrupo contextual. |
| Subestipulante | Propostas da apólice para o Subestipulante global correspondente. |
| Módulo | Plano contextual, Vidas ou propostas de suas Vidas. |
| Plano do Módulo | Coberturas contextuais, Vidas/propostas do Módulo. |
| Cobertura do Módulo | Vidas/propostas do Módulo, pois as propostas guardam snapshots sem FK para essa configuração. |
| Produto anterior | Planos contextuais ou itens de propostas da apólice para esse Produto. |
| Plano anterior | Coberturas contextuais ou itens de propostas da apólice para esse Plano. |
| Cobertura anterior | Coberturas de propostas da apólice com a mesma cobertura global; propostas sem identificação da cobertura são tratadas conservadoramente como uso possível. |

## Identidade e Persistência

Ramos e Subestipulantes recebem UUID próprio do vínculo, separado dos UUIDs globais já utilizados para edição. Produtos/Planos anteriores também recebem UUID contextual. O contrato recebe campos adicionais; identificadores antigos são preservados para compatibilidade, mas novos DELETEs usam somente o UUID do vínculo e a apólice. Não são expostas PKs numéricas novas.

Acrescentar `deleted_at` aos vínculos que ainda não o possuem: Ramo, Produto/Plano/Cobertura anteriores e Plano/Cobertura de Módulo. `ativo=false`, `updated_at` e `deleted_at` são gravados juntos. Consultas e validações de duplicidade não reutilizam registros excluídos. Índices únicos de Plano/Coberturas do Módulo passam a considerar apenas registros não excluídos, mantendo a unicidade também para inativos. Nomes/preços/dados históricos existentes não são reescritos. Campos novos são técnicos (UUID/timestamp); não há novo texto humano nem alteração da classificação Title Case.

## Segurança e Atomicidade

Extensão do módulo/recurso APOLICES existente. Permissões novas: `apolices.ramos.excluir`, `apolices.subgrupos.excluir`, `apolices.subestipulantes.excluir`, `apolices.modulos.excluir` (inclui Plano/Coberturas contextuais) e `apolices.universo.excluir` (estrutura anterior). `apolices.vidas.excluir` permanece a permissão de Vidas. Sem perfil novo ou concessão automática a perfis/usuários; Acesso Total segue a disponibilidade do catálogo existente. Módulo/recurso desabilitado bloqueia a ação.

As exclusões bloqueiam a apólice na transação e validam dependências antes de alterar o vínculo. Triggers de escrita serializam a gravação de filhos/usos na mesma apólice e impedem referências novas a ancestrais excluídos, cobrindo concorrência e SQL direto. Apenas o schema Seguro é consultado pela implementação de suas portas; Cadastro/Financeiro mantêm seus limites. Exclusão, histórico funcional e auditoria `VINCULO_APOLICE_EXCLUIDO` usam a mesma transação. Falha de validação/auditoria reverte integralmente. Mensagens indicam a dependência que impede excluir.

## Entrega e Validação

Implementado nos nove níveis descritos. Os DELETEs ficam em `/api/apolices/{apolicePublicId}/vinculos/{tipo}/{vinculoPublicId}`; tipos: `ramos`, `subgrupos`, `subestipulantes`, `modulos`, `planos-modulo`, `coberturas-modulo`, `produtos-anteriores`, `planos-anteriores` e `coberturas-anteriores`. A participação de Vida mantém o endpoint e a permissão da entrega anterior. A API valida o UUID contextual e sua ancestralidade na apólice. Listas, detalhes e o contador de Ramos ocultam excluídos. Chaves de renderização também usam o UUID contextual para distinguir dois vínculos do mesmo cadastro global.

O botão compartilhado usa os componentes existentes de confirmação/erro. A confirmação de exclusão é montada fora do editor pai; Escape fecha somente a confirmação e preserva o editor de Plano/Coberturas. Durante o envio, cancelamento/fechamento e envio duplicado são bloqueados. O erro conserva o diálogo para nova tentativa. A exclusão é oferecida também nos vínculos inativos, mediante a permissão específica; a disponibilidade para excluir independe da permissão de editar/reativar.

Validação final do frontend: `format:check`, `typecheck`, `lint`, `lint:design-system`, `test:run` e `build` aprovados. Foram 1.212 testes em 78 arquivos, com zero erros/avisos de ESLint e zero ocorrências no scanner do design system. O build mantém o aviso preexistente de chunk acima de 500 kB; não se alteraram limites/verificações. Os 32 testes novos do botão cobrem os nove tipos nos temas claro/escuro/sistema, cancelamento, erro/nova tentativa, envio pendente, Escape dentro de um editor e dois Ramos com o mesmo UUID global e UUIDs contextuais distintos.

Backend completo aprovado: 518 testes (6 Auditoria Unit, 2 Auditoria Integration, 51 Cadastro, 28 Arquitetura, 137 API, 294 Integration). Os 42 testes da hierarquia cobrem os nove tipos, propostas excluídas, inativos, exclusão sem cascata, isolamento entre apólices, UUID global/contextual, rollback, recriação do Plano, ancestralidade e concorrência nos dois sentidos. Um teste adicional aplica a migration a uma base histórica populada, preservando todos os campos anteriores e garantindo UUIDs distintos. Nove cenários de endpoint verificam 401, permissão incorreta, catálogo indisponível e envio autorizado do UUID/usuário/tipo corretos. O teste de catálogo cobre as cinco permissões, ausência de concessões e preservação de UUID/inativação na reexecução. Após a correção final do contador de Ramos, os 42 testes da hierarquia foram repetidos. Scanner de segredos e `git diff --check` aprovados.

As falhas intermediárias ficaram restritas aos testes locais e foram corrigidas antes da aplicação: nome de propriedade na consulta de propostas, variável SQL ambígua no trigger, conflito de rastreamento no helper de leitura do teste, referência a biblioteca de mock indisponível no projeto API e preparação histórica com o modelo atual. A fixture histórica passou a inserir diretamente as colunas daquela versão, incluindo seus campos obrigatórios. O teste de consulta que restaura uma Vida o faz antes de excluir a apólice; a proteção de ancestralidade não foi enfraquecida. Não se instalaram dependências nem se flexibilizaram checks.

## Aplicação na Base de Teste

Aplicadas pontualmente, em uma única transação, `20261111213321_ExclusaoHierarquicaVinculosApolice` (Seguro) e `20261111220000_AdicionarExclusaoVinculosApolice` (Segurança), em `webapolice_teste`, `painel.bravida.com.br:5432`. O aplicador validou rigidamente o alvo, disponibilidade do recurso APOLICES, ausência das migrations/permissões/função antes da instalação e comandos sem supressão de transação. Usou timeout de lock de 10 segundos e de comando de 30 segundos. As outras migrations pendentes não foram executadas.

Comparações de contagem/hash preservaram todos os campos anteriores dos 14 conjuntos de registros de Seguro, módulos/recursos/perfis, vínculos perfil/permissão e usuário/perfil, demais permissões e históricos de migrations anteriores. Houve apenas a identificação técnica adicional dos vínculos existentes, seis campos de exclusão, ajuste dos dois índices de Plano/Coberturas, 13 triggers de proteção, cinco permissões, os dois registros de migration e o evento de auditoria `HIERARQUIA_EXCLUSAO_INSTALADA`. UUIDs novos foram conferidos como não nulos, não vazios e únicos; as permissões foram confirmadas disponíveis em conexão independente. Não houve concessão a perfis/usuários nem exclusão de negócio.

Uma inspeção prévia em transação `READ ONLY` confirmou as FKs reais das tabelas envolvidas, incluindo Vidas e Propostas. Após aplicação, outra transação de leitura consultou as duas apólices existentes: dois Ramos, um Subestipulante e dois Planos de Módulo com duas Coberturas na primeira; a segunda não possuía esses registros. UUIDs e consultas foram validados sem alteração de dados. SQL e resultado da aplicação estão em `scratch/hierarquia-aplicacao-20261111/`; diagnósticos locais não contêm credenciais.

A API final está em `bin/ExecucaoExclusaoHierarquiaFinal-20261111`, ativada na porta local 5007 (PID 28188), após publicação isolada e verificação temporária na porta 5008. Os hashes de `appsettings.json` e `appsettings.Development.json`, ambiente e configuração externa foram preservados; não houve alteração de Keycloak/JWT. A versão final retornou HTTP 200 em `/api/health/live` e `/api/health/ready`, sem conteúdo em stderr. Os nove DELETEs retornaram HTTP 401 sem token, com UUIDs fictícios, na instância final. O frontend local respondeu HTTP 200. Não se executou exclusão autenticada em registros reais para homologar a entrega.

Não executar a reversão do schema automaticamente após uso da funcionalidade: o histórico pode conter mais de um Plano excluído para o mesmo Módulo, enquanto o índice da versão anterior exigia unicidade física. Os registros históricos devem ser preservados ao planejar uma reversão. A aplicação não depende de `database update` até a última migration nem faz atualização automática na inicialização da API.

## Arquivos Centrais

Frontend: `ExcluirVinculoButton.tsx`/teste, `exclusaoVinculos.api.ts`, `RamosTab.tsx`, `SubgruposTab.tsx`, `SubestipulantesTab.tsx`, `ModulosTab.tsx`, `PlanoModuloEditor.tsx`, `UniversoPermitidoTab.tsx`/teste e contratos de Apólice. Backend: `IExclusaoVinculosApolice.cs`, `ExclusaoVinculosApolice.cs`, `ExcluirVinculosApoliceController.cs`, registro DI, entidades/configurações/snapshot de Seguro, consultas/repositórios afetados, tratamento de erro de hierarquia, constantes/migration de Segurança, migration/SQL de proteção e testes de hierarquia/migration/autorização/catálogo. As alterações locais das entregas anteriores foram preservadas. Homologação visual autenticada em navegador não executada nesta entrega; testes automatizados não a substituem.
