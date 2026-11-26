# Manutenção Editorial do Manual

Este arquivo orienta a equipe mantenedora. O ponto de entrada do usuário é o [manual](README.md) ou sua [edição de navegador](index.html).

## Fontes e Geração

Os arquivos `README.md` e `01` a `09` são as fontes editoriais. `index.html` é a edição gerada, autossuficiente e sem dependências de rede. A busca é local; o manual não autentica, não consulta a API e não altera registros.

Para regenerar, na raiz do projeto, execute:

`python docs/manual-usuario/gerar_manual.py`

O gerador usa apenas a biblioteca padrão do Python e o subconjunto Markdown utilizado nos capítulos: títulos de níveis 1 e 2, parágrafos, listas simples, tabelas, links, negrito e código em linha. Não acrescente sintaxes não suportadas sem atualizar a geração. Ele confere os destinos dos links HTML e a unicidade dos identificadores antes de gravar a saída.

Edite as fontes e regenere o HTML na mesma entrega. Não mantenha correções apenas no HTML. Preserve links entre capítulos; mudanças em títulos podem alterar âncoras e devem ser verificadas.

## Referências de Conteúdo

| Conteúdo do manual | Base de conferência |
| --- | --- |
| Áreas, acesso e atalhos | [88](../88-organizacao-menu-principal.md), [89](../89-atalhos-pessoais-home.md), `apps/web/src/app/navigation/navigation.ts` |
| Clientes e matrículas | [19](../19-modulo-clientes-detalhes.md), [81](../81-vinculos-clientes-participacoes.md), [101](../101-consulta-propostas-vinculo-cliente.md), formulários e detalhes de Clientes |
| Ramos e Estipulantes | [102](../102-consulta-detalhes-ramos.md), [103](../103-vinculo-seguradora-estipulantes.md), formulários e páginas atuais |
| Apólices e Plano do Módulo | [21](../21-cadastros-convenios-planos-coberturas.md), [22](../22-plano-coberturas-modulo-apolice.md), [81](../81-vinculos-clientes-participacoes.md), [82](../82-exclusao-hierarquica-vinculos-apolice.md), abas da Apólice |
| Propostas | [Índice 106](../106-indice-funcional-atual.md), sequência 83–94, [100](../100-criacao-pre-proposta-menu-propostas.md), componentes atuais de Propostas |
| Financeiro | [104](../104-alcance-atual-rotinas-financeiras.md), `FinanceiroPropostaForm.tsx`, `LotesMensaisApolice.tsx`, histórico e casos de uso financeiros |
| Segurança | Formulários de Usuários/Perfis, listagem de Auditoria, Catálogo e Módulos do Sistema em `apps/web/src/features/seguranca` |

As referências técnicas pertencem a este arquivo; as orientações do usuário devem priorizar o que ele precisa compreender e fazer na tela. Não transportar nomes de tabelas, endpoints, migrations ou procedimentos internos para os passos operacionais sem necessidade.

## Situação e Pendências da Edição 0.1

Conteúdo elaborado a partir do código e das referências funcionais disponíveis em 25/11/2026. A homologação autenticada foi iniciada; as correções e verificações parciais estão no [registro 107](../107-homologacao-parcial-manual.md). Esta edição não deve ser identificada como manual homologado.

Antes da aprovação final:

1. Executar os cenários aplicáveis H01–H15 e conferir as instruções com os Perfis de operação e administração.
2. Verificar rótulos, ordem dos comandos e resultados na versão efetivamente implantada.
3. Conferir navegação da edição HTML em desktop e celular, busca, teclado e impressão.
4. Incluir capturas reais com dados fictícios, legenda, texto alternativo e contexto da tarefa, se necessárias após a revisão. Não usar imagens simuladas como evidência da interface.
5. Confirmar endereço de acesso e canal de suporte para a distribuição da organização.
6. Validar o conteúdo com responsáveis das rotinas e registrar versão/data de aprovação.

Cadastros de parceiros e operações gerais da estrutura da Apólice possuem instruções por tarefa sem um inventário completo de todos os campos. Validar esses formulários na homologação antes de ampliar para instruções campo a campo. Não documentar edição completa de dependentes sem confirmar o fluxo entregue.

## Verificações da Entrega

Esta criação é exclusivamente documental; não altera o aplicativo, os acessos ou o banco. Verificar a geração, os links Markdown/HTML, âncoras, sintaxe do JavaScript e `git diff --check`. Os testes do produto já registrados nas entregas anteriores não equivalem à validação visual desta edição.

Resultados da edição 0.1:

- Geração concluída: 10 capítulos, 94 seções, 113 identificadores únicos e 186 links HTML com destinos verificados.
- Conferidos 80 links locais/âncoras nos 11 arquivos Markdown, incluindo este guia.
- Sintaxe Python e JavaScript aprovadas; nenhuma dependência externa de script, estilo ou imagem na edição HTML.
- Verificação funcional do JavaScript em Node com adaptador de DOM e conteúdo real gerado: busca sem acentos e sem distinção de caixa, destino dos resultados, busca vazia/sem resultados, limpeza, alternância do menu, acionamento da impressão e indicação do capítulo por âncora aprovados.
- Essas verificações não renderizam um navegador. Layout responsivo, leitura com tecnologia assistiva, paginação da impressão e comportamento visual permanecem para conferência na etapa de revisão.

Não foram repetidas as suítes de frontend/backend nesta entrega documental.

Continuidade de 26/11/2026: foram incluídas três capturas reais em Cadastros, corrigida a renderização de links dentro de negrito e adicionada a renderização de figuras com texto alternativo e legenda. A navegação e a busca foram conferidas em navegador real nas larguras de desktop e celular, conforme o registro 107. As verificações de produto executadas para as correções de cadastros também estão nesse registro e não alteram as evidências da criação documental original.
