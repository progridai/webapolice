# Mapeamento da migração de clientes

Data: 13/10/2026. Origem: SQL Server, `dbo.Clientes`. Destino: PostgreSQL.

Simulação implementada em [ClientesMigrationSimulator](../backend/tools/ClientesMigrationSimulator/README.md), com exportador SQL Server, relatórios locais e conferência do PostgreSQL somente de leitura. Executada sobre a extração real e, em 14/10/2026, com o banco de homologação.

Atualização de 14/10/2026: migration `20261014010000_AdicionarRgClienteDependente` aplicada em **webapolice_teste**, com verificação das três colunas opcionais e da entrada no histórico EF. A simulação `simulacao-clientes-06` consultou o destino em leitura: os quatro catálogos (cidade, banco, lotação e órgão SIAPE) estão vazios. O status existe com código `ATIVO`, ID 1; a consulta do simulador foi ajustada para resolver o código sem diferença de caixa/espaços. A base de produção não foi alterada.

Este mapa foi conferido contra os modelos, configurações EF, migrations e telas do repositório. Ainda não foi conferido contra uma conexão com o banco novo. Não representa uma carga executada. Distingue regras confirmadas pelo usuário de propostas e pendências necessárias à simulação.

## Regras confirmadas

- `Cli_CPF_11` é o CPF oficial. Não reconstruir o documento a partir de `Cli_CPF`, não completar zeros adicionalmente e não remover caracteres para produzir outro CPF. Preservar zeros iniciais como texto.
- Todos os clientes importados entram como **Ativo**, inclusive falecidos. Resolver o ID pelo código `ativo` do catálogo (`ClienteStatusCodigos.Ativo`), sem presumir ID numérico.
- Ignorar `Cli_Status`, `Grupo` e `Subgrupo` na carga de clientes.
- Também desconsiderar `Cli_Empresa`, `Cli_Rubrica`, `Cidade_Lotacao`, `Cli_Aposentado_Pensionista`, `Cli_ant` e `Cli_id_cliente_vind`, por decisão do usuário. Não importar nem gerar pendências para esses seis campos; os campos específicos Corsan continuam no mapeamento.
- Sexo antigo: `1` masculino, `0` feminino, NULL não informado. O cadastro novo usa `1` masculino e **`2` feminino**: converter `0 → 2`.
- Os campos terminados em C representam endereço de correspondência e também vão para `core.pessoa_endereco`, em registro separado quando houver dados.
- O conjunto de endereço sem C é **residencial**: `tipo_endereco = RESIDENCIAL`; o conjunto C recebe `CORRESPONDENCIA`.
- `Cli_Endereco_Padrao = 1` ou NULL seleciona o endereço do conjunto sem C (`Cli_Logradouro`); `0` seleciona o endereço de correspondência (`Cli_LogradouroC`). Esta confirmação substitui a interpretação anterior do campo como booleano independente. Se o endereço selecionado não existir entre os efetivamente importados e nenhum outro estiver marcado como padrão, escolher o primeiro. Não criar endereço vazio.
- Telefones 2–4 estão vazios no diagnóstico atual; não gerar contatos vazios. Caso recebam conteúdo em uma extração futura, aplicar o mesmo tratamento do telefone principal.
- Dados pessoais conflitantes no mesmo CPF: usar o registro com maior `Cliente` para nome, nascimento, sexo, falecido e data de óbito. Por decisão do usuário, assumir o maior PK como registro mais recente. Nulos também prevalecem; não preencher com versões antigas. Validar os valores selecionados. Endereços permanecem preservados com a regra própria de primeiro padrão em conflitos.
- Registros sem nome são ignorados e não importados. Registrar `IGNORADO` no relatório, sem candidatos de registros filhos. Excluir esses registros antes de selecionar o maior Cliente do grupo; assim, um registro ignorado não fornece dados pessoais para outro que será importado.
- Registros sem `Cli_CPF_11` também são ignorados antes da unificação. Gerar `nao-importados.csv` com código antigo, classificação e motivos, incluindo exclusões por nome e CPF ausentes.
- Data padrão confirmada: **01/01/1900** para falecidos sem data de óbito e datas inválidas/futuras de óbito, nascimento ou emissão de RG (incluindo campos de cônjuge). Havendo óbito informado, marcar falecido como verdadeiro e preservar a data se válida. A data padrão de óbito representa valor desconhecido e não deve gerar bloqueio por ser anterior ao nascimento. Datas de cadastro não recebem essa correção. Nascimento/emissão ausentes continuam nulos, pois a regra confirmada trata valores inválidos.

## Mapa de campos

| Origem | Destino / tratamento | Situação |
|---|---|---|
| `Cliente` | `legado.cliente_migration_map.legado_cliente_id`; correspondência com pessoa, cliente e vínculo | Confirmado |
| `Cli_CPF_11` | `core.pessoa.documento_principal` e `documento_principal_limpo`; `legado.cliente_migration_map.cpf_limpo` | Confirmado; validar formato e dígitos sem alterar o CPF; sem CPF, ignorar e listar |
| `Cli_CPF` | `legado.cliente_migration_map.cpf_original`, somente rastreabilidade | Sem uso para identificação |
| `Cli_Nome` | `core.pessoa.nome`, `nome_normalizado`; mapa `nome_original` | Nome obrigatório; ausência ignora o registro, sem importação |
| `Cli_Data_Nascimento` | `core.pessoa.data_nascimento` | Converter para date; inválida/futura recebe 01/01/1900 |
| `Cli_Sexo` | `core.pessoa.sexo` | `1 → 1`, `0 → 2`, NULL → NULL |
| Tipo de pessoa | `core.pessoa.tipo_pessoa = 1` (física) | Proposta para esta origem de CPF; confirmar exceções na simulação |
| `Cli_Falecido` | `cadastro.cliente.falecido` | Preservar verdadeiro/falso; NULL exige regra explícita |
| `Cli_Data_Obito` | `cadastro.cliente.data_obito` | Óbito informado marca falecido; inválido/futuro ou ausente em falecido recebe 01/01/1900 |
| `Cli_Data_Cadastro` | `cadastro.cliente.data_cadastro_legado` | Não substituir por data de execução |
| `Cli_Observacoes` | `cadastro.cliente.observacao` | Preservar texto útil; não duplicar em pessoa sem necessidade |
| `Cli_RG`, `Cli_Orgao_RG`, `Cli_Data_Emissao_RG` | `core.pessoa_documento.numero`, `orgao_emissor`, `data_emissao`, tipo `RG` | Não criar documento sem número; preservar campos isolados em pendência |
| `Cli_Telefone`, `Cli_Telefone2`, `Cli_Telefone3`, `Cli_Telefone4` | `core.pessoa_contato.valor`, `valor_normalizado`, `tipo_contato` | Proposta: não concatenar vários números em um único número normalizado; analisar separadores |
| `Cli_Email` | `core.pessoa_contato`, tipo `EMAIL` | Preservar valor; validar e analisar múltiplos e-mails antes de separar |
| `Cidade` | `core.pessoa_endereco.cidade_id` por `core.cidade.legado_id` | Verificar cobertura do catálogo |
| `Cli_CEP`, `Cli_Logradouro`, `Cli_Numero`, `Cli_Complemento`, `Cli_Bairro` | Primeiro candidato a `core.pessoa_endereco`: `cep`, `logradouro`, `numero`, `complemento`, `bairro` | Tipo RESIDENCIAL confirmado |
| `Cli_Situacao_Endereco` | `core.pessoa_endereco.legado_situacao_endereco` | Preservar código sem inferir ativo/inativo |
| `CidadeC`, `Cli_CEPC`, `Cli_LogradouroC`, `Cli_NumeroC`, `Cli_ComplementoC`, `Cli_BairroC` | Segundo candidato a `core.pessoa_endereco`, endereço de correspondência; mesma correspondência de cidade | Significado confirmado; usar código de tipo de correspondência compatível com o destino |
| `Cli_Situacao_EnderecoC` | `core.pessoa_endereco.legado_situacao_endereco` do segundo candidato | Preservar código |
| `Cli_Endereco_Padrao` | `core.pessoa_endereco.principal` | `1` ou NULL: conjunto sem C; `0`: correspondência (C); fallback para primeiro importado se não houver principal |
| `Cli_Matricula` | `cadastro.cliente_vinculo.matricula`; mapa `matricula_original` | Não usar como documento nem como RE do cliente |
| `Lotacao` | `cadastro.cliente_vinculo.lotacao_id` | Verificar catálogo e critério de criação do vínculo |
| `Banco`, `Cli_Agencia`, `Cli_Conta_Corrente` | `cadastro.cliente_vinculo.banco_id`, `agencia`, `conta_corrente` | Banco por `core.banco.legado_id`; preservar zeros e dígitos como texto |
| `Cli_Nome_Conjuge`, `Cli_CPF_Conjuge`, `Cli_Data_Nascimento_Conjuge` | `cadastro.cliente_dependente.nome`, `cpf`, `data_nascimento` | Relação CONJUGE; origem registrada em spouseSourceLegacyId na simulação; sem nome, emitir pendência |
| `Cli_RG_Conjuge`, `Cli_Orgao_RG_Conjuge`, `Cli_Data_Emissao_RG_Conjuge` | `cadastro.cliente_dependente.rg`, `orgao_rg`, `data_emissao_rg` | Campos opcionais adicionados no modelo e na migration 20261014010000_AdicionarRgClienteDependente; aplicar no destino antes da carga |
| `Cli_Siape` | `convenio.siape_cliente.siape` | Modelo disponível |
| `Cli_Siape_Orgao` | `convenio.siape_cliente.orgao_id` por `convenio.siape_orgao.legado_id` | Verificar catálogo |
| `Cli_Siape_Categoria`, `Cli_Siape_Setor`, `Cli_Siape_Instituto`, `Cli_Siape_Agencia`, `Cli_Siape_Funcao`, `Cli_Siape_Contrato` | `convenio.siape_cliente.categoria`, `setor`, `instituto`, `agencia`, `funcao`, `contrato` | Preservar texto e zeros; não confundir agência SIAPE com agência bancária |
| `Cli_Digito_Verificador_Siape`, `Cli_Siape_Instituidor` | `convenio.siape_cliente.digito_verificador`, `instituidor` | Modelo disponível |
| `Cli_Empresa_Corsan`, `Cli_Rubrica_Corsan`, `Cli_Grupo_Corsan`, `Cli_Func_Corsan` | `convenio.corsan_cliente.empresa`, `rubrica`, `grupo`, `funcionario` | Criar somente quando houver contexto Corsan, não pelo default falso isolado |
| `Cli_Empresa`, `Cli_Rubrica` | Não importar | Desconsiderar por decisão confirmada; não confundir com campos Corsan |
| `Cidade_Lotacao` | Não importar | Desconsiderar por decisão confirmada |
| `Cli_Aposentado_Pensionista` | Não importar | Desconsiderar por decisão confirmada |
| `Cli_ant`, `Cli_id_cliente_vind` | Não importar | Desconsiderar por decisão confirmada |
| `Cli_Status`, `Grupo`, `Subgrupo` | Não importar | Decisão confirmada; não confundir `Grupo` com `Cli_Grupo_Corsan` |

Campos gerados no destino: PKs próprias, `public_id` UUID para pessoa e cliente, timestamps da execução e `deleted_at` NULL. A data histórica permanece no campo de legado. UF deve vir da cidade mapeada quando disponível; não adivinhar UF pelo nome da cidade.

## Divergências encontradas no código

1. **Sexo:** formulário e detalhes usam `1/2`; copiar o `0` antigo diretamente exibiria feminino como não informado.
2. **Cadastro normal não atende ao legado:** `CadastrarClienteHandler` exige nome, documento, nascimento e data de óbito quando falecido. Há dados ausentes na origem. Recomenda-se importador específico, com política explícita de pendências, sem enfraquecer automaticamente as regras da API.
3. **Validação atual do documento é apenas por tamanho:** não equivale à validação dos dígitos verificadores. Na simulação, classificar validade de CPF separadamente; usar sempre `Cli_CPF_11` como fonte, mesmo quando inválido.
4. **Documentos do dependente:** a migration de setembro não criava RG, órgão ou emissão. Em 14/10/2026, o modelo, configuração EF e snapshot receberam `rg`/`orgao_rg` (text opcionais) e `data_emissao_rg` (date opcional), com migration `20261014010000_AdicionarRgClienteDependente`. A simulação passa a exigir essas colunas no destino. A migration ainda precisa ser aplicada no banco correto.
5. **Vínculo também é parcial:** o modelo não representa todos os campos históricos descritos na documentação. Conferir DDL aplicado, constraints e defaults para a escrita.
6. **Documento de pessoa não é chave única no EF de Core:** a busca pode retornar múltiplas pessoas já existentes. Nunca escolher a primeira silenciosamente.
7. **Mapa de migração:** o EF configura unicidade do cliente legado, mas migrations históricas e configuração podem divergir. Conferir o índice aplicado; usar uma correspondência por registro antigo e transação que inclua o mapa.

## Proposta para a simulação

Não realizar inserts, updates, merges ou criação de estrutura. Ler uma extração consistente da origem e o estado do destino, com data/hora registradas. A simulação deve gerar:

- Resumo de registros prontos, com advertências e bloqueados, sem contar um registro mais de uma vez no total.
- Pendências por `Cliente` antigo, tipo de problema e campo. Não imprimir CPF e dados bancários em logs comuns.
- Plano de correspondência pessoa/cliente/vínculo e estimativa de registros em cada tabela.
- Cobertura dos quatro catálogos: cidade, banco, lotação e órgão SIAPE.
- Conflitos de CPF e dados pessoais, incluindo pessoas já existentes na nova base e registros excluídos logicamente.
- Campos sem destino definido preservados na extração, sem perda silenciosa.

Identificação na simulação: CPF com formato e dígitos válidos pode ser candidato à unificação. Nome, nascimento, sexo e informações de falecimento da origem vêm do registro com maior `Cliente`, conforme regra confirmada; divergências antigas viram advertências resolvidas, preservando a rastreabilidade. Ausência de um campo não equivale a divergência e valores nulos do registro prioritário prevalecem. Essa decisão não autoriza sobrescrever dados conflitantes já existentes no destino. CPF inválido ou ausente não deve unir pessoas automaticamente. Nome isolado também não deve unir pessoas. Políticas de escrita e casos sem documento válido ainda precisam ser fechados antes da carga.

Um cliente unificado pode corresponder a vários `Cliente` antigos. Cada origem deve ter sua linha no mapa; o campo escalar `cadastro.cliente.legado_id` sozinho não representa essa relação. A escolha de um ID representativo, se necessária, deve ser documentada. Vínculos devem preservar matrículas/lotação/dados bancários distintos, sem usar Grupo/Subgrupo como critério. Critérios de deduplicação precisam ser fechados antes de escrever.

Ordem dos endereços: por ID antigo crescente após unificação, conjunto sem C antes do conjunto C em cada registro. Na origem, `1` ou NULL seleciona o conjunto sem C e `0` seleciona o conjunto C, confirmado como correspondência. Preservar um principal existente no destino. Se houver endereços distintos conflitantes como principal após unificação, preservar todos os endereços distintos e selecionar o primeiro endereço efetivamente importado; registrar advertência, sem bloquear por esse motivo (regra confirmada pelo usuário). Se nenhum endereço efetivamente importado estiver indicado, também aplicar o primeiro disponível. Outros valores do indicador geram pendência.

Datas inválidas de nascimento, RG e óbito recebem 01/01/1900 por decisão explícita do usuário; registrar a substituição e preservar o original na extração. Data de cadastro inválida continua pendente. A simulação registra datas selecionadas em `dates` no plano e propaga nascimento/óbito do registro prioritário.

## Pendências para fechar o importador

| Pendência | Como resolver |
|---|---|
| Schema real, constraints, índices e catálogos do destino | Inspeção somente de leitura do PostgreSQL de homologação |
| Cônjuge e documentos | Migration de RG aplicada e conferida em homologação em 14/10; fechar deduplicação de dependentes no importador de escrita |
| Unificação e conflito com registros já existentes | Revisar relatório da simulação; não sobrescrever automaticamente |
| Nulos de falecido e faltas de dados obrigatórios | Definir política de carga histórica e pendências |
| Telefones/e-mails compostos | Inspecionar padrões sem normalizar vários contatos como um único valor |

## Critérios para a carga piloto

1. Cada registro da extração deve estar planejado ou ter pendência explicada. Os 487.262 registros são a referência do diagnóstico de 13/10, não um total fixo para extrações futuras.
2. Todos os clientes efetivamente importados devem ter status Ativo e sexo convertido corretamente.
3. Cada pessoa com endereços deve ter um padrão segundo as regras definidas, sem gerar endereço vazio e sem alterar principal existente silenciosamente.
4. Repetir ou retomar a execução não pode duplicar pessoa, cliente, vínculo, contato, documento, endereço ou dependente.
5. Transações devem registrar dados e mapa juntos, com checkpoints e pendências persistidas por lote.
6. A amostra deve conter cliente completo, sem CPF, CPF inválido, CPF repetido compatível, CPF conflitante, falecido sem óbito, SIAPE, Corsan, cônjuge e dois conjuntos de endereço. Casos bloqueados devem continuar bloqueados conforme política.
7. Conferir comportamento nas telas da aplicação, além da conciliação no banco. Não executar carga em produção nessa etapa.

## Fontes conferidas

- [Modelagem de clientes](18-modelagem-clientes-core-cadastro.md) e [estratégia geral](03-estrategia-de-migracao.md).
- `backend/src/WebApolice.Shared.Infrastructure/Persistence/CoreDbContext.cs` e `InfraestruturaDbContext.cs`.
- `backend/src/WebApolice.Modulos.Cadastro/Infrastructure/Persistence/Configurations/ClienteConfiguration.cs` e modelos de vínculo/dependente.
- `backend/src/WebApolice.Modulos.Cadastro/Infrastructure/Migrations/20260917165419_CriarTabelaClienteDependente.cs`.
- `backend/src/WebApolice.Modulos.Cadastro/Application/UseCases/CadastrarCliente/CadastrarClienteHandler.cs`.
- `backend/src/WebApolice.Modulos.Convenio/src/WebApolice.Modulos.Convenio/Infrastructure/Persistence/ConvenioDbContext.cs` e modelos SIAPE/Corsan.
- `apps/web/src/features/clientes/components/ClienteForm.tsx` e `pages/ClienteDetalhePage.tsx`.
