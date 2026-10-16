# Carga de catálogos da extração oficial

Ferramenta separada do simulador, que continua somente de leitura. Prepara ou aplica os catálogos exclusivamente em `webapolice_teste`. Não importa clientes.

Origem: pasta com cinco JSONL e `manifesto-carga.json`. Os hashes dos cinco arquivos precisam coincidir com o manifesto; os dados de clientes são usados somente para identificar as lotações efetivamente referenciadas por candidatos com nome e CPF preenchidos.

```powershell
dotnet run --project backend/tools/CatalogosMigrationImporter -- --source-dir backend/.artifacts/carga-oficial-2026-10-15 --output backend/.artifacts/carga-oficial-2026-10-15/plano-catalogos-novo.json
```

Sem `--apply`, gera apenas o plano local. Para aplicar, definir `WEBAPOLICE_MIGRATION_TARGET` com a conexão de homologação e adicionar `--apply`; sempre usar um arquivo de saída novo. O destino escolhido pelo usuário e registrado no manifesto é `webapolice_teste`; outros bancos são recusados.

## Regras confirmadas em 15/10/2026

- Carregar cidades, bancos, órgãos SIAPE e lotações referenciadas pelos clientes candidatos.
- Preservar IDs antigos em `legado_id`; gerar as PKs novas normalmente.
- Lotação 3714 sem nome recebe **Lotação não informada**.
- Preservar lotações cuja cidade não existe no catálogo, com `cidade_id` NULL. A ocorrência fica no relatório, com IDs originais.
- Cidade da lotação usa `Lot_Cidade`; cidade de endereço do cliente não é alterada.
- Cidade preserva UF e usa estado correspondente se já houver registro em `core.estado`.
- Não substituir registros existentes. Uma reexecução aceita correspondências por `legado_id` somente quando os campos mapeados coincidem.

## Aplicação e verificação

Usa tabela temporária, COPY binário, transação serializável e lock de migração. Os quatro catálogos são inseridos e conciliados na mesma transação. Divergência em registros existentes gera rollback. Os relatórios incluem hashes, contagens, inserções e lotações preservadas sem cidade; não incluem credenciais ou dados pessoais dos clientes.

Em falha antes do commit, a transação é revertida. Se a gravação do relatório falhar após o commit, conferir o banco antes de repetir; a reexecução deve conciliar os registros existentes sem duplicá-los. Após a carga, executar novamente o simulador com `--target` para verificar as referências dos clientes.
