# Revisão de Arquitetura, Segurança, Qualidade e Documentação

Data da análise original: 23/10/2026. Consolidação iniciada em 23/10/2026; acompanhamento abaixo. As evidências e os resultados da análise original foram preservados nas seções seguintes.

## Situação Atual — 26/10/2026

Continuidade autorizada para preparar a integração da fix em main, após o commit 891beb7. Os três commits recentes de main foram incorporados, as dependências arquiteturais corrigidas e o Docker iniciado para as integrações efêmeras. Detalhes, causas das falhas iniciais e evidências finais estão no [documento 79](79-preparacao-integracao-main.md). O acompanhamento de 23/10 abaixo foi preservado como registro daquela execução.

| Achado | Situação atual e evidência adicional |
|---|---|
| A01 | Configuração externa e scanner mantidos. Lançador de catálogos deixou de ler .env/clonar bases. Rotação/revogação e histórico Git continuam a cargo dos responsáveis. |
| A02 | Proteção de erros preservada ao incorporar main: mensagens Keycloak desconhecidas/JSON não objeto não são expostos; warning não registra corpo bruto. Regressões novas aprovadas. |
| A03 | Configuração/JWT mantidos; 101 testes API aprovados. IdP implantado ainda exige homologação própria. |
| A04 | Persistência de Propostas validada em PostgreSQL 18.4, com Cliente/Pessoa/vínculo reais, UUID de item, duas gravações nativas sem legado inventado e rollback por falha da auditoria em nova conexão. Código real do status inicial e regras de liberação continuam pendentes. |
| A05 | Testes físicos de falha/cancelamento de auditoria de Clientes e falha de Propostas aprovados. A uniformização dos demais CRUD indicada abaixo continua pendência de código; não presumir cobertura universal. |
| A06 | Dependências proibidas detectadas em Application foram corrigidas em Cadastro/Segurança/Seguro por portas específicas, queries/adaptadores EF e tipos Domain. Contratos não expõem EF ou IQueryable. Não equivale a revisão manual exaustiva de todo SQL e fronteira de negócio. |
| A07 | 28 testes arquiteturais aprovados sobre os 14 assemblies, sem allowlist, exclusão ou relaxamento; as duas regras antes reprovadas passaram. |
| A08 | Regras de CPF/CNPJ e preservação do legado mantidas; 50 testes Cadastro aprovados. Expectativas antigas de Title Case em outras integrações foram corrigidas sem mudar a política. |
| A09 | Matriz de negação preservada; testes de catálogos reais também executam, sem ignorados por falta de conexão externa. |
| A10 | Zod recebeu patch 4.6.6 pelo novo advisory alto; auditorias npm/NuGet aprovadas sem vulnerabilidades reportadas. Build sem avisos/erros. |
| A11 | Gate local completo aprovado com 393 testes backend e 1.139 frontend; TRX ignorados reprovam. O CI de 8b20807 aprovou backend/segredos e falhou no frontend pela ausência das quatro entradas VITE obrigatórias. Workflow e gate local agora fornecem valores públicos fictícios; seis checks passaram sem .env local. Evidências e conferência remota no [documento 80](80-correcao-configuracao-ci-frontend.md). Proteções de branch ainda não verificadas. |
| A12 | Docker/containers PostgreSQL efêmeros disponíveis e executados. Imagens de aplicação e infraestrutura real (TLS/proxy, backup, triggers no destino, carga, alertas) não homologadas. |
| A13 | Documentos normativos, ADR 008 e guias atualizados para a integração, preservando histórico de testes e migrations. README alinhado ao SDK do global.json. |
| A14 | Cancelamento HTTP mantido e suíte frontend completa aprovada novamente após merge e patch Zod. |

A integração do código em main não libera Propostas nem implanta a aplicação. Permanecem as decisões de negócio, credenciais e homologações acima, além da auditoria uniforme dos escritores remanescentes. Nenhum dado de produção, registro de auditoria ou evidência original de migração foi modificado.

## Acompanhamento da Correção — 23/10/2026

Execução sem commit/push e sem acesso a dados de produção. Alterações locais anteriores preservadas. O relatório original abaixo permanece como registro da análise; esta seção registra o resultado da implementação e distingue correção local de homologação. **A consolidação ainda tem pendências de código e de ambiente; não está integralmente aprovada.**

| Achado | Situação após conferir e corrigir o código | Evidências e pendências |
|---|---|---|
| A01 | Correção aplicada nos arquivos atuais | Removidos fallbacks de conexão de sete contextos e factories; ferramentas exigem variáveis. Scanner local passou e detectou credencial artificial na regressão. Não analisa todo o histórico. Responsáveis precisam substituir/revogar credenciais expostas; nenhum valor foi testado em servidores. |
| A02 | Correção aplicada e regressões locais aprovadas | 500 genérico com traceId inclusive em host Production; exceção apenas no logger. CEP retorna 502 ProblemDetails sem detalhes internos; erros técnicos de Seguradoras/Subestipulantes não viram validação de negócio. Testes API: 101 aprovados. Mudanças do contrato de erro estão explícitas no documento 08. |
| A03 | Correção aplicada e regressões locais aprovadas | Audience obrigatória, validação JWT sempre ativa, HTTPS/metadata fora das exceções locais restritas. Startup rejeita configuração inválida. Factory preserva validações reais, RSA artificial e clock controlado. Keycloak implantado não foi consultado. |
| A04 | Defeitos comprovados corrigidos; liberação pendente | DI/MediatR, Cliente/Pessoa/vínculo real, permissões aprovadas, paginação, UUID persistido e escritas auditadas. Duas migrations não aplicadas: Seguro (UUID e legado_id anulável) e catálogo Segurança (sem atribuir perfis). Nove testes em memória e barreiras HTTP passam; status inicial aguarda código do responsável. Ver documento 77. |
| A05 | Correção aplicada em recortes; auditoria uniforme ainda pendente | OperacaoAuditada por portas de Application em todas as quatro escritas de Cliente, Cooperados, Estipulantes, habilitação de módulo/recurso e Propostas. Negócio/auditoria participam do mesmo coordenador físico. Três testes PostgreSQL tentados falharam na inicialização do Docker, antes de comprovar rollback. Outros CRUD de Cadastro ainda precisam revisão/adaptação: Seguradoras, Corretoras, Subestipulantes e Módulos. Keycloak não participa de transação distribuída. |
| A06 | Redução progressiva; pendência real de código | Clientes não expõe modelos de Infrastructure nem consulta SegurancaDbContext: modelos ficam em Domain, consultas de disponibilidade em contrato público, operação auditada por porta. Adaptadores HTTP de Segurança foram para Api. Atributo AuthorizePermissao foi para Contracts.Authorization: regra entre módulos agora passa. Ainda falham as duas regras de dependência de Application (48 tipos EF/Npgsql e 69 tipos Infrastructure/API, conjuntos sobrepostos). |
| A07 | Cobertura corrigida; gate reprova a dívida real | 14 assemblies de produção explícitos, inventário contra projetos, seletores de namespaces aninhados não vazios, conferência dos tipos selecionados e violação conhecida detectável. Regras incluem APIs dos módulos e dependências entre módulos. Resultado: 25 aprovados / 2 reprovados / 27, sem allowlist ou relaxamento. |
| A08 | Correção aplicada no fluxo de Clientes | DocumentoValidator compartilhado verifica CPF/CNPJ e dígitos; cadastro/substituição rejeitam inválidos, legado inválido inalterado é preservado. Cadastro: 50 testes aprovados, incluindo regressões documentais. Não estender este resultado automaticamente aos validadores de outros cadastros. |
| A09 | Correção aplicada e matriz local aprovada | Ausência/inatividade/desabilitação do catálogo negam inclusive AcessoTotal; usuário ERP ausente/inativo também nega. 66 casos de catálogo com InMemory; testes HTTP com JWT e handler reais, portas externas isoladas. Fallback exige autenticação; TestController existe somente no projeto de testes. PostgreSQL/IdP reais pendentes. |
| A10 | Atualizações corretivas e verificações locais aprovadas | SSH.NET 2026.0.0, Testcontainers 4.13.0, EF InMemory estável; Vitest 5.0.5, Vite 8.1.6, patches Router/transitivas e override limitado do parser PostCSS. Build Release isolado sem avisos/erros. npm: zero vulnerabilidades; NuGet: 23 projetos, zero problemas/vulnerabilidades reportados. Integrações das novas dependências aguardam Docker. |
| A11 | Automação implementada; execução completa pendente | Workflow GitHub Actions e script Python de gate local com seis checks frontend, build/testes, scanner, npm/NuGet e rejeição de testes ignorados. Gate completo não aprovado nem executado remotamente: dívida arquitetural, Docker indisponível e baseline isolado de catálogos ainda pendentes. Proteção de branch não verificada. |
| A12 | Ajustes locais implementados; homologação operacional pendente | Imagens oficiais com versões explícitas, usuários sem privilégios, frontend porta 8080, headers Nginx; API com HSTS, proxies explicitamente confiáveis e rate limiting (regressão 429 aprovada). Imagens, migrations, triggers, TLS/proxy, backup/restore, privilégios, carga, logs/alertas e ambiente implantado não homologados. |
| A13 | Documentação atualizada nesta entrega | README, guias normativos, ADRs 008/011 e documentos 03/05/06/08/10/11/26/77/78 distinguem alvo arquitetural, estado local, histórico e pendências. Nenhum registro de auditoria, dado de produção ou evidência original de migração foi reescrito. |
| A14 | Correção aplicada e regressões aprovadas | Cancelamento antes/durante token, fetch e leitura do corpo; limpeza de listener/timer e timeout distinguido de cancelamento. Cliente HTTP e quatro serviços exercitados. As seis verificações frontend passaram, sem afrouxar regras. |

### Validações da Consolidação

| Verificação executada | Resultado | Evidência local |
|---|---|---|
| Frontend: format:check, typecheck, lint, lint:design-system, test:run e build | Seis comandos com exit 0; 1.135 testes / 74 arquivos; zero pendências de formatação, tipos, avisos/erros de lint e scanner | scratch/consolidacao-frontend-results.json e logs consolidacao-frontend-* |
| Backend Release em saída isolada | Zero avisos e zero erros | scratch/consolidacao-build-isolado-final.log |
| API (inclui JWT, autorização, erros Testing/Production, CEP, rate limit e barreiras Propostas) | 101 aprovados | consolidacao-api-final.trx / scratch/consolidacao-api-final.log |
| Ferramentas administrativas alteradas (CheckSchemas, CountData, DataMigrator, RenameDb, SqlRunner) | Cinco builds Release aprovados sem avisos/erros; programas não executados | scratch/consolidacao-ferramenta-*.log |
| Cadastro | 50 aprovados | consolidacao-cadastro-final.trx |
| Auditoria unitária | 6 aprovados | consolidacao-auditoria-final.trx |
| Regras/contratos selecionados da suíte Integration | 119 aprovados; incluem catálogo em memória, Propostas, Estipulantes, identidade, clientes/estipulantes/perfis de endpoints e clientes Keycloak simulados | consolidacao-regras-final.trx / scratch/consolidacao-regras-final.log |
| Arquitetura corrigida | 25 aprovados, 2 reprovados por violações existentes em Application | consolidacao-arquitetura-final.trx / scratch/consolidacao-arquitetura-final.log |
| ClienteAuditadoPostgresTests | 3 falhas de inicialização: Docker Engine indisponível; nenhum resultado de transação física aprovado | consolidacao-postgres-final.trx / scratch/consolidacao-postgres-final.log |
| Auditoria npm | Zero vulnerabilidades reportadas | scratch/consolidacao-npm-audit-final.json |
| Auditoria NuGet, incluindo transitivas | 23 projetos; zero problemas/vulnerabilidades reportados | scratch/consolidacao-nuget-audit.json |
| Scanner de segredos atuais | Aprovado; regressão negativa artificial detectada sem imprimir valor | python backend/scripts/verificar-segredos.py |

TRX ficam em backend/tests/<projeto>/TestResults e são artefatos locais, assim como scratch; os resultados resumidos acima ficam preservados neste documento. Testes selecionados da suíte chamada Integration que usam mocks/InMemory não equivalem a integração real. Os 276 testes locais aprovados (101 + 50 + 6 + 119) não incluem os 25 de arquitetura nem os 1.135 do frontend.

Build padrão Release encontrou DLLs bloqueadas pela API já em execução. A verificação final usa `dotnet build backend/WebApolice.slnx --no-restore -c Release -p:OutputPath=bin/Consolidacao/`, preservando o processo existente. Testes backend usam a mesma propriedade, `--no-build --no-restore` e logger TRX. Esta saída isolada não substitui o build padrão em CI. O processo Vite local foi reiniciado com o mesmo comando/porta após bloqueio do binário nativo durante atualização npm; API e outros serviços não foram interrompidos.

As duas regras arquiteturais que reprovam são `Application_ShouldNot_DependOnEntityFrameworkCore` e `Application_ShouldNot_HaveDependencyOnInfrastructureOrApi`. Elas são pendência de implementação, não falha do ambiente. O recorte de Clientes e a regra entre módulos passam. A redução restante deve preservar contratos e transações por fluxo, com regressões PostgreSQL; não foi feita uma refatoração abrangente dos 69 tipos sem esse ambiente. O gate deve continuar reprovando enquanto houver essas violações.

### Condições Ainda Necessárias

1. Responsável: informar o código real do status inicial de Propostas; substituir/revogar credenciais expostas. Não há permissão implícita para reescrever histórico Git.
2. Ambiente: disponibilizar Docker Engine para PostgreSQL 18.4 efêmero, validar migrations novas e rollback com falha/cancelamento de auditoria; preparar baseline isolado dos catálogos sem usar bases reais. Homologar Keycloak, contratos com legado e infraestrutura separadamente.
3. Código: concluir separação de persistência dos casos de uso remanescentes e uniformização da auditoria dos demais escritores indicados em A05. Não estão resolvidos por compilar ou por aprovação dos recortes locais.
4. Entrega: validar imagens/digests e nova porta frontend, triggers Title Case sem reprocessamento histórico, TLS/proxy e restauração; executar gate completo e configurar proteção de branch. Não houve implantação, alteração de dados ou commit/push.

Registro inicial preservado: etapa 1 teve 37 testes aprovados (`scratch/consolidacao-api-etapa1.log`, `etapa1.trx`), ampliados pelas regressões acima. Nenhum destes resultados constitui comprovação de segurança completa ou homologação com o legado.

## Parecer

O WebApolice tem uma base tecnológica moderna e uma escolha arquitetural adequada: monólito modular em .NET 10, PostgreSQL/EF Core, identidade no Keycloak e frontend React/TypeScript organizado por funcionalidades. Não há motivo demonstrado para trocar essa stack ou converter o sistema em microsserviços.

Entretanto, o código **ainda não cumpre integralmente as próprias regras de separação de camadas, autorização, proteção de segredos e rastreabilidade**. A padronização recente do frontend melhorou sua qualidade verificável, mas não encerrou a revisão do backend nem a preparação operacional para produção.

Há cinco grupos de prioridade alta abaixo. Eles precisam ser tratados antes de considerar concluída a segurança da base ou liberar os fluxos afetados. Não foi verificada a exploração desses problemas nem a configuração efetivamente implantada em produção.

## Escopo e Método

- Inventário dos projetos da solução, referências entre módulos, estrutura do frontend, contextos de banco e documentação/ADRs.
- Revisão estática orientada a risco dos pontos de entrada HTTP, autenticação, autorização, casos de uso, persistência, transações, auditoria, configurações e Dockerfiles.
- Execução das seis verificações do frontend, compilação da solução backend e testes selecionados que dispensam banco externo/Docker.
- Verificação de senhas literais em arquivos versionados, sem reproduzir valores neste relatório ou testar essas credenciais contra servidores.
- Comparação com documentação primária da Microsoft, OWASP e o aviso de segurança do mantenedor de SSH.NET.

Esta é uma revisão ampla de engenharia baseada no repositório local, incluindo as alterações locais ainda não commitadas. Não equivale a pentest, certificação de conformidade, revisão manual de cada linha, teste de carga, homologação funcional com o legado ou inspeção da infraestrutura implantada. Nenhum dado de negócio, evidência de migração ou histórico de auditoria foi alterado.

## Estrutura Encontrada

| Área | Estado observado | Avaliação |
|---|---|---|
| Host da API | Composição de DI, JWT, CORS, Problem Details, health checks e controllers | Base adequada; há falhas concretas no tratamento de erros e na configuração de segurança |
| Cadastro | Casos de uso, portas, entidades, controllers e persistência | Funcionalidade desenvolvida, com dependências invertidas incorretamente e validação/auditoria inconsistentes |
| Segurança | Usuários, perfis, catálogo, permissões e provisionamento JIT | Boa fundação; aplicação das barreiras precisa ser uniforme |
| Seguro | Apólices, vínculos, catálogos e parametrizações; Propostas em estágio inicial | Maturidade desigual dentro do próprio módulo |
| Financeiro | Persistência e recorte de convênios de cobrança | Implementação parcial; presença do projeto não significa módulo completo |
| Atendimento, Comissão, Convênio, Documentos, Integração e Sinistros | Predominam modelos, DbContexts e fundações, sem camada Application desenvolvida no inventário | Estruturas preparatórias, não funcionalidades completas |
| SharedKernel / Shared.Infrastructure | Primitivas, contratos e infraestrutura transversal | Separação existe, mas tipos concretos de infraestrutura vazam para Application |
| Auditoria | Contexto próprio, contrato de registro e coordenador de operação auditada | Mecanismo disponível, porém não adotado consistentemente |
| Frontend | Features, APIs, hooks, layouts, componentes compartilhados e design tokens | Organização consistente e verificações locais aprovadas; integração real ainda pendente |
| Documentação | ADRs, guias, regras permanentes e registros de entregas | Boa quantidade de decisões; parte das afirmações não corresponde à implementação atual |

Os módulos concentram suas camadas em um único projeto por módulo. Isso é uma opção possível para um monólito modular, mas exige regras automatizadas eficazes e controle da superfície pública: nomes de pastas, por si só, não impedem referências indevidas. Também existem caminhos duplicados como `WebApolice.Modulos.Seguro/src/WebApolice.Modulos.Seguro`, que dificultam navegação e consistência de namespaces.

## Achados Prioritários

Prioridades: **P1 — alta**, tratar antes da liberação dos fluxos afetados; **P2 — média**, corrigir na consolidação da base; **P3 — melhoria**, correção localizada posterior. Prioridade considera impacto no projeto; não é pontuação CVSS nem comprovação de exploração.

### A01 — P1 — Senhas de Conexão em Arquivos Versionados

**Evidências:** `backend/src/WebApolice.Api/appsettings.Development.json:15`; `backend/src/WebApolice.Shared.Infrastructure/Persistence/InfraestruturaDbContext.cs:65`; `backend/src/WebApolice.Modulos.Integracao/src/WebApolice.Modulos.Integracao/Infrastructure/Persistence/IntegracaoDbContext.cs:23`; `backend/src/SqlRunner/Program.cs:10`; `backend/DataMigrator/Program.cs:10`.

Há strings de conexão com senhas literais em configurações de desenvolvimento, ferramentas e contextos gerados por scaffolding. Os exemplos acima incluem destinos que não são localhost. Também há credenciais artificiais em testes; essas não devem ser confundidas com os casos operacionais. Não foi testada a validade de nenhuma senha.

**Risco:** distribuição de credenciais pelo Git e conexão involuntária de ferramentas/contextos com destinos fixos. Ignorar `.env` não protege senhas incluídas em C# ou JSON.

**Encaminhamento:** externalizar as conexões, retirar os fallbacks embutidos e inventariar quais credenciais ainda são válidas. As credenciais expostas devem ser substituídas no ambiente correspondente. A avaliação do histórico Git e eventual limpeza precisam ser coordenadas; apagar o valor do arquivo atual não remove versões anteriores.

### A02 — P1 — Respostas 500 Expõem Exceção Interna e Stack Trace

**Evidência:** `backend/src/WebApolice.Api/Infrastructure/Errors/GlobalExceptionHandler.cs:114`, com `Detail = exception.ToString()` sem condição de ambiente.

O teste existente `ExceptionHandlingTests.Get_ErroInterno_DeveRetornar500ComProblemDetailsSemStackTrace` falhou nesta revisão: a mensagem esperada era genérica, mas a resposta continha `System.Exception` e a mensagem interna. Isso contradiz `docs/08-erros-respostas-api.md` e o ADR de tratamento de erros.

**Encaminhamento:** devolver mensagem genérica ao cliente, preservar `traceId` e manter a exceção completa somente nos logs protegidos. Critério de aceite: o teste existente passar e a proteção também ser verificada em configuração de produção.

### A03 — P1 — Validação de Audiência JWT Pode Ser Desligada em Qualquer Ambiente

**Evidências:** `backend/src/WebApolice.Api/Program.cs:38`, `:64` e `:70`. O startup exige Authority, mas não Audience; `ValidateAudience` depende apenas de a configuração estar preenchida. O arquivo de desenvolvimento deixa Audience vazia.

**Risco:** uma configuração incompleta pode fazer a API aceitar tokens do mesmo emissor destinados a outro cliente. A assinatura e o emissor continuam sendo validados; o problema é a ausência de uma das verificações necessárias. Não foi inspecionada a configuração real de produção.

**Encaminhamento:** exigir audiência válida e metadata HTTPS para ambientes de entrega, falhar no startup quando a configuração estiver incompleta e testar token de audiência incorreta sem sobrescrever a regra que se pretende verificar. Qualquer exceção local precisa ser explícita e restrita ao ambiente autorizado.

### A04 — P1 — Propostas Tem Contratos e Barreiras Ainda Incompletos

**Evidências:** caminhos sob `backend/src/WebApolice.Modulos.Seguro/src/WebApolice.Modulos.Seguro/`:

- `Api/Controllers/PropostasController.cs:14`: apenas `[Authorize]`, sem permissão granular por operação. Os handlers revisados também não verificam permissões do usuário nem seu estado ativo no ERP.
- O controller exige `IMediator`, mas não foi localizado registro de MediatR/handlers na composição do host ou em `SeguroModuleExtensions`. A ativação e o fluxo HTTP precisam ser comprovados antes da liberação; não basta o projeto compilar.
- `Application/UseCases/Propostas/CriarProposta/CriarPropostaHandler.cs:62`: ausência de vínculo convertida em ID `0`; em `:63`, `PessoaId` recebe `ClienteId`, embora sejam identidades de entidades distintas; em `:75`, o status é fixado em `1`.
- `Application/UseCases/Propostas/AdicionarItem/AdicionarPropostaItemHandler.cs:65`: retorna GUID novo que não identifica o item persistido.
- Listagem aplica paginação recebida diretamente; comandos aceitam IDs internos e valores financeiros sem um conjunto completo de invariantes demonstrado. Não foi localizado registro de auditoria nas operações revisadas.

Existem validações úteis de associação entre apólice/vida e produto/plano, mas elas não substituem autorização do usuário ou resolução correta das identidades. O cadastro incompleto de DI não deve ser considerado uma proteção de segurança.

**Encaminhamento:** manter esse fluxo fora da condição de funcionalidade homologada; fechar permissões, composição, identidade, catálogos/status, valores financeiros, auditoria e retorno de IDs reais em uma entrega própria, com testes de negação e integridade. Isso não exige implementar Propostas agora para concluir as correções gerais de segurança.

### A05 — P1 — Alteração de Cliente Confirma Dados Antes da Auditoria

**Evidência:** `backend/src/WebApolice.Modulos.Cadastro/Application/UseCases/AlterarCliente/AlterarClienteHandler.cs:191`: commit do negócio; `:193`: registro de auditoria posterior. O catch ainda tenta rollback após esse commit.

**Risco:** falha da auditoria pode deixar dados já alterados sem a evidência correspondente e devolver erro ao usuário, que pode repetir a operação. O rollback posterior não desfaz um commit concluído.

**Encaminhamento:** coordenar negócio e auditoria na mesma transação física, usando o mecanismo existente em `WebApolice.Auditoria/Infrastructure/OperacaoAuditada.cs` por uma fronteira coerente com as camadas. Testar falha da auditoria e comprovar ausência de commit parcial. Revisar os demais casos de escrita pelo mesmo critério.

### A06 — P2 — Application Depende de Infraestrutura Própria e de Outros Módulos

**Evidências:** `AlterarClienteHandler.cs:25` injeta `SegurancaDbContext` de outro módulo e consulta suas tabelas diretamente. `Cadastro/Application/Ports/IClienteRepository.cs:5` importa modelos de Infrastructure e os expõe em seu contrato.

No levantamento estático, **48 arquivos em Application importam EF Core e 65 importam namespaces de Infrastructure**; os conjuntos se sobrepõem. São indicadores de acoplamento, não 113 defeitos independentes. A situação contraria `docs/03-arquitetura-backend.md` e o ADR de persistência.

**Encaminhamento:** colocar contratos e modelos adequados ao lado que define a necessidade, manter implementações de persistência em Infrastructure e criar fronteiras públicas pequenas para consultas entre módulos. Aplicar progressivamente por fluxo, preservando comportamento. Caso a equipe prefira permitir EF Core em determinados casos de uso, isso exige decisão arquitetural explícita e documentação consistente; não se deve afirmar Clean Architecture estrita enquanto o código aplica outra regra.

### A07 — P2 — Testes de Arquitetura Não Demonstram Cobertura dos Módulos

**Evidências:** `backend/tests/WebApolice.Architecture.Tests/WebApolice.Architecture.Tests.csproj` referencia apenas SharedKernel e Auditoria; `ArchitectureTests.cs:21` e demais regras usam `Types.InCurrentDomain()`, sem carregar explicitamente todos os assemblies de negócio nem exigir seleção não vazia.

Os 20 testes passaram, embora existam dependências proibidas claras no código de Application. Portanto, o resultado atual não comprova a separação declarada. Os seletores com `WebApolice.Modulos.*` também precisam ter sua abrangência demonstrada, em vez de presumir que selecionam todos os namespaces desejados.

**Encaminhamento:** carregar explicitamente os módulos, testar a quantidade/tipos selecionados, verificar dependências entre camadas e módulos e comprovar que uma violação conhecida faz a suíte falhar. Não é necessário transformar cada pasta em projeto separado para começar a obter essa proteção.

### A08 — P2 — Validação de CPF/CNPJ no Backend É Apenas de Comprimento

**Evidências:** `Cadastro/Application/UseCases/CadastrarCliente/CadastrarClienteHandler.cs:197` e `AlterarCliente/AlterarClienteHandler.cs:221`: após manter apenas dígitos, aceitam 11 ou 14 posições conforme o tipo. O frontend possui validação mais forte, que não protege chamadas diretas à API.

**Encaminhamento:** centralizar a regra de documento no backend e testar dígitos verificadores, sequências inválidas e comportamento de atualização. Separar a política de gravações novas da preservação de evidências e documentos inválidos vindos do legado; não alterar esses históricos automaticamente.

### A09 — P2 — Negação por Padrão Incompleta no Catálogo de Permissões

**Evidências:** `Seguranca/Infrastructure/Authorization/PermissaoAuthorizationHandler.cs:52` consulta Habilitado, mas não Ativo da permissão/recurso/módulo; `:55` permite seguir quando não existe entrada no catálogo; `:104` aprova AcessoTotal. `UsuarioRepository` filtra os estados ativos ao calcular permissões comuns, mas o caminho AcessoTotal não depende dessa lista.

**Risco:** permissões inexistentes ou entradas inativas podem ter tratamento diferente para AcessoTotal, em desacordo com a disponibilidade apresentada no catálogo/frontend. AcessoTotal é uma regra legítima do produto; o ponto a definir é seu limite diante de catálogo inválido ou indisponível.

**Encaminhamento:** explicitar e testar a matriz Ativo/Habilitado/inexistente para usuários comuns e AcessoTotal; negar catálogo inexistente e estados que representem desativação do recurso segundo a regra aprovada. Usar também uma proteção padrão contra endpoints de negócio esquecidos sem autorização. `Program.cs:460` ainda contém um `TestController` anônimo fora da condição de ambiente Testing; removê-lo da superfície de entrega ou restringi-lo ao ambiente de teste.

### A10 — P2 — Backend Compila com Avisos e Dependência de Testes Vulnerável

O build da solução produziu **111 avisos e zero erros**. Entre eles há nulabilidade, APIs obsoletas, imports duplicados, avisos do scaffolding e SQL interpolado em testes. Esses avisos, isoladamente, não comprovam SQL injection em endpoints de produção.

Dois avisos NU1903 identificam **SSH.NET 2024.1.0 e 2025.1.0** nos grafos das duas suítes de integração. O aviso do mantenedor GHSA-q939-rpr3-3284 descreve escrita arbitrária por nomes de arquivo em download SCP recursivo, corrigida em **2026.0.0**. A presença foi confirmada nos `project.assets.json` de testes; não foi encontrada nesses grafos uma dependência SSH.NET do host de produção. A explorabilidade depende de usar a operação afetada contra um servidor não confiável.

**Encaminhamento:** atualizar a cadeia de dependências de testes compatível com a correção e repetir as integrações. Classificar e reduzir os avisos sem desativar validadores indiscriminadamente. Reexecutar auditorias completas de npm/NuGet quando houver acesso aos serviços de advisories; elas não foram concluídas nesta sessão.

### A11 — P2 — Padrões Ainda Dependem de Execução Manual

Não foram encontrados arquivos de pipeline versionados em `.github`, GitLab CI, Azure Pipelines ou Jenkins. Isso não permite concluir que inexiste automação externa, mas o repositório não demonstra um gate reproduzível. `docs/06-testes-backend.md` afirma proteção em CI que não foi comprovada aqui.

**Encaminhamento:** automatizar as seis verificações frontend, build/testes backend, testes arquiteturais corrigidos e auditoria de dependências/segredos. Executar integração PostgreSQL em ambiente isolado. A proteção de branch e as exigências de merge precisam ser conferidas na plataforma de hospedagem do Git, que não foi inspecionada.

### A12 — P2 — Implantação e Operação Ainda Precisam de Evidência de Homologação

**Evidências:** Dockerfiles usam tags flutuantes como `nginx:alpine`, `node:24-alpine` e `.NET:10.0`; fixtures usam `postgres:16`, enquanto a base documentada do projeto é PostgreSQL 18.4. Isso reduz a reprodutibilidade e contraria a regra local de versões explícitas. O Dockerfile backend não declara usuário de execução sem privilégios.

O Compose local usa Keycloak `start-dev` e portas de banco/IdP restritas a localhost, coerentes com desenvolvimento. Ele não deve ser tratado como implantação de produção. Não foi encontrada no pipeline HTTP da API configuração de rate limiting, forwarded headers ou HSTS; o Nginx apresentado também não demonstra uma política de cabeçalhos de segurança. Esses controles podem existir numa borda externa, que não foi examinada.

**Encaminhamento:** definir onde cada controle será aplicado, fixar versões/digests com política de atualização e testar compatibilidade com o PostgreSQL de destino. Comprovar TLS/proxy confiável, limites de requisição e paginação, privilégios mínimos, logs protegidos, alertas e restauração de backup. Confirmar a reaplicação dos triggers Title Case após novas tabelas/colunas. Ausência no repositório não prova ausência no ambiente real.

### A13 — P2 — Documentação Mistura Regra, Planejamento e Estado Concluído

**Exemplos:**

- `docs/03-arquitetura-backend.md` e o ADR de persistência proíbem referências existentes em Application.
- `docs/08-erros-respostas-api.md` afirma que detalhes de erro não vazam; o teste atual demonstra o contrário.
- `docs/seguranca/03-fluxo-autenticacao-autorizacao.md:54` e o plano de segurança ainda citam `/api/auth/permissoes` e `useAuth().hasPermission()`, enquanto o fluxo atual usa `/api/seguranca/me` e o mecanismo de autorização correspondente.
- O README mantém contagens e afirmações antigas de homologação; `docs/29-estabilizacao-frontend.md` já distingue corretamente checks locais de homologação integrada pendente.
- Existem ferramentas fora da solução oficial, como ScratchTest com referência ao antigo projeto Clientes. Elas não causaram falha no build da solução, mas precisam ter finalidade e estado identificados.

**Encaminhamento:** manter um índice de estado atual, com distinção entre implementado, testado localmente, homologado e planejado. Atualizar guias normativos quando a implementação for corrigida, preservar registros históricos com data/escopo e vincular os achados aos critérios de aceite. O guia de padrões frontend continua válido; não representa certificação automática do backend.

### A14 — P3 — Cancelamento HTTP Pode Ser Perdido Antes do Fetch

**Evidência:** `apps/web/src/services/http/httpClient.ts:77` apenas adiciona listener ao signal externo, sem verificar se ele já estava abortado nem remover o listener no final. A obtenção assíncrona de token ocorre antes dessa composição em `:110`.

**Risco:** uma consulta cancelada durante a obtenção do token pode iniciar a chamada de rede mesmo assim. Isso não implica, por si só, atualização incorreta de tela, pois hooks também podem descartar resultados antigos.

**Encaminhamento:** propagar estado já cancelado, liberar listeners e temporizadores e testar cancelamento antes da chamada e durante a obtenção do token. Preservar a distinção entre cancelamento e erro real nos consumidores.

## Pontos que Devem Ser Preservados

- Monólito modular com schemas/contextos de banco separados, em vez de distribuir serviços prematuramente.
- Autenticação JWT com emissor, assinatura e validade; PKCE S256 no frontend e tokens administrados pelo adaptador de identidade.
- Permissões granulares existentes em Clientes, Apólices e demais cadastros; autorização precisa continuar no servidor.
- CORS com origens configuradas e OpenAPI/Scalar restritos a Development.
- Contratos de auditoria e coordenador transacional já existentes; corrigi-los/adotá-los antes de criar outro mecanismo.
- Padronização centralizada de texto em C#, cliente HTTP e SQL; os DbContexts de negócio inventariados herdam da base prevista. Auditoria permanece separada.
- Frontend por features, cliente HTTP central, hooks de consulta, componentes reutilizáveis, design tokens, testes e guia de novas implementações.
- ADRs e documentação de migração que preservam decisões, evidências e limitações da transição do legado.

## Validações Executadas

| Verificação | Resultado desta revisão |
|---|---|
| Frontend — TypeScript | Aprovado |
| Frontend — ESLint com zero warnings permitido | Aprovado |
| Frontend — scanner do design system | Aprovado |
| Frontend — Prettier global | Aprovado |
| Frontend — Vitest | 1.125 aprovados, zero falhas, 74 arquivos |
| Frontend — build | Aprovado; repetido isoladamente com exit code nativo 0 após aviso de tempo de plugin na execução paralela |
| Backend — build da solução com `--no-restore` | Aprovado: zero erros, 111 avisos |
| Backend — Architecture.Tests | 20 aprovados, com limitação de abrangência descrita em A07 |
| Backend — Auditoria.UnitTests | 6 aprovados |
| Backend — Modulos.Cadastro.Tests | 33 aprovados; testes de modelo/normalização sem conexão real de banco |
| API — transformação de roles, normalização e contexto de auditoria | 19 aprovados |
| API — ExceptionHandlingTests | 7 aprovados e 1 falha real: exposição de exceção interna |
| Integrações dependentes de containers | Não executadas: Docker Engine indisponível |
| Auditoria completa npm | Inconclusiva: endpoint de advisories inacessível nesta sessão |
| Auditoria completa NuGet | Inconclusiva: acesso ao NuGet.Config do usuário bloqueado e tentativa alternativa sem concluir restauração; avisos NU1903 do build registrados separadamente |
| Homologação navegador/API/PostgreSQL/Keycloak reais | Não executada nesta análise |

A primeira tentativa de testes HTTP foi impedida pelo logger Windows EventLog sem acesso. A repetição desativou apenas esse provider de log por variável de ambiente do processo de teste, sem alterar código ou políticas de segurança; então surgiu a falha real descrita em A02. Não se deve registrar a primeira tentativa como oito defeitos funcionais do produto.

Os registros locais estão em `scratch/auditoria76-*`, incluindo logs, relatório JSON frontend e TRX. Não são documentos de produção e permanecem fora do versionamento. Testes aprovados não demonstram cobertura integral, segurança completa ou paridade com o legado.

## Ordem Recomendada de Continuidade

1. **Conter exposição de segredos e detalhes internos:** A01 e A02. Aceite: ausência de credenciais operacionais no código/configuração versionados, configuração externa validada, plano de substituição das credenciais e teste existente de erros aprovado.
2. **Consolidar fronteiras de segurança e rastreabilidade:** A03, A05 e A09. Aceite: configuração JWT inválida impede startup de entrega; catálogo/usuários desativados não obtêm acesso indevido; falha de auditoria não confirma negócio. Catalogar Propostas como incompleto até a entrega específica de A04.
3. **Tornar as regras arquiteturais verificáveis:** A07 antes de ampliar refatorações de A06; iniciar por um fluxo de Clientes. Aceite: seleção não vazia de todos os módulos e teste que detecta dependência proibida real, com contratos corrigidos no recorte escolhido.
4. **Fechar qualidade e automação:** A08, A10, A11 e A14. Aceite: regras backend consistentes, dependências corrigidas, checks reproduzíveis e gate de CI demonstrado.
5. **Homologar operação e evolução funcional:** A12, A13 e entrega própria de Propostas. Aceite: integração em infraestrutura isolada compatível com o destino, restauração comprovada, matriz de permissões e comparação funcional com o legado por módulo. Somente então ampliar novas telas apoiadas nessa base.

Cada etapa deve atualizar este registro e os guias afetados, informando exatamente o que foi corrigido, testado e o que permanece pendente. Esta análise não aplicou correções de produção, não atualizou dependências e não fez commit/push.

## Referências Primárias Consultadas

- [Microsoft — princípios arquiteturais](https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/architectural-principles): separação de responsabilidades e inversão de dependências.
- [Microsoft — configuração JWT Bearer](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/configure-jwt-bearer-authentication?view=aspnetcore-10.0): validação de tokens, incluindo emissor e audiência.
- [Microsoft — tratamento de erros](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling?view=aspnetcore-10.0): não divulgar detalhes internos de exceções em produção.
- [OWASP — autorização](https://cheatsheetseries.owasp.org/cheatsheets/Authorization_Cheat_Sheet.html): negar por padrão e verificar autorização em cada requisição.
- [OWASP — gestão de segredos](https://cheatsheetseries.owasp.org/cheatsheets/Secrets_Management_Cheat_Sheet.html): controlar o ciclo de vida de segredos e evitar credenciais embutidas.
- [SSH.NET — GHSA-q939-rpr3-3284](https://github.com/sshnet/SSH.NET/security/advisories/GHSA-q939-rpr3-3284): versões afetadas, cenário de download SCP e versão corrigida.
