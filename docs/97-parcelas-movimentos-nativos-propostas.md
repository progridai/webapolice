# Parcelas e Movimentos Nativos de Propostas

Data: 18/11/2026. Status: fundação de identidade e histórico validada; regras finais e implementação do ciclo financeiro no [documento 98](98-lotes-mensais-baixa-parcelas.md).

Continuidade da [simulação mensal](95-formacao-valores-financeiros-propostas.md) e da [aplicação das migrations em testes](96-aplicacao-migrations-propostas.md). O próximo objetivo é definir como o valor mensal conferido se torna parcela e movimento registrado. A autorização para continuar não define automaticamente as regras de competência, vencimento, proporcionalidade ou emissão de títulos.

## Regras Iniciais Aprovadas

1. Gerar automaticamente a primeira parcela e o movimento na efetivação da vigência, sem emitir título ou enviar cobrança.
2. Cobrar o valor mensal integral, independentemente do dia de início. Não implementar proporcionalidade nesta entrega.
3. O usuário informa competência e data de vencimento, com conferência anterior à geração. Na vigência futura, esses dados precisam existir antes da execução automática.

## Regras Complementares Aprovadas

- Congelar valores e alíquota conferidos na confirmação, inclusive na vigência futura. Alterar condições ou dados da parcela exige nova conferência antes da ativação.
- Competência a partir do mês inicial da cobertura; vencimento livre, inclusive retroativo ou anterior ao início.
- A numeração das parcelas deve continuar crescente entre versões/alterações, mantendo o histórico completo. A competência da alteração define de onde em diante passa a valer o novo valor, com geração mensal recorrente. A entrega não fica mais limitada à primeira parcela.

## Geração Mensal Manual e Vencimentos

Os meses seguintes são gerados **manualmente em lote por um usuário**, após o **fechamento da competência anterior**, que pode manter parcelas abertas. A definição inicial citava a baixa do mês anterior; o refinamento em "Definições do Lote e da Baixa Aprovadas", abaixo, consolidado no [documento 98](98-lotes-mensais-baixa-parcelas.md), não exige que todos tenham pago. Não implementar geração mensal automática por calendário ou vencimento. O controle permite identificar inadimplentes, cancelamentos e novas inclusões antes da geração seguinte. A primeira parcela automática na ativação continua sendo o recorte aprovado; sua articulação com o lote impede duplicar uma parcela já existente. Conferência do estado atual no [documento 104](104-alcance-atual-rotinas-financeiras.md).

Os vencimentos manterão o dia original e a distância em meses em relação à competência inicial. Em mês curto, usar o último dia daquele mês e retomar o dia original nos meses seguintes, sem deslocamento permanente da referência.

Quando a competência de uma alteração já tiver parcela gerada, manter integralmente essa parcela e aplicar o novo valor somente na próxima competência ainda não gerada. Não reescrever a mensalidade anterior nem criar automaticamente ajuste pela diferença nesta entrega.

## Definições do Lote e da Baixa Aprovadas

O lote abrange uma Apólice e uma competência, mostrando inclusões, exclusões e pendências antes da confirmação. O fechamento é manual na data aplicável ao Estipulante e pode manter parcelas abertas. Os atrasados permanecem nas parcelas originais; a conferência seguinte mostra o saldo anterior e o total acumulado por Vida, sem duplicar débitos na nova mensalidade. O próximo mês depende do fechamento do anterior, sem exigir que todos tenham pago. Implementação detalhada no documento 98.

## Consulta do Histórico entre Versões

Preparação independente das definições de recorrência: `GET /api/propostas/{publicId}/parcelas` é uma consulta paginada implementada localmente, protegida por `propostas.visualizar`, dos movimentos registrados nas versões visíveis da mesma Apólice/Vida. Sem Vida vinculada, a consulta ficará restrita à própria proposta, sem presumir família por Pessoa ou Cliente. Serão incluídas versões substituídas/canceladas visíveis, preservando seus valores. Propostas excluídas ou ocultadas não serão expostas por esta consulta operacional.

A resposta identificará versão/proposta, número de parcela, competência, vencimento, lançamento, classificação técnica e valores gravados. Campos legados incompletos serão exibidos como não informados, sem inventar sequência, competência ou total. Valores monetários canônicos textuais preservarão centavos e sinais; a interface apenas os formata. Movimentos legados podem não ter UUID: serão identificados como origem legada e somente consultados, sem expor a PK interna ou oferecer uma rota de escrita por número fictício.

A interface permite abrir o histórico a partir do detalhe, navegar por todas as páginas, recarregar, fechar/cancelar consultas e distinguir carregamento, falha e ausência de registros. Nenhuma parcela será criada ou alterada por essa consulta. Não representa a conclusão do gerador mensal.

## Modelagem da Identidade Nativa

Evolução técnica necessária, independente das decisões complementares: `seguro.proposta_movimento.public_id` será UUID anulável, sem preencher os registros legados. Novos movimentos nativos deverão receber UUID persistido e único; APIs não exporão o ID interno. `legado_id` passará a ser anulável, preservando os valores e a unicidade existentes. Não inventar números de legado para permitir INSERT nativo.

Uma restrição exigirá `legado_id` ou `public_id` válido, recusando movimento sem identificação e UUID vazio. Índice único parcial protegerá UUIDs informados. O rollback será bloqueado após qualquer UUID registrado, evitando apagar identidades públicas ou forçar legado artificial. Os campos são técnicos, sem coluna humana nova; os mecanismos e listas Title Case permanecem intactos. A migration deverá preservar integralmente os campos anteriores e deixar UUID nulo em todos os movimentos preexistentes.

Esta fundação não autoriza geração de movimentos, não altera classificações ou tipos existentes e não define a chave de unicidade da parcela. Essa chave dependerá das regras complementares aprovadas. A nova estrutura só será aplicada a bases persistentes em uma etapa explícita de implantação.

## Diagnóstico Confirmado no Código

- `PropostaMovimento` pertence a Seguro e possui proposta, competência, vencimento, parcela, prêmios e IOF. Antes desta evolução, `LegadoId` era inteiro obrigatório, com índice único, e o modelo não possuía UUID público. Escrita nativa exige evolução explícita, sem inventar identificador de legado ou reescrever evidências existentes.
- `MovimentoTipo` possui nome e flags de classificação, geração de título, financeiro, cancelamento e alteração de proposta. Não possui código estável/UUID público no modelo atual. As flags e os nomes do legado não definem por si a operação nativa aprovada; não selecionar tipo por ID presumido ou texto aproximado.
- Os campos `PremioLiquido`, `IofPercentual`, `IofValor`, `ValorParcela`, `DataPrimeiroVencimento` e `DataProximoVencimento` existem na proposta, mas a simulação atual não os grava. Não reinterpretar ou atualizar campos históricos sem regra aprovada.
- `Titulo` pertence a Financeiro, possui referência ao movimento, competência e vencimento, mas não UUID público no modelo encontrado. Sua presença não constitui um fluxo nativo de emissão disponível. Não injetar FinanceiroDbContext em Seguro.
- `EstipulanteFaturamentoConfig` contém `DiaDebito`, referências de forma/convênio de pagamento e alíquotas históricas por modalidade. A existência desses campos não define regra de vencimento, precedência ou acesso operacional pela proposta. O IOF aprovado nesta frente continua vindo do Ramo vinculado à Apólice.
- `IOperacaoPropostas` permite confirmar ou reverter alteração de negócio e auditoria juntos. Reutilizar a coordenação existente ao acrescentar gravações, sem separar movimento, revisão e auditoria em commits independentes.
- O cálculo mensal existente usa decimal, prêmios ativos registrados e escolha explícita de cônjuge/Ramo. Reutilizar suas validações e convenção de IOF; valores monetários da nova API devem preservar centavos, conforme o contrato textual do documento 95.

## Preparação que Independe das Respostas

Preservar os limites dos ADRs 004/008 e a auditoria do ADR 011. Entidades expostas por novas APIs precisarão de UUID público persistido, conforme ADR 007. A modelagem e a classificação de novos campos deverão ser documentadas antes de criar migrations. A identidade técnica está especificada acima; os demais campos e relações dependem das decisões complementares.

Uma gravação precisa revalidar os insumos apresentados ao usuário no servidor: proposta/revisão, condições registradas, contratação de cônjuge, Ramo/alíquota, período e datas aprovadas. Não confiar em totais calculados pelo cliente ou reutilizar uma simulação antiga como valor imutável sem verificar mudanças. A conferência deve deixar claro o líquido, IOF e total que serão registrados.

A prevenção de duplicidade precisa existir no backend e no banco, além do bloqueio visual. Sua chave depende do conceito aprovado de parcela/competência e de como versões da mesma Vida dividem uma competência. Repetição e concorrência não podem criar movimentos duplicados ou alterar registros financeiros anteriores silenciosamente. Cancelamentos, substituições e reinclusões com financeiro já existente exigirão regras próprias antes de ajustes ou estornos.

Preparar testes de cálculo, datas, revisão/configuração alterada, autorização, concorrência, repetição, rollback por falha de auditoria e preservação do legado. Usar PostgreSQL efêmero e dados isolados; não gerar movimentos em propostas reais para ensaios. Quando houver interface, executar os seis checks obrigatórios e testar os três temas, carregamento, erro, vazio, conferência e bloqueios.

## Estado Desta Preparação

Foram conferidos o Git, os modelos, o mapeamento EF, o cálculo existente e os ADRs pertinentes. Além do diagnóstico, foram preparados domínio, mapeamento, snapshot e migration de identidade nativa, com testes PostgreSQL de preservação, UUID único, rejeição de identidade ausente/vazia e proteção de rollback. A suíte completa do backend passou nesta execução: 785 testes (515 Integration, 164 API, 28 Arquitetura, 70 Cadastro, 6 Auditoria Unit e 2 Auditoria Integration). O build passou com zero erros e avisos. Os dois testes novos da migration também passaram isoladamente. `git diff --check` e o scanner local de segredos passaram. Os 783 testes backend e 1.381 frontend citados anteriormente pertencem à entrega anterior e não são apresentados como resultado desta execução. Não houve alteração de frontend, commit, push, publicação ou aplicação em bases persistentes.

## Continuidade da Consulta de Histórico

Arquivos novos: caso de uso `ListarParcelasQuery.cs`, API/tipo/componente `HistoricoParcelasProposta` e seus testes. Porta/repositório e controlador foram ampliados; o detalhe passou a oferecer a consulta sob os mesmos bloqueios das operações em aberto. O formatador monetário compartilhado agora preserva também sinais negativos de registros históricos. Não foi criada coluna humana: observações reutilizam campo já classificado, enquanto UUIDs, números/códigos, posição visual, datas e valores canônicos são técnicos.

A consulta exige aplicar a migration de identidade nativa antes de publicar a API que lê `proposta_movimento.public_id`. Essa migration continua somente preparada e testada em PostgreSQL efêmero; não foi aplicada novamente à base de testes persistente. As três migrations anteriores do documento 96 permanecem aplicadas. O histórico não cria nem altera parcelas, baixa, títulos ou pagamentos.

Validação final desta continuação em 18/11/2026:

| Verificação | Resultado |
| --- | --- |
| Suíte completa do backend | 793 aprovados: 522 Integration, 165 API, 28 Arquitetura, 70 Cadastro, 6 Auditoria Unit e 2 Auditoria Integration |
| Build backend | Aprovado; zero erros e avisos |
| `npm.cmd run format:check` | Aprovado; nenhum arquivo pendente |
| `npm.cmd run typecheck` | Aprovado; zero erros |
| `npm.cmd run lint` | Aprovado; zero erros e avisos |
| `npm.cmd run lint:design-system` | Aprovado; zero ocorrências |
| `npm.cmd run test:run -- --maxWorkers=2` | 1.393 aprovados em 96 arquivos |
| `npm.cmd run build` | Aprovado |
| Diff check e scanner local de segredos | Aprovados |

Os testes novos da consulta cobrem versões da mesma Vida, isolamento de outra Vida, proposta sem Vida, visibilidade, paginação inválida, preservação de valores altos/negativos/nulos e ausência de alteração de auditoria. A autorização verifica autenticação, permissão específica, catálogo e parâmetros da rota. A interface cobre os três temas, todas as páginas, erro/nova tentativa/vazio, troca de contexto, cancelamento pendente/resposta tardia e sinais monetários. Foram acrescentados 7 cenários de integração, 1 de autorização e 12 de frontend nesta continuação. Os 785 testes da fundação acima são históricos.

Os avisos preexistentes de tamanho de chunk e tempo de plugins do Vite permanecem; nenhuma verificação foi flexibilizada. Homologação visual autenticada não executada nesta continuação. Não houve geração/baixa financeira, aplicação da nova migration em base persistente, commit, push ou publicação. Os resultados acima registram a etapa de fundação/histórico; a entrega financeira posterior e suas verificações estão no documento 98.
