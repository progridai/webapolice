# Exemplo Completo: do Cadastro ao Pagamento

[Início do manual](README.md) · [Anterior: dúvidas](08-duvidas.md)

## Cenário Didático

Ana Souza será incluída no seguro da Empresa Exemplo, com matrícula 123. A Apólice, os cadastros e as condições usados aqui são fictícios. O objetivo é explicar a sequência; não reproduza lançamentos fictícios em registros reais.

Cada etapa possui um ponto de conferência. Se ele não for atendido, resolva a causa antes de seguir.

## Etapa 1 — Conferir os Cadastros

Pesquise Ana em **Operação → Clientes**. Se não existir, faça seu cadastro com dados válidos. Confira a Empresa Exemplo em **Parceiros e Estrutura → Estipulantes**.

**Ponto de conferência:** pessoa e organização corretas, sem cadastro duplicado.

[Ver cadastro de Clientes e vínculos](03-cadastros.md#cadastrar-um-cliente).

## Etapa 2 — Registrar a Matrícula

Nos detalhes de Ana, em **Vínculos**, adicione ou confira o Estipulante Empresa Exemplo e a matrícula 123.

**Ponto de conferência:** o vínculo identifica Ana, o Estipulante e a matrícula corretos. Nenhuma participação em Apólice foi presumida só por esse cadastro.

## Etapa 3 — Conferir a Estrutura da Apólice

Abra a Apólice destinada à Empresa Exemplo. Confira períodos e situação, Subgrupo quando aplicável e o Módulo desejado. No Plano do Módulo, revise as Coberturas e seus prêmios.

**Ponto de conferência:** condições completas e disponíveis para o uso pretendido.

[Ver Plano e Coberturas](04-apolices.md#configurar-plano-e-coberturas).

## Etapa 4 — Incluir a Vida

Na aba **Vidas**, inclua a participação de Ana usando o vínculo/matrícula 123 e confira Módulo, Subgrupo e demais dados aplicáveis.

**Ponto de conferência:** Ana está na Apólice correta. A Vida está pronta para a avaliação de criação de Proposta; isso ainda não significa cobertura vigente.

## Etapa 5 — Criar e Preencher a Pré Proposta

Em **Operação → Propostas → Nova pré proposta**, selecione a Apólice e a Vida de Ana. Confira e crie. Preencha beneficiários e demais informações. Neste exemplo, dois beneficiários com 60% e 40% completam os 100% exigidos.

**Ponto de conferência:** Cliente, matrícula, Apólice, condições, beneficiários e eventuais ajustes corretos.

[Ver preenchimento e revisão](05-propostas.md#preencher-a-pre-proposta).

## Etapa 6 — Conferir Valores e Confirmar

Informe início válido, competência inicial, primeiro vencimento e Ramo para IOF. Execute **Conferir primeira parcela**, revise a composição e confirme os valores e a Proposta.

Se o início for futuro, acompanhe o agendamento. Se for hoje ou retroativo válido, confira o retorno da efetivação. Neste exemplo, suponha uma mensalidade final conferida de R$ 100,00.

**Ponto de conferência:** resultado e datas corretos. Quando ocorrer a ativação com plano financeiro nativo, haverá a primeira parcela integral; nenhum pagamento foi presumido.

## Etapa 7 — Registrar o Pagamento Recebido

Quando o recebimento integral da primeira parcela estiver confirmado, abra **Apólice → Parcelas**, confira a competência, localize a Vida e a parcela, informe a data e confirme a baixa integral.

**Ponto de conferência:** pagamento na parcela certa, data correta e saldo atualizado.

[Ver baixa integral](06-financeiro.md#registrar-uma-baixa-integral).

## Etapa 8 — Fechar e Seguir para o Próximo Mês

Depois da revisão da competência da Apólice, confirme seu fechamento. No mês seguinte, confira e gere manualmente o lote elegível.

Se uma parcela anterior continuar aberta, ela permanece como dívida. O mês pode estar fechado e a parcela ainda ser paga depois. Acompanhe separadamente mensalidade, saldo anterior e total acumulado.

**Ponto de conferência:** sequência mensal liberada sem apagar ou duplicar débitos.

## Se as Condições Mudarem Depois

Antes da confirmação/ativação, confira se cabe revisar as condições da preparação. Para uma contratação vigente, avalie nova versão. Para encerrar, use cancelamento de vigência; para retomar após cancelamento com vigência, avalie reinclusão.

Cada transição mantém sua própria história. Consulte [o capítulo de Propostas](05-propostas.md) antes de escolher a ação.
