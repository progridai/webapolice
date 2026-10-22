# Estabilização de Tipos dos Formulários

Data: 21/10/2026. Continuação do [lote de consumidores de componentes](31-estabilizacao-componentes.md).

## Resultado e escopo

Alinhados os tipos de entrada e saída dos formulários de Clientes, Estipulantes, Cooperados, Módulos e vínculo de Ramos da Apólice. A verificação global de tipos passou de 82 erros em 28 arquivos para 57 erros em 20 arquivos. Os cinco formulários deste lote não apresentam mais erros de compilação; a estabilização global ainda está pendente.

Foram alterados sete arquivos de produção e criado um arquivo de testes. As alterações locais anteriores foram preservadas. Não houve alteração de endpoints, payloads publicados, backend, banco, permissões, dependências, tokens visuais ou regras de Title Case. Não foram criados campos de negócio nem reduzida a abrangência do compilador.

## Decisões de tipagem

O estado de React Hook Form deve representar `z.input<typeof schema>`, enquanto o callback de envio recebe `z.output<typeof schema>`. Uma seleção HTML pode conter texto, como `"3509502"`, antes da conversão; o endereço validado deve conter o número `3509502`. Campos com `default` podem estar ausentes na entrada e presentes na saída. Esses contratos são distintos e não devem ser forçados com casts de resolver ou com `any`.

- **Cinco formulários:** `useForm<Entrada, unknown, Saida>` com o resolver existente. O contexto não utilizado é `unknown`. Os schemas continuam sendo a origem dos tipos, sem interfaces locais duplicando seus campos.
- **Conversões numéricas:** declaradas entradas `string | number` nas coerções usadas por controles HTML e valores iniciais. Essa declaração só afeta os tipos; não muda a execução da coerção do Zod.
- **Contatos de Clientes e Estipulantes:** valor ausente tem padrão `''` na saída validada. Os contatos vazios continuam sendo filtrados antes do callback, como já ocorria. Contatos enviados têm valor textual, compatível com as requisições existentes. Removidas as asserções desnecessárias na montagem das coleções de Clientes.
- **Linha de endereço de Cliente:** `register`, `control` e `setValue` usam o tipo de entrada do formulário; os erros representam apenas o endereço da linha, podendo estar ausentes. `setValue` deixou de aceitar `any`.
- **Cooperados:** o mapper reutilizado recebe e devolve `CooperadoFormData`, sem retorno `any` nem cast no consumidor. A remoção de campos vazios/IDs zero e a normalização existente foram mantidas. A asserção de chaves em `Object.keys` se limita às propriedades do objeto local copiado; não converte valores ou contratos HTTP.

## Correções de comportamento

- Ao trocar pessoa jurídica por física, o tipo selecionado é comparado como número para escolher a máscara correta de CPF. A validação do documento e a normalização anteriores continuam em vigor.
- A consulta de CEP pode devolver logradouro e bairro nulos. Esses campos passam a receber `''` em Clientes, preservando um valor válido no estado do formulário.
- A composição de `Controller` e `FormField` foi corrigida no seletor Cidade da linha de endereço de Clientes e no status do Módulo, associando os atributos ao controle HTML.
- O tipo de Cooperado é validado como `1 | 2`, conforme o contrato e as opções existentes. Além dos limites anteriores, o schema rejeita valores fracionários como `1.5`, que antes passavam por `min(1).max(2)`. Isso não altera as opções disponíveis ao usuário nem o contrato da API.
- Removido o log de depuração que imprimia o objeto completo validado de Cooperado.

As demais regras de documentos, contatos, datas, obrigatoriedade de coordenador e limites de IOF foram preservadas. O IOF vazio continua sendo convertido para zero pelo schema existente; este lote não redefine essa regra.

## Testes

Novo arquivo: `apps/web/tests/forms/FormSchemaContracts.test.tsx`, com 30 casos usando os componentes reais e substituindo somente consultas/APIs e autorização.

- 20 casos de envio nos temas claro, escuro e sistema (preferências clara e escura): números de seletores, contatos filtrados, documentos normalizados, descrição transformada, estado booleano e IOF decimal.
- Dez casos adicionais: troca da máscara CPF/CNPJ; bloqueio de CPF e endereço inválidos; CEP com campos nulos; vigência final inválida; coordenador obrigatório; tipo fracionário rejeitado; IOF zero/vazio e limites inferior/superior.

Os testes conferem callbacks/payloads, bloqueio do envio inválido, tema resolvido e associação acessível das mensagens aos campos. A suíte não substitui homologação visual, verificação de contraste ou fluxo autenticado em navegador.

## Validações

| Verificação | Resultado |
|---|---|
| `npm.cmd run typecheck` | 57 erros em 20 arquivos; antes, 82 em 28 arquivos |
| `npm.cmd run test:run` | 321 aprovados, zero falhas, 47 arquivos; antes, 291 em 46 arquivos |
| `npm.cmd run build` | Aprovado, código de saída 0; aviso de tempo gasto em plugins |
| `npm.cmd run lint` | 111 erros e 9 avisos; antes, 116 erros e 9 avisos |
| Lint do teste novo | Zero erros e avisos |
| `npm.cmd run lint:design-system` | Sete ocorrências preexistentes, sem alteração |
| `npm.cmd run format:check` | 286 arquivos pendentes, sem alteração na contagem; teste novo formatado |
| `git diff --check` | Aprovado |

Relatórios locais em `scratch/formularios-etapa4-*`, ignorados pelo Git. As verificações são locais, sem consulta ao ambiente publicado ou envio de operações reais. Backend não alterado; seus testes não foram executados. Os erros globais restantes não foram ocultados por supressões ou exclusões de arquivos.

## Arquivos da entrega

- `src/features/clientes/components/ClienteForm.tsx`.
- `src/features/estipulantes/components/EstipulanteForm.tsx`.
- `src/features/cooperados/components/CooperadoForm.tsx` e `utils/cooperados.mappers.ts`.
- `src/features/modulos/components/ModuloForm.tsx` e `schemas/modulo.schema.ts`.
- `src/features/apolices/components/tabs/ApoliceRamoFormModal.tsx`.
- `tests/forms/FormSchemaContracts.test.tsx` (novo).
- Documentação: este registro, continuidade dos registros 29 e 31 e orientação de tipagem no guia do design system.

Os caminhos de código acima são relativos a `apps/web`.

## Próximo lote

Atualização de 21/10/2026: os [contratos de Usuários e consumidores de Apólices](33-estabilizacao-contratos-usuarios-apolices.md) foram tratados no lote seguinte, com 26 erros de tipos restantes e 363 testes aprovados. As contagens anteriores registram o fechamento do lote de formulários; a lista abaixo preserva o escopo planejado naquele momento.

1. Separar corretamente os contratos de criação e edição de Usuários, incluindo os campos de senha temporária exigidos apenas na criação, preservando as validações atuais e o fluxo de identidade.
2. Alinhar os seletores de entidades de Apólices e os consumidores aos DTOs e parâmetros efetivos da API. Conferir identificadores e propriedades de apresentação, sem reintroduzir contratos da hierarquia antiga.
3. Resolver as demais referências antigas e fixtures indicadas pelo compilador; depois, tratar lint, cores fixas e formatação em lotes próprios.

Não estão concluídos os contratos dinâmicos de Usuários, os DTOs/consultas de Apólices, os callbacks de identificadores de Clientes ou o mapeamento tipado de endereços na página de edição de Clientes. O detalhe das falhas restantes continua no relatório de tipos. Este lote também não implementa novos campos ou rotinas de negócio do legado.
