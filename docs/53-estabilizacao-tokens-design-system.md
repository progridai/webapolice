# Estabilização dos Tokens do Design System

Data: 22/10/2026. Continuação da [correção da cidade por CEP em Cooperados](52-correcao-cidade-cep-cooperados.md).

## Resultado

Corrigidas as sete ocorrências de cores fixas apontadas pelo scanner: uma no texto de opção selecionada do `AsyncSelect` e seis nos botões do simulador de identidade. `npm.cmd run lint:design-system` passou com zero ocorrências, mantendo regras, abrangência e exceções existentes. Isso atesta o escopo do scanner, não conformidade visual integral da aplicação.

A suíte completa passou de 1.101 para 1.111 testes aprovados, em 74 arquivos. TypeScript, build e ESLint global aprovados, com zero erros e avisos. Permanecem formatação geral e homologação integrada.

## Alterações

- **AsyncSelect:** substituídos os aliases inexistentes por tokens oficiais de superfície, fundo, texto, borda, foco e erro. O menu passou a usar `--sombra-card`, removendo também a sombra RGBA embutida que não era apontada pelo scanner. Conferido que todas as variáveis consumidas pelo componente estão declaradas nos arquivos oficiais de tokens.
- **Seleção e estado ativo:** o par de fundo/texto usa `--cor-marca-principal` e `--cor-sobre-marca-principal`. Foi removida a suposição de texto branco sobre a marca; seleção e estado ativo acompanham o contraste escolhido pelo serviço. Preservados contratos, busca, cache, referência, callbacks e estados de carregamento/vazio do seletor.
- **Simulador white-label:** os seis controles usam `Button` secundário, com seus estados de interação e tokens existentes. As mesmas seis cores e rótulos continuam disponíveis. As configurações foram centralizadas em `shared/identidade/marcasDemonstracao.ts` como dados técnicos de entrada do serviço; não são usadas para sobrescrever cores dos botões. A aplicação e a restauração da identidade continuam usando `IdentidadeVisualService`, inclusive o comportamento preexistente de cache local.
- **Contraste automático:** a revisão identificou que o corte anterior de luminância não garantia a razão mínima com preto suave. O serviço agora confere a razão efetiva: prefere preto suave quando alcança `4.5:1`, depois branco e, na faixa intermediária em que ambos falham, preto puro. O vermelho da demonstração é um desses casos. Mantidos os valores explícitos de `corSobreMarcaPrincipal`, conforme o contrato anterior, e as demais regras de validação/cache.

O mínimo de `4.5:1` para texto normal e a decisão sem arredondamento seguem o [critério 1.4.3 do W3C](https://www.w3.org/WAI/WCAG22/Understanding/contrast-minimum.html). A política e os consumidores de tokens foram atualizados no [guia oficial de identidade](12-identidade-visual-design-system.md). Nenhuma paleta ou token novo foi criado.

Não houve alteração de dependências, permissões, endpoint, campo persistido, classificação de texto humano ou regra de Title Case. As cores das configurações são valores técnicos, preservados. Não foram acrescentados `any`, casts para esconder contratos, supressões ou relaxamentos de lint/compilação. As alterações locais anteriores foram preservadas.

## Validações

| Verificação | Resultado |
|---|---|
| Testes direcionados de tokens, seletor e identidade | 22 aprovados, zero falhas, três arquivos; dez novos e 12 anteriores |
| `npm.cmd run test:run` | 1.111 aprovados, zero falhas, 74 arquivos |
| `npm.cmd run typecheck` | Aprovado: zero erros, saída 0 |
| `npm.cmd run build` | Aprovado, saída 0 |
| `npm.cmd run lint` global | Aprovado: zero erros e avisos |
| `npm.cmd run lint:design-system` | Aprovado: zero ocorrências; antes, sete |
| `npm.cmd run format:check` final | Reprovado: 262 arquivos pendentes, mesma contagem anterior |
| Prettier dos dois arquivos novos | Aprovado; mantida a formatação preexistente dos demais arquivos |
| Referências de tokens do `AsyncSelect` | Todas declaradas na camada oficial |
| `git diff --check` | Aprovado |

O novo `DesignSystemTokens.test.tsx` renderiza página, seletor, ThemeProvider e serviço de identidade reais. Oito cenários verificam as seis marcas, botões secundários sem cor inline, restauração do dourado, cache, tokens das opções selecionadas/menu/controle/erro e suporte a claro, escuro e sistema com preferência clara/escura. Outros dois conferem contraste com cálculo independente: as seis marcas, o dourado, os 256 níveis da escala cinza e a regressão do vermelho intermediário. A expectativa antiga do vermelho no teste do serviço foi ajustada para preto puro.

Os testes direcionados passaram na primeira execução. O primeiro check de formatação restrito apontou o teste novo; ele foi reformatado e o check repetido passou. A formatação global foi repetida e continua reprovando pelos 262 arquivos anteriores, sem formatação em massa.

Os testes de DOM verificam os tokens efetivamente aplicados e os cálculos verificam as combinações de marca/texto da escolha automática. Não equivalem a homologação em navegador, auditoria de foco/contraste de todos os estados/componentes, validação de uma cor de texto explicitamente configurada ou de todas as identidades já armazenadas. Backend não alterado; seus testes não foram executados.

Relatórios locais ignorados pelo Git: `scratch/lint-etapa25-*`, incluindo retrato anterior, scanner inicial/final, testes, tipos, build, lint e formatação.

## Arquivos alterados e continuidade

- Componentes: `apps/web/src/components/ui/Select/AsyncSelect.tsx` e `apps/web/src/pages/DesignSystem/DesignSystemPage.tsx`.
- Identidade: novo `apps/web/src/shared/identidade/marcasDemonstracao.ts` e ajuste de `IdentidadeVisualService.ts` / `IdentidadeVisualService.spec.ts` no mesmo diretório.
- Teste novo: `apps/web/tests/pages/DesignSystemTokens.test.tsx`.
- Documentação: este registro e atualização dos documentos 12, 29 e 52.

O primeiro lote de formatação foi concluído no [documento 54](54-formatacao-tokens-identidade.md): oito arquivos de tokens/identidade formatados e 254 pendentes no global. Os números acima preservam o retrato desta revisão de tokens. Próxima etapa: os componentes básicos `Button`, `Input`, `Select` e `FormField`, sem mudanças funcionais. Homologação integrada permanece no [plano de estabilização](29-estabilizacao-frontend.md).
