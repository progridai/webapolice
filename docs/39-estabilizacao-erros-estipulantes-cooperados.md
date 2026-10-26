# Estabilização dos Erros de Estipulantes e Cooperados

Data: 22/10/2026. Continuação do [lote dos formulários de cadastros](38-estabilizacao-erros-cadastros.md).

## Resultado

Corrigido o tratamento de erros em oito arquivos de produção de Estipulantes e Cooperados: consultas, criação, edição e consulta de CEP. Removidos nove usos de `any`, dois casts de erros sem verificação e um cast de mensagens de validação que já possuíam tipo definido. O lint global passou de 42 para 33 erros, mantendo quatro avisos, sem novas ocorrências por arquivo/regra. Restam seis apontamentos de `no-explicit-any`, nos componentes de campos e seleção.

Adicionados 49 cenários de regressão. A suíte completa passou com 598 testes em 59 arquivos; TypeScript e build aprovados. As alterações locais do lote anterior foram preservadas.

## Comportamento corrigido

- **Cadastro de Estipulante:** conflitos são identificados por `HttpApiError.isConflict()`. A mensagem normalizada da API é preservada; conflito sem mensagem mantém o texto de CNPJ já cadastrado. Outros erros sem mensagem usam a contingência existente, sem acessar propriedades de `null` ou objetos fora do contrato.
- **Edição de Estipulante:** o 404 do cadastro é identificado pelo erro HTTP real, em lugar de converter a falha em uma mensagem sentinela `NOT_FOUND`. O 404 apenas da configuração continua sendo tolerado. Conflitos de Pessoa compartilhada preservam a mensagem da API e sua contingência específica. Os contatos aproveitam os tipos da resposta da API, dispensando dois `any` nos mapeamentos.
- **Consulta e detalhe de Estipulante:** o hook preserva `Error`/`HttpApiError`, incluindo metadados, e converte rejeições sem contrato em erro com mensagem de contingência. O detalhe reconhece 403/404 pelos métodos do erro HTTP, sem cast. Outras falhas de configuração não são confundidas com configuração ausente. Cancelamento não aparece como erro nem mantém o indicador de carregamento ativo; a nova tentativa continua disponível.
- **Cooperados:** criação usa `HttpApiError.isValidationError()` e `getValidationErrors()` para os erros de campo de 400/422. A edição preserva mensagens de instâncias de `Error`, incluindo o erro HTTP, sem depender de `err.name` ou `response.data`. Rejeições sem mensagem mantêm as contingências. As consultas de edição/detalhe mantêm seus textos genéricos atuais e encerram o carregamento após falha.
- **CEP do Cooperado:** 404 normalizado apresenta `CEP não encontrado`; outras falhas permitem preenchimento manual. Cancelamentos por `AbortError`, `CanceledError`, código normalizado `ERR_CANCELED` ou sinal abortado não apagam o endereço nem apresentam indisponibilidade. Respostas e falhas de consultas abortadas não substituem dados da consulta atual nem encerram seu indicador. A limpeza da consulta encerra o indicador ao apagar/trocar o CEP.

O cliente HTTP central continua responsável pela normalização das falhas, conforme a ADR 010. Não houve mudanças de endpoints, payloads, permissões reais, banco, validação de CPF/CNPJ ou classificação de campos humanos. A padronização Title Case e as evidências de auditoria foram preservadas. Não foram adicionados dependências, supressões ou ajustes de configuração para obter aprovação.

## Validações

| Verificação | Resultado |
|---|---|
| Testes direcionados do lote | 58 aprovados, zero falhas, cinco arquivos |
| `npm.cmd run test:run` | 598 aprovados, zero falhas, 59 arquivos |
| `npm.cmd run typecheck` | Aprovado: zero erros, código de saída 0 |
| `npm.cmd run build` | Aprovado, código de saída 0 |
| `npm.cmd run lint` | 33 erros e 4 avisos; antes, 42 erros e 4 avisos |
| `npm.cmd run lint:design-system` | Sete ocorrências preexistentes em `AsyncSelect.tsx` e `DesignSystemPage.tsx` |
| `npm.cmd run format:check` | 278 arquivos pendentes, sem alteração da contagem |
| `git diff --check` | Aprovado |

Os novos testes cobrem:

- `EstipulantesCooperadosErrors.test.tsx`: 33 cenários com páginas, formulários e roteador reais. Incluem conflitos em criação/edição nos temas claro, escuro e sistema (claro/escuro), rejeições com `null`, contingências dos conflitos de Estipulantes, validação 400/422 de Cooperados, configuração opcional 404, cadastro inexistente, acesso negado, recuperação da consulta e falhas de carregamento. Verificam conservação de campos, liberação da ação, navegação somente após sucesso e payloads/identificadores, incluindo contatos de Estipulante.
- `useEstipulanteDetalhe.test.tsx`: oito cenários de preservação de erros, conversão de rejeições inesperadas, falha da configuração, retry e cancelamento como `Error` ou `DOMException`.
- `CooperadoCepErrors.test.tsx`: oito cenários de 404, indisponibilidade, rejeição com `null`, três formatos de cancelamento, limpeza do CEP durante consulta e descarte de resultados/falhas tardias enquanto outra consulta permanece ativa.

As fixtures antigas de conflito e 404 nas páginas de Estipulantes passaram a usar `HttpApiError`. O teste de contingência da criação usa mensagem vazia, mantendo sua asserção genérica. A primeira execução dos novos testes detectou seis expectativas incorretas para o ramal opcional dos contatos institucionais: o formulário existente envia texto vazio. Corrigida a expectativa, sem alterar a normalização do formulário, os testes finais passaram.

As verificações de tema conferem o DOM e alertas semânticos; não substituem homologação visual em navegador. As APIs foram simuladas, sem operações no ambiente publicado. Backend não alterado; seus testes não foram executados. Relatórios ignorados pelo Git em `scratch/lint-etapa11-*`, com retrato inicial dos arquivos de produção e comparativo inicial/final de lint.

## Arquivos alterados

Caminhos relativos a `apps/web`:

- Estipulantes: `src/features/estipulantes/hooks/useEstipulanteDetalhe.ts` e páginas `CadastrarEstipulantePage.tsx`, `EditarEstipulantePage.tsx` e `EstipulanteDetalhePage.tsx`.
- Cooperados: `src/features/cooperados/components/CooperadoForm.tsx` e páginas `NovoCooperadoPage.tsx`, `EditarCooperadoPage.tsx` e `CooperadoDetalhePage.tsx`.
- Testes novos: `tests/pages/EstipulantesCooperadosErrors.test.tsx`, `src/features/estipulantes/hooks/useEstipulanteDetalhe.test.tsx` e `src/features/cooperados/components/CooperadoCepErrors.test.tsx`.
- Testes atualizados: `src/features/estipulantes/pages/CadastrarEstipulantePage.test.tsx` e `EditarEstipulantePage.test.tsx`.

Documentação: este registro e continuidade nos documentos 29 e 38. A formatação ficou restrita aos testes novos; os arquivos existentes receberam alterações locais ao escopo do lote.

## Próxima etapa

Atualização de 22/10/2026: o [lote de Segurança](40-estabilizacao-erros-seguranca.md) corrigiu o tratamento de erros de gravação das quatro páginas de Usuários e Perfis. A suíte passou a 631 testes aprovados, com TypeScript e build aprovados; o lint global permanece com 33 erros e quatro avisos. Os resultados acima preservam o encerramento do lote de Estipulantes e Cooperados.

Revisar o tratamento de erros das páginas de Segurança, principalmente cadastro/edição de usuários e perfis que ainda acessam o formato antigo de erro por casts. Depois, concluir a tipagem dos seis usos de `any` sinalizados nos campos/seletores e revisar efeitos, dependências e concorrência em lote próprio.

Permanecem pendentes design system, formatação geral e homologação integrada, conforme o [plano de estabilização](29-estabilizacao-frontend.md).
