# Conceitos e Glossário

[Início do manual](README.md) · [Anterior: primeiros passos](01-primeiros-passos.md) · [Próximo: cadastros](03-cadastros.md)

## Como as Informações se Relacionam

O caminho mais comum de uma operação é:

**Cliente → vínculo com Estipulante e matrícula → Vida na Apólice → Pré Proposta → confirmação e vigência → parcelas.**

Cada etapa tem uma finalidade. Cadastrar o Cliente não cria matrícula; cadastrar matrícula não inclui automaticamente o Cliente na Apólice; incluir uma Vida não confirma uma Proposta; criar a pré proposta não concede cobertura nem gera parcela.

## Pessoas, Vínculos e Participações

| Termo | Significado no sistema | Exemplo didático |
| --- | --- | --- |
| Cliente | Cadastro da pessoa física ou jurídica, com seus dados, contatos e endereços | Ana Souza |
| Estipulante | Organização à qual uma matrícula do Cliente está vinculada e que participa da estrutura do seguro | Empresa Exemplo |
| Vínculo do Cliente | Relação entre Cliente, Estipulante e matrícula | Ana, Empresa Exemplo, matrícula 123 |
| Matrícula | Identificação do vínculo no Estipulante | 123 |
| Apólice | Contexto que reúne a estrutura do seguro, suas configurações e participações | Apólice da Empresa Exemplo |
| Vida | Participação do Cliente e de sua matrícula em uma Apólice | Participação de Ana naquela Apólice |
| Subgrupo | Organização contextual dentro da Apólice | Grupo definido para uma unidade |
| Subestipulante | Cadastro de parceiro/estrutura que pode ser vinculado à Apólice | Unidade participante cadastrada |
| Cooperado | Cadastro próprio disponível em Parceiros e Estrutura | Registro utilizado pela organização |

O mesmo Estipulante pode participar de mais de uma Apólice. Por isso, o nome do Estipulante sozinho não identifica onde uma matrícula está segurada. Confira a Apólice e a Vida.

## Configurações do Seguro

| Termo | O que representa | O que não confundir |
| --- | --- | --- |
| Ramo | Classificação do seguro; participa das regras aplicáveis à Proposta e ao IOF | Número da Apólice é uma identificação própria, não o nome do Ramo |
| Cobertura | Cadastro compartilhado de uma cobertura e seus atributos | Prêmio do vínculo pertence ao Plano em que a Cobertura foi associada |
| Módulo Global | Cadastro reutilizável de Módulo | Não configura sozinho as condições de cada Apólice |
| Módulo da Apólice | Vínculo contextual do Módulo naquela Apólice | Pode ter configuração diferente de outro vínculo do mesmo Módulo |
| Plano do Módulo | Plano exclusivo do vínculo do Módulo na Apólice | Não é o Plano global histórico nem o Universo anterior |
| Prêmio | Valor informado para a contratação da cobertura, com valores de titular e cônjuge conforme o caso | Não é capital segurado nem pagamento já recebido |
| Convênio de Cobrança | Cadastro de informações do convênio e configurações de cobrança | Preencher dados bancários não executa integração ou liquidação bancária |

**Exemplo:** a mesma Cobertura pode existir em dois Planos. Alterar seu nome no cadastro compartilhado muda a identificação apresentada nos vínculos. Alterar o prêmio no Plano A não muda o prêmio no Plano B.

## Propostas e Datas

| Termo | Como interpretar |
| --- | --- |
| Pré Proposta | Preparação ainda sem vigência; permite preencher e revisar as condições |
| Proposta nativa | Criada pelo fluxo atual do WebApolice |
| Registro legado | Informação oriunda do sistema anterior, mantida conforme as regras de preservação histórica |
| Nova versão | Nova tentativa ligada à anterior, com histórico preservado |
| Confirmação | Validação e registro das condições e da data de início |
| Agendada | Confirmada para início futuro; ainda depende da efetivação na data e das validações |
| Vigente | Versão com vigência operacional registrada; confira também seus períodos e eventuais impedimentos |
| Substituída | Versão cuja vigência foi encerrada pela efetivação de uma sucessora |
| Cancelamento agendado | Pedido para encerrar a vigência em data futura |
| Cancelada | Encerramento concluído; consulte se chegou a ter vigência ou se foi cancelada antes do início |
| Descartada | Pré proposta encerrada sem confirmação nem vigência |
| Reinclusão | Preparação de nova versão após cancelamento de uma proposta que teve vigência |

**Situação, vigência e datas devem ser lidas juntas.** Uma Proposta confirmada pode estar agendada, vigente ou já substituída. A palavra “confirmada”, sozinha, não prova cobertura atual.

As operações de início e cancelamento consideram o calendário de São Paulo. A data do cancelamento de vigência é o **primeiro dia sem cobertura**. Exemplo: cancelamento em 20/08 significa ausência de cobertura a partir de 20/08; o sistema registra separadamente quando a ação foi executada.

## Financeiro

| Termo | Significado |
| --- | --- |
| Competência | Mês a que a mensalidade pertence |
| Vencimento | Data prevista para pagamento da parcela |
| Parcela | Registro individual do valor devido |
| Lote mensal | Conjunto de mensalidades elegíveis de uma Apólice para uma competência |
| Conferência | Apresentação dos dados para revisão antes da operação |
| Baixa integral | Registro de pagamento de todo o saldo da parcela |
| Saldo anterior | Débito ainda aberto de competências anteriores |
| Total acumulado | Mensalidade aberta da competência somada ao saldo anterior |
| Fechamento | Encerramento da conferência do mês para liberar a sequência mensal; pode manter parcelas abertas |
| IOF | Imposto considerado na formação do valor conforme o Ramo e a conferência |

**Fechado não significa pago.** Uma competência fechada pode conter parcelas em aberto e receber baixas posteriores.

## Nomes Parecidos que Exigem Atenção

- **Grupo e Subgrupo:** Grupo foi retirado do cadastro de Estipulantes. Subgrupo da Apólice continua sendo uma configuração própria da estrutura da Apólice.
- **Encerrar e excluir Vida:** encerrar mantém a participação como encerrada; excluir corrige uma participação indevida dentro das restrições do sistema.
- **Cancelar e descartar:** cancelar trata o agendamento ou a vigência conforme a situação; descartar encerra uma pré proposta sem confirmação.
- **Retirar pedido e reincluir:** retirar atua antes de efetivar o cancelamento; reincluir prepara nova versão após cancelamento com vigência.
- **Histórico e Auditoria:** o histórico explica a evolução funcional; a Auditoria permite rastrear operações registradas conforme o acesso administrativo.
