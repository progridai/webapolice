# Conclusão da formatação do frontend

Data: 23/10/2026. Continuação do [registro 74](74-formatacao-utilitarios-compartilhados.md). Por solicitação do usuário, todas as pendências restantes foram tratadas em uma única etapa.

## Resultado

Foram formatados os 168 arquivos ainda apontados pelo Prettier em `apps/web`. O check global passou com **zero pendências**, mantendo a configuração e a abrangência existentes. Oito arquivos precisavam apenas de finais de linha LF, incluindo `package.json` e `package-lock.json`; os conteúdos JSON desses dois arquivos permaneceram equivalentes, sem alteração de versões ou scripts.

| Área | Arquivos formatados |
|---|---:|
| Apólices | 27 |
| Clientes | 25 |
| Cooperados | 12 |
| Corretoras | 4 |
| Estipulantes | 16 |
| Módulos | 5 |
| Ramos | 4 |
| Seguradoras | 6 |
| Segurança | 28 |
| Subestipulantes | 8 |
| Páginas gerais | 9 |
| Estilos globais | 1 |
| Testes compartilhados | 9 |
| Raiz, configuração e scripts auxiliares | 14 |
| **Total** | **168** |

O escopo inclui TypeScript/TSX, JavaScript/CJS, CSS, JSON, HTML e Markdown. Foram preservados contratos, literais, expressões regulares, regras de negócio, permissões, parâmetros de configuração, tokens e política de Title Case. Nenhuma dependência foi instalada e nenhuma validação do projeto foi flexibilizada.

## Conferência das alterações

Antes da formatação, foram preservados os bytes e textos dos 168 arquivos, o estado do Git e hashes dos 1.374 arquivos existentes e não ignorados do repositório. Os hashes fora do escopo foram conferidos; as alterações locais anteriores permaneceram preservadas.

A comparação usou as ferramentas já instaladas: AST do TypeScript para código e JavaScript emitido, PostCSS e parser de valores para CSS, objetos JSON, árvore Markdown e parse5 para HTML, incluindo o JavaScript inline. A normalização da comparação considerou diferenças de representação: quebras de texto JSX e espaços explícitos, fragmentos textuais adjacentes, parênteses redundantes em operações lógicas associativas, aspas CSS e posições de origem. Foram preservados o conteúdo textual renderizado e a ordem dos operandos. As comparações finais passaram nos 168 arquivos; a verificação adicional do código emitido cobriu os 160 arquivos TS/JS.

O README de Clientes estava em CP1252. Foi convertido para UTF-8 preservando seu texto antes da formatação. A primeira checagem ainda apontou esse documento; uma tentativa de gravação direta pelo Prettier retornou `EPERM`. A saída formatada foi gravada com Python, e o check global final aprovou o documento. O README geral também foi formatado sem alteração de conteúdo.

## Validações finais

| Verificação | Resultado |
|---|---|
| `npm.cmd run format:check` global | Aprovado: 168 pendências antes, zero depois |
| Comparação estrutural e de conteúdo | Aprovada nos 168 arquivos |
| `npm.cmd run test:run` | 1.125 aprovados, zero falhas, 74 arquivos |
| `npm.cmd run typecheck` | Aprovado: zero erros, saída 0 |
| `npm.cmd run build` | Aprovado, saída 0 |
| `npm.cmd run lint` | Aprovado: zero erros e avisos |
| `npm.cmd run lint:design-system` | Aprovado: zero ocorrências |
| `git diff --check` | Aprovado |

Não foram criados testes para mudanças de formatação; foi executada a suíte existente completa do frontend. Backend não alterado; seus testes não foram executados. Não houve execução de migração, importação, acesso ao banco ou alteração de dados de negócio. Os checks locais não substituem homologação visual e dos fluxos autenticados em navegador.

Relatórios locais ignorados pelo Git: `scratch/lint-etapa47-*`, incluindo inventário dos arquivos, bytes anteriores, hashes, comparações e logs. Atualizados este registro e a continuidade dos documentos 29 e 74. Não restam lotes de formatação do frontend nesta sequência. O próximo marco é a homologação integrada indicada no [plano de estabilização](29-estabilizacao-frontend.md); a evolução funcional permanece orientada pela [sequência documentada](17-modelagem-banco-dados-webapolice.md#sequência-recomendada-de-evolução).
