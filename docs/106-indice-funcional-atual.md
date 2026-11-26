# Índice Funcional Atual para Preparação do Manual

Data da revisão: 25/11/2026.

Atualização de 26/11/2026: a conferência visual foi iniciada; resultados parciais e correções estão no [registro 107](107-homologacao-parcial-manual.md). A homologação final continua pendente.

Continuidade: a primeira edição revisável do [manual do usuário](manual-usuario/README.md) está disponível também em [HTML com menu e busca](manual-usuario/index.html). A homologação visual permanece pendente conforme o roteiro 105.

Este documento organiza as referências do funcionamento implementado no WebApolice. Não é o manual do usuário nem uma aprovação de homologação. A validação visual autenticada foi adiada por decisão do responsável; seus cenários continuam pendentes no [roteiro 105](105-roteiro-homologacao-antes-manual.md).

## Como Interpretar as Referências

Os documentos numerados registram entregas em datas diferentes. Expressões como “próxima etapa”, “sem financeiro” e “não há confirmação” descrevem o alcance daquela entrega, e não necessariamente o sistema atual. Para preparar o manual, seguir as continuidades abaixo e conferir a tela correspondente. Resultados de testes, aplicação de migrations e implantação valem para a versão e o ambiente explicitamente registrados em cada documento.

A [visão do produto](00-visao-do-produto.md) contém objetivos de evolução. A existência de um projeto de backend não comprova uma rotina pronta para uso. Sinistros, comissões, integrações externas e outros objetivos estratégicos não devem ser apresentados como funcionalidades operacionais apenas por constarem da arquitetura.

## Organização por Área do Menu

O menu implementado está definido em [navigation.ts](../apps/web/src/app/navigation/navigation.ts), com destinos em [routePaths.ts](../apps/web/src/app/routes/routePaths.ts). A disponibilidade depende do perfil, das permissões e da situação dos módulos e recursos. A ausência de uma opção para um usuário não comprova ausência da funcionalidade no produto.

| Área | Rotinas e distinções para o manual | Referência principal |
| --- | --- | --- |
| Início | Navegação e atalhos pessoais conforme os acessos disponíveis | [Menu 88](88-organizacao-menu-principal.md), [atalhos 89](89-atalhos-pessoais-home.md) |
| Operação — Clientes | Cadastro, consulta e detalhes; contatos, endereços e vínculos com Estipulante/matrícula. A participação na Apólice é selecionada em Vidas; criar matrícula não inclui automaticamente o Cliente em uma Apólice | [Detalhes 19](19-modulo-clientes-detalhes.md), [vínculos 81](81-vinculos-clientes-participacoes.md) |
| Operação — Apólices | Resumo, Ramos, Configurações, Subgrupos, Módulos, Subestipulantes, Vidas, Universo anterior, Parcelas e Histórico. Distinguir configuração contextual da Apólice dos cadastros globais e do histórico legado | [Hierarquia 21](21-cadastros-convenios-planos-coberturas.md), [Módulo 22](22-plano-coberturas-modulo-apolice.md), [vínculos 81](81-vinculos-clientes-participacoes.md), [exclusão 82](82-exclusao-hierarquica-vinculos-apolice.md) |
| Operação — Propostas | Consulta, criação de pré proposta, preenchimento, revisão das condições, confirmação, substituição, agendamento, cancelamento e reinclusão conforme a situação e elegibilidade | Sequência funcional abaixo |
| Parceiros e Estrutura | Seguradoras, Corretoras, Estipulantes, Subestipulantes e Cooperados. No Estipulante, Grupo foi retirado da interface e a Seguradora pode ser selecionada; vínculos históricos são preservados | [Entrega 103](103-vinculo-seguradora-estipulantes.md), [rotas do frontend](../apps/web/src/app/routes/routePaths.ts) |
| Configurações do Seguro | Ramos, Coberturas, Módulos Globais e Convênios de Cobrança. Módulo Global é distinto do Módulo contextual da Apólice. Ramos possui tela de detalhes | [Ramos 102](102-consulta-detalhes-ramos.md), [Módulo 22](22-plano-coberturas-modulo-apolice.md), [menu 88](88-organizacao-menu-principal.md) |
| Segurança | Usuários, Perfis, Catálogo de Permissões e Auditoria; catálogo de permissões é consulta, sem presumir edição livre | [Menu e restrições 88](88-organizacao-menu-principal.md), [navegação implementada](../apps/web/src/app/navigation/navigation.ts) |
| Sistema | Módulos do Sistema e Design System; acesso de operador, com Design System também condicionado à configuração do ambiente | [Menu 88](88-organizacao-menu-principal.md), [navegação implementada](../apps/web/src/app/navigation/navigation.ts) |

Os caminhos do frontend são usados pelo roteador com hash: no ambiente local, por exemplo, Clientes é acessado em `http://127.0.0.1:5173/#/clientes`. O endereço e a disponibilidade do ambiente devem ser confirmados antes de produzir as imagens finais.

## Sequência Funcional de Propostas

| Assunto | Referência atual | Cuidado ao interpretar o histórico |
| --- | --- | --- |
| Criação contextual e consulta | [83](83-fluxo-operacional-propostas.md), [criação pelo menu 100](100-criacao-pre-proposta-menu-propostas.md), [consulta pelo vínculo do Cliente 101](101-consulta-propostas-vinculo-cliente.md) | O diagnóstico inicial do 77 e a primeira etapa do 83 não representam todo o fluxo entregue |
| Nome e situação inicial | [93](93-nomenclatura-pre-proposta.md) | Pré Proposta é o nome exibido; `RASCUNHO` permanece código técnico. Criação não significa início de vigência |
| Preenchimento e beneficiários | [84](84-preenchimento-rascunho-propostas.md) | Preservar as validações e condições registradas; não confundir dados atuais dos cadastros com valores históricos da proposta |
| Revisão das condições | [94](94-revisao-explicita-condicoes-propostas.md), [formação de valores 95](95-formacao-valores-financeiros-propostas.md) | Alterações que afetam a conferência exigem nova revisão antes da confirmação |
| Confirmação e substituição | [85](85-confirmacao-substituicao-propostas.md), [alcance financeiro 104](104-alcance-atual-rotinas-financeiras.md) | A ausência de financeiro na entrega original não é uma limitação atual de toda confirmação |
| Agendamento | [86](86-gestao-agendamentos-propostas.md) | Separar data de confirmação, data de início e processamento do agendamento |
| Cancelamento e descarte | [87](87-encerramento-vigencia-descarte-propostas.md), [gestão do pedido 91](91-gestao-pedido-cancelamento-vigencia.md) | Ações dependem da situação; preservar justificativas, datas e histórico |
| Reinclusão | [92](92-reinclusao-propostas.md) | Preparar uma nova versão não reativa a anterior nem gera cobrança por si só; eventual confirmação/ativação segue o fluxo atual |
| Parcelas, lotes e baixa | [97](97-parcelas-movimentos-nativos-propostas.md), [98](98-lotes-mensais-baixa-parcelas.md), consolidados no [104](104-alcance-atual-rotinas-financeiras.md) | O fechamento admite parcelas abertas; não exige quitação para liberar o mês seguinte |

## Limites que Precisam Aparecer no Manual

- Financeiro: baixa integral; sem pagamento parcial, estorno, emissão de títulos/boletos ou integração bancária. Geração mensal e fechamento são operações manuais. O [documento 104](104-alcance-atual-rotinas-financeiras.md) registra a decisão de manter esse escopo e detalha seus limites.
- Histórico: Universo anterior e evidências de migração não devem ser descritos como o fluxo de novas propostas. Alterações cadastrais não autorizam reescrever condições históricas.
- Clientes: a entrega de vínculos substitui a pendência genérica de manutenção de vínculos do documento 19. Ela não comprova um fluxo completo de manutenção de dependentes; esse assunto precisa de conferência específica antes de instruções de edição.
- Acesso: ações podem depender de permissões diferentes das de consulta. O manual deve explicar impedimentos observáveis sem orientar concessões indiscriminadas.
- Homologação: os testes automatizados registrados nas entregas não substituem os cenários de navegação, temas e gravação autenticada ainda pendentes.

## Situação da Preparação do Manual

| Pendência tratada | Situação e evidência |
| --- | --- |
| Visualizar Ramos | Implementado; verificações registradas no [102](102-consulta-detalhes-ramos.md) |
| Grupo e Seguradora no Estipulante | Decisão aplicada e verificações registradas no [103](103-vinculo-seguradora-estipulantes.md) |
| Escopo financeiro | Revisado; responsável optou por manter os limites atuais, documentados no [104](104-alcance-atual-rotinas-financeiras.md) |
| Homologação visual autenticada | Adiada pelo responsável em 25/11/2026; H01–H15 continuam pendentes no [105](105-roteiro-homologacao-antes-manual.md) |
| Referências históricas conflitantes | Reconciliadas por este índice e pelas notas de continuidade nos documentos de origem; registros anteriores preservados |

A base documental está organizada para preparar o manual por rotina. As instruções e imagens finais das telas dependem da retomada da homologação e da correção de eventuais defeitos encontrados nela. Este índice não declara o sistema inteiro homologado nem elimina as pendências técnicas ou operacionais registradas em outras revisões.

## Verificação Desta Revisão

Entrega somente documental: índice funcional, referências de continuidade, entrada no README e registro do adiamento da homologação. Sem alteração de código, permissões, banco ou dados financeiros.

Verificação executada: 94 links locais nos 11 arquivos desta revisão apontam para destinos existentes; leitura UTF-8 sem caracteres de substituição e `git diff --check` aprovados. Não foram repetidos os testes do produto nesta etapa documental; seus resultados permanecem vinculados às entregas em que foram executados.
