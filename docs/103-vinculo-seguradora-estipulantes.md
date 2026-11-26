# Seguradora e Retirada de Grupo das Telas de Estipulantes

Data: 24/11/2026. Continuidade da correção de pendências antes do manual, após a consulta de Ramos do documento 102.

## Regras Funcionais

Por decisão do responsável, **Grupo foi retirado das telas de cadastro, edição e detalhes de Estipulantes**. Não há exclusão de tabela, coluna ou vínculos legados. Na edição, o cliente conserva o `grupoId` recebido pela API e o devolve sem alteração, usando o contrato numérico já existente. O antigo `grupoPublicId` do frontend não existia no backend e foi removido dos contratos locais. Novos cadastros não informam Grupo.

**Seguradora permanece opcional** e utiliza o cadastro existente. O operador pode buscar pelo nome, CNPJ, código ou SUSEP e selecionar uma das primeiras 20 correspondências ativas; pode refinar a busca para localizar outros registros. A busca reutiliza EntitySearchField, os componentes do design system e `seguradorasApi.listar`, com carregamento, erro/nova tentativa, ausência de opções e cancelamento. Não há criação implícita de seguradoras.

O cadastro passa a enviar o UUID escolhido, anteriormente descartado no payload. A edição conserva a seleção existente e permite substituí-la ou removê-la antes de salvar. A remoção só é persistida no envio do formulário. A consulta é bloqueada durante a gravação.

A busca requer `seguradoras.visualizar`, já utilizada pela API de Seguradoras. Sem essa permissão, o campo fica desabilitado, explica o motivo e conserva o vínculo existente ao salvar outros dados. Não foram criadas ou concedidas permissões. As rotas e endpoints de Estipulantes mantêm suas autorizações próprias.

O detalhe do Estipulante passa a mostrar o **nome da Seguradora**. Vínculos atuais com seguradoras inativas continuam sendo apresentados; a busca de novas escolhas lista somente ativas. Se o vínculo existir e o nome não estiver disponível, a tela informa **Nome indisponível**, sem mostrar UUID como nome.

## Contratos e Dados

`EstipulanteDetalheResult` acrescenta `seguradoraNome` à resposta existente. A consulta obtém UUID e nome da Pessoa associada à Seguradora, sem alteração de dados ou nova tabela. As requisições de criação/edição mantêm `seguradoraPublicId`. Não há migration nesta entrega.

`obterEstipulante` e `obterConfiguracao` aceitam AbortSignal opcional. A carga da edição usa `useConsulta`, descarta respostas antigas ao trocar de UUID e oferece nova tentativa em caso de falha. Assim, o nome da Seguradora e o identificador legado de Grupo pertencem sempre ao Estipulante atual.

`SeguradoraNome` é humano e foi classificado explicitamente em C#, TypeScript e SQL. O valor exibido vem da coluna humana `core.pessoa.nome`, já coberta pela padronização compartilhada. UUIDs e `grupoId` são técnicos. Não há nova coluna humana, necessidade de reaplicar triggers, normalização retroativa ou alteração de evidências/auditoria. Testes verificam caixa alta, caixa mista, acentos, null, idempotência e a preservação dos campos técnicos.

## Arquivos e Validação

Frontend: formulário, cadastro, edição, detalhes, contratos e cliente de API de Estipulantes; classificação compartilhada de Title Case; testes funcionais e de contrato HTTP.

Backend: DTO de detalhe e consulta de Estipulantes; classificação compartilhada de Title Case e script SQL; testes de consulta PostgreSQL e de classificação.

Validações finais concluídas:

| Verificação | Resultado |
| --- | --- |
| `npm.cmd run format:check` | Aprovado, nenhum arquivo pendente |
| `npm.cmd run typecheck` | Aprovado, zero erros |
| `npm.cmd run lint` | Aprovado, zero erros e avisos |
| `npm.cmd run lint:design-system` | Aprovado, zero ocorrências |
| `npm.cmd run test:run -- --maxWorkers=2` | 1.462 testes aprovados em 104 arquivos, sem excluir testes |
| `npm.cmd run build` | Aprovado; permanecem os avisos preexistentes de chunk acima de 500 kB e tempo de plugins, sem alterar limites |
| `dotnet build WebApolice.slnx --no-restore -c Release` | Aprovado, zero erros e avisos |
| `dotnet test WebApolice.slnx --no-build --no-restore -c Release` | 829 testes aprovados nas seis suítes, zero falhas e ignorados |
| `python backend/scripts/verificar-segredos.py` | Nenhuma ocorrência nas regras locais |

A homologação visual autenticada permanece separada das validações automatizadas. Não houve navegação autenticada nem gravação manual de registros reais para testar o fluxo.

As primeiras execuções identificaram a necessidade de atualizar os mocks de autorização dos testes antigos de cadastro/edição, já que essas páginas agora consultam a permissão da busca de Seguradoras. Também foram corrigidas duas verificações assíncronas: o foco em Ramos e a liberação do bloqueio no reagendamento de Propostas passaram a aguardar seus efeitos antes de conferir o resultado. As asserções e os limites de tempo foram mantidos; não houve mudança no comportamento de Propostas.

## Execução Local

A API foi publicada em `backend/src/WebApolice.Api/bin/ExecucaoEstipulantes-20261124`. A primeira tentativa dentro da sandbox falhou pela restrição de escrita no log de eventos padrão do .NET no Windows; a execução fora da sandbox foi aprovada e resolveu a restrição, sem desativar logs. A instância temporária na porta 5008 foi validada com processamento de agendamentos desativado e encerrada após a troca.

A API atualizada está na porta **5007**, PID **68588**, em Development com destino confirmado **webapolice_teste** e a configuração de agendamentos original do arquivo local. Os hashes de `appsettings.json` e `appsettings.Development.json` permaneceram iguais aos da execução anterior. O módulo de Cadastro publicado corresponde ao assembly Release testado. Health/ready retornou HTTP 200 Healthy; a consulta de Estipulante por UUID fictício sem token retornou 401.

O Vite existente na porta **5173** respondeu HTTP 200 ao formulário atualizado, contendo a busca de Seguradoras e sem o campo Grupo desabilitado. Logs e lançador local estão em `scratch/estipulantes-20261124/`, ignorados pelo Git. Não houve migration, alteração de configurações de segurança, acesso a produção, commit ou push.
