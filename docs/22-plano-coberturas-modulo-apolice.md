# Plano do Módulo e vínculos de Coberturas

Regra atual confirmada em 06/10/2026, no horário de São Paulo. A Cobertura é um cadastro compartilhado; o Plano continua exclusivo do vínculo do Módulo na Apólice.

## Regras atuais

- Cada vínculo em `seguro.apolice_modulo` possui um único Plano, inclusive quando inativo.
- As Coberturas são mantidas no cadastro `seguro.cobertura`. Nome, nome reduzido, básica e reajuste são atributos desse cadastro e podem ser reutilizados por vários Planos de Módulos, na mesma Apólice ou em Apólices diferentes.
- No Plano do Módulo, o usuário seleciona uma Cobertura existente e informa `premio_titular` e `premio_conjuge`. Esses valores pertencem ao vínculo, são independentes dos valores dos demais Planos e não alteram o cadastro da Cobertura.
- Ambos os prêmios são obrigatórios, monetários em R$, não negativos e com até duas casas decimais. Zero significa ausência de cobrança. Nenhum valor é herdado automaticamente dos vínculos globais de Planos ou do Universo anterior.
- O par Plano do Módulo/Cobertura é único, incluindo vínculos inativos. Para reutilizar uma associação inativa, editar/reativar o vínculo existente.
- Uma Cobertura vinculada não pode ser substituída dentro do mesmo registro. É possível inativar o vínculo e vincular outra Cobertura, preservando o histórico.
- A inativação do cadastro da Cobertura impede novos vínculos e reativações. Preserva as associações existentes e permite ajustar seus valores/status; reativar uma associação exige primeiro reativar o cadastro da Cobertura.

## Persistência

| Tabela | Finalidade |
|---|---|
| `seguro.apolice_modulo` | Vínculo contextual de um Módulo à Apólice, com UUID próprio |
| `seguro.apolice_modulo_plano` | Plano exclusivo, com FK `apolice_modulo_id` única |
| `seguro.cobertura` | Cadastro reutilizável da Cobertura e de seus atributos |
| `seguro.apolice_modulo_cobertura` | Associação entre o Plano do Módulo e a Cobertura, com FKs `apolice_modulo_plano_id`/`cobertura_id`, status e os dois prêmios `numeric(18,2) NOT NULL` |

O UUID de `apolice_modulo_cobertura` identifica a associação; `cobertura.public_id` identifica o cadastro compartilhado. São identificadores distintos. O vínculo não mantém cópias do nome/atributos: a leitura usa o cadastro atual, e alterações no nome aparecem nos Planos que o utilizam. Alterar os prêmios de um vínculo não altera os prêmios dos demais.

As PKs/FKs numéricas são internas. UUIDs públicos identificam as entidades nas rotas e requisições. As FKs usam restrição à exclusão e a unicidade física inclui vínculos inativos. As constraints verificam valores não negativos; a aplicação também rejeita casas decimais adicionais antes da gravação, evitando arredondamento silencioso.

## Uso na interface

1. Cadastrar/revisar a Cobertura em **Coberturas**, no menu do módulo Cadastro.
2. Abrir **Apólice → Módulos → Plano e Coberturas**, ou **Editar** o vínculo do Módulo.
3. Cadastrar seu único Plano, se ainda não houver.
4. Clicar **Vincular Cobertura**, selecionar o cadastro existente e informar os dois prêmios.
5. Clicar **Salvar vínculo da Cobertura**. Para alterações posteriores, usar **Editar vínculo**.

O cadastro de novos Módulos continua por **Salvar vínculo e continuar**, mantendo o modal aberto para cadastrar o Plano e vincular Coberturas. O cadastro do vínculo, o Plano e cada associação são etapas de gravação separadas; uma interrupção preserva as etapas já salvas.

O seletor carrega todas as páginas de Coberturas ativas e exclui as já associadas ao Plano, inclusive as associações inativas. **Atualizar Coberturas** recarrega as opções após cadastrar uma nova. O acesso **Cadastro de Coberturas** abre em outra aba e respeita `coberturas.visualizar`. Nome e atributos da Cobertura são editados no cadastro, e os prêmios são editados na associação.

Apólice ou Módulo inativo permite consulta e bloqueia as mutações. Plano inativo deve ser reativado antes de alterar suas associações. O status do vínculo não muda o status do cadastro compartilhado.

## API, segurança e auditoria

Prefixo: `/api/apolices/{apolicePublicId}/modulos/{apoliceModuloPublicId}/plano`.

| Método e sufixo | Operação | Permissão |
|---|---|---|
| `GET` | Consulta Plano e associações, inclusive inativas | `apolices.visualizar` |
| `GET /coberturas/opcoes?pagina=1&tamanho=100` | Opções mínimas de Coberturas ativas, com paginação e validação do vínculo do Módulo | `apolices.visualizar` |
| `PUT` | Cria/atualiza o único Plano | `apolices.modulos.alterar` |
| `POST /coberturas` | Associa uma Cobertura existente | `apolices.modulos.alterar` |
| `PUT /coberturas/{vinculoPublicId}` | Altera prêmios e status da associação existente | `apolices.modulos.alterar` |

As mutações de associação recebem `coberturaPublicId`, `premioTitular`, `premioConjuge` e `ativo`. Não recebem nome/atributos para criar Coberturas. A resposta inclui o UUID da associação, o UUID do cadastro, nome/atributos atuais, status do cadastro e os valores/status da associação.

As rotas exigem autenticação e validam a cadeia Apólice → vínculo → Plano → associação. Permissão geral de alterar Apólice não substitui a permissão de alterar Módulos. A consulta mínima de opções não exige acesso ao CRUD completo de Coberturas. Nenhuma concessão foi feita automaticamente a perfis.

As mutações usam `OperacaoAuditada` na mesma conexão/transação física do Seguro e da Auditoria. Snapshots incluem os UUIDs da Apólice, do Módulo e do cadastro/associação. Uma falha na auditoria reverte integralmente a operação correspondente. Locks da Apólice, do vínculo e da Cobertura serializam a validação e a gravação, inclusive frente a inativações concorrentes.

## Migrations e transição

- `20261007010053_PlanoECoberturasExclusivosModuloApolice` criou o Plano contextual e a versão inicial das Coberturas locais. Seu histórico permanece preservado.
- `20261007012211_CompartilharCoberturaNoPlanoModulo` transforma a Cobertura local em associação ao cadastro compartilhado. Acrescenta `cobertura_id`, converte eventuais registros locais em cadastros base com seus atributos, preserva UUID/prêmios/status da associação e então remove as cópias locais dos atributos. Não presume que nomes iguais sejam a mesma entidade. Cria FK e índice único Plano/Cobertura. O rollback recompõe os atributos locais a partir do cadastro sem excluir cadastros compartilhados.

O identificador técnico das migrations segue UTC. Os scripts SQL idempotentes são `05-plano-modulo.sql` e `06-cobertura-compartilhada.sql`, incluídos na aplicação restrita a `webapolice_teste`.

Antes desta correção, a consulta somente de leitura encontrou um Plano do Módulo, nenhuma associação de Cobertura e duas Coberturas no cadastro compartilhado. O Plano e os cadastros existentes são preservados, sem duplicar as Coberturas já cadastradas.

A nova migration foi aplicada em `webapolice_teste` e a API local foi recompilada/reiniciada. A verificação posterior confirmou o Plano e as duas Coberturas preservados, FK `cobertura_id NOT NULL`, prêmios obrigatórios e unicidade do par Plano/Cobertura. `/api/health/live` e `/api/health/ready` retornaram 200; a consulta das opções sem token retornou 401. Nenhuma alteração foi realizada em produção.

Planos globais e a hierarquia do Universo anterior continuam preservados por compatibilidade, inclusive Produto → Plano. O novo fluxo usa o Plano do Módulo e suas associações próprias. A aba **Universo anterior** permanece como consulta histórica. Esta entrega não implementa emissão/cobrança nem converte propostas/vidas para outra estrutura.

## Validação

Os testes PostgreSQL em base temporária verificam compartilhamento de uma mesma Cobertura entre Módulos/Apólices, valores independentes, atualização do nome compartilhado, unicidade física, validação monetária, zero, propriedade, status e rollback de auditoria. Na aplicação da nova migration, um registro local sintético é convertido e são conferidos UUID, nome, nome reduzido, básica, reajuste, prêmios e status preservados.

Os testes de interface verificam seleção do cadastro existente, envio dos UUIDs corretos, ausência de edição local do nome, prêmios, permissões, paginação, exclusão de associações já existentes e cadastro inativo. A checagem completa de TypeScript continua com 338 diagnósticos fora dos arquivos deste fluxo; não há diagnósticos nos arquivos novos/refatorados. A inspeção visual autenticada não está disponível nesta sessão.

Passaram 16 testes PostgreSQL, 12 de autenticação HTTP, 20 de arquitetura e 26 da interface. Builds do backend/frontend e lint dos arquivos alterados passaram. A base temporária foi removida ao finalizar os testes.
