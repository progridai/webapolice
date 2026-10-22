# Estabilização de Lint dos Testes de Apólices

Data: 21/10/2026. Primeiro lote de lint após a [conclusão da estabilização de tipos](35-conclusao-estabilizacao-tipos.md).

## Resultado

Removidas 34 ocorrências de `any` em quatro arquivos de testes das abas de Apólices: Módulos, Subgrupos, Subestipulantes e Vidas. Esses arquivos passaram no ESLint sem erros ou avisos. O lint global passou de 94 para 60 erros, mantendo sete avisos; as ocorrências de `no-explicit-any` caíram de 66 para 32. Não foram introduzidas ocorrências novas por arquivo/regra.

TypeScript continua com zero erros, o build foi aprovado e os 389 testes passaram em 53 arquivos. O lint global e a estabilização geral continuam pendentes.

## Correções

- Os mocks de hooks e funções de API usam `vi.mocked`, preservando os tipos das implementações reais. As fixtures de Módulos, Subestipulantes e Vidas reutilizam os DTOs existentes; os dados inline de Subgrupos são verificados pelo retorno tipado do hook.
- A autorização simulada usa `makeAuthorization`, que fornece o contrato completo e nega acesso por padrão. Os cenários de Vidas declaram somente as três permissões de mutação exercitadas; os testes de ausência de permissão continuam verificando a ocultação das ações. Os outros grupos mantêm suas simulações específicas de permissões.
- Os retornos simulados dos hooks incluem os estados de erro e as funções assíncronas de atualização. `refetch` e `retry` passam a resolver `Promise<void>`, conforme o contrato real. O reset de `refetch` em Subestipulantes restaura também essa implementação.
- As fixtures de Módulos em Vidas incluem `moduloPublicId`, distinto do `publicId` do vínculo. Os testes de Módulos continuam verificando que a inativação utiliza o identificador do vínculo.
- Os exemplos de observação ausente em Subgrupos e de fim de vigência ausente em Subestipulantes usam `undefined`, conforme os opcionais dos DTOs atuais do frontend. Não foi alterada a definição de nulabilidade das APIs. O envio de `null` ao remover vínculos de Vidas continua coberto pelos testes existentes.
- O mock de `ClienteAsyncSelect` deriva as propriedades utilizadas do componente real. Os filtros de elementos DOM aproveitam a inferência do Testing Library, dispensando `any`.

Este lote altera somente testes e documentação. Não houve mudanças em componentes de produção, regras de negócio, banco, backend, permissões reais, estilos ou padronização Title Case. Nenhum campo humano foi criado. Não foram adicionadas dependências, supressões de lint ou flexibilizações da compilação. As alterações locais anteriores foram preservadas.

## Validações

| Verificação | Resultado |
|---|---|
| ESLint dos quatro arquivos alterados | Zero erros e avisos |
| Testes das quatro abas | 34 aprovados, zero falhas |
| `npm.cmd run test:run` | 389 aprovados, zero falhas, 53 arquivos |
| `npm.cmd run typecheck` | Aprovado: zero erros, código de saída 0 |
| `npm.cmd run build` | Aprovado, código de saída 0 |
| `npm.cmd run lint` | 60 erros e 7 avisos; antes, 94 erros e 7 avisos |
| `npm.cmd run lint:design-system` | Sete ocorrências preexistentes em `AsyncSelect.tsx` e `DesignSystemPage.tsx` |
| `npm.cmd run format:check` | 278 arquivos pendentes; antes, 282; formatados somente os quatro testes deste lote |
| `git diff --check` | Aprovado |

Os 34 testes existentes do lote foram mantidos, incluindo suas asserções de autorização, estados vazios/erro/carregamento, abertura de modais, inativação, participações repetidas do mesmo cliente e seleção independente de vínculos. Não foram acrescentados testes para as mudanças de tipagem. As verificações são locais, com serviços simulados, sem operações em ambiente publicado. Não constituem homologação integrada ou visual em navegador; os testes de backend não foram executados.

Relatórios locais ignorados pelo Git em `scratch/lint-etapa8-*`. O comparativo de lint usa o relatório do fechamento de tipos, `scratch/tipos-etapa7-lint.json`.

## Arquivos alterados

Caminhos relativos a `apps/web`:

- `src/features/apolices/components/tabs/ModulosTab.test.tsx`.
- `src/features/apolices/components/tabs/SubgruposTab.test.tsx`.
- `src/features/apolices/components/tabs/SubestipulantesTab.test.tsx`.
- `src/features/apolices/components/tabs/VidasTab.test.tsx`.

Documentação: este registro e continuidade nos documentos 29 e 35.

## Próxima etapa

Atualização de 21/10/2026: o [lote de tratamento de erros de Apólices](37-estabilizacao-erros-apolices.md) concluiu a correção dos 11 usos de `any` no código de produção do domínio, com 485 testes aprovados, TypeScript e build aprovados. O lint global passou a 48 erros e quatro avisos. Os resultados acima preservam o encerramento deste primeiro lote de lint.

Continuar pelos usos de `any` no tratamento de erros dos hooks e consumidores de API, começando por Apólices. Reutilizar os contratos e auxiliares de erro existentes, preservando mensagens, carregamento e atualização após operações. Os efeitos e suas dependências exigem revisão própria de comportamento; não basta remover a sinalização do lint.

Conformidade do design system, formatação geral e homologação integrada continuam como etapas separadas. A sequência funcional de Clientes, Propostas e Financeiro permanece conforme o [plano de estabilização](29-estabilizacao-frontend.md).
