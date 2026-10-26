# Operação e Validação da Consolidação — 23/10/2026

Complementa a revisão 76 e o recorte de Propostas do documento 77. **Este parágrafo registra a execução de 23/10; a continuidade atual está no documento 79.** O gate completo não foi aprovado: há duas regras arquiteturais reprovadas e integrações indisponíveis. Os comandos foram executados individualmente conforme as evidências da revisão; o script agregado não foi executado integralmente nesta sessão. Não constitui autorização de implantação ou homologação com o legado.

## Configuração Externa e Credenciais

Não há fallback operacional de conexão nos contextos gerados. Injetar opções pelo host/DI; factories EF e ferramentas recusam configuração ausente. O arquivo appsettings.Development.json não contém conexão. O host exige ConnectionStrings__PostgreSql e configuração JWT válida. A API mantém a validação de audiência em todos os ambientes.

| Configuração | Utilização |
|---|---|
| ConnectionStrings__PostgreSql | Host, factories EF e SqlRunner |
| WEBAPOLICE_SOURCE_CONNECTION | CheckSchemas, CountData e origem do DataMigrator |
| WEBAPOLICE_TARGET_CONNECTION | Destino explícito do DataMigrator |
| WEBAPOLICE_ADMIN_CONNECTION | RenameDb; ferramenta administrativa fora da solução, não executada nesta correção |
| Authentication__Authority | URL absoluta do realm, sem credenciais, query ou fragmento |
| Authentication__Audience | Audiência obrigatória; usar o valor emitido para a API |
| Authentication__RequireHttpsMetadata | true nos ambientes de entrega. HTTP é permitido apenas em Testing ou Development com Authority em loopback |
| Propostas__StatusInicialCodigo | Código a confirmar pelo responsável, sem fallback para ID 1 |
| ReverseProxy__KnownProxies__0 | IP explícito do proxy confiável; adicionar índices para outros proxies |
| RateLimiting__PermitLimit | Padrão 120 requisições/minuto por sub autenticado ou IP, sem fila; deve ser positivo |

Fornecer segredos por variáveis ou gerenciador de segredos. Nunca usar os destinos/valores removidos como exemplos. A separação de nomes de connection strings não garante isolamento: conferir identidade do servidor, base, privilégios e origem de cada ferramenta antes de qualquer operação autorizada.

Responsáveis pelo ambiente devem inventariar e substituir as credenciais anteriormente expostas, revogar as antigas, conferir permissões/acessos e atualizar consumidores. Nenhuma credencial removida foi testada. Remover do arquivo atual não remove o histórico; sua avaliação e eventual limpeza exigem autorização específica. Não apagar dados, trilhas de auditoria ou evidências de migração para cumprir essa tarefa.

SqlRunner/DataMigrator/RenameDb são ferramentas administrativas fora da solução oficial, com alterações de schema/dados potencialmente destrutivas. Externalizar suas conexões não autoriza executá-las. ScratchTest e demais experimentos fora da solução não integram os critérios de homologação.

DataMigrator também deixou de desabilitar triggers via session_replication_role, preservando as barreiras de novas gravações. A consulta auxiliar CountData passou a citar identificadores SQL corretamente. Essas ferramentas não foram executadas contra servidores; a compilação da solução não inclui seus projetos administrativos. CheckSchemas, CountData, DataMigrator, RenameDb e SqlRunner foram compilados separadamente com --no-restore em Release: zero avisos/erros, sem executar os programas. Logs scratch/consolidacao-ferramenta-<nome>.log.

## Fronteira HTTP e Containers

- O host aplica HSTS por 30 dias e redirecionamento HTTPS fora de Development/Testing. O TLS termina na borda ou no host; configuração e certificados reais continuam por homologar.
- Forwarded headers são processados somente com lista explícita de proxies conhecidos, um salto. A lista padrão vazia não confia em cabeçalhos recebidos. Não configurar redes irrestritas nem interpretar cabeçalhos enviados diretamente pela internet como evidência de identidade/IP.
- Rate limiting é aplicado após autenticação e antes do provisionamento JIT; liveness/readiness são exceções técnicas. Resposta 429 inclui traceId. Ajustar o limite somente com evidência de carga e sem desabilitar a proteção.
- Containers backend usam usuário APP_UID e frontend usa nginx sem privilégio. **A porta HTTP do container frontend mudou de 80 para 8080**; ajustar o mapeamento na implantação. Nginx usa /tmp para temporários e aplica nosniff, DENY, no-referrer e restrição de câmera/microfone/geolocalização.
- Dockerfiles fixam versões completas de SDK/runtime/Node/Nginx. Fixtures PostgreSQL usam 18.4. Fixação de tag não equivale a digest imutável: homologar imagens, registrar digests e atualizar patches/advisories periodicamente. As imagens não foram construídas nesta execução por indisponibilidade do Docker Engine.
- CORS, CSP com origens reais de API/IdP, TLS/proxy, privilégios de banco, logs protegidos, alertas, retenção e restauração de backup exigem validação do ambiente de destino. Não foram homologados aqui. Compose com Keycloak start-dev continua exclusivo de desenvolvimento.

Após migrations no destino, reaplicar os triggers Title Case conforme documento 26, com normalização histórica desativada nesta consolidação. Validar acentos, exclusão da auditoria e preservação dos campos técnicos. Nenhum script de padronização foi aplicado a bases existentes nesta execução.

## Gate Reproduzível

`python backend/scripts/verificar-qualidade.py` executa scanner, seis checks frontend, auditoria npm, restore/build/testes da solução e auditoria NuGet. Logs ficam em scratch/qualidade. Qualquer falha retorna código diferente de zero; integrações precisam de Docker. O processo remove a conexão externa opcional de catálogos, evitando reutilizar banco do host. O script não faz commit/push, migrations de produção ou implantação.

O gate local e o job frontend fornecem as quatro entradas públicas `VITE_*` obrigatórias com valores fictícios de CI, sem depender do `.env` do desenvolvedor. O build desse gate não se destina à implantação. A falha remota que motivou essa correção e suas evidências estão no [documento 80](80-correcao-configuracao-ci-frontend.md). A configuração real continua obrigatória para o ambiente de destino.

O workflow `.github/workflows/qualidade.yml` registra os mesmos gates para GitHub Actions, com runner Linux e Testcontainers PostgreSQL isolados. Sua execução remota, proteção de branch e exigências de merge não foram verificadas. Na execução de 23/10, os testes de catálogos dependiam de baseline externo. Em 26/10 passaram a usar fixture própria e as dependências arquiteturais detectadas foram corrigidas; o gate completo passou, conforme documento 79. Testes ignorados e qualquer regressão arquitetural continuam impedindo aprovação.

O gate completo reprova quando há testes ignorados. O antigo teste de consulta de Estipulantes deixou de acessar um banco fixo em localhost: agora cria registro e schema de teste em container efêmero. A fixture de Cadastro usa o modelo atual numa base vazia; isso não substitui validação do schema implantado. O teste de falha de endereço injeta um erro controlado de banco após gravações anteriores, sem presumir que o antigo SQL parcial da fixture demonstrava FKs do produto.

O scanner local inspeciona arquivos atuais versionados e não ignorados, sem imprimir valores. Possui regras limitadas e exceções estreitas para fixtures artificiais e placeholders; não é análise de histórico, prova de ausência universal de segredos ou substituto da rotação. Sua regressão com credencial artificial foi executada.

## Dependências Corretivas

As duas suítes Testcontainers passaram a usar 4.13.0, com SSH.NET 2026.0.0 explicitado para retirar as transitivas vulneráveis; o pacote já existia nos grafos, não foi introduzida outra biblioteca de SSH. EF Core InMemory saiu da versão preview para 10.0.11. Referências redundantes a ASP.NET HTTP/configuração foram removidas dos módulos que já usam Microsoft.AspNetCore.App.

No frontend, Vitest 5.0.5 substitui 3.x para corrigir a cadeia de advisories do runner/mocker/pool. Vite recebe correção dentro de 8.x, React Router dentro de 7.x e transitivas de source maps/expansão recebem patches. Tailwind permanece 3.4; override de postcss-selector-parser 7.1.6 corrige sua transitiva sem migrar o design system para Tailwind 4 ou acrescentar outro plugin. Mocks de construtores e funções foram adequados aos contratos tipados do Vitest 5, mantendo asserções de comportamento e sem any/supressões. A compatibilidade é aferida pelas seis verificações e continua sujeita à homologação visual integrada.

Fontes primárias: [advisory SSH.NET](https://github.com/sshnet/SSH.NET/security/advisories/GHSA-q939-rpr3-3284), [JWT Bearer Microsoft](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/configure-jwt-bearer-authentication?view=aspnetcore-10.0), [imagens oficiais Node](https://github.com/docker-library/official-images/blob/master/library/node), [imagens oficiais Nginx](https://github.com/docker-library/official-images/blob/master/library/nginx). Resultados locais e limitações estão na revisão 76.


## Continuidade — 26/10/2026

Docker Desktop foi iniciado; integrações PostgreSQL 18.4 passaram a executar em containers efêmeros. Catálogos não dependem mais de baseline externo: a fixture cria Cadastro pelo modelo atual e aplica migrations, inclusive a transição do compartilhamento de coberturas com dados artificiais de regressão. O script test-catalogos-isolado.ps1 não lê .env, copia banco ou remove bases existentes. O gate admite --output-path bin/Consolidacao/ para binários isolados e reprova também resultados TRX individuais ignorados, mesmo quando o contador agregado está zerado. Zod recebeu patch corretivo para 4.6.6 após novo apontamento da auditoria. O documento 79 registra os resultados e limites desta continuidade; os números anteriores são históricos.
