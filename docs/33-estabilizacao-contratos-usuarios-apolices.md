# Estabilização dos Contratos de Usuários e Apólices

Data: 21/10/2026. Continuação do [lote de formulários](32-estabilizacao-formularios.md).

## Resultado e escopo

Separados os contratos de criação/edição de Usuários e alinhados os consumidores de Apólices aos endpoints existentes. O TypeScript passou de 57 erros em 20 arquivos para 26 erros em 13 arquivos; a verificação global continua reprovada.

Alterados 13 arquivos de produção e criados quatro arquivos de testes. Preservados os trabalhos locais anteriores. Não houve alteração de backend, banco, endpoints publicados, permissões, configuração do Keycloak, dependências ou política de Title Case. Não foram criados campos de negócio ou alteradas as opções do compilador.

## Usuários

- `CriarUsuarioFormData` deriva do schema de criação, com senha temporária obrigatória de pelo menos seis caracteres e confirmação correspondente. `EditarUsuarioFormData` deriva do schema básico, sem senhas. Os callbacks das páginas e as propriedades de `UsuarioForm` distinguem os modos.
- O estado do formulário inclui os campos editáveis e as senhas opcionais antes da validação. O resolver usa o schema do modo atual; na edição, os campos de senha são retirados da saída pelo schema básico. Não foram criados casts de resolver ou novas permissões.
- A página de criação envia `senhaTemporaria` validada, sem fallback para senha vazia e sem transmitir a confirmação. A edição envia apenas nome, e-mail, ativo e perfis, como já determina o backend. Username continua não editável.
- O cliente de atualização de Usuário passou a retornar `Promise<void>`, correspondente ao `204 NoContent` de `UsuariosController.Atualizar`.
- O `FormField` de status passou para dentro do `Controller`, associando o rótulo ao checkbox real. Perfis inativos e estados de processamento mantêm os bloqueios existentes.

O fluxo de provisionamento e as regras de autenticação/autorização não foram modificados. Os testes não criam contas ou acessam o Keycloak.

## Contratos conferidos no backend

| Operação | Contrato atual conferido | Adequação do frontend |
|---|---|---|
| Criar/alterar base da Apólice | `CriarApoliceRequest`/`AlterarApoliceRequest`: referências `long`/`long?`, nome, datas e observação | `SalvarApoliceRequest` e mapeamento explícito compartilhado entre POST e PUT |
| Consultar detalhe da Apólice | `ApoliceDetalheResult`: IDs numéricos e nomes das entidades | Dados iniciais tipados, com rótulos locais separados do resultado validado |
| Listar Estipulantes | Controller aceita `pagina`, `tamanhoPagina`, `nome`, `cnpj`; handler devolve detalhe com razão social | Adicionados `nome`/`cnpj` à query e corrigida a paginação; seletor de Apólice consulta por `nome` e usa `razaoSocial`/`nomeFantasia` |
| Listar Seguradoras/Corretoras | `pagina`, `tamanhoPagina`, `busca`, `ativo`; itens com `id`, `publicId` e `nome` | Corrigidos parâmetros e propriedades de apresentação dos seletores |
| Listar Vidas | `apoliceSubgrupoPublicId`, `apoliceModuloPublicId`, `vigenciaDataReferencia` | Removidas referências aos nomes antigos na serialização e nas dependências do hook |
| Listar Apólices | Aceita `vigenciaDataReferencia` | Data de referência passa a ser encaminhada |

As fontes consultadas estão em `backend/src/WebApolice.Modulos.Seguro/src/WebApolice.Modulos.Seguro/Api/Controllers`, nos use cases de Apólices, em `WebApolice.Modulos.Cadastro/Api/Controllers` e em `WebApolice.Modulos.Seguranca/Api/Controllers/UsuariosController.cs`.

### Identificadores e campos enviados

O [ADR 007](adr/ADR-007-identificadores.md) determina UUIDs nos novos contratos públicos. A gravação básica de Apólices ainda possui referências numéricas no código atual do backend. Este lote preserva essa exceção existente: os seletores guardam o `id` numérico como texto no formulário e o cliente converte para número ao enviar. URLs da Apólice e referências de Vidas continuam usando UUIDs. A migração das referências da base para UUIDs requer uma entrega coordenada de backend/frontend e não foi realizada aqui.

A gravação básica passa a selecionar explicitamente os campos aceitos, sem propagar rótulos locais ou `ramos` da estrutura antiga. A vinculação de Ramos continua nos endpoints próprios existentes. Datas opcionais e corretora vazia mantêm a conversão para `null`; o cliente HTTP compartilhado continua aplicando Title Case aos campos humanos e preservando credenciais, e-mails e identificadores.

## Seletores e consultas

- `AsyncEntityField` usa o `Control` tipado de Apólices e restringe os nomes aos três campos de entidade existentes, sem `any`. IDs e atributos ARIA de `FormField` chegam ao input real.
- Rótulos iniciais de edição usam um tipo explícito; não são propriedades do schema validado ou do contrato HTTP. Editar sem abrir as listas não dispara consultas adicionais.
- A troca de uma seleção anterior preserva o termo digitado. Respostas de buscas anteriores não substituem as opções de uma consulta mais recente.
- Falha de busca apresenta alerta e retentativa com componentes existentes. O botão de retentativa não submete o formulário. Seletores ficam desabilitados durante a gravação.
- `useApoliceVidas` reage aos filtros atuais e à data de referência; cancela a consulta anterior e ignora respostas canceladas. Uma falha continua permitindo retentativa com os mesmos filtros.

Este lote não adiciona novos controles de filtro na aba de Vidas. Também não representa homologação completa do teclado ou do layout do seletor existente.

## Testes e validações

| Arquivo novo, relativo a `apps/web` | Casos |
|---|---|
| `tests/forms/UsuarioContracts.test.tsx` | 14: criação/edição nos temas suportados, validações, bloqueio durante gravação e payloads das páginas reais |
| `tests/forms/ApoliceContracts.test.tsx` | 12: seleção de entidades, dados iniciais, erros acessíveis, bloqueio durante gravação, troca de seleção, retentativa, resposta antiga e página real de edição |
| `tests/http/FeatureContracts.test.ts` | 11: corpo real serializado e queries usando o cliente HTTP, normalização, proteção de credenciais e resposta 204, com `fetch` simulado |
| `src/features/apolices/hooks/useApoliceVidas.test.tsx` | Cinco: mudança de filtros, cancelamento, resposta antiga e retentativa |

Os envios de Usuários e os seletores de Apólices foram exercitados nos temas claro, escuro e sistema (preferências clara e escura). A suíte verifica DOM, tema resolvido, mensagens, callbacks e payloads. Não houve chamadas de rede reais, criação de usuários ou alteração de registros. As senhas nos testes são fixtures fictícias.

| Verificação | Resultado |
|---|---|
| `npm.cmd run typecheck` | 26 erros em 13 arquivos; antes, 57 em 20 arquivos |
| `npm.cmd run test:run` | 363 aprovados, zero falhas, 51 arquivos; 42 casos adicionados |
| `npm.cmd run build` | Aprovado, código de saída 0 |
| `npm.cmd run lint` | 103 erros e 8 avisos; antes, 111 erros e 9 avisos; nenhuma ocorrência nova por arquivo/regra |
| Lint dos quatro testes novos | Zero erros e avisos |
| `npm.cmd run lint:design-system` | Sete ocorrências preexistentes, sem alteração |
| `npm.cmd run format:check` | 285 arquivos pendentes; antes, 286; testes novos e `UsuarioForm` formatados |
| `git diff --check` | Aprovado |

Relatórios locais: `scratch/contratos-etapa5-*`, ignorados pelo Git. Backend somente consultado; seus testes não foram executados. A homologação visual e os fluxos autenticados no ambiente integrado continuam pendentes. Não houve exclusão de arquivos, supressão de tipos ou relaxamento de validações para reduzir erros.

## Arquivos de produção alterados

- Segurança: `components/UsuarioForm.tsx`, `pages/CadastrarUsuarioPage.tsx`, `pages/EditarUsuarioPage.tsx` e `api/usuariosApi.ts`.
- Apólices: `components/forms/ApoliceForm.tsx`, `components/forms/AsyncEntityField.tsx`, `pages/ApoliceFormPage.tsx`, `api/apolices.api.ts`, `hooks/useApoliceVidas.ts` e `types/apolice.types.ts`.
- Consultas de cadastro: `corretoras/api/corretoras.api.ts`, `estipulantes/api/estipulantes.api.ts` e `estipulantes/types/estipulante.types.ts`.

Os caminhos acima são relativos a `apps/web/src/features`. A documentação de continuidade foi atualizada nos registros 29 e 32.

## Pendências e próximo lote

Atualização de 21/10/2026: o [lote de Clientes e referências antigas de Apólices](34-estabilizacao-clientes-referencias-apolices.md) corrigiu essas divergências, com seis erros de tipos restantes em outros módulos e 389 testes aprovados. As contagens acima preservam o encerramento do lote de contratos; a lista seguinte registra o planejamento daquele momento.

Concluir os 26 erros restantes em grupos pequenos: callbacks de identificadores e mapeamento de endereços de Clientes; componente antigo de módulos por Subestipulante e chave de Histórico em Apólices; parâmetros de Módulos, payload de Corretoras, fixtures e variáveis sem uso. Não reintroduzir a hierarquia antiga para recuperar imports inexistentes.

A listagem geral de Estipulantes ainda expõe filtros antigos de busca/status/ordenação que não constam no controller atual. O seletor de Apólices usa os parâmetros suportados; a revisão completa da listagem deve ser uma entrega própria. Lint, cores fixas, formatação global e a migração das referências numéricas da base de Apólices continuam pendentes.
