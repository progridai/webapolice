# Apólices e Participações

[Início do manual](README.md) · [Anterior: cadastros](03-cadastros.md) · [Próximo: Propostas](05-propostas.md)

## Abrir e Conferir uma Apólice

**Onde:** Operação → Apólices.

1. Localize a Apólice na consulta.
2. Abra o registro e confira nome, identificação, situação e períodos.
3. Escolha a aba correspondente à tarefa.

| Aba | Finalidade |
| --- | --- |
| Resumo | Conferir a identificação geral |
| Ramos | Consultar e manter os vínculos de Ramos e suas identificações na Apólice |
| Configurações | Conferir as configurações próprias da Apólice |
| Subgrupos | Organizar os Subgrupos do contexto |
| Módulos | Vincular Módulos e configurar Plano e Coberturas |
| Subestipulantes | Manter as associações correspondentes |
| Vidas | Consultar e manter as participações e acessar suas Propostas |
| Universo anterior | Consultar a estrutura histórica correspondente |
| Parcelas | Conferir competências, gerar lotes, registrar baixas e fechar o mês |
| Histórico | Consultar os eventos funcionais registrados |

Antes de alterar uma configuração, confirme que está na Apólice correta. Cadastros parecidos podem existir em contextos diferentes.

## Cadastrar ou Editar os Dados Gerais

**Antes de começar:** confira os cadastros de Estipulante e Seguradora e, quando aplicável, de Corretora.

1. Em **Operação → Apólices**, abra a ação de cadastro ou a edição da Apólice desejada.
2. Informe **Nome (Denominação)**.
3. Selecione **Estipulante** e **Seguradora**.
4. Selecione **Corretora (Opcional)** quando fizer parte da operação.
5. Confira **Início da Vigência**, **Fim da Vigência** e **Data de Aniversário** conforme os campos e regras apresentados.
6. Preencha a observação quando necessária e salve.
7. Abra os detalhes e revise a identificação antes de manter os vínculos nas abas.

**Resultado esperado:** dados gerais da Apólice gravados. O número associado a um Ramo e a estrutura de Módulos, Plano, Coberturas e Vidas são conferidos em suas respectivas etapas.

## Preparar a Estrutura

Para uma nova estrutura, revise os cadastros que serão utilizados: parceiros, Ramos, Coberturas e Módulos Globais. Na Apólice, confira os dados gerais e mantenha os vínculos aplicáveis pelas abas correspondentes.

Uma sequência útil é conferir **Ramos e Configurações**, organizar **Subgrupos e Subestipulantes**, configurar **Módulos, Plano e Coberturas** e, depois, incluir **Vidas**. A necessidade de cada vínculo depende da configuração da operação; não cadastre associações sem correspondência real apenas para preencher a estrutura.

Para alterar um vínculo, use a edição da própria linha ou registro. A situação do cadastro global e a situação do vínculo são controles diferentes; confira ambos quando uma seleção estiver indisponível.

## Configurar Plano e Coberturas

**Antes de começar:** a Cobertura deve existir no cadastro compartilhado; a Apólice e o Módulo precisam permitir a alteração.

1. Na Apólice, abra **Módulos**.
2. Abra **Plano e Coberturas** do vínculo desejado ou edite o vínculo do Módulo.
3. Cadastre seu Plano, se ainda não existir. Cada vínculo do Módulo possui um único Plano.
4. Escolha **Vincular Cobertura**.
5. Selecione uma Cobertura existente.
6. Informe **prêmio do titular** e **prêmio do cônjuge**.
7. Confira a situação e escolha **Salvar vínculo da Cobertura**.
8. Repita para as demais Coberturas e revise a lista final.

Os dois prêmios são obrigatórios, não negativos e aceitam até duas casas decimais. Zero representa ausência de cobrança naquele campo. O sistema não herda automaticamente os valores de Planos globais ou do Universo anterior.

Ao cadastrar um novo vínculo de Módulo, **Salvar vínculo e continuar** permite seguir para Plano e Coberturas. Essas etapas são salvas separadamente: interromper depois de salvar o vínculo não desfaz o que já foi gravado.

**Resultado esperado:** Plano com as Coberturas e os prêmios corretos para aquele vínculo de Módulo naquela Apólice.

## Alterar uma Cobertura do Plano

1. Localize a associação no Plano.
2. Use **Editar vínculo**.
3. Altere prêmios ou situação conforme necessário.
4. Salve e confira o resultado.

A identidade da Cobertura não é trocada dentro da mesma associação. Para usar outra, mantenha o histórico da anterior e faça a nova associação permitida.

Uma Cobertura já associada, mesmo inativa, não aparece como uma associação nova disponível. Edite ou reative o vínculo existente. Para reativá-lo, o cadastro da Cobertura também precisa estar ativo. Se a Cobertura acabou de ser cadastrada, use **Atualizar Coberturas** no seletor.

Alterar a configuração do Módulo não reescreve silenciosamente as condições das Propostas já criadas. Para uma Proposta elegível, utilize [Revisar condições](05-propostas.md#revisar-condicoes).

## Incluir uma Vida

**Antes de começar:** tenha o Cliente cadastrado, seu vínculo com Estipulante e matrícula, e a estrutura de seguro aplicável preparada.

1. Abra a Apólice correta e entre em **Vidas**.
2. Escolha a ação de inclusão de participação.
3. Selecione o Cliente e o vínculo/matrícula correto conforme o formulário.
4. Confira Subgrupo, Módulo, situação e períodos aplicáveis.
5. Revise os dados e salve.
6. Reabra a participação e confira sua identificação.

**Resultado esperado:** participação disponível na Apólice. O relacionamento com a matrícula poderá aparecer também nos detalhes do Cliente. Ainda será necessário criar e confirmar uma Proposta para seguir o fluxo de contratação.

Uma Vida pode ter impedimentos que bloqueiam a criação ou confirmação de Proposta. Leia o motivo apresentado e confira o contexto em vez de criar outra participação para contornar a regra.

## Abrir as Propostas de uma Vida

1. Em **Vidas**, localize a participação correta.
2. Abra a consulta de Propostas disponível para ela.
3. Confira Cliente, matrícula e Apólice.
4. Escolha a versão desejada ou inicie a criação permitida.

Uma Vida encerrada pode continuar permitindo a consulta de seu histórico de Propostas. Consultar não significa poder criar ou confirmar outra versão nesse contexto.

## Encerrar ou Excluir uma Participação

| Necessidade | Ação e efeito |
| --- | --- |
| Registrar o encerramento da participação | **Encerrar** mantém a Vida visível como encerrada |
| Corrigir uma participação incluída indevidamente | **Excluir**, quando permitido, retira a participação das consultas operacionais e preserva histórico e Auditoria |

A exclusão exige confirmação e **não é permitida se a Vida tiver qualquer Proposta**, inclusive inativa ou excluída. Cliente e matrícula não são apagados por essa operação. A permissão de excluir é diferente da de encerrar ou alterar.

Antes de confirmar, confira a Apólice, o Cliente e a matrícula. Se houver Proposta ou outro uso protegido, mantenha o histórico e avalie a operação adequada com o responsável.

## Exclusões na Estrutura da Apólice

Ramos, Subgrupos, Subestipulantes, Módulos e seus vínculos possuem proteções de uso e histórico. Leia o conteúdo da confirmação e os impedimentos apresentados. Não exclua registros relacionados para tentar liberar uma exclusão bloqueada sem avaliar a consequência funcional.

Encerrar participação, cancelar Proposta e inativar configuração são ações diferentes. Confira a finalidade da alteração antes de escolher.

## Consultar o Universo Anterior e o Histórico

Use **Universo anterior** para compreender os dados históricos apresentados. A preparação de novas Propostas segue o contexto atual de Vida, Módulo, Plano e Coberturas; não reutilize identificações antigas como se fossem a estrutura atual.

Use **Histórico** para conferir eventos funcionais. Para acompanhar parcelas e pagamentos entre versões, consulte também [o histórico financeiro](06-financeiro.md#consultar-o-historico-financeiro).
