# Alcance Atual das Rotinas Financeiras

Data da revisão: 24/11/2026. Terceira etapa de revisão das pendências antes do manual do usuário, após os documentos 102 e 103. Este arquivo consolida o comportamento existente; não acrescenta regras de cobrança nem autoriza movimentação de registros reais para testes.

## Funcionalidades Implementadas

| Rotina | Comportamento atual | Referência de implementação |
| --- | --- | --- |
| Simulação mensal | Soma prêmios das coberturas registradas, inclui cônjuge quando contratado e calcula IOF do Ramo | `SimularValorMensal` |
| Conferência antes da confirmação | Exige início, competência, vencimento e Ramo; valida a revisão e as condições atuais | `ConferenciaFinanceira.cs` |
| Congelamento de valores | Salva valores mensais e alíquota conferidos; mudança posterior no cadastro de IOF não recalcula o plano confirmado | `FinanceiroConferencia.SalvarAsync` |
| Primeira parcela | Gera uma única parcela mensal integral na efetivação da vigência, inclusive agendada, quando há plano financeiro nativo | `ParcelasNativas.cs` |
| Demais parcelas | Geração manual em lote por Apólice e competência, com conferência anterior e confirmação | `LotesMensais.cs` |
| Sequência mensal | Exige gerar e fechar a competência anterior para liberar a seguinte, sem exigir quitação de todas as parcelas | `ConferenciaLote.CarregarAsync` |
| Vencimentos | Preserva dia e deslocamento de meses originais; usa último dia no mês curto e retoma o dia original depois | `CalendarioParcelas.Vencimento` |
| Baixa | Registra o pagamento integral de uma parcela nativa aberta, mediante data e revisão válidas | `BaixarParcelaHandler` |
| Atrasados | Mantém o débito na parcela original; apresenta saldo anterior e total acumulado por Vida, sem duplicá-lo na nova mensalidade | `ConferenciaLote.CarregarAsync` |
| Fechamento | Encerra a conferência da competência, preservando parcelas não pagas | `FecharLoteHandler` |
| Pagamento após fechamento | Permite baixar a parcela antiga sem reabrir a competência; atualiza os saldos apresentados | `BaixarParcelaHandler` |
| Nova versão | Preserva a parcela já gerada e aplica as condições novas à próxima competência elegível ainda não gerada | `ParcelasNativas.PrimeiraAsync` e `ConferenciaLote` |
| Cancelamento | Preserva as parcelas e pagamentos anteriores; o lote considera o período de cobertura para as próximas inclusões | `ConferenciaLote` |
| Histórico entre versões | Consulta paginada dos movimentos registrados da família Apólice/Vida visível, sem reescrever o legado | `ListarParcelas` e `HistoricoParcelasProposta` |

## Limites que Precisam Constar no Manual

- A baixa desta versão é **integral**. Não existe registro de pagamento parcial ou fracionamento de uma baixa entre vários pagamentos.
- Não existe operação de **estorno de baixa**. Uma correção de pagamento registrado não deve ser ensinada como edição direta do banco ou repetição do lançamento.
- Não há emissão de títulos, boletos, envio de cobrança, remessa/retorno ou liquidação bancária neste ciclo.
- O valor da mensalidade é integral; não há proporcionalidade pelo número de dias de cobertura. Cancelamento e substituição não geram automaticamente crédito, devolução ou ajuste de diferenças.
- Somente a primeira parcela é gerada automaticamente na ativação. A passagem do calendário não gera os lotes seguintes nem fecha competências.
- Fechamento não significa quitação. Parcelas abertas continuam em aberto e podem ser baixadas depois.
- O total acumulado reúne a mensalidade aberta da competência e o saldo anterior. Esse total não transforma os atrasados em uma nova parcela.
- As operações de geração/baixa desta rotina tratam parcelas nativas. Movimentos legados são preservados e consultáveis conforme as regras do histórico; o sistema não presume plano financeiro nem cobrança retroativa para registros antigos.
- Uma nova conferência é necessária quando início ou condições mudam antes da ativação. Conferências antigas não autorizam gravação com dados diferentes.

Decisão do responsável, confirmada em 25/11/2026: **manter o escopo financeiro atual e documentar seus limites**. Pagamentos parciais, estornos e integrações bancárias permanecem fora desta entrega e do conjunto de funcionalidades disponíveis a ensinar no manual. São ampliações futuras, com regras próprias ainda a definir. A existência desses limites não impede documentar o fluxo já entregue.

## Proteções Existentes

As consultas exigem `propostas.visualizar`; conferência financeira e escritas exigem `propostas.alterar`, respeitando disponibilidade de módulo/recurso no mecanismo de autorização existente. As rotas são autenticadas. UUIDs e revisões identificam os registros e protegem o contexto; uma parcela de outra Apólice não pode ser baixada pelo mesmo comando.

Geração, baixa e fechamento bloqueiam a Apólice e revalidam a conferência na transação. Índices e restrições do banco protegem identidade, sequência e unicidade mensal. A operação de negócio e a auditoria compartilham transação; falha da auditoria reverte a operação. Essas proteções não equivalem a integração bancária.

## Divergências Documentais Corrigidas

1. Documento 95: a limitação de "somente simulação" descrevia a etapa original; foi acrescentada referência ao ciclo financeiro entregue posteriormente.
2. Documento 97: a frase que condicionava o mês seguinte à baixa foi reconciliada com a regra refinada de fechamento, que aceita inadimplência. A evolução da decisão foi mantida explícita.
3. Documento 98: acrescentada uma nota no início sobre a implantação local posterior, evitando interpretar os relatos históricos de migrations preparadas como pendência atual.

Os resultados históricos de testes e de implantação permanecem preservados. Não há alteração em auditoria, evidências originais ou dados de negócio.

## Verificação Desta Revisão

Foram conferidos os casos de uso, repositório, controlador, contratos e telas existentes. A revisão não modificou código de produto, testes, tabelas, permissões ou dados; não houve geração ou baixa em registros reais.

Reexecuções direcionadas concluídas, sem falhas ou testes ignorados:

| Verificação | Resultado |
| --- | --- |
| Integração financeira/calendário em PostgreSQL efêmero | 26 testes aprovados |
| Autorização dos fluxos de Propostas e rotas financeiras da API | 28 testes aprovados |
| Conferência financeira, lotes/baixa e histórico no frontend | 22 testes aprovados em 3 arquivos |

Comandos executados: `dotnet test` dos projetos Integration e API em Release, com `--no-build --no-restore` e filtros financeiros/autorização, e `npm.cmd run test:run --` para os três arquivos de testes financeiros com `--maxWorkers=2`. Os binários do backend correspondem ao código compilado e validado na etapa 103, sem alterações posteriores de produto. A verificação integral do frontend da etapa anterior permanece registrada no documento 103; os seis checks não foram reexecutados nesta revisão exclusivamente documental.

A homologação com login no navegador permanece uma etapa separada antes da publicação do manual.
