# Roteiro de Homologação Antes do Manual

Data: 25/11/2026. Continuidade das entregas 102, 103 e da revisão financeira 104.

## Situação Atual

Atualização em 26/11/2026: a homologação foi retomada com navegador conectado em 25/11. As verificações parciais, correções e o ponto de retomada estão no [registro 107](107-homologacao-parcial-manual.md). Os estados pendentes da matriz original abaixo não representam aprovação dos cenários completos. O relato de indisponibilidade e adiamento preserva o histórico anterior à conexão.

A revisão financeira foi concluída com a decisão do responsável de manter o escopo atual e documentar seus limites. Foram corrigidas as referências históricas conflitantes nos documentos 95, 97 e 98. As alterações de Ramos e Estipulantes e suas verificações automatizadas estão registradas nos documentos 102 e 103.

A homologação autenticada está **pendente e adiada por decisão do responsável em 25/11/2026**. O responsável solicitou prosseguir para a reconciliação documental, registrada no [índice funcional 106](106-indice-funcional-atual.md). Os cenários H01–H15 continuam sem execução autenticada.

No levantamento anterior, em 25/11/2026, a ferramenta de navegação retornou nenhuma aplicação ou navegador conectado; a tentativa de abrir o navegador integrado retornou `Browser is not available: iab`. Foi solicitada ao responsável uma sessão de navegador conectada, com o WebApolice aberto e o login realizado.

Nesta conferência, o frontend local em `http://127.0.0.1:5173/` e a saúde da API em `http://127.0.0.1:5007/api/health/ready` responderam HTTP 200. Essas respostas confirmam disponibilidade dos serviços; os cenários abaixo ainda precisam ser executados nas telas com login.

## Preparação

- Confirmar o ambiente de testes antes de qualquer gravação. Para os serviços locais, conferir novamente que a API aponta para `webapolice_teste`.
- Usar registros fictícios e identificados de homologação para criação, edição e transições. Os registros financeiros gerados durante os testes devem permanecer rastreáveis, sem apagar auditoria ou histórico para limpar o ambiente.
- Utilizar perfis e permissões já existentes. O teste de acesso restrito pode ser feito em outra sessão disponibilizada pelo responsável, sem conceder permissões apenas para aprovar a homologação.
- Conferir temas claro, escuro e sistema e larguras de desktop e celular. Registrar data, cenário, resultado e evidência sem copiar credenciais ou dados pessoais reais.

## Cenários e Resultados Esperados

| ID | Cenário | Resultado esperado | Situação |
| --- | --- | --- | --- |
| H01 | Menu e página inicial | Grupos e acessos correspondem às permissões; navegação móvel fecha ao escolher uma tela; atalhos levam ao destino correto | Pendente |
| H02 | Ramos: Visualizar | Abre o registro correto; mostra campos, situação e datas; permite voltar; edição aparece conforme permissão | Pendente |
| H03 | Estipulantes: Grupo | Campo ausente em cadastro, edição e detalhes; editar outros dados preserva eventual vínculo legado | Pendente |
| H04 | Estipulantes: Seguradora | Busca apresenta nomes e permite selecionar, trocar e remover ao salvar; detalhes mostram nome; sem permissão de busca, o vínculo atual permanece preservado | Pendente |
| H05 | Clientes e vínculos | Consultas e detalhes identificam corretamente o Cliente e a matrícula; abrir propostas do vínculo mantém o contexto e permite escolher a versão | Pendente |
| H06 | Apólices e Vidas | Consulta e abas exibem a Apólice correta; seleções de Vida, Subgrupo e Módulo mantêm o contexto e mostram impedimentos aplicáveis | Pendente |
| H07 | Nova pré proposta | Seleciona Apólice e Vida elegíveis; apresenta o contexto antes de gravar e abre a proposta criada; cancelamento retorna sem criar registro | Pendente |
| H08 | Preenchimento e beneficiários | Grava os dados válidos; aponta pendências nos campos; mantém edição após falha; exige beneficiários completos e percentuais conforme as regras da confirmação | Pendente |
| H09 | Financeiro e confirmação | Apresenta os valores conferidos, IOF, competência e vencimento; mudanças invalidam a conferência anterior; confirmação e início da vigência seguem as datas informadas | Pendente |
| H10 | Agendamento, cancelamento e reinclusão | Ações disponíveis correspondem à situação da proposta; justificativas e datas são exigidas quando aplicáveis; histórico anterior permanece acessível | Pendente |
| H11 | Lote mensal e inadimplência | Conferência separa mensalidade e saldo anterior; geração não duplica primeira parcela; fechamento pode manter abertas e libera o mês seguinte | Pendente |
| H12 | Baixa e histórico | Baixa integral exige data e confirmação; dívida antiga pode ser baixada após fechamento sem reabrir o mês; histórico e saldos refletem a operação | Pendente |
| H13 | Acessibilidade e gravações pendentes | Rótulos, foco e teclado permitem operar; durante gravação, ações incompatíveis e duplo envio permanecem bloqueados | Pendente |
| H14 | Falhas, vazio e troca de contexto | Telas diferenciam carregamento, ausência de registros e erro; nova tentativa funciona; resposta antiga não substitui outro registro ou competência | Pendente |
| H15 | Acesso restrito | Menu, rota e API respeitam o acesso; indisponibilidade de módulo/recurso bloqueia a funcionalidade, inclusive para Acesso Total quando aplicável | Pendente |

Executar cada cenário aplicável nos três temas. Nas rotinas com escrita, registrar os UUIDs dos dados fictícios utilizados e verificar a persistência após reabrir a tela. Para testar falhas de rede ou restrições, utilizar o ambiente e as sessões de teste disponíveis, preservando as configurações de segurança.

O escopo financeiro segue o [documento 104](104-alcance-atual-rotinas-financeiras.md): baixa integral, sem pagamento parcial, estorno, emissão de títulos ou execução bancária. Essas funcionalidades não fazem parte da aprovação dos cenários atuais.

## Registro de Execução

| Data | Cenário | Ambiente e tema | Resultado | Evidência ou defeito |
| --- | --- | --- | --- | --- |
| 25/11/2026 | Preparação de navegador | Ferramenta de navegação desta sessão | Impedido por ausência de navegador conectado | Inventário vazio e indisponibilidade do navegador integrado; serviços locais responderam HTTP 200 |

Ao encontrar um defeito, registrar o comportamento observado, o esperado e como reproduzir. Corrigir e verificar o cenário afetado, além dos checks obrigatórios pertinentes, antes de marcar sua aprovação. Resultados automatizados anteriores continuam como evidência própria; cada aprovação visual deve corresponder a uma execução efetiva.

## Critério para Iniciar a Versão Revisável do Manual

Concluir os cenários necessários às rotinas que entrarão no manual, corrigir os defeitos impeditivos e manter visíveis as limitações ou cenários ainda pendentes. As imagens e instruções finais devem corresponder à versão homologada, sem apresentar recursos planejados como disponíveis. O levantamento e a organização da documentação existente podem avançar com a homologação adiada. O adiamento não substitui a execução dos cenários antes da revisão final das instruções e imagens.

Esta etapa alterou somente documentação. Não houve mudança de código, permissões ou dados, nem criação, geração ou baixa financeira para ensaio.
