# Revisão compacta do design system

Data: 26/10/2026.

## Resultado

A referência visual enviada pelo usuário orientou a redução da escala de texto, dos controles e dos espaços dos componentes compartilhados. A revisão alcança listagens, detalhes, formulários e catálogo por meio dos tokens e do kit de UI existente.

- Tipografia: base de 14px, tabelas/controles de 13px, seções de 16px e títulos de página de 22px. Utilidades Tailwind e CSS compartilham a mesma família e escala.
- Grids: cabeçalhos menores em maiúsculas, divisores de 1px, células com padding de 8px por 12px e ações de 28px.
- Controles: botões padrão e seletores assíncronos passam a acompanhar os campos de 32px; pequenos usam 28px e grandes usam 40px.
- Superfícies: bordas mais discretas, cantos consistentes, cards sem sombra permanente e pares de rótulo/valor sem caixas individuais.
- Navegação: cabeçalho com mínimo de 56px, sidebar de 208px, itens compactos e tokens inválidos substituídos pelos equivalentes oficiais.
- Clientes: listagem em largura disponível, layout e filtros em Tailwind, rótulos compartilhados e bases flex restritas ao desktop. Os dois arquivos CSS estruturais substituídos foram removidos.
- Menus: superfícies, foco, estados de perigo e sombras usam tokens existentes. A extrapolação dos menus de linha no desktop foi preservada.

As regras de dados, contratos HTTP, classificação de campos humanos, persistência Title Case e permissões não foram alteradas. Não há dependências novas.

## Verificação

Executados em `apps/web`:

| Comando | Resultado |
| :--- | :--- |
| `npm.cmd run format:check` | Aprovado, sem arquivos pendentes |
| `npm.cmd run typecheck` | Aprovado |
| `npm.cmd run lint` | Aprovado, sem erros ou avisos |
| `npm.cmd run lint:design-system` | Aprovado, zero ocorrências |
| `npm.cmd run test:run` | 74 arquivos e 1.139 testes aprovados |
| `npm.cmd run build` | Aprovado |

O build emitiu aviso de chunk acima de 500kB e diagnóstico de tempo no plugin CSS. Nenhuma configuração ou limite foi flexibilizado. A suíte existente foi executada; não foram criados testes que apenas reproduzem propriedades de CSS.

## Limitação de homologação

A inspeção no navegador foi tentada, mas a ferramenta não encontrou navegadores disponíveis e recusou a abertura do navegador integrado (`Browser is not available: iab`). Portanto, esta entrega não declara homologação visual de telas autenticadas, responsividade, zoom, foco ou contraste em navegador real.

Os estilos continuam consumindo os tokens claro/escuro, e os testes existentes incluem a resolução de tema sistema. Isso não substitui a conferência visual dos temas claro, escuro e sistema nos fluxos autenticados, especialmente em tabelas com conteúdo longo e menus de ações.

Medidas oficiais: [Identidade visual e design system](12-identidade-visual-design-system.md). Orientações de composição: [Guia de uso](guias/guia-uso-design-system.md).
