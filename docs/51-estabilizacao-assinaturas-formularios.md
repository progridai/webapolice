# Estabilização das Assinaturas de Campos dos Formulários

Data: 22/10/2026. Continuação da [revisão das constantes dos validadores](50-estabilizacao-constantes-validadores.md).

## Resultado

Eliminados os três avisos restantes de ESLint em Clientes e Cooperados: dois apontamentos de `react-hooks/incompatible-library` e uma supressão não utilizada. O lint global passou com zero erros e zero avisos, sem novos apontamentos por arquivo/regra/severidade. As regras de lint e suas severidades foram preservadas.

A suíte completa passou de 1.065 para 1.077 testes aprovados, em 72 arquivos. TypeScript e build aprovados. A estabilização geral permanece em andamento por causa das pendências de design system, formatação e homologação, além do defeito de seleção de cidade descrito abaixo.

## Alterações

- **ClienteForm:** leituras de `falecido`, `tipoPessoa` e `contatos` durante renderização passaram de `watch` para assinaturas de `useWatch` com o `control` do formulário. As assinaturas ficam antes do efeito que aplica `reset(initialData)`. O tipo de cada contato continua determinando o campo de e-mail ou telefone, acompanhando inclusão, remoção e mudança dos índices. Removida a supressão não utilizada. A assinatura de UF dos endereços, que já usava `useWatch`, foi preservada.
- **CooperadoForm:** UF, tipo e CEP passam a usar `useWatch`. Os efeitos existentes continuam recebendo os valores observados para carregar cidades e consultar CEP; a exibição do coordenador responsável continua condicionada ao tipo Cooperado.
- **Regras preservadas:** mesmos schemas, mensagens, exigências de coordenador/óbito, máscaras, conversão dos tipos numéricos, normalização do envio, contatos principais, contratos e callbacks. Não houve alteração de autorização, endpoint, dependência, campo persistido, Title Case ou apresentação visual. Nenhuma supressão nova, `any`, cast para esconder contrato ou relaxamento de compilação foi introduzido. As alterações locais anteriores foram preservadas.

A substituição segue a alternativa indicada pela [regra oficial do React](https://react.dev/reference/eslint-plugin-react-hooks/lints/incompatible-library). A [documentação de `useWatch`](https://react-hook-form.com/docs/usewatch) descreve a assinatura de campos, valores iniciais e a importância da ordem de assinatura. Foi reutilizada a biblioteca instalada e o padrão já presente no projeto.

## Validações

| Verificação | Resultado |
|---|---|
| Testes direcionados de formulários, principais de contato e CEP | 53 aprovados, zero falhas, quatro arquivos; 12 novos e 41 anteriores |
| `npm.cmd run test:run` | 1.077 aprovados, zero falhas, 72 arquivos |
| `npm.cmd run typecheck` | Aprovado: zero erros, saída 0 |
| `npm.cmd run build` | Aprovado, saída 0 |
| ESLint dos dois formulários e teste novo | Zero erros e avisos |
| `npm.cmd run lint` global | Aprovado, saída 0: zero erros e avisos; antes, zero erros e três avisos |
| `npm.cmd run lint:design-system` | Reprovado: sete ocorrências preexistentes em `AsyncSelect.tsx` e `DesignSystemPage.tsx` |
| `npm.cmd run format:check` | Reprovado: 262 arquivos pendentes, mesma contagem anterior |
| Prettier do teste novo | Aprovado; mantida a formatação preexistente dos formulários |
| `git diff --check` | Aprovado |

O novo `tests/forms/FormSubscriptions.test.tsx` usa formulários, React Hook Form, schemas e campos com máscara reais. Serviços de localidades, CEP e coordenadores são simulados, assim como a disponibilidade do recurso opcional RE. Oito cenários exercitam a alternância CPF/CNPJ, óbito, Cooperado/Coordenador, validação de coordenador responsável e valores enviados nos temas claro, escuro e sistema com preferência clara/escura. Outros quatro verificam tipos de contato após inclusão/remoção, `reset(initialData)`, mudança/limpeza da UF e atualização da UF por CEP com seleção manual da cidade.

Na primeira rodada direcionada, 50 testes passaram e três falharam. Dois testes esperavam `type="tel"`, mas o componente de telefone existente usa máscara em um input de texto; as expectativas foram corrigidas, preservando a verificação das máscaras e valores enviados. O typecheck também identificou quatro usos da opção `exact` em consultas `getByRole`, que não a suportam; foram substituídos por expressões regulares de nome exato. Após essas correções, permaneceu apenas a falha de cidade, reproduzida também no código anterior e registrada a seguir. Os 53 testes direcionados e a suíte completa passaram na versão final deste lote.

As verificações são locais, em DOM e com serviços simulados. Não substituem homologação visual/autenticada com API e Keycloak reais. Backend não alterado; seus testes não foram executados. Não há novas cores ou estilos neste lote.

## Defeito preexistente encontrado e próxima etapa

No cenário de CEP com `cidadeId` resolvido, a cidade não ficou selecionada no campo: uma resposta com `cidadeId: 35` e uma lista contendo essa cidade resultou em valor vazio no select. O mesmo cenário foi executado com `CooperadoForm.tsx` do retrato anterior às alterações deste lote e falhou da mesma maneira. O arquivo de produção foi restaurado à versão deste lote após o diagnóstico. A falha não foi atribuída à migração para `useWatch`.

A seleção automática da cidade foi corrigida na etapa seguinte, registrada no [documento 52](52-correcao-cidade-cep-cooperados.md), incluindo trocas de UF/CEP e descarte de respostas antigas. Os números e o diagnóstico acima preservam o retrato desta revisão de assinaturas. O teste final deste lote cobre separadamente a resposta de CEP sem `cidadeId`, a atualização da UF/lista e a seleção manual; ele não atesta que a seleção automática esteja corrigida.

Os relatórios da falha inicial e do reprodutor com o código anterior foram preservados em `scratch/lint-etapa23-*`, ignorados pelo Git, junto ao retrato inicial, comparativo de lint, testes e logs finais. Não foram acrescentados testes ignorados nem falhas esperadas à suíte.

## Arquivos alterados

- Produção: `apps/web/src/features/clientes/components/ClienteForm.tsx` e `apps/web/src/features/cooperados/components/CooperadoForm.tsx`.
- Teste novo: `apps/web/tests/forms/FormSubscriptions.test.tsx`.
- Documentação: este registro e continuidade nos documentos 29 e 50.

O próximo passo, após a correção documentada da cidade, é tratar as sete ocorrências de design system. A formatação geral permanece em lote próprio. Homologação integrada permanece no [plano de estabilização](29-estabilizacao-frontend.md).
