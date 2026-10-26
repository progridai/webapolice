# Correção da Seleção de Cidade por CEP em Cooperados

Data: 22/10/2026. Continuação da [revisão das assinaturas dos formulários](51-estabilizacao-assinaturas-formularios.md).

## Resultado

Corrigida a seleção automática da cidade após consultar o CEP em Cooperados. Antes, o ID era atribuído ao select não controlado antes de suas opções estarem disponíveis: o formulário podia conter um ID enquanto o campo permanecia visualmente vazio. Agora o campo usa `Controller`, mantém o valor controlado e seleciona a opção correspondente quando a lista da UF chega.

O reprodutor com lista de cidades pendente falhou nos quatro modos de tema antes da correção, com valor vazio em vez do ID esperado. O mesmo cenário passou depois, incluindo a conferência do ID numérico enviado. A suíte completa passou de 1.077 para 1.101 testes aprovados, em 73 arquivos. TypeScript, build e lint aprovados, com zero erros e avisos de ESLint.

## Comportamento

- **Consulta por UF:** reutilizado `useConsulta` para carregar cidades da UF atual. Trocar/limpar a UF ou desmontar aborta a operação anterior, esconde suas opções e descarta sucesso/falha tardios. Alterar o ID selecionado não dispara outra consulta; CEPs da mesma UF reutilizam sua lista ou consulta pendente.
- **Cancelamento HTTP:** `buscarCidadesPorUf` passou a aceitar um segundo argumento opcional `AbortSignal`, encaminhado ao cliente HTTP existente. Endpoint, resposta e chamadas anteriores com apenas UF permanecem compatíveis.
- **Seleção automática e edição:** a cidade resolvida pelo CEP é atribuída ao formulário e aparece quando a opção fica disponível. Uma cidade inicial válida da edição também é preservada durante o carregamento; o CEP inicial não é consultado novamente. Removido o estado pendente que ocasionava consultas adicionais ao ser definido/limpo.
- **Consistência:** mudar o CEP ou a UF manualmente limpa o ID anterior. Quando a lista conclui, IDs ausentes nela são removidos; CEP sem cidade resolvida ou UF sem opções deixam o campo vazio para escolha manual. O mapper existente continua omitindo o ID zero no envio. Uma mudança manual de cidade não é reaplicada por renderizações sem novo CEP.
- **Intervenção manual:** mudar a UF pelo campo cancela a consulta de CEP em andamento. Uma resposta tardia desse CEP não substitui a UF/cidade escolhida pelo usuário. O cancelamento acontece no evento de mudança, mantendo o registro do campo no React Hook Form.
- **Carregamento e recuperação:** cidade fica desabilitada enquanto CEP/cidades carregam ou não há UF. O botão Salvar aguarda essas consultas, evitando envio durante uma seleção incompleta. Falha da consulta de cidades apresenta mensagem genérica associada ao campo e ação “Tentar novamente”. A nova tentativa não restaura um ID inválido; a seleção manual volta a funcionar após recuperação.

Preservados o schema, tipos, props, lista de UFs, máscaras, validações, regras de coordenador responsável e mapper do formulário. Nenhuma política de acesso, contrato de backend, dependência, campo persistido ou regra de Title Case foi alterada. Não foram acrescentados `any`, casts para esconder contratos, supressões ou relaxamentos de configuração. As alterações locais anteriores foram preservadas.

## Validações

| Verificação | Resultado |
|---|---|
| Reprodutor antes da correção | Quatro falhas esperadas no diagnóstico: cidade vazia em vez de `35` |
| Testes de cidade novos | 24 aprovados na versão final |
| Recorte dos quatro arquivos de cidade, assinaturas, erros de CEP e contratos na suíte final | 74 aprovados, zero falhas; 24 novos e 50 anteriores |
| `npm.cmd run test:run` | 1.101 aprovados, zero falhas, 73 arquivos |
| `npm.cmd run typecheck` | Aprovado: zero erros, saída 0 |
| `npm.cmd run build` | Aprovado, saída 0 |
| ESLint dos quatro arquivos de código/teste alterados | Zero erros e avisos |
| `npm.cmd run lint` global | Aprovado: zero erros e avisos, sem novos apontamentos |
| `npm.cmd run lint:design-system` | Reprovado: sete ocorrências preexistentes em `AsyncSelect.tsx` e `DesignSystemPage.tsx` |
| `npm.cmd run format:check` final | Reprovado: 262 arquivos pendentes, mesma contagem anterior |
| Prettier do teste novo e teste ajustado | Aprovado; preservada a formatação preexistente dos arquivos de produção |
| Comparação de schema, tipos e props do formulário | Preservados |
| `git diff --check` | Aprovado |

`CooperadoCidade.test.tsx` usa formulário, campos com máscara, React Hook Form e envio reais, com serviços simulados. Oito cenários cobrem seleção automática e recuperação de erro nos temas claro, escuro e sistema com preferência clara/escura, incluindo rótulo, `aria-invalid`, mensagem associada e ausência de detalhes técnicos. Outros 16 cobrem edição, lista previamente carregada, mudança manual, cidade ausente/nula, lista vazia, UF/CEP sucessivos, falhas e sucessos tardios, limpeza, desmontagem e StrictMode.

Depois da regressão inicial e da primeira correção, os testes foram ampliados. A rodada ampliada teve 16 falhas de configuração: `vi.resetAllMocks` apagava a implementação global de `matchMedia`; o mock padrão passou a ser configurado após o reset. O lint também rejeitou o acesso à referência de cancelamento no callback passado a `register`; a operação foi movida para o evento de mudança do select, sem supressão. TypeScript e lint passaram na versão final. O primeiro check de formatação apontou o teste novo; ele foi reformatado e os checks foram repetidos, retornando à dívida global anterior de 262 arquivos. Não há testes ignorados ou marcados como falha esperada na suíte.

As verificações são locais, em DOM e com serviços simulados; não substituem homologação visual e autenticada com API/Keycloak reais. A nova mensagem e ação usam `FormField`, `Select` e `Button` existentes, sem cores, estilos ou tokens novos. Backend não alterado; seus testes não foram executados.

Relatórios locais ignorados pelo Git: `scratch/lint-etapa24-*`, incluindo retrato anterior, regressão inicial, execuções intermediárias e finais, comparativo de lint e logs de formatação.

## Arquivos alterados e continuidade

- Produção: `apps/web/src/features/cooperados/components/CooperadoForm.tsx` e `apps/web/src/features/clientes/api/localidadesApi.ts`.
- Testes: novo `apps/web/tests/forms/CooperadoCidade.test.tsx` e ajuste de três expectativas de sinal em `apps/web/tests/forms/FormSubscriptions.test.tsx`.
- Documentação: este registro e continuidade nos documentos 29 e 51.

As sete ocorrências do scanner de design system foram corrigidas no [documento 53](53-estabilizacao-tokens-design-system.md), preservando as seis simulações de marca e os temas. Os números acima preservam o retrato desta etapa de cidade. Próxima etapa: formatação em lotes delimitados, começando pelos tokens e arquivos de identidade. Formatação geral e homologação integrada continuam no [plano de estabilização](29-estabilizacao-frontend.md).
