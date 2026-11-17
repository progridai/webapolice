# Organização do Menu Principal

Data: 17/11/2026.

## Comportamento

O menu principal apresenta Início isolado no topo e os demais acessos agrupados
por área de trabalho. A operação diária aparece primeiro; cadastros de apoio,
configurações e administração vêm em seguida. Os links ficam sempre visíveis,
sem submenus ou grupos recolhíveis nesta entrega.

| Grupo | Ordem dos acessos |
| --- | --- |
| Início, sem título adicional | Início |
| Operação | Clientes → Apólices → Propostas |
| Parceiros e Estrutura | Seguradoras → Corretoras → Estipulantes → Subestipulantes → Cooperados |
| Configurações do Seguro | Ramos → Coberturas → Módulos Globais → Convênios de Cobrança |
| Segurança | Usuários → Perfis → Catálogo de Permissões → Auditoria |
| Sistema | Módulos do Sistema → Design System |

A sequência Clientes → Apólices → Propostas acompanha o acesso ao cliente,
à participação na apólice e à proposta criada a partir da Vida. Estipulantes e
Subestipulantes ficam próximos para facilitar a localização dos cadastros
relacionados.

Os rótulos anteriores Catálogo e Módulos passam a ser Catálogo de Permissões e
Módulos do Sistema. Módulos Globais mantém sua identificação de cadastro de
negócio, distinto da habilitação técnica dos módulos do sistema.

## Apresentação e Acessibilidade

Cada grupo tem um título discreto, uma lista com nome acessível e um divisor
superior fino, exceto o primeiro. Grupos vazios são removidos após a filtragem
de acesso, incluindo seu título e divisor. Início não repete seu texto como
título visual.

Os estilos reutilizam os tokens existentes de texto secundário, borda,
espaçamento e tipografia. Permanecem os estados de hover, foco e seleção dos
links. Não há nova paleta ou token. Os títulos podem quebrar linha na largura
disponível.

AppSidebar e MobileNavigation continuam consumindo o mesmo AppNavigation;
portanto, compartilham ordem, nomes e regras de visibilidade. Navegar no menu
mobile continua fechando o painel. Escape e os demais controles do painel
permanecem no componente existente.

## Contratos e Segurança

Rotas e critérios de módulo, recurso, permissão, Acesso Total, operador do
sistema e flag de Design System permanecem os mesmos. Em especial, Propostas
continua exigindo módulo APOLICES e recurso PROPOSTAS disponíveis mesmo para
Acesso Total. Sistema permanece exclusivo do operador.

Não foram criados campos, APIs, permissões ou módulos habilitáveis. Os grupos
são apenas organização visual. Não há alteração de persistência, classificação
Title Case, dados históricos ou auditoria.

## Validação

Os testes de navegação cobrem a ordem dos grupos e links nos temas claro,
escuro e sistema, grupos vazios, filtragem parcial, restrições de Propostas,
operador, flag de Design System, rota ativa e retorno de navegação. O teste do
layout confere a equivalência entre desktop e mobile e o fechamento ao navegar.

Resultados finais em `apps/web`:

| Check | Resultado |
| --- | --- |
| `npm.cmd run format:check` | Aprovado; nenhum arquivo pendente |
| `npm.cmd run typecheck` | Aprovado; zero erros |
| `npm.cmd run lint` | Aprovado; zero erros ou avisos |
| `npm.cmd run lint:design-system` | Aprovado; zero ocorrências |
| `npm.cmd run test:run -- --maxWorkers=2` | Aprovado; 1.313 testes em 86 arquivos |
| `npm.cmd run build` | Aprovado |

O build mantém o aviso preexistente de chunk acima de 500 kB e apresentou aviso
informativo de tempo gasto nos plugins CSS/HTML. Não foram alterados limites ou
configurações para ocultar avisos. A comparação com HEAD confirmou a preservação
das 19 rotas, ícones, metadados de autorização e regras de visibilidade. A
verificação `git diff --check` também passou.

A primeira execução encontrou um ajuste de tipagem no mock da flag de ambiente,
corrigido sem alterar o contrato readonly de ENV. A suíte também apresentou uma
falha no teste existente de EncerramentoProposta: a expectativa de onBusy(false)
foi verificada enquanto a última chamada ainda era onBusy(true), após a
confirmação assíncrona. A mesma falha se repetiu em outro tema. O teste agora
aguarda a liberação assíncrona com waitFor, preservando a expectativa original
e sem alterar código de produção de Propostas.

A segunda execução completa acumulou timeouts em testes de diferentes áreas e
foi interrompida. A execução final usou `npm.cmd run test:run -- --maxWorkers=2`
para reduzir o consumo simultâneo de recursos. Nenhum teste, timeout, escopo,
configuração ou critério de aprovação foi flexibilizado.

A homologação visual e dos fluxos autenticados em navegador permanece pendente:
o inventário de navegadores estava vazio e a tentativa de abrir o navegador
integrado retornou indisponibilidade. Testes em jsdom não substituem essa etapa.

## Arquivos

- `apps/web/src/layouts/AuthenticatedLayout/AppNavigation.tsx`: agrupamentos,
  ordem, rótulos e remoção de grupos vazios.
- `apps/web/src/layouts/AuthenticatedLayout/AuthenticatedLayout.css`: títulos
  e divisores com tokens existentes.
- `apps/web/tests/layouts/AppNavigation.test.tsx`: cobertura funcional do menu.
- `apps/web/tests/layouts/AuthenticatedLayout.test.tsx`: paridade mobile/desktop.
- `apps/web/src/features/propostas/components/EncerramentoProposta.test.tsx`:
  espera pelo efeito assíncrono de liberação do processamento.
- Este documento: regras, validações e limitações da entrega.
