# Estabilização de Clientes e Referências Antigas de Apólices

Data: 21/10/2026. Continuação do [lote de contratos de Usuários e Apólices](33-estabilizacao-contratos-usuarios-apolices.md).

## Resultado e escopo

Corrigidas as divergências de tipos restantes em Clientes e Apólices. A verificação global passou de 26 erros em 13 arquivos para seis erros em quatro arquivos. Nenhum erro de TypeScript permanece nos arquivos de Clientes ou Apólices; a aprovação global ainda está pendente.

Foram alterados oito arquivos de produção, removidos dois arquivos de produção sem consumidores, atualizados quatro arquivos de testes e criados dois testes. Preservado o trabalho local anterior, incluindo o formulário e a seleção de contatos principais de Clientes. Não houve alteração de backend, banco, contratos HTTP publicados, permissões, dependências, tokens visuais ou regras de Title Case.

## Clientes

- **Navegação:** callbacks de Detalhes e Editar em tabela/mobile usam `ClienteListItem['id']`, preservando o UUID textual. O acesso aos detalhes continua enviando o estado de retorno e os filtros da listagem.
- **Detalhe:** `ClienteDetalheResponse.id` passou de `number` para `string`, conforme `ConsultarClienteResult(Guid Id, ...)` no backend. `ClienteEnderecoResponse.cidadeId` aceita `null`, conforme `long? CidadeId` da resposta C#.
- **Edição:** removida a declaração incorreta de endereço como `Record<string, string>`. O mapeamento infere os tipos da resposta existente, mantendo cidade numérica e principal booleano. A ausência de cidade continua sendo tratada como campo opcional no formulário e no envio. Contatos também reutilizam o tipo inferido do contrato.
- **Status:** o hook converte os valores de URL `"1"`/`"2"` para `1`/`2`; vazio ou valor inválido resulta em ausência de filtro. O seletor aplica a mesma conversão na entrada HTML. Foram removidos os casts que declaravam texto como enum numérico sem convertê-lo. A serialização da URL continua textual, preservando os parâmetros existentes e o reinício da paginação ao alterar filtros.

Os campos continuam sendo os já existentes; este lote não amplia cadastro, vínculos ou dependentes. Não foram modificados os normalizadores compartilhados ou as classificações de dados humanos/técnicos.

## Apólices

### Remoção de código sem uso

Removidos:

- `src/features/apolices/components/modals/ModuloSubestipulanteApoliceModal.tsx`.
- `src/features/apolices/schemas/moduloSubestipulanteApolice.schema.ts`.

A busca no frontend e nos testes confirmou que o modal não era importado, exportado por barris ou carregado por rotas. Seu schema só era consumido pelo próprio modal. O arquivo tentava importar duas operações de API e um tipo de resposta que não existem mais.

A página de detalhe atual usa `ModulosTab` e `ModuloApoliceModal`, com vínculo direto à Apólice, conforme o [modelo de Plano e Coberturas](22-plano-coberturas-modulo-apolice.md). Esse fluxo e os registros existentes foram preservados. Não foram recriados endpoints antigos nem removidas tabelas ou relações do banco para eliminar erros de compilação. O Universo anterior continua disponível para consulta histórica.

### Histórico e ajustes menores

- A API de histórico não expõe ID individual do evento. `HistoricoTab` cria uma cópia de apresentação com chave local composta por Apólice, página recebida e posição da linha. `DataTable` recebe um extrator com o contrato existente de um único argumento. Eventos com data/hora iguais permanecem distintos.
- Essa chave é técnica e existe apenas na interface. Nenhum texto ou evidência de auditoria é reformatado, persistido ou acrescentado aos objetos recebidos da API. A suíte usa objetos/coleção congelados para verificar essa preservação.
- Removidos import e leituras de formulário sem uso em `ApoliceVidaFormModal`, mantendo seletores, opções atuais/inativas e reset existentes.
- A resposta simulada de criação de Subgrupo passou a usar `{ subgrupoPublicId }`, conforme o cliente real.
- O teste do Universo Permitido deixou de simular um contexto de autorização que o componente atual não utiliza. Mantém as verificações de consulta histórica e ausência de mutação; nenhuma permissão de produção foi alterada.

## Testes

| Arquivo, relativo a `apps/web` | Cobertura |
|---|---|
| `tests/pages/ClientesNavigation.test.tsx` (novo) | 13 casos: UUID nas ações de tabela/mobile e filtros de retorno nos quatro cenários de tema; edição ausente sem permissão; status numérico/vazio no seletor |
| `src/features/apolices/components/tabs/HistoricoTab.test.tsx` (novo) | Quatro casos de tema: eventos com mesma data/hora, paginação sem chaves duplicadas e evidências originais preservadas |
| `src/features/clientes/hooks/useClientesFilters.test.tsx` | Sete casos novos para valores válidos, ausentes e inválidos de status na URL; casos anteriores atualizados para enum numérico |
| `src/features/clientes/pages/EditarClientePage.test.tsx` | Dois casos novos com endereços, cidade numérica/nula e principal verdadeiro/falso; fixture tipada com UUID; preservado o caso de conflito 409 |
| Testes de Subgrupo e Universo Permitido | Adequação das fixtures ao comportamento e aos contratos atuais |

Os testes novos de navegação e histórico exercitam claro, escuro e sistema (preferências clara e escura). As APIs são simuladas, sem consultas a ambientes publicados ou gravações reais. A suíte verifica DOM, rotas, callbacks, dados enviados e tema resolvido; não substitui homologação visual/contraste ou navegação autenticada em navegador.

## Validações

| Verificação | Resultado |
|---|---|
| `npm.cmd run typecheck` | Seis erros em quatro arquivos; antes, 26 em 13 arquivos |
| `npm.cmd run test:run` | 389 aprovados, zero falhas, 53 arquivos; 26 casos adicionados |
| `npm.cmd run build` | Aprovado, código de saída 0 |
| `npm.cmd run lint` | 96 erros e 7 avisos; antes, 103 erros e 8 avisos; nenhuma ocorrência nova por arquivo/regra |
| Lint dos testes novos e dos testes de filtro/edição de Clientes | Zero erros e avisos |
| `npm.cmd run lint:design-system` | Sete ocorrências preexistentes, sem alteração |
| `npm.cmd run format:check` | 282 arquivos pendentes; antes, 285; sem reformatação geral |
| `git diff --check` | Aprovado |

Relatórios locais em `scratch/clientes-apolices-etapa6-*`, ignorados pelo Git. Backend somente consultado; seus testes não foram executados. Não houve supressões, exclusões de compilação ou relaxamento de tipos. A remoção dos dois arquivos decorre da ausência de consumidores e da substituição comprovada pelo fluxo atual.

## Arquivos da entrega

- Clientes: `ClientesTable`, `ClientesMobileList`, `ClientesFilters`, `useClientesFilters`, `EditarClientePage` e `types/clienteDetalhe.types.ts`.
- Apólices: `HistoricoTab`, `ApoliceVidaFormModal` e os dois arquivos antigos removidos, indicados acima.
- Testes: os dois novos e as quatro suítes atualizadas descritas neste registro.
- Documentação: este registro, estado de Clientes e continuidade nos registros 29 e 33.

## Próximo lote

Atualização de 21/10/2026: os seis erros abaixo foram resolvidos no [fechamento da estabilização de tipos](35-conclusao-estabilizacao-tipos.md). TypeScript e build aprovados, com 389 testes passando. As contagens anteriores e a lista seguinte preservam o registro do planejamento deste lote.

Concluir os seis erros de tipos restantes, sem misturar com a dívida global de lint:

1. `CorretoraFormPage.tsx`: remover a divergência entre o payload informado e a assinatura de alteração.
2. `modulos/api/modulos.api.ts`: alinhar a montagem dos parâmetros ao cliente HTTP.
3. `SeguradorasListPage.test.tsx`: completar os identificadores das fixtures tipadas.
4. `HomePage.tsx`: remover declarações sem uso indicadas pelo compilador.

Depois da aprovação global de tipos, tratar lint/design system e formatação em lotes próprios, seguindo para a homologação dos fluxos autenticados. Não estão concluídas a manutenção funcional de vínculos/dependentes de Clientes ou a migração para UUIDs das referências numéricas da gravação básica de Apólices, documentada no registro 33.
