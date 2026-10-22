# Estabilização do Frontend

Data: 21/10/2026. Primeira entrega: recuperação dos testes. A estabilização geral permanece em andamento.

Estado atual: a [estabilização de tipos está concluída](35-conclusao-estabilizacao-tipos.md), com zero erros de TypeScript, 485 testes aprovados e build aprovado. Após os lotes de [testes](36-estabilizacao-lint-testes-apolices.md) e [tratamento de erros de Apólices](37-estabilizacao-erros-apolices.md), restam 48 erros e quatro avisos no lint global. Permanecem pendentes conformidade do design system, formatação e homologação integrada. As tabelas abaixo preservam o retrato da primeira entrega; resultados posteriores estão no registro de cada lote.

## Escopo

Recuperar a suíte existente, corrigir defeitos de formulário evidenciados pelos testes e registrar uma base verificável. Não implementa regras novas de negócio nem acessa banco, Keycloak ou serviços externos.

`ClienteForm.tsx`, `ClienteDetalhePage.tsx` e o teste não versionado `ClienteForm.test.tsx` já estavam alterados antes desta etapa. Foram preservados. Os testes dos contatos principais também foram executados.

## Correções

- A suíte de `RoleProtectedRoute`, que importava um componente removido, foi substituída por testes de `PermissionProtectedRoute`: carregamento, erro, usuário indisponível/inativo, permissão, módulo habilitado, acesso total e função de operador. Módulo desabilitado bloqueia também acesso total.
- Home, layout e páginas de Clientes declaram a autorização necessária ao cenário. O auxiliar `tests/helpers/authorization.ts` nega acesso por padrão. Não existe concessão global em `tests/setup.ts` nem alteração de permissões de produção. Home ainda exerce seu comportamento atual baseado em roles para o cartão Clientes; sua migração para permissões granulares não integra esta entrega.
- Ramos passou de APIs do Jest para Vitest, preservando os cenários de leitura, permissões e abertura do modal, sem instalar dependências.
- `FormField` passou para dentro do `render` do `Controller`, envolvendo o campo real. Antes, o rótulo e os atributos de erro injetados no Controller não alcançavam o HTML. Corrigidos CNPJ de Seguradora/Subestipulante e CEP, cidade, telefone institucional e início/fim de vigência de Estipulante.
- Regressão verifica rótulos, `aria-invalid`, `aria-describedby` e rejeição de CNPJ inválido. Renderização DOM exercitada em claro, escuro e sistema, incluindo sistema claro/escuro. Não houve alteração de estilos.
- Os testes usam eventos de entrada para máscaras, conferem CNPJ formatado na tela e em dígitos no envio e usam CPF/CNPJ válidos nos cenários de sucesso. As validações de produção não foram flexibilizadas.
- Conflito de Clientes usa `HttpApiError`, conforme o cliente HTTP. Consulta de status delimitada à tabela para não confundir o resultado com a opção do filtro.

## Verificação de tipos

O comando anterior, `tsc --noEmit`, era executado sobre um `tsconfig.json` com `files: []` e referências a outros projetos. Não verificava os projetos referenciados e podia indicar sucesso sem analisar a aplicação.

`npm run typecheck` agora usa `tsc -b --pretty false`, verificando aplicação/testes e configuração do Vite. Reprova com erros reais pendentes de correção. O build do Vite não substitui essa verificação.

## Validações executadas

| Verificação | Resultado |
|---|---|
| Testes iniciais | 179 aprovados, 28 falhando e duas suítes sem carregar |
| `npm.cmd run test:run` | 228 aprovados, zero falhas, 42 arquivos |
| `npm.cmd run build` | Aprovado |
| ESLint dos 16 arquivos TypeScript desta etapa | Zero erros e avisos |
| `npm.cmd run typecheck` corrigido | Reprovado: 284 erros em 92 arquivos |
| `npm.cmd run lint` global | Reprovado: 126 erros e 9 avisos; inicialmente 128 erros e 9 avisos |
| `npm.cmd run lint:design-system` | Reprovado: sete ocorrências em `AsyncSelect.tsx` e `DesignSystemPage.tsx` |
| `npm.cmd run format:check` | Reprovado: 288 arquivos na primeira execução desta etapa |
| `git diff --check` | Aprovado |

No Windows, `npm.cmd` usa o lançador instalado sem mudar a política do PowerShell. Não foram instaladas dependências nem desativadas verificações. Testes em jsdom não constituem homologação visual em navegador ou verificação do ambiente publicado. Backend não alterado; seus testes não foram executados.

Relatórios locais ignorados pelo Git: `scratch/estabilizacao-frontend-*.json` e `.log`. Contagens incluem as alterações locais preexistentes de Clientes.

## Continuidade em etapas

1. **Testes do frontend:** concluídos nesta entrega.
2. **Tipos:** concluídos no [fechamento da estabilização](35-conclusao-estabilizacao-tipos.md), com zero erros no compilador, 389 testes aprovados e build aprovado. Foram mantidas as opções e a abrangência da verificação; os usos de `any` preexistentes seguem como dívida de lint.
3. **Lint e design system:** em andamento. Os lotes [36](36-estabilizacao-lint-testes-apolices.md) e [37](37-estabilizacao-erros-apolices.md) corrigiram a tipagem dos testes de quatro abas e o tratamento de erros de seis hooks e cinco ações de Apólices. Continuar pelos consumidores de API dos demais cadastros; depois, revisar efeitos e dependências. Tratar a formatação separadamente, sem misturar reformatação geral com regras de negócio.
4. **Homologação:** conferir fluxos autenticados e apresentação em navegador nos temas suportados.
5. **Próxima entrega funcional:** completar manutenção de vínculos, dependentes/cônjuge e dados pessoais necessários, levantando as rotinas no legado. Depois, concluir Propostas alinhadas a Apólice → Módulo → Plano → Coberturas. Financeiro vem após essa base.

Referências: [sequência funcional](17-modelagem-banco-dados-webapolice.md#sequência-recomendada-de-evolução), [estado de Clientes](19-modulo-clientes-detalhes.md) e [Planos/Coberturas atuais](22-plano-coberturas-modulo-apolice.md).

## Arquivos da entrega

- Produção: `EstipulanteForm.tsx`, `SeguradoraForm.tsx`, `SubestipulanteForm.tsx` e somente o script `typecheck` em `apps/web/package.json`.
- Testes atualizados: Ramos, cadastro/listagem/edição de Clientes, cadastro/edição de Estipulantes, formulário de Seguradora/Subestipulante, Home e layout autenticado.
- Novos: `PermissionProtectedRoute.test.tsx`, `FormAccessibility.test.tsx` e auxiliar `tests/helpers/authorization.ts`. A suíte antiga `RoleProtectedRoute.test.tsx` foi substituída.
- Documentação: este registro, fundação do frontend, guia de `FormField` e atualização de estado do detalhe de Clientes.

Nenhuma migration, estrutura de banco, classificação de campo humano ou regra de Title Case foi alterada.
