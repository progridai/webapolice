# Vínculos do Cliente e Participações em Apólices

Data: 09/11/2026.

> Continuidade de 11/11/2026: a exclusão de Ramos, Subgrupos, Subestipulantes, Módulos e seus subvínculos, incluindo o Universo anterior, está registrada no [documento 82](82-exclusao-hierarquica-vinculos-apolice.md). As proteções de Vidas/propostas deste documento permanecem em vigor.

## Exclusão de Participação Incluída por Engano — 11/11/2026

Vidas oferece as ações distintas Encerrar e Excluir. Encerrar mantém a participação visível como encerrada. Excluir corrige um lançamento incorreto: preenche `deleted_at` e `updated_at`, define `ativo=false` e retira a participação das consultas operacionais, sem apagar fisicamente o registro, o histórico ou a auditoria. A participação pode estar ativa ou encerrada, mas não pode possuir nenhuma proposta, inclusive propostas inativas/excluídas. Cliente, pessoa e matrícula cadastral não são excluídos nem alterados. A matrícula pode ser selecionada novamente em uma participação correta; sua proteção histórica de identidade permanece.

A exclusão exige confirmação na interface. Falhas são apresentadas no diálogo e permitem nova tentativa. O backend bloqueia a linha da participação em transação antes de verificar propostas; o trigger existente de propostas bloqueia a mesma linha, impedindo criação concorrente de proposta após exclusão. Participação, histórico funcional e auditoria `VIDA_EXCLUIDA` são gravados atomicamente. UUIDs de apólice/participação devem corresponder; a repetição ou o uso de registro já excluído é rejeitado.

Matriz de segurança desta extensão do recurso existente: módulo `APOLICES`, recurso `APOLICES`, ação Excluir Participação de Vida, permissão `apolices.vidas.excluir`, endpoint `DELETE /api/apolices/{publicId}/vidas/{vidaPublicId}`, auditoria `VIDA_EXCLUIDA`. Não se cria módulo, recurso, perfil ou concessão individual. A permissão tem controle separado de encerrar/alterar; o Acesso Total segue as regras atuais de disponibilidade do catálogo. Nenhum perfil sem Acesso Total recebe concessão automática. A migration adiciona somente a permissão ao recurso existente, preservando a inativação de um código já cadastrado.

Nenhuma coluna de negócio ou campo humano novo é necessário. A exclusão reutiliza o `deleted_at` existente; textos do histórico e o nome da permissão seguem a padronização compartilhada. Chaves de permissão, ações de auditoria e UUIDs são técnicos. A migration de catálogo não altera o modelo EF nem os triggers de Title Case; deve ser aplicada pontualmente, sem executar outras migrations pendentes.

Validação: seis checks do frontend aprovados (`format:check`, `typecheck`, `lint`, `lint:design-system`, `test:run`, `build`), com 1.180 testes em 77 arquivos, sem erros/avisos de ESLint ou ocorrências no scanner do design system. O aviso preexistente de chunk acima de 500 kB permanece no build. O conjunto backend passou com 465 testes (6 Auditoria Unit, 2 Auditoria Integration, 51 Cadastro, 28 Arquitetura, 128 API, 250 Integration). A primeira execução completa encontrou um import ausente no novo teste de concorrência; após a correção, o projeto de integração inteiro foi repetido e aprovado, mantendo os demais projetos já aprovados. Cobertura inclui exclusão ativa/encerrada, preservação de cliente/matrícula, ocultação em Vidas/Detalhes do Cliente, correção com a mesma matrícula, proposta existente/excluída, UUID fora da apólice, repetição, rollback da auditoria e criação concorrente de proposta bloqueada e rejeitada após commit. A API exige a permissão própria e a disponibilidade do catálogo; encerrar/alterar isoladamente não autorizam excluir. Testes de interface cobrem os três temas, confirmação/cancelamento, bloqueio por propostas, erro/nova tentativa e retorno da última página. Scanner de segredos e `git diff --check` aprovados.

A migration `20261111190000_AdicionarPermissaoExcluirVida` foi aplicada pontualmente a `webapolice_teste` em `painel.bravida.com.br:5432`, com guardas rígidas do destino, transação, timeout de lock/comando, histórico e auditoria `CATALOGO_EXCLUSAO_VIDA_REGISTRADO`. As comparações por hash preservaram módulos, recursos, perfis, vínculos perfil/permissão e usuário/perfil, demais permissões e migrations anteriores. Nenhum perfil recebeu concessão; a permissão ficou disponível no recurso APOLICES existente. A primeira tentativa do utilitário falhou na compilação antes de abrir conexão (uso de span atravessando await); a correção foi local ao utilitário e a execução seguinte confirmou commit e disponibilidade em conexão independente. SQL e resultado estão em `scratch/excluir-vida-catalogo-20261111/`. Nenhum outro schema/migration pendente ou registro de participação foi alterado.

A API foi publicada isoladamente em `bin/ExecucaoExcluirVida-20261111`, verificada na porta 5008 e ativada na porta 5007, PID 34912. Os hashes de `appsettings.json` e `appsettings.Development.json` foram preservados, assim como o ambiente/configuração externa e a validação JWT. `/api/health/live` e `/api/health/ready` retornaram 200 e stderr ficou vazio. DELETE com UUIDs fictícios, sem token, retornou 401 na instância nova; não se executou exclusão autenticada em registros da base compartilhada. Validação visual autenticada em navegador continua pendente de homologação; os fluxos foram validados automaticamente em ambientes isolados.

Arquivos centrais: `VidasTab.tsx`/teste, `apolices.api.ts`, `ExcluirApoliceVidaHandler.cs`, `IApoliceVinculosRepository.cs`/`ApoliceVinculosRepository.cs`, `ApolicesController.cs`, `SeguroModuleExtensions.cs`, `PermissoesSeguranca.cs`, migration de permissão, `ExcluirVidaAuthTests.cs`, `ClienteVinculosVidaTests.cs`, `VidasCatalogoMigrationTests.cs` e este documento. As alterações anteriores do workspace foram preservadas.

## Seleção do Estipulante no Vínculo — 11/11/2026

Os modais Adicionar/Editar vínculo exibem somente o campo de seleção `Estipulante`, sem o campo separado `Buscar estipulante`. A consulta utiliza o endpoint contextual existente sem filtro de nome e mantém a paginação, carregamento, erro/nova tentativa e a opção atual durante a edição. Matrícula e situação seguem o fluxo existente. Não há alteração de API, banco, permissões ou campos persistidos/Title Case.

Arquivos alterados nesta continuidade: `ClienteVinculosSection.tsx`, `ClienteVinculosSection.test.tsx` e este documento. Os testes nos três temas verificam ausência do campo de busca e salvamento pela seleção; um teste de paginação verifica a escolha e o envio do UUID de um estipulante de outra página.

Validação: os seis checks obrigatórios do frontend passaram (`format:check`, `typecheck`, `lint`, `lint:design-system`, `test:run` e `build`), com 1.173 testes em 77 arquivos, zero erros/avisos de ESLint e zero ocorrências no scanner do design system. O build mantém o aviso preexistente de chunk acima de 500 kB. `git diff --check` aprovado; servidor frontend local respondeu HTTP 200. Não houve reinício da API nem alteração de backend. Validação visual autenticada em navegador não executada nesta continuidade.

## Identificação das Apólices no Cadastro — 10/11/2026

A seção Vínculos dos Detalhes do Cliente mostra, para cada matrícula, as apólices efetivamente associadas por `seguro.apolice_vida.cliente_vinculo_id`. Exibe nome e todos os números distintos cadastrados nos ramos da apólice. O mesmo estipulante pode ter várias apólices, por isso o estipulante sozinho não identifica a participação. O relacionamento continua sendo escolhido e salvo em Vidas da apólice: esta apresentação não cria associações automaticamente nem torna uma matrícula exclusiva de uma apólice.

O GET paginado `/api/clientes/{clienteId}/vinculos` recebe o campo adicional `apolices`, sempre uma lista. Cada item contém `publicId`, `nome`, `numeros`, `ativa` e `participacaoAtiva`. Várias participações da mesma matrícula na mesma apólice geram uma única identificação; `participacaoAtiva` é verdadeiro quando há ao menos uma participação não excluída ativa. Participações/apólices inativas continuam visíveis e recebem indicação textual. Participações ou apólices excluídas logicamente não são apresentadas. Esses filtros não alteram a proteção histórica contra edição de identidade de vínculos em uso. A consulta reflete a associação atual da participação, não é um relatório de alterações passadas de matrícula.

Sem associação, a interface orienta selecionar a matrícula em Vidas da apólice. Sem nome, mantém os números disponíveis; sem ambos, usa o UUID público como identificação. A exibição funciona também para leitores sem permissão de alteração, nos temas claro, escuro e sistema.

A consulta segue a permissão `clientes.visualizar` do endpoint: são metadados contextuais mínimos das participações daquele cliente, sem dados de cobertura, valores, propostas ou acesso à tela completa de apólices. As políticas específicas de apólices permanecem exigidas em seus respectivos endpoints. O Cadastro pagina seus vínculos e solicita as identificações em lote pelo contrato `IClienteVinculosUso`, implementado no Seguro. A implementação valida também o cliente na consulta de participações; não há acesso direto de um módulo às tabelas do outro, nem consulta individual por vínculo. IDs internos ficam na comunicação interna; somente UUIDs públicos saem no JSON.

Esta entrega é uma projeção de leitura: não cria tabelas, colunas, migrations, permissões ou campos humanos persistidos. Nomes usam a padronização compartilhada existente; números de apólice são códigos técnicos e preservam sua caixa. Não há reprocessamento de dados nem necessidade de reaplicar triggers de Title Case.

Validação: seis checks obrigatórios do frontend aprovados (`format:check`, `typecheck`, `lint`, `lint:design-system`, `test:run`, `build`), com 1.172 testes em 77 arquivos, zero erros/avisos de ESLint e zero ocorrências no scanner do design system. O build mantém o aviso preexistente de chunk acima de 500 kB, sem alterar os limites. Backend completo: 452 testes aprovados (6 Auditoria Unit, 2 Auditoria Integration, 51 Cadastro, 28 Arquitetura, 123 API, 242 Integration). Os dois testes PostgreSQL novos cobrem matrículas do mesmo estipulante, múltiplas apólices por matrícula, deduplicação de participações/números, paginação/vazio, isolamento por cliente, inativação, exclusão lógica e preservação da proteção histórica. Três testes de interface cobrem os temas claro/escuro/sistema, identificação por matrícula, preservação da caixa dos números, ausência de identificação e estado vazio. Scanner de segredos e `git diff --check` aprovados.

A consulta à base de teste foi executada em transação `READ ONLY` com rollback. Confirmou a matrícula `123` associada a `APO TESTE` (dois números de ramo) e `12345` a `Apo Teste 2` (sem número cadastrado), ambas com participação ativa. Nenhuma matrícula, participação, nome histórico ou apólice foi alterada. A API foi publicada em pasta isolada `bin/ExecucaoApolicesVinculo-20261110`, testada na porta 5008 e ativada na porta 5007 (PID 49064), preservando os hashes de `appsettings.json`/`appsettings.Development.json`, ambiente e configuração externa. A primeira checagem usou incorretamente `/health/ready` e recebeu 401; na rota correta `/api/health/ready`, a nova instância retornou 200 antes da troca. Após a troca, `/api/health/live` e `/api/health/ready` retornaram 200, com stderr vazio. O endpoint de vínculos permanece exigindo autenticação (401 sem token). A inspeção visual autenticada no navegador não foi executada nesta entrega; os testes automatizados e a consulta somente leitura não a substituem.

Arquivos centrais desta continuidade: `ClienteVinculosSection.tsx` e seu teste, `clienteVinculosApi.ts`, `ClienteVinculosHandler.cs`, `IClienteVinculosRepository.cs`, `ClienteVinculosRepository.cs`, `IClienteVinculosUso.cs`, `ClienteVinculosUso.cs`, `ClienteVinculosVidaTests.cs` e os documentos 18/81. As demais alterações locais das continuidades anteriores foram preservadas.

## Correção de 10/11/2026: Catálogo de Permissões de Vidas

O salvamento autenticado chegou a retornar HTTP 403 antes de entrar no controller: a política `apolices.vidas.alterar` não estava disponível no catálogo da base de teste. A leitura do catálogo confirmou a ausência das três permissões de Vidas, enquanto os recursos/módulos APOLICES e CLIENTES e `clientes.alterar` estavam ativos e habilitados. A interface permitia abrir a edição pelo Acesso Total, mas a proteção definitiva da API exige também a existência da permissão no catálogo.

A migration existente `20260822210000_AdicionarPermissoesVidasApolice` não possuía atributos de identificação nem arquivo Designer correspondente e não era descoberta pelo EF; também não constava no histórico do destino. Foram adicionados apenas os metadados DbContext/Migration, mantendo seu SQL e a matriz existentes. A carga registra `apolices.vidas.inserir`, `apolices.vidas.alterar` e `apolices.vidas.inativar` no recurso APOLICES. Não adiciona vínculos de perfil/usuário, não concede Acesso Total nem habilita módulos/recursos. Uma permissão existente inativa permanece inativa, pois o conflito só atualiza o nome.

A aplicação deve ser pontual e transacional, usando somente o SQL dessa migration e seu registro no histórico, com verificação do recurso de destino e preservação das demais permissões, vínculos de perfis/usuários e flags de catálogo. Não usar `database update` até a última migration para aplicar outras pendências de Segurança. O backend consulta a disponibilidade a cada requisição; a correção do catálogo não exige desativar validações ou reiniciar a sessão do Keycloak.

Validação desta correção: dois testes PostgreSQL específicos aprovados e suíte completa com 450 testes aprovados (6 Auditoria Unit, 2 Auditoria Integration, 51 Cadastro, 28 Arquitetura, 123 API, 240 Integration), sem falhas/ignorados. Testes confirmam descoberta pelo EF, existência e disponibilidade das três permissões em uma base nova, restauração pontual, preservação de UUIDs/inativação na reexecução e ausência de concessões a perfis/usuários. Nenhum arquivo frontend foi alterado nesta correção.

O SQL revisável está em `scratch/catalogo-vidas-20261110/migration.sql`, extraído diretamente da migration existente. O aplicador local foi preparado para transação única, alvo restrito a `webapolice_teste` em `painel.bravida.com.br:5432`, comparação por hashes do estado fora do escopo, histórico pontual e auditoria. Sua execução para geração do SQL foi recusada pela revisão automática, que apontou risco de alteração no banco errado, pois o executável também contém o modo de aplicação. A geração do arquivo SQL foi então concluída apenas por leitura do código, sem executar esse aplicador nem abrir conexão. Naquele momento, a carga real ficou pendente de autorização explícita do responsável; a preparação não alterou registros/perfis. A autorização e a aplicação posteriores estão registradas abaixo.

### Aplicação Autorizada do Catálogo — 10/11/2026

Após autorização explícita para esse destino, foi aplicada somente `20260822210000_AdicionarPermissoesVidasApolice` em `webapolice_teste`, `painel.bravida.com.br:5432`. O aplicador conferiu rigidamente host/porta/banco e o baseline (permissões e migration ausentes, recurso APOLICES único, ativo e habilitado), e executou o SQL original, o registro no histórico e o evento de auditoria `CATALOGO_VIDAS_REGISTRADO` em uma única transação. Foram usados timeout de lock de 10 segundos e timeout de comando de 30 segundos. Nenhuma outra migration pendente foi aplicada.

Três permissões foram registradas e confirmadas disponíveis em uma conexão independente após commit: `apolices.vidas.inserir`, `apolices.vidas.alterar` e `apolices.vidas.inativar`. As comparações de contagem/hash, sob locks das tabelas de catálogo, preservaram integralmente os sete módulos, 18 recursos, dois perfis, 82 vínculos perfil/permissão, cinco vínculos usuário/perfil, as 82 demais permissões e os 18 registros anteriores de migrations. O histórico recebeu somente a migration autorizada. Não houve concessão individual a perfis/usuários, alteração de habilitação ou gravação de matrícula/participação por esta aplicação.

A API permaneceu ativa e retornou HTTP 200 no readiness. Como a disponibilidade do catálogo é consultada por requisição, a correção fica efetiva no próximo salvamento, sem reinício da API ou do Keycloak. O salvamento autenticado da participação permanece a confirmar pelo usuário. Evidência técnica com contagens/hashes e resultado em `scratch/catalogo-vidas-20261110/resultado.json`, ignorada pelo Git e sem credenciais/dados pessoais.

## Correção de 10/11/2026: Selecionar Matrícula Existente Sem Estipulante

A criação contextual anterior não cobria a escolha das matrículas legadas que já estavam cadastradas sem estipulante. O seletor da Vida passa a listar também vínculos ativos do mesmo Cliente/Pessoa, com matrícula preenchida, sem estipulante e sem uso em qualquer Vida/Proposta, além dos vínculos ativos já compatíveis com a apólice. A paginação e a contagem consideram as duas categorias. Vínculos de outro estipulante, inativos, sem matrícula ou com identidade já utilizada permanecem excluídos dessa nova opção.

Essas opções são identificadas por `requerEstipulante` na resposta. Ao escolher explicitamente uma delas, o formulário informa que o estipulante da apólice será associado ao salvar e envia `completarVinculoSemEstipulante: true` com `clienteVinculoPublicId`. O novo indicador é técnico, booleano e não persistido. Sem esse indicador, uma seleção incompleta continua inválida. Não se escolhe matrícula automaticamente. `clientes.alterar` é obrigatório para completar o cadastro, além da permissão de Vida. O backend verifica novamente cliente/pessoa, status, ausência de uso e duplicidade sob o mesmo lock de Cliente usado nas demais mutações.

Somente o estipulante e a data técnica de atualização do vínculo escolhido podem mudar; matrícula, UUID, campos legados, dados bancários e evidências de migração são preservados. Nenhum vínculo adicional é criado nessa operação. A associação e seu evento `VINCULO_COMPLETADO` participam da transação da Vida, histórico e auditoria; falha ou cancelamento reverte tudo. Se outra requisição já completou o vínculo para o mesmo estipulante, preserva-se a associação compatível; se o atribuiu a outro, a operação é rejeitada. Nenhuma nova migration ou alteração de política Title Case é necessária.

### Validação e Atualização Local da Correção

O diagnóstico real, em transação PostgreSQL `READ ONLY` finalizada com rollback, confirmou que o cliente tinha duas matrículas ativas sem estipulante, e a consulta anterior retornava zero opções. A consulta corrigida retornou as duas opções com `requerEstipulante: true`, sem completar ou criar registros na base existente.

Os seis checks frontend passaram: formatação, tipos, lint sem erros/avisos, scanner de design system sem ocorrências, 1.169 testes em 77 arquivos e build. Permanece o aviso já existente do Vite para chunk acima de 500 kB. A execução final da solução backend aprovou 448 testes (6 Auditoria Unit, 2 Auditoria Integration, 51 Cadastro, 28 Arquitetura, 123 API, 238 Integration), sem ignorados ou falhas. A primeira execução completa encontrou uma falha na comparação do registro anterior em memória; o teste foi corrigido para comparar a imagem efetivamente persistida antes/depois, e a solução inteira foi executada novamente com sucesso. Nenhuma configuração de qualidade foi flexibilizada.

Os cenários novos cobrem paginação/contagem das opções incompletas, indicação e seleção nos três temas, intenção explícita e permissão adicional, preservação dos demais campos do registro persistido, rejeição por outro cliente/pessoa/estipulante, inativação, duplicidade, vínculo usado em proposta legada, rollback do complemento e concorrência entre dois estipulantes em conexões independentes. O teste de concorrência confirma somente uma associação/participação vencedora.

A API foi publicada em `backend/src/WebApolice.Api/bin/ExecucaoVinculoExistente-20261110`, com compilação isolada e sem erros/avisos. Os arquivos de configuração foram comparados por hash com a versão anterior, e permaneceram iguais. A versão iniciou na porta temporária 5008 e substituiu somente o processo identificado da porta 5007 após a aprovação dos testes. A verificação local confirmou liveness/readiness e frontend HTTP 200, CORS HTTP 204 com a origem correta e consulta sem token HTTP 401. Não houve mudança de Keycloak, migration adicional ou gravação de demonstração nos vínculos reais. A confirmação visual/autenticada no navegador depende de reabrir a edição e escolher a matrícula; a associação será efetivada apenas pelo salvamento do usuário.

Evidências locais ignoradas pelo Git: `scratch/vinculo-existente-api-20261110/` e `scratch/vinculo-existente-validacao-20261110/`. Scanner de segredos e `git diff --check` aprovados.

## Continuidade: Criar a Matrícula Dentro da Vida

O formulário de inclusão/edição da participação passa a oferecer seleção de vínculo existente ou cadastro de nova matrícula, sem sair da apólice. No cadastro novo, o estipulante é obtido exclusivamente da apólice no backend, e o cliente da participação (imutável na edição). O campo opcional `novaMatricula` é um código técnico, preservado pela política compartilhada de Title Case; não há campo humano novo, alteração de classificação, tabela, coluna ou migration adicional.

POST/PUT de Vidas aceitam `clienteVinculoPublicId` ou `novaMatricula`, mutuamente exclusivos. POST exige uma das opções; na edição, ambos ausentes preservam o vínculo anterior. Matrícula nova exige conteúdo não vazio após Trim e no máximo 50 caracteres. Vínculo ativo com a mesma matrícula normalizada/cliente/estipulante causa rejeição e orientação para selecionar o existente. Vínculos legados incompletos não são preenchidos ou atribuídos automaticamente. É possível usar o mesmo código em estipulantes diferentes.

A criação contextual exige as permissões já existentes de inserir/alterar Vidas e `clientes.alterar`. A seleção de um vínculo existente mantém as permissões atuais da Vida. Nenhum perfil recebe concessões novas. Uma participação com propostas continua impedida de trocar/criar vínculo, e uma participação encerrada continua sem edição.

O adaptador de Cadastro participa da transação física da Vida por um contrato explícito baseado em `DbTransaction` de ADO.NET (sem tipos EF/provider nos handlers e sem acesso à infraestrutura de outro módulo). Cadastro, Vida, histórico e os dois eventos de auditoria são confirmados em conjunto; falha de validação, persistência, cancelamento ou auditoria deve reverter as gravações. O lock do Cliente é o mesmo da manutenção de vínculos, incluindo a verificação de duplicidade concorrente. Cancelar o formulário não chama endpoint para criar o vínculo.

### Verificação e Atualização Local Dessa Continuidade

| Verificação | Resultado |
|---|---|
| Frontend: format:check, typecheck, lint, lint:design-system | Aprovados; sem pendências de formatação, erros de tipos, erros/avisos de lint ou ocorrências no scanner |
| Frontend: test:run | 1.164 testes aprovados em 77 arquivos |
| Frontend: build | Aprovado; permanece o aviso existente de chunk acima de 500 kB |
| Backend: solução completa | 433 testes aprovados: 6 Auditoria Unit, 2 Auditoria Integration, 51 Cadastro, 28 Arquitetura, 119 API e 227 Integration |
| Integração específica de vínculos após correção de aviso do analisador de testes | 21 testes aprovados; nenhuma falha ou aviso na execução final |
| Publicação Release da API | Concluída sem erros/avisos de compilação |
| Scanner de segredos e git diff --check | Aprovados |

Os novos cenários cobrem criação contextual na inclusão/edição, estipulante obtido da apólice, preservação de matrícula técnica e normalização de observações, matrícula vazia/longa/duplicada, envio simultâneo das duas opções, estipulante inativo, propostas existentes, rollback dos dois registros e dos eventos após falha na auditoria, cancelamento e duas inclusões concorrentes em conexões independentes. Os testes HTTP verificam a permissão adicional somente no cadastro novo. Frontend cobre os três temas, limpeza ao mudar cliente/modo, cancelamento sem requisição de criação, validação, erro com preservação da digitação e restrições de permissões/propostas.

A versão foi publicada isoladamente em `backend/src/WebApolice.Api/bin/ExecucaoVidaMatricula-20261109`, validada em localhost:5008 e colocada em localhost:5007 após conferir o processo anterior exato. Os arquivos de configuração publicados foram comparados por hash com a versão anterior e permaneceram iguais; nenhuma configuração de autenticação/Keycloak ou registro de negócio da base existente foi alterado por esta continuidade. Não houve migration adicional. O Vite serve o componente atualizado em localhost:5173. Após a troca, liveness/readiness e frontend/módulo responderam HTTP 200, preflight CORS HTTP 204 com a origem correta e consulta contextual sem token HTTP 401.

Logs da atualização local em `scratch/vida-matricula-api-20261109/`; logs dos seis checks frontend em `scratch/vida-matricula-validacao/`, ambos ignorados pelo Git. A homologação visual e a gravação autenticada pelo usuário no navegador continuam a confirmar após recarregar a tela; os testes automatizados executaram as gravações em PostgreSQL efêmero, sem criar matrículas de demonstração no cadastro existente.

## Regras Funcionais

O cadastro central do Cliente pode ter vários vínculos, inclusive matrículas diferentes no mesmo estipulante. Cada participação (Vida) seleciona explicitamente um vínculo ativo desse Cliente/Pessoa e do mesmo estipulante da Apólice. Subgrupo e Módulo da Apólice continuam independentes do vínculo cadastral. Vínculo ativo não significa seguro vigente.

No detalhe do Cliente será possível incluir vínculos, completar registros legados e editar matrícula/estipulante antes do uso. Após uso em Vida ou Proposta, esses dois campos ficam imutáveis; uma nova relação exige outro vínculo. Inativação, inclusive de registros legados incompletos, preserva todas as referências históricas e impede novas seleções/propostas. Não haverá exclusão física, alteração de evidências de migração nem edição bancária nesta entrega. Novos vínculos exigem estipulante e matrícula (até 50 caracteres); matrícula é código técnico e mantém sua grafia. Duplicidade ativa de Cliente/Estipulante/Matrícula normalizada é rejeitada.

Na inclusão de Vida, deve-se selecionar um vínculo existente por clienteVinculoPublicId ou informar novaMatricula, conforme a continuidade acima. Na edição, omitir/null preserva a referência existente para compatibilidade com consumidores anteriores; uma Vida antiga sem vínculo pode ser completada se não possuir propostas. A troca exige vínculo válido e ausência de qualquer proposta nessa Vida, inclusive histórica. Enviar o mesmo vínculo preserva relações inativas já existentes e permite editar os demais campos. Não se escolhe matrícula automaticamente. Um vínculo legado sem estipulante pode ser completado mediante seleção explícita e envio de completarVinculoSemEstipulante, conforme a correção de 10/11/2026.

Propostas continuam obtendo Cliente/Pessoa/Vínculo da Vida; a compatibilidade do estipulante também será validada. Alterar o vínculo da Vida nunca reescreve propostas. A persistência PostgreSQL protegerá essas invariantes também contra concorrência entre gravações.

## Modelagem e Contratos

Adicionar somente cadastro.cliente_vinculo.public_id (uuid persistido, único), preenchendo registros existentes sem alterar seus demais campos. Mapear também as colunas existentes matricula_normalizada, criterio_criacao e updated_at para gravações nativas. Nenhum novo campo humano. Não modificar classificações Title Case: nomes continuam normalizados pelo mecanismo compartilhado; matrícula, UUID e critérios técnicos são preservados.

Novos endpoints filhos de /api/clientes/{id}/vinculos usam UUID, paginação e permissões existentes: clientes.visualizar na consulta e clientes.alterar nas mutações. O seletor contextual de Vidas usa apolices.visualizar; criar/alterar continuam com apolices.vidas.inserir/alterar. O seletor de estipulantes do vínculo é contextual, paginado e retorna somente UUID/nome de estipulantes ativos com clientes.visualizar, sem exigir acesso à gestão de Estipulantes. O PATCH /api/clientes/{id}/vinculos/{publicId}/situacao usa clientes.alterar; a reativação exige matrícula/estipulante preenchidos e ausência de duplicidade ativa. Sem novos módulos, recursos, perfis ou concessões. Toda mutação de vínculo cadastral e seleção na Vida deve registrar auditoria na mesma transação física, sem documentos, dados bancários ou matrícula no payload de auditoria. Histórico funcional da Apólice é preservado.

As migrations 20261109232452_IdentificarClienteVinculo e 20261109232500_ProtegerVinculosVidas devem ser aplicadas em ordem Cadastro → Seguro, antes de publicar a API/frontend. A inclusão de Vida passa a rejeitar payload antigo sem clienteVinculoPublicId: mudança funcional intencional documentada. As leituras de Vida acrescentam UUID/matrícula do vínculo e indicação de propostas, preservando os campos anteriores. Registros legados continuam consultáveis. Na entrega inicial de código, as migrations não foram aplicadas à base existente; a aplicação posteriormente autorizada está registrada na continuidade abaixo. Reaplicar o script Title Case após as migrations com normalização histórica desativada, conforme documento 26.

## Validação

A ferramenta EF detectou uma navegação Estipulantes órfã no snapshot do Cadastro, incompatível com WithMany() atual. O snapshot foi corrigido para permitir gerar a migration, sem alterar relacionamentos físicos nem designers históricos. As propriedades já existentes no banco (matricula_normalizada, criterio_criacao e updated_at) foram reconciliadas no snapshot; a migration adiciona somente o UUID/índice.

| Verificação | Resultado |
|---|---|
| Frontend: format:check, typecheck, lint, lint:design-system | Aprovados, zero pendências/erros/avisos de lint e zero ocorrências no scanner |
| Frontend: test:run | 1.157 testes aprovados, 77 arquivos |
| Frontend: build | Aprovado; permanece o aviso existente de chunk acima de 500 kB |
| Backend: build e test da solução | 415 testes aprovados, nenhum ignorado ou falho |
| EF: has-pending-model-changes do Cadastro | Sem divergências |
| Scanner de segredos e git diff --check | Aprovados |

As integrações usaram PostgreSQL 18.4 efêmero via Docker. Cobrem múltiplas matrículas/estipulantes, duplicidade ativa, filtro contextual paginado, vínculo ausente/inativo/de outro cliente ou estipulante, troca antes de propostas, bloqueio após propostas, preservação de vínculo inativo/histórico, inativação de legado incompleto, UUIDs distintos sem alteração das matrículas, rollback real da auditoria e corrida entre proposta e troca de vínculo em conexões independentes. As barreiras HTTP verificam 401/403 nas consultas e mutações novas.

Os testes frontend cobrem temas claro, escuro e sistema no DOM, seleção explícita, limpeza ao trocar cliente, cancelamento/descarte de respostas antigas, erro/retry, preservação de edição em falha e bloqueio por propostas. Não constituem homologação visual: a consulta às superfícies disponíveis não encontrou navegador conectado. A validação autenticada/visual no ambiente de destino permanece pendente. No fechamento inicial da implementação, nenhuma migration havia sido aplicada à base existente. A aplicação posterior consta abaixo. Nenhum processo publicado foi reiniciado nesta entrega.

## Arquivos Principais

- Frontend: ClienteVinculosSection e clienteVinculosApi; VinculoVidaSelect, useVinculosVida, ApoliceVidaFormModal e VidasTab. Contratos e testes correspondentes atualizados.
- Cadastro: ClienteVinculosController, ClienteVinculosHandler, ClienteVinculosRepository e modelo/configuração de ClienteVinculo. UUID e reconciliação do snapshot na migration IdentificarClienteVinculo.
- Seguro: validação nos casos de uso de Vida e Proposta, consultas de vínculos via contrato do Cadastro, auditoria transacional de Vidas e migration ProtegerVinculosVidas para proteção concorrente no banco.
- Regras vigentes também refletidas no documento 18. O documento 77 continua válido quanto às demais pendências de liberação de Propostas; esta entrega não define cálculos financeiros nem emissão.


## Aplicação Autorizada na Base de Teste — 09/11/2026

Após autorização explícita do responsável, foram aplicadas em `webapolice_teste` (PostgreSQL 17.11) somente `20261109232452_IdentificarClienteVinculo` e `20261109232500_ProtegerVinculosVidas`, nessa ordem. Foram usados os scripts gerados pelo EF para cada migration, reunidos em uma transação única com timeout de lock de 10 segundos, verificação dos baselines e comparação dos registros originais antes/depois, sob locks das tabelas afetadas. A comparação excluiu somente a nova propriedade public_id do vínculo. O histórico recebeu somente as duas migrations autorizadas.

A migration anterior `20261023000100_AdicionarPublicIdPropostaItem` estava pendente antes desta operação e continua pendente. As novas proteções não dependem dela. Não foi usado `database update` até a última migration para evitar aplicar silenciosamente essa outra alteração. A criação de Propostas continua sujeita também às demais pendências do documento 77.

A revisão automática recusou a exportação completa dos registros para backup local por envolver uma cópia de toda a base potencialmente sensível sem autorização específica para essa exportação. Como alternativa, foi gerado apenas um backup da estrutura, em diretório com acesso restrito e ignorado pelo Git; sua extração integral para descarte foi validada. Esse arquivo não contém os registros de negócio nem representa um backup completo ou um ensaio de restauração da base. A aplicação das migrations preservou os dados originais e foi protegida pela transação e pelas comparações descritas acima.

Validação em nova conexão após commit:

| Verificação | Resultado |
|---|---|
| Histórico das duas migrations | Confirmado nos contextos Cadastro e Seguro |
| Vínculos / UUIDs distintos | 447.228 / 447.228; nenhum UUID nulo ou zero |
| Índice único de UUID | Válido |
| Registros originais | Preservados: 447.228 vínculos, duas Vidas e zero Propostas |
| Proteções de vínculo | Três triggers habilitados; corpos das funções conferidos por hash com o SQL gerado |
| Title Case | Script integral reaplicado com normalização histórica desligada; 54 triggers habilitados e zero registros históricos alterados |
| Exemplo com acentos | Função do destino confirmou `JOÃO DA SILVA` → `João da Silva` |

Evidências locais ignoradas pelo Git: `scratch/vinculos-aplicacao-20261109/resultado-aplicacao.json` (métricas e hashes técnicos), scripts SQL gerados/aplicado no mesmo diretório e `scratch/backups/vinculos-20261109/estrutura.dump` (estrutura apenas, SHA-256 `19eca85a667124b76926e536d6fca46b74d47cf1b9e052bc1ec571b35925eb4c`). Nenhuma credencial foi incluída nas evidências ou nesta documentação. A API/frontend em execução não foram atualizados ou reiniciados por esta operação de banco.


## Correção da API Local em Execução — 09/11/2026

Após a aplicação das migrations, o frontend novo encontrou erros ao consultar vínculos no Cliente e na participação da Vida. Foi confirmado HTTP 404 no endpoint filho de Clientes e identificado que a porta local 5007 ainda era atendida por um processo antigo, iniciado antes da implementação dessas rotas. Aplicar migrations e compilar os projetos não substitui o processo que já está em execução.

A API atual foi publicada em `backend/src/WebApolice.Api/bin/ExecucaoVinculos-20261109`, usando `OutputPath=bin/ExecucaoVinculosBuild/` para manter a compilação independente das DLLs bloqueadas pelo processo antigo. A primeira publicação com a pasta padrão falhou por esse bloqueio; a publicação isolada foi concluída. A nova versão foi iniciada temporariamente na porta 5008 e passou por liveness/readiness antes da substituição do processo exato que atendia a porta 5007. A configuração externa existente foi preservada, com destino `webapolice_teste`; não houve alteração de credenciais, permissões, banco ou frontend nesta correção operacional.

Verificações após a troca:

- `/api/health/live` e `/api/health/ready`: HTTP 200, PostgreSQL Healthy.
- Rotas de vínculos do Cliente, estipulantes disponíveis e vínculos disponíveis na Vida: reconhecidas e respondendo HTTP 401 sem sessão, preservando a exigência de autenticação.
- Preflight CORS do endpoint de vínculos: HTTP 204 com a origem correta para localhost:5173 e 127.0.0.1:5173.
- Diagnóstico com os repositórios/consultas reais e transação PostgreSQL `READ ONLY`: cinco clientes com vínculos e as duas participações existentes foram consultados sem erro, incluindo listagem, detalhes e seleção contextual. A transação terminou com rollback, sem gravar registros.

As verificações HTTP acima não utilizaram a sessão autenticada do usuário; a confirmação visual depende de recarregar a página no navegador. Logs e metadados técnicos da execução local ficam em `scratch/vinculos-api-20261109/`, ignorados pelo Git. A API permanece ativa na porta 5007 com os binários atuais. O frontend servido pelo Vite na porta 5173 já continha a implementação nova.

### Bloqueio de Autenticação Identificado Após a Troca

O usuário confirmou erro de autorização em `http://127.0.0.1:5173/#/app`. Os logs mostram `/api/seguranca/me` retornando HTTP 401 com `SecurityTokenInvalidAudienceException`; a validação estrita de audiência da versão atual rejeita o token recebido. Os serviços e módulos do Vite continuam acessíveis, e os health checks da API retornam HTTP 200. A configuração da API exige `webapolice-api` e o client frontend configurado é `webapolice-web`.

A autenticação da credencial administrativa já configurada no backend foi bem-sucedida, mas a consulta aos clients do realm retornou HTTP 403. Nenhum client, mapper, usuário ou perfil remoto foi alterado; a configuração efetiva do mapper remoto e um novo login ainda precisam ser conferidos por um administrador. O documento 05 descreve o reparo pontual e sua validação. A declaração local do realm foi corrigida para incluir o mapper já previsto pelo script de provisionamento. A validação JWT da API permanece estrita; a página autenticada ainda não foi homologada após esse bloqueio.

### Retorno do Login e Conferência do Seletor de Vínculos

Na continuidade, o responsável acessou o escopo dedicado do client web no Keycloak e mostrou a tela sem mapeadores dedicados. Foi orientado a configurar o mapper Audience para `webapolice-api` no access token. Após essa orientação, confirmou o retorno do acesso; os logs locais também registraram HTTP 200 em `/api/seguranca/me` e na consulta de vínculos disponíveis da Vida. A sessão do administrador remoto não foi operada pela automação.

O seletor vazio apresentado em seguida foi conferido com as consultas reais em uma transação PostgreSQL `READ ONLY`, finalizada com rollback. Os dois vínculos do cliente em questão estavam ativos e referiam-se à mesma Pessoa, mas ambos possuíam `estipulante_id` nulo. A apólice tinha um estipulante ativo. Assim, a consulta contextual retornou zero opções conforme a regra funcional, sem erro de carregamento. Nenhum registro de negócio foi alterado pelo diagnóstico.

Para completar esse cadastro, editar nos detalhes do Cliente o vínculo que efetivamente corresponde à relação de negócio e informar o mesmo estipulante da Apólice. Ao salvar e reabrir a participação em Vidas, esse vínculo passa a atender ao filtro de seleção. A escolha do vínculo/estipulante depende da confirmação funcional do responsável; não preencher automaticamente todos os vínculos legados com o estipulante da apólice aberta.
