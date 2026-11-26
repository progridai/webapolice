# Parcelas e Financeiro

[Início do manual](README.md) · [Anterior: Propostas](05-propostas.md) · [Próximo: Segurança](07-seguranca.md)

## Visão Geral da Rotina

A sequência mensal é **conferir → gerar as mensalidades elegíveis → registrar os pagamentos recebidos → fechar a competência**. Pagamentos de parcelas abertas também podem ser registrados depois do fechamento.

A primeira parcela tem origem na ativação da Proposta com plano financeiro nativo. As mensalidades seguintes são geradas manualmente por Apólice e competência. A mudança do mês no calendário não gera nem fecha lotes sozinha.

## Conferir os Valores da Proposta

**Onde:** detalhe da Proposta → confirmação ou conferência financeira disponível.

1. Confira **Data de início da vigência**.
2. Informe **Competência inicial** e **Primeiro vencimento**.
3. Selecione **Ramo para o IOF**.
4. Escolha **Conferir primeira parcela**.
5. Revise a composição apresentada: prêmios, cônjuge quando contratado, IOF e total mensal.
6. Na confirmação inicial, use **Confirmar proposta e valores** somente após a revisão completa.
7. Para a conferência permitida de uma confirmada ainda não ativada, confira o resultado e use **Salvar financeiro conferido**, quando oferecido.

Os valores e a alíquota conferidos ficam registrados. Uma mudança posterior no cadastro do IOF não recalcula automaticamente o plano já confirmado. Mudanças de início ou condições antes da ativação podem exigir nova conferência.

## Entender a Primeira Parcela

A primeira parcela é mensal e integral; não é proporcional à quantidade de dias de cobertura. Ela é gerada uma única vez na ativação quando há plano financeiro nativo, inclusive na efetivação de início agendado.

**Exemplo didático:** uma mensalidade conferida de R$ 100,00 gera uma parcela mensal integral de R$ 100,00, mesmo que a data válida de início não seja o primeiro dia do mês. Não faça um rateio manual esperando que o sistema o reconheça como regra de cobrança.

A geração mensal considera a parcela inicial para evitar repetir a mesma mensalidade. Em alterações de versão, consulte o histórico antes de concluir que deveria existir outra parcela na mesma competência.

## Conferir uma Competência

**Onde:** Operação → Apólices → abrir a Apólice → Parcelas → **Parcelas e lote mensal**.

1. Confira a Apólice.
2. Informe **Competência do lote**.
3. Escolha **Conferir competência**.
4. Leia a situação: conferência anterior à geração, lote gerado ou competência fechada.
5. Revise todas as páginas de Vidas e as pendências apresentadas.
6. Examine os valores e os detalhes das parcelas antes de gerar, baixar ou fechar.

| Informação | Como interpretar |
| --- | --- |
| Mensalidade da competência | Valor do mês analisado para a Vida, conforme sua elegibilidade |
| Saldo anterior | Parcelas antigas ainda abertas |
| Total acumulado | Mensalidade aberta do mês mais o saldo anterior |
| Impedimento ou motivo | Explicação de por que uma ação ou inclusão não está permitida |

Se houver pendência, corrija a causa e confira novamente. Não mude a competência apenas para contornar um bloqueio da sequência mensal.

## Gerar o Lote Mensal

1. Faça a conferência da competência.
2. Confira a elegibilidade das Vidas, os valores e a situação do mês anterior.
3. Quando a geração estiver liberada, escolha **Confirmar geração do lote**.
4. Aguarde a conclusão.
5. Confira as parcelas resultantes e a mensagem de sucesso.

**Resultado esperado:** criação somente das novas mensalidades elegíveis. Dívidas anteriores permanecem em suas parcelas originais; não são somadas ao valor da nova parcela.

Para liberar a competência seguinte, a anterior precisa estar gerada e fechada. Isso não exige que todas as parcelas tenham sido pagas.

## Registrar uma Baixa Integral

**Antes de começar:** tenha a confirmação do recebimento integral e a data do pagamento. Confira Apólice, Vida, competência e parcela.

1. Na conferência do lote, abra os detalhes da Vida.
2. Localize a parcela pelo número, competência, vencimento, total e saldo aberto.
3. Informe a **data do pagamento** daquela parcela.
4. Confira novamente a identificação e a data.
5. Escolha **Confirmar baixa integral da parcela** correspondente.
6. Aguarde e verifique a data registrada e a atualização dos saldos.

**Resultado esperado:** pagamento integral registrado e saldo da parcela atualizado. A operação não transfere dinheiro e não executa liquidação bancária; registra no WebApolice um pagamento recebido e conferido.

Uma parcela antiga ainda aberta pode ser baixada depois que sua competência foi fechada. Não é necessário reabrir o mês. Confira se está baixando a parcela antiga correta, e não a mensalidade atual.

**Atenção:** não há pagamento parcial nem estorno de baixa nesta versão. Se o valor recebido for parcial, não registre como integral. Se uma baixa foi lançada indevidamente, comunique o responsável com a identificação da parcela para avaliação; não repita o lançamento nem altere o banco de dados.

## Fechar a Competência

1. Consulte novamente a competência e confira se o lote foi gerado.
2. Revise as parcelas e os pagamentos registrados.
3. Identifique quais parcelas permanecerão abertas.
4. Quando o fechamento estiver liberado, escolha **Confirmar fechamento, preservando abertas**.
5. Confira a situação de competência fechada.

**Resultado esperado:** mês fechado para a sequência de geração. Os valores não pagos continuam devidos e podem receber baixa posteriormente.

Fechar a competência não comprova que todos pagaram. Ao informar a situação a outra pessoa, diferencie **mês fechado** de **parcelas quitadas**.

## Entender Atrasados sem Duplicar Cobrança

**Exemplo didático:** janeiro possui R$ 100,00 em aberto. Fevereiro tem nova mensalidade de R$ 100,00. A conferência pode mostrar R$ 100,00 de saldo anterior e R$ 200,00 de total acumulado.

Existem duas parcelas de R$ 100,00; não foi criada uma parcela de fevereiro de R$ 200,00. Se janeiro for pago depois, registre a baixa de janeiro. O saldo anterior será atualizado sem duplicar o pagamento na mensalidade de fevereiro.

## Entender Vencimentos e Novas Versões

O calendário preserva o dia de vencimento e o deslocamento de meses originalmente definidos. Se o dia não existe no mês, usa o último dia daquele mês e retoma o dia original nos seguintes. Exemplo: dia 31 usa o último dia de fevereiro e volta ao dia 31 em março.

Uma nova versão preserva a parcela já gerada e aplica as novas condições à próxima competência elegível ainda não gerada. Cancelamento preserva parcelas e pagamentos anteriores; a geração seguinte considera o período de cobertura. Essas transições não geram automaticamente crédito, devolução ou acerto de diferenças.

## Consultar o Histórico Financeiro

Consulte as parcelas na Apólice e o histórico de parcelas disponível na Proposta. Confira competência, vencimento, valor, pagamento e identificação da versão quando apresentados.

O histórico acompanha os movimentos da família Apólice/Vida visível, inclusive entre versões, conforme as regras de consulta. Movimentos legados são preservados; não presuma que um registro antigo tenha o mesmo plano financeiro das Propostas nativas.

## Limites da Versão Atual

| Disponível | Fora do escopo atual |
| --- | --- |
| Conferência dos valores e IOF | Recálculo automático de todo o histórico por alteração cadastral |
| Primeira parcela na ativação com plano nativo | Proporcionalidade diária da mensalidade |
| Geração manual de lotes mensais | Geração e fechamento automáticos pelo calendário |
| Baixa integral por parcela | Pagamento parcial e fracionamento da baixa |
| Registro de pagamento com data | Estorno de baixa |
| Fechamento com parcelas abertas | Crédito ou devolução automática por cancelamento/substituição |
| Consulta de parcelas e histórico | Emissão de títulos/boletos, remessa/retorno e liquidação bancária |

O responsável optou por manter esse alcance nesta versão. Campos de convênio e dados bancários não ampliam essas funcionalidades.
