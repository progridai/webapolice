# Preparação da Integração em Main — 26/10/2026

Continuação da revisão 76. Objetivo: incorporar os três commits de usuários/Keycloak de origin/main, resolver as dependências indevidas reais e validar a composição antes de finalizar a fix. Nenhum dado de produção, evidência histórica ou credencial será utilizado nos testes.

## Fronteiras de Persistência

A norma dos ADRs 008/011 permanece: regras e orquestração em Application; entidades sem EF em Domain; queries e adaptadores EF em Infrastructure. Contratos de persistência expõem operações necessárias aos casos de uso, sem DbContext, DbSet, IQueryable ou IDbContextTransaction. Consultas retornam DTOs ou entidades de domínio; CancellationToken é propagado. Transações mantêm commit/rollback/dispose no adaptador existente, sem trocar a conexão física nem colocar auditoria após commit.

Tipos POCO usados como entidades dos módulos podem mudar de namespace/pasta para Domain, preservando propriedades, relações, schemas, colunas, índices, IDs e classificações Title Case. Essa alteração de organização não autoriza migration de schema. Snapshots atuais devem refletir os nomes CLR; designers/migrations históricos permanecem preservados.

Consultas entre módulos devem passar por contratos públicos pequenos; não copiar SQL de outro schema para esconder a dependência. Não substituir regras por wrappers genéricos, excluir tipos dos testes arquiteturais ou mover regras de negócio para Infrastructure apenas para aprovar a suíte.

## Integração de Usuários

Preservar a validação de username introduzida em main, com trim e caracteres técnicos permitidos, e os ajustes de tratamento de falhas Keycloak. Preservar os contratos tipados de criação/edição e os componentes do design system da fix. Mensagens de provedores externos precisam continuar sem revelar segredos ou detalhes internos.

## Validação

O Docker Desktop foi iniciado e o daemon tornou-se acessível. As integrações agora executam em PostgreSQL 18.4 efêmero. A primeira execução revelou fixtures incompletas, tracker reaproveitado após falha, expectativas anteriores à política Title Case e histórico de vínculo de Ramo desabilitado por workaround antigo. Essas causas foram corrigidas sem retirar validações ou assertions de integridade.

As 36 fontes POCO movidas foram comparadas com HEAD: nenhuma diferença além dos namespaces/imports correspondentes. Nenhum designer histórico foi alterado. O teste da migration de RG identifica a entidade histórica por schema/tabela, preservando as assertions de três colunas opcionais sem preenchimento de dados.

Os testes de catálogos passaram a criar seu próprio container e modelo de Cadastro, aplicar as migrations e exercer a atualização do plano/cobertura anterior ao compartilhamento, preservando os valores da fixture. Não dependem de .env, cópia de schema de base existente ou conexão externa. Foram removidos os atributos que ignoravam esses testes sem configuração externa. O script test-catalogos-isolado.ps1 tornou-se apenas um lançador dessa suíte efêmera.

As regressões PostgreSQL de Propostas verificam duas gravações nativas com legado_id nulo, identidades distintas de Cliente/Pessoa, UUIDs persistidos dos itens e reversão por falha da auditoria, usando consulta em outra conexão. O status é exclusivamente artificial da fixture, com código explícito e ID a partir de 7: não define o status inicial do produto.

O gate Python aceita --output-path bin/Consolidacao/ para preservar a API já aberta com DLLs Release bloqueadas. Essa opção muda somente o local dos binários. O comando continua executando scanner, seis checks frontend, auditorias, restore/build e testes completos. Python/CI agora conferem também UnitTestResult com outcome NotExecuted, pois o primeiro TRX continha testes ignorados com contador notExecuted igual a zero.

Gate completo aprovado com saída 0: `python backend/scripts/verificar-qualidade.py --output-path bin/Consolidacao/`. Todos os 12 passos terminaram com saída 0. Nenhum comando aplicou migrations a bases existentes.

| Verificação final | Resultado |
|---|---|
| format:check, typecheck, lint, lint:design-system, test:run, build frontend | Seis aprovadas; 1.139 testes em 74 arquivos, zero avisos/erros de lint e scanner |
| Restore e build backend Release isolado | Aprovados; zero avisos/erros no build |
| API | 101 aprovados |
| Arquitetura (14 assemblies) | 28 aprovados, incluindo nova proteção contra IQueryable nas portas |
| Cadastro | 50 aprovados |
| Auditoria unitária / PostgreSQL | 6 / 2 aprovados |
| Suíte Integration (regras e PostgreSQL) | 206 aprovados |
| Total backend | 393 aprovados, zero falhas e zero ignorados |
| Scanner de segredos e auditoria npm | Aprovados; npm sem vulnerabilidades reportadas |
| Auditoria NuGet | 23 projetos, zero problemas/vulnerabilidades reportados |

Logs: scratch/qualidade/resultados.json e arquivos por passo; TRX em backend/tests/<projeto>/TestResults, resumo scratch/pre-finish-backend-results.json. A primeira execução reprovada do gate foi preservada em scratch/pre-finish-gate-inicial. São artefatos locais ignorados pelo Git; os números e limites ficam preservados aqui. Os testes em memória/mocks continuam distintos das integrações reais e não homologam o IdP ou o legado.

Verificação remota posterior: o CI de 8b20807 aprovou backend e segredos, mas reprovou os testes frontend por ausência de configuração `VITE_*` no runner. O gate local acima utilizava o `.env` ignorado; não comprovava a configuração remota. A correção e a validação sem esse arquivo são registradas no [documento 80](80-correcao-configuracao-ci-frontend.md), preservando o resultado histórico desta preparação.

A preparação partiu da fix em 891beb7 e incorporou origin/main em fc72083 (também 14fb8c4 e 1edbc66), com conflitos conciliados nos dois arquivos frontend de usuários. Foram preservadas a validação técnica de username e a tradução de erros Keycloak, sem retornar mensagens arbitrárias do provedor. Não houve reescrita de histórico Git.


## Correção Adicional de Dependência

A auditoria de 26/10/2026 passou a reportar Zod anterior a 4.6.6 com severidade alta. Atualização corretiva da mesma biblioteca para 4.6.6, sem nova dependência nem mudança de versão principal. Fonte: [advisory GHSA-mrhx-c7xc-m3w5](https://github.com/advisories/GHSA-mrhx-c7xc-m3w5). O npm reportou zero vulnerabilidades após a instalação; as seis verificações são repetidas no gate final.

## Limites de Liberação

Finalizar a branch significa integrar o código validado em main; não implanta a aplicação nem homologa os dados reais. Permanecem: código real do status inicial de Propostas, regras financeiras/operacionais ainda não definidas, rotação/revogação de credenciais expostas e validação do ambiente de destino/legado/Keycloak real. A uniformização de auditoria dos demais CRUD de Cadastro indicada em A05 continua pendência; aprovação das regras de dependência não comprova auditoria completa de todos os escritores. Imagens de aplicação, TLS/proxy, triggers no destino, backup/restauração, carga, alertas e execução remota do CI exigem evidências próprias.
