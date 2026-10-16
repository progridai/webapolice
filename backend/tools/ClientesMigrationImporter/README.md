# Importador da carga oficial de clientes

Ferramenta de carga separada do simulador, restrita a `webapolice_teste` e à extração identificada no `manifesto-carga.json`. A origem permanece aberta para leitura sem compartilhamento de escrita; o SHA-256 deve coincidir com o manifesto. O manifesto deve registrar a autorização `authorizedPersonOverrides: { "435698": 1 }`.

```powershell
# WEBAPOLICE_MIGRATION_TARGET deve conter a conexão de homologação, sem gravá-la em arquivos versionados.
dotnet run --project backend/tools/ClientesMigrationImporter -- --source-dir backend/.artifacts/carga-oficial-2026-10-15 --output backend/.artifacts/carga-oficial-2026-10-15/plano-clientes-novo
```

Sem opção de escrita, prepara apenas os relatórios. `--validate-transaction` executa a carga e conciliação com rollback final; IDs de sequences podem ser consumidos mesmo com rollback. `--apply` confirma a transação somente se a conciliação passar. Os dois modos são mutuamente exclusivos. A pasta de saída precisa ser nova.

## Dados e prioridades

- Clientes sem nome ou CPF oficial são ignorados e listados; CPF preenchido inválido permanece isolado, com `documento_valido=false`.
- CPF oficial é `Cli_CPF_11`; documentos não são reconstruídos pelo campo antigo.
- Cada grupo válido usa os dados pessoais do maior `Cliente`. A pessoa/cliente 1 pode receber os dados pessoais do registro 435698 por autorização explícita; os demais registros divergentes continuam bloqueados.
- Todos os clientes importados ficam Ativos. Sexo 0 antigo vira 2 (feminino); 1 permanece masculino.
- Datas de nascimento/RG/óbito seguem a regra confirmada de 01/01/1900 para valores inválidos; falecido sem óbito recebe a mesma data. Óbito informado marca falecido. NULL de falecido sem óbito é tratado como falso na coluna não anulável do destino, com advertência no plano.
- Dados pessoais principais vêm do registro prioritário; documentos RG distintos, vínculos e endereços de outras linhas são preservados e deduplicados por seus atributos.
- Endereços residencial/correspondência seguem a seleção e o fallback confirmados. Principal existente é preservado. Cada pessoa com endereços ativos deve terminar com exatamente um principal.
- Contatos mantêm o conteúdo original, inclusive valores compostos ou inválidos. São deduplicados por pessoa/tipo/valor normalizado; não concatenar vários telefones como se fossem um número válido. Essa carga preserva o texto composto em vez de tentar separar números por suposição.
- Cônjuges com nome são salvos como dependentes CONJUGE com CPF, nascimento, RG, órgão e emissão. CPF válido identifica a consolidação por cliente; nos demais casos, nome normalizado/nascimento. Maior Cliente prevalece entre versões do mesmo cônjuge. Dados de cônjuge sem nome ficam na lista `conjuges-nao-importados.json`, com os IDs da origem, sem criar nomes fictícios.
- SIAPE e Corsan são preservados com vínculo quando disponível. Campos anteriormente desconsiderados não geram entidades no destino.

## Transação e rastreabilidade

O processo usa COPY binário para tabela temporária, transação serializável, lock de migração e lock das tabelas escritas para evitar concorrência com alterações na aplicação de homologação. Não executar simultaneamente a outras cargas. UUIDs e PKs do destino são próprios; uma correspondência é registrada em `legado.cliente_migration_map` para cada registro antigo elegível.

A carga gera backup local do registro existente antes da alteração, plano, listas de exclusão, conciliação e resultado final. O backup contém dados pessoais: manter na pasta privada `.artifacts`, ignorada pelo Git. Senhas não são impressas. Valores originais das demais linhas permanecem na extração identificada pelo hash.

Antes do commit, conferir quantidade de mapas, existência e relacionamento pessoa/cliente, status Ativo e unicidade do endereço padrão. Qualquer falha causa rollback. Em falha de conexão na confirmação ou na gravação do relatório, conferir o destino antes de repetir: o commit pode ter ocorrido.

O hash da extração é registrado nos mapas. Reexecução da mesma extração completamente mapeada não reimporta nem altera dados; mapas parciais ou de outra extração interrompem a operação para revisão. A ferramenta não é um sincronizador incremental de alterações posteriores à carga.

## Validação desta versão

O simulador possui testes sintéticos para as regras e a autorização limitada. Um piloto de 11 linhas oficiais representativas executou as operações e a conciliação com rollback. A conferência final do banco e a reexecução devem ser feitas após a carga completa.
