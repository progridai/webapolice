# Formação de Valores Financeiros das Propostas

Estado atual conferido em 24/11/2026: este documento registra a etapa de simulação. A confirmação com plano financeiro, a geração de parcelas, os lotes e as baixas foram entregues posteriormente no [documento 98](98-lotes-mensais-baixa-parcelas.md), com aplicação local registrada no [documento 99](99-aplicacao-financeiro-execucao-local.md). As limitações abaixo se referem ao recorte original. Consulte o [quadro atual de alcance financeiro](104-alcance-atual-rotinas-financeiras.md) antes de preparar o manual.

Data: 18/11/2026. Status: implementação local concluída e validada. Migrations aplicadas posteriormente em testes, conforme documento 96.

Continuidade dos documentos [22](22-plano-coberturas-modulo-apolice.md), [83](83-fluxo-operacional-propostas.md) e [94](94-revisao-explicita-condicoes-propostas.md). As alterações locais anteriores, inclusive migrations preparadas e trabalho independente de navegação, permanecem preservadas. A implantação posterior em testes está registrada no documento 96.

## Regras Aprovadas

O responsável escolheu **simular o valor mensal**, sem gerar movimentos ou títulos e sem registrar valores financeiros na proposta.

Composição aprovada:

- Somar os prêmios líquidos registrados nas coberturas ativas do titular, incluindo os ajustes justificados.
- Incluir os prêmios do cônjuge somente quando essa contratação estiver indicada na proposta.
- Acrescentar IOF com a alíquota do Ramo. O líquido mais IOF forma o prêmio total mensal, referência para o valor da parcela em etapa futura. Não extrair imposto por dentro nem presumir alíquota do Estipulante ou informada livremente.

Capital segurado, comissões, competência, vencimentos, retroatividade, gravação de valores financeiros e títulos permanecem fora deste recorte e sujeitos a regras próprias.

## Definições Complementares Aprovadas e Modelagem

1. Registrar escolha explícita de cônjuge na Pré Proposta e reutilizar na simulação, confirmação e novas versões. Acrescentar `seguro.proposta.inclui_conjuge`, booleano técnico anulável, sem valor padrão. `null` preserva a ausência de informação nos registros existentes; não significa não contratado. O preenchimento informa Sim/Não sob revisão e auditoria. Novas confirmações e simulações exigem a escolha; confirmações anteriores não são reescritas nem impedidas de efetivar por falta do novo metadado. Novas versões/reinclusões copiam a escolha da origem, sem presumir valor quando ausente.
2. Somar os prêmios líquidos mensais, aplicar o percentual do Ramo uma vez e arredondar o IOF para duas casas com `MidpointRounding.AwayFromZero`. O total é líquido mais imposto arredondado, sem arredondamento por cobertura.
3. Na simulação, exigir escolha explícita de Ramo ativo vinculado à mesma Apólice e mostrar a alíquota utilizada. Não associar o texto livre do Plano automaticamente ao catálogo nem gravar essa seleção na proposta.

Não há nova coluna humana: o indicador, flags, UUIDs, revisão, alíquota e valores são técnicos. As propriedades textuais de nomes reutilizam a classificação existente. A migration deve preservar todos os campos anteriores e bloquear rollback se houver escolhas registradas. Não reinstalar triggers de Title Case por uma coluna booleana.

Valores monetários da nova resposta serão strings decimais canônicas com duas casas, evitando perda de centavos no JavaScript para valores altos. O cálculo usa decimal e o limite monetário já adotado pelo backend; nulo em prêmio considerado ou alíquota ausente não é zero. A interface apenas formata os valores retornados, sem refazer cálculos financeiros.

## Implementação e Contratos

A consulta fica no módulo Seguro, com a permissão existente `propostas.visualizar`, sem unidade de escrita, auditoria de alteração, incremento de revisão ou dependência de Financeiro. Os endpoints são:

- `GET /api/propostas/{publicId}/simulacao-mensal/opcoes`: vínculos de Ramos ativos, com cadastro de Ramo ativo e sem exclusão, da mesma Apólice.
- `POST /api/propostas/{publicId}/simulacao-mensal`: recebe `revisaoEsperada` e `ramoPublicId`, este sendo o UUID público do vínculo Apólice/Ramo listado nas opções. Retorna revisão, contratação de cônjuge, Ramo/código/alíquota, instante do cálculo, coberturas consideradas e totais.

A proposta precisa ser nativa, visível operacionalmente e vinculada à Apólice/Vida. A simulação não depende de estar vigente: condições registradas de uma versão visível podem ser conferidas como estimativa, sempre com a alíquota atual do Ramo escolhido. Propostas antigas com contratação desconhecida não são inferidas; a interface orienta completar a Pré Proposta ou preparar nova versão se já confirmadas.

Exige um único item contextual ativo e coberturas nativas ativas pertencentes a ele, com identificação de Cobertura única. Condições históricas inativas ficam fora da soma. Os prêmios vêm exclusivamente do snapshot da proposta, inclusive valores ajustados; alterações de preços no Módulo não mudam essa consulta. Prêmios considerados devem existir, ser não negativos e ter até duas casas. Prêmio de cônjuge ausente não impede simulação quando a contratação é Não. Alíquota ausente impede cálculo, zero explícito é aceito, e o percentual deve estar entre 0 e 100 com até quatro casas.

O limite de cada prêmio, subtotal e total é `9.999.999.999.999.999,99`, consistente com `numeric(18,2)`. Valores acima do limite são recusados, sem truncamento. Líquido titular e cônjuge são somados antes de aplicar o IOF uma única vez. Exemplo: líquido de R$ 5,00 com IOF de 2,5% resulta em R$ 0,13 de imposto e R$ 5,13 de total. Duas coberturas de R$ 5,00 resultam em R$ 0,25 de imposto total, e não R$ 0,26.

Como a leitura do agregado usa consultas separadas, a revisão é conferida ao receber a seleção e novamente ao finalizar; disponibilidade e alíquota do Ramo também são relidas. Alterações detectadas exigem nova consulta. O resultado informa sua revisão e instante e não representa um valor congelado para cobrança.

O detalhe passa a retornar `incluiConjuge` e `podeSimularValorMensal`. O preenchimento aceita `incluiConjuge` opcional: `true`/`false` registram a escolha sob o fluxo existente de revisão e auditoria; omissão ou `null` preservam a escolha atual, mantendo compatibilidade de payloads antigos. **Mudança funcional explícita:** novas confirmações sem escolha agora são recusadas, mesmo que o cliente antigo ainda consiga salvar o preenchimento. Ativação de propostas já confirmadas antes dessa exigência continua possível. A criação de novas versões e ambas as fontes de reinclusão copiam o metadado da origem; revisão de condições o preserva.

No frontend, o preenchimento requer Sim/Não sem valor presumido nos registros novos. O detalhe contém a seção Simulação mensal, com Ramo inicialmente sem seleção. O resultado discrimina cobertura, ajuste, líquidos e IOF. Carregamento, erro, nova tentativa e ausência de Ramos têm estados próprios. Fechar, trocar Ramo, mudar de proposta/revisão, iniciar edição ou sair da tela cancela consultas e descarta valores anteriores. `useConsulta` e o cliente HTTP compartilhados controlam sinais e respostas tardias. Valores monetários canônicos são formatados por `formatarMoedaDecimal`, sem conversão para Number.

## Arquivos e Modelagem

Backend: `SimularValorMensal/SimulacaoMensal.cs`, porta/repositório de Propostas e controlador; campo de domínio, configuração EF e snapshot; migration `20261118103000_IncluirConjugeProposta`; preenchimento, confirmação, criação e detalhe. Frontend: `SimulacaoMensalProposta.tsx`, API/tipos próprios, preenchimento/schema, detalhe e formatador compartilhado. Testes: `PropostasSimulacaoMensalTests.cs`, `ConjugePropostaMigrationTests.cs`, autorização HTTP e testes de componente/API.

O indicador é booleano técnico anulável, sem default e sem atualização de registros antigos. UUIDs, revisão, valores decimais textuais, alíquota e instante também são técnicos. Nomes reutilizam campos humanos existentes e os mecanismos de Title Case; não foi criada coluna humana nem alterada a lista compartilhada C#/TypeScript/SQL. Não é necessário reaplicar triggers por esta coluna booleana. A migration compara a imagem dos dados anteriores, verifica ausência de default presumido e bloqueia rollback quando qualquer escolha, inclusive Não, estiver registrada.

## Implantação e Limites

Aplicar a migration do contexto Seguro antes de publicar a API que consulta a nova coluna, após conferir conexão, baseline e backup pelo procedimento do ambiente. Respeitar a ordem das migrations preparadas nos documentos 93/94. A implementação inicial não aplicou migrations em bases persistentes. Após autorização, as três migrations dos documentos 93–95 foram aplicadas em `webapolice_teste`, com backup e verificação em nova conexão; ver [registro 96](96-aplicacao-migrations-propostas.md). Não houve publicação ou reinício da API nem alteração de Keycloak/perfis nesta instalação.

O Financeiro encontrado contém modelos históricos de títulos e movimentos que não demonstram um fluxo nativo de emissão pronto. A simulação não usa esses modelos nem gera títulos, parcelas, competências, vencimentos, comissões, remessas, baixas, pagamentos ou integrações externas. Campos existentes `PremioLiquido`, `IofPercentual`, `IofValor` e `ValorParcela` não são alterados. Uma etapa futura de faturamento deverá definir seus próprios estados, regras de competência, idempotência, UUIDs e contrato com Financeiro.

## Validação

As verificações finais passaram em 18/11/2026. Testes isolados cobrem cálculo sobre líquido total, empate arredondado para cima, contratação explícita, cônjuge excluído, zero/nulo, limites monetários, ajustes e preços atuais distintos do Módulo, condições inativas, revisão concorrente, cópia nas novas versões/reinclusões, compatibilidade da ativação anterior e preservação de registros/auditoria. A migration é exercitada em PostgreSQL efêmero e os endpoints verificam autenticação, permissão e disponibilidade do catálogo.

A interface é testada nos temas claro, escuro e sistema, com escolha explícita, estados de consulta, cancelamento pendente, resposta tardia, recarga de revisão e preservação de centavos grandes. Os testes DOM não substituem homologação visual autenticada: o inventário disponível retornou `apps: []` e `browsers: []` na etapa anterior, sem sessão conectada para homologar o fluxo completo.

| Verificação | Resultado final |
| --- | --- |
| `dotnet test backend/WebApolice.slnx --no-restore` | 783 aprovados: 513 Integration, 164 API, 28 Arquitetura, 70 Cadastro, 6 Auditoria Unit e 2 Auditoria Integration |
| `dotnet build backend/WebApolice.slnx --no-restore --verbosity quiet` | Aprovado; zero erros e avisos |
| `npm.cmd run format:check` | Aprovado; nenhum arquivo pendente |
| `npm.cmd run typecheck` | Aprovado; zero erros |
| `npm.cmd run lint` | Aprovado; zero erros e avisos |
| `npm.cmd run lint:design-system` | Aprovado; zero ocorrências |
| `npm.cmd run test:run -- --maxWorkers=2` | 1.381 aprovados em 94 arquivos |
| `npm.cmd run build` | Aprovado |
| `git diff --check` e scanner local de segredos | Aprovados |

Foram acrescentados 30 cenários de integração/migration/cálculo, 2 de autorização HTTP e 13 de frontend. As contagens dos documentos anteriores são históricas. Os avisos preexistentes do build Vite sobre chunk acima de 500 kB e tempo de plugins permanecem; nenhuma regra ou configuração de verificação foi flexibilizada. As falhas iniciais de tipagem do formulário e preparação dos testes de tema/matchers foram corrigidas antes das execuções completas finais.
