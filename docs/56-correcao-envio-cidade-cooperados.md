# Correção da Captura de Cidade no Envio de Cooperados

Data: 22/10/2026. Investigação da falha intermitente registrada no [lote de formatação dos componentes](55-formatacao-componentes-formulario.md).

## Causa e resultado

Confirmada uma janela entre disponibilizar o campo de cidade e executar o efeito passivo que remove um ID ausente da lista. O campo podia aparecer vazio e habilitado enquanto o React Hook Form ainda guardava o ID retornado pelo CEP. Um envio nesse instante capturava esse ID para validação; a limpeza posterior do estado não alterava a cópia já capturada para o envio.

Foi criado um reprodutor determinístico: `Profiler` observa o commit que habilita o campo, e o teste despacha o evento de envio nesse ponto, antes do efeito passivo. O formulário e a validação são reais, com serviços simulados. A regressão falhou antes da correção, com `cidadeId` indevido no payload, e passou após a alteração. A investigação não se limitou a repetir a suíte até obter sucesso.

Na montagem do payload, `CooperadoForm` passou a conferir se o ID validado pertence à lista de cidades disponível para aquela consulta. Um ID ausente é convertido em zero, que o mapper existente já omite; um ID válido é preservado. O efeito de limpeza visual continua funcionando, mas deixa de ser a única garantia de consistência do envio.

O handler também bloqueia envio durante carregamento externo, consulta de CEP ou consulta de cidades, complementando o botão desabilitado. Depois da conclusão, o usuário pode enviar novamente. Não foram alterados o schema, tipos, props, mapper, API, banco, permissões, dependências, mensagens ou regras de Title Case. Não foram introduzidos `any`, casts para esconder contrato, supressões ou relaxamentos de checks. As alterações locais anteriores foram preservadas.

## Validações

| Verificação | Resultado |
|---|---|
| Reprodutor determinístico antes da correção | Uma falha: envio com cidade inexistente |
| Mesmo reprodutor após a correção | Aprovado |
| Testes direcionados de cidade, assinaturas, CEP e contratos | 88 aprovados, zero falhas, quatro arquivos; 14 novos e 74 anteriores |
| `npm.cmd run test:run` | 1.125 aprovados, zero falhas, 74 arquivos; antes, 1.111 testes |
| `npm.cmd run typecheck` | Aprovado: zero erros, saída 0 |
| `npm.cmd run build` | Aprovado, saída 0 |
| `npm.cmd run lint` | Aprovado: zero erros e avisos |
| `npm.cmd run lint:design-system` | Aprovado: zero ocorrências |
| `npm.cmd run format:check` global | Reprovado: 246 arquivos pendentes, mesma contagem anterior |
| Prettier do teste alterado | Aprovado; preservada a formatação preexistente do formulário |
| Comparação de schema, tipos e props do formulário | Preservados |
| `git diff --check` | Aprovado |

Os 14 testes novos foram acrescentados a `CooperadoCidade.test.tsx`: 12 exercitam o envio no primeiro commit disponível para lista vazia, ID fora da lista e ID válido, nos temas claro, escuro e sistema com preferência clara/escura. Dois despacham envio com CEP/cidades ainda pendentes, conferem ausência de callback e verificam uma nova tentativa bem-sucedida após a conclusão. O cenário original de cidade vazia e suas asserções foram mantidos.

Nenhum teste foi ignorado ou marcado como falha esperada. Após a correção, o reprodutor, os testes ampliados e a suíte completa passaram. Os testes usam DOM e serviços simulados; não substituem homologação visual/autenticada. Backend não alterado; seus testes não foram executados. Nenhuma apresentação, cor ou token foi alterado neste lote funcional.

Relatórios locais ignorados pelo Git: `scratch/lint-etapa28-*`, incluindo retrato anterior, regressão antes/depois, testes direcionados/completos e logs dos checks.

## Arquivos e continuidade

- Produção: `apps/web/src/features/cooperados/components/CooperadoForm.tsx`, somente o handler de envio.
- Testes: `apps/web/tests/forms/CooperadoCidade.test.tsx`.
- Documentação: este registro e continuidade nos documentos 29 e 55.

A formatação de `Checkbox`, `Textarea` e `components/fields` foi concluída no [documento 57](57-formatacao-checkbox-textarea-campos.md), com 11 pendências removidas e 235 arquivos restantes no global. Os números acima preservam o retrato da correção de cidade. Próximo lote: `FormSection`, `FormGrid`, `FormActions` e `ReadOnlyField`, sem mudanças funcionais. A homologação integrada continua no [plano de estabilização](29-estabilizacao-frontend.md).
