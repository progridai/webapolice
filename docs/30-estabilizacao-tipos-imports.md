# Estabilização de Tipos — Imports e Exportações

Data: 21/10/2026. Continuação da [recuperação dos testes](29-estabilizacao-frontend.md). Este lote conclui as correções de imports/exportações identificadas no diagnóstico; a aprovação global de tipos continua pendente.

## Escopo

Alterações pontuais em 31 arquivos TypeScript, mantendo os arquivos e as opções de verificação do compilador. Não há mudança de schema, contrato HTTP, regras de autorização, cálculos de negócio, layout ou estilos. As alterações anteriores da estabilização e o trabalho local de Clientes foram preservados.

## Correções realizadas

- **React:** `ReactNode` declarado como import de tipo em `AuthorizationProvider`, `DataTable`, `DropdownMenu`, `FilterBar` e `RowActions`, conforme `verbatimModuleSyntax`.
- **Autenticação:** o barril `auth/index.ts` passou a exportar os tipos existentes `AuthContextValue`, `AuthUser`, `AuthStatus` e `AppRole`. As referências a `UserProfile` e `RoleEnum` não existiam na origem e não tinham consumidores no repositório. Nenhuma role ou permissão foi criada ou alterada.
- **Segurança:** `useModulos` reutiliza o `ModuloDto` já declarado e retornado por `api/modulosApi.ts`, evitando importar um tipo inexistente ou duplicar o contrato. Isso recupera também a inferência dos recursos na tela de módulos.
- **Corretoras:** corrigido o caminho relativo para `types/corretora.types.ts`. O compilador passou a detectar cinco divergências adicionais nos consumidores desse contrato antes ocultadas pela importação inválida; permanecem para o lote de contratos.
- **Testes:** imports explícitos de `beforeEach`, `describe`, `expect` e `it` em `ModulosTab.test.tsx` e `SubgruposTab.test.tsx`. O runtime Vitest não foi substituído e a configuração não foi relaxada.
- **Limpeza de imports:** removidos imports não utilizados em campos, rotas, testes e páginas indicados pelo compilador. Nos quatro campos com máscara, removido somente o parâmetro não utilizado `mask` dos callbacks `onAccept`, preservando valor, evento e normalização. Removida uma supressão de lint que ficou desnecessária após retirar o import `act` não utilizado de `useClientes.test.ts`.

## Validações

| Verificação | Resultado |
|---|---|
| `npm.cmd run typecheck` antes do lote | 284 erros em 92 arquivos |
| `npm.cmd run typecheck` após os imports | 180 erros em 67 arquivos; redução líquida de 104 |
| Categorias eliminadas neste lote | `TS1484`, `TS2305`, `TS2307`, `TS2593`, `TS2304` e `TS6196` |
| `npm.cmd run test:run` | 228 aprovados, zero falhas, 42 arquivos |
| `npm.cmd run build` | Aprovado; aviso de tempo gasto em plugins do build |
| `npm.cmd run lint` | 116 erros e 9 avisos; antes deste lote, 126 erros e 9 avisos |
| `npm.cmd run lint:design-system` | Sete ocorrências pendentes, sem alteração em relação à etapa anterior |
| `npm.cmd run format:check` | Problemas de formatação em 286 arquivos; nenhuma reformatação geral aplicada |
| `git diff --check` | Aprovado |

Os relatórios locais ficam em `scratch/tipos-etapa2-*`, ignorados pelo Git. As validações são locais, com APIs substituídas nos testes, sem consulta ao ambiente publicado. Não foram instaladas dependências, criados novos testes para mudanças apenas de imports, excluídos arquivos da compilação ou adicionadas supressões de tipos. Backend e banco não foram alterados nem tiveram testes executados neste lote.

## Próximo lote

Atualização de 21/10/2026: os contratos de componentes foram tratados no [lote seguinte](31-estabilizacao-componentes.md), reduzindo os erros de 180 para 82. As contagens acima registram o encerramento do lote de imports. Seguir agora com formulários e contratos da API.

O próximo escopo previsto ao encerrar este lote era corrigir os contratos dos componentes e seus consumidores, seguindo as propriedades efetivamente implementadas no design system. Essa parte foi concluída no registro 31. Tratar agora entrada/saída dos schemas Zod nos formulários e os DTOs reais da API. Manter as validações e permissões vigentes e testar qualquer correção com efeito em comportamento.

No diagnóstico deste lote constavam propriedades antigas de botões, alertas, paginação e rotas protegidas, posteriormente corrigidas no registro 31. Permanecem formulários com tipos de entrada/saída diferentes, identificadores e formatos de DTOs e referências da hierarquia anterior de apólices. Essas pendências não foram mascaradas por aliases genéricos, `any` ou exclusão de arquivos.

## Arquivos e grupos alterados

- `src/auth/AuthorizationProvider.tsx` e `src/auth/index.ts`.
- Tipos de React em `DataTable`, `DropdownMenu`, `FilterBar` e `RowActions`; import não utilizado em `AsyncSelect`.
- `CepInput`, `CnpjInput`, `CpfInput`, `PhoneInput`, `DateInput` e `EmailInput`.
- `useModulos`, `corretoras.api.ts` e import de tipo não utilizado em `estipulantes.api.ts`.
- Imports não utilizados de `ApoliceForm`, `VidasTab`, `EstipulanteDetalhePage`, `EstipulantesListPage` e rotas de Apólices, Clientes, Cooperados, Estipulantes, Segurança e Módulos Globais.
- Testes de Módulos/Subgrupos/Subestipulantes da Apólice, listagem de Subestipulantes e hook `useClientes`.
- Documentação: este registro e a continuidade em `docs/29-estabilizacao-frontend.md`.
