# Consulta dos Detalhes de Ramos

Data: 24/11/2026.

## Comportamento

Na listagem de Ramos, **Visualizar** abre `/ramos/:publicId`, usando o UUID público do registro. A tela mostra nome e situação no cabeçalho, código, nome, descrição, data de cadastro e última atualização. O código fica oculto quando repete o nome, desconsiderando maiúsculas/minúsculas, conforme o guia de telas; o valor recebido não é modificado. Descrição ausente aparece como **Não informado**. Os campos reutilizam DescriptionList, DetailsSection e os formatadores compartilhados, com suporte aos temas claro, escuro e sistema.

**Voltar para ramos** retorna à listagem. **Editar ramo** aparece somente para quem possui `ramos.alterar` ou Acesso Total e abre a edição existente do registro. A consulta não salva alterações nem muda a situação do Ramo.

Há indicação de carregamento, registro não encontrado, acesso negado e falha com nova tentativa. Consultas são canceladas ao sair ou trocar de UUID; respostas atrasadas não substituem o contexto atual. O conteúdo recebe foco ao concluir o carregamento.

## Contrato e Segurança

A tela reutiliza `GET /api/ramos/{publicId}`, já protegido por `ramos.visualizar` no backend. A rota exige essa permissão e o módulo APOLICES disponível, inclusive para Acesso Total. Não há novo endpoint, permissão, perfil, dependência ou alteração de banco.

O serviço `ramosApi.obter` aceita AbortSignal opcional e o encaminha ao cliente HTTP compartilhado; chamadas anteriores permanecem compatíveis. Nome e descrição reutilizam campos humanos existentes. Código, UUID, situação e datas mantêm sua natureza técnica. Nenhuma classificação de Title Case ou trigger foi alterada e nenhum dado histórico foi reprocessado.

## Arquivos e Validação

Implementação: `RamoDetalhePage.tsx`, `RamosListPage.tsx`, `ramos.routes.tsx`, `ramos.api.ts` e `routePaths.ts` em `apps/web/src`.

Testes: fluxo real da listagem para detalhes, retorno e edição, temas, situação inativa, descrição ausente, erros HTTP, nova tentativa, restrições de acesso, cancelamento pendente e descarte de respostas antigas. O teste de contrato HTTP verifica também a propagação do cancelamento de `obter` até a rede.

Validações finais em `apps/web`:

| Verificação | Resultado |
| --- | --- |
| `npm.cmd run format:check` | Aprovado, nenhum arquivo pendente |
| `npm.cmd run typecheck` | Aprovado, zero erros |
| `npm.cmd run lint` | Aprovado, zero erros e avisos |
| `npm.cmd run lint:design-system` | Aprovado, zero ocorrências |
| `npm.cmd run test:run -- --maxWorkers=2` | 1.448 testes aprovados em 103 arquivos |
| `npm.cmd run build` | Aprovado; permanece o aviso preexistente de chunk acima de 500 kB, sem alterar limites |

Os testes incluem foco ao concluir a consulta e ocultação do código redundante. O limite de dois workers reduz o uso simultâneo de recursos sem excluir testes ou alterar as verificações. O servidor Vite local respondeu HTTP 200 ao módulo da nova tela, com a consulta compartilhada e o conteúdo atual.

A consulta das superfícies de navegação retornou nenhum navegador conectado; a tentativa de abrir o navegador integrado retornou `Browser is not available: iab`. Portanto, a homologação visual autenticada não foi executada. Nenhum registro real foi criado ou alterado, não houve mudança no backend, aplicação de migration, commit ou push nesta entrega.

## Continuidade das Pendências Antes do Manual

Esta entrega trata da consulta de Ramos. As demais frentes seguem separadas: campos Grupo/Seguradora de Estipulantes, delimitação das rotinas financeiras disponíveis, homologação autenticada e reconciliação da documentação histórica. Pagamentos parciais, estornos e operações bancárias são ampliações de escopo, com regras próprias a definir antes de qualquer implementação.

Continuidade de 24/11/2026: o [documento 103](103-vinculo-seguradora-estipulantes.md) registra a seleção de Seguradora e a retirada de Grupo das telas por decisão do responsável. O teste de foco de Ramos passou a aguardar o efeito assíncrono de foco, preservando a mesma verificação e os limites existentes.
