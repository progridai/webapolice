# Simulação de migração de clientes

Ferramenta .NET 10 separada da API. Não contém comandos para gravar no banco. Analisa uma extração completa de `dbo.Clientes` em JSONL e, opcionalmente, consulta o PostgreSQL de homologação em transação `REPEATABLE READ READ ONLY`, com `default_transaction_read_only=on` na conexão.

## 1. Exportar a origem

Na raiz do repositório, em PowerShell, configure a conexão da **base antiga** e um caminho novo:

```powershell
New-Item -ItemType Directory -Force backend/.artifacts | Out-Null
$env:WEBAPOLICE_LEGACY_SQLSERVER = 'Server=SERVIDOR;Database=BASE_ANTIGA;Integrated Security=True;'
./backend/scripts/exportar-clientes-simulacao.ps1 -OutputPath ./backend/.artifacts/clientes.jsonl
```

O script usa `System.Data.SqlClient` e SQL Server 2016+. Exporta todos os campos, incluindo nulos e a coluna calculada `Cli_CPF_11`, em UTF-8. Não confundir com a saída tabular do diagnóstico: a simulação precisa dos registros completos. O JSONL contém dados pessoais; mantenha em pasta de acesso restrito, como a pasta local `.artifacts` já ignorada pelo Git.

A consulta roda em transação SERIALIZABLE para uma leitura consistente e pode segurar locks durante a exportação. Execute fora do horário de maior uso e prefira credenciais de leitura. O arquivo só recebe o nome final depois da exportação completa; `.partial` não deve ser usado como entrada. O exportador não executa INSERT, UPDATE, DELETE ou alteração de estrutura.

## 2. Simular

Os catálogos auxiliares também precisam ser exportados para preparar a correspondência dos códigos antigos. Use a mesma conexão SQL Server e caminhos novos:

```powershell
./backend/scripts/exportar-clientes-simulacao.ps1 -Table Cidades -OutputPath ./backend/.artifacts/cidades.jsonl
./backend/scripts/exportar-clientes-simulacao.ps1 -Table Bancos -OutputPath ./backend/.artifacts/bancos.jsonl
./backend/scripts/exportar-clientes-simulacao.ps1 -Table Lotacoes -OutputPath ./backend/.artifacts/lotacoes.jsonl
./backend/scripts/exportar-clientes-simulacao.ps1 -Table Orgao_Siape -OutputPath ./backend/.artifacts/orgaos-siape.jsonl
```

Esses arquivos ainda não são consumidos automaticamente pelo simulador; são a entrada para mapear e preparar a carga dos catálogos. Em 14/10, foi confirmado que essas quatro tabelas estão vazias em `webapolice_teste`.

Sem conexão com o destino:

```powershell
dotnet run --project backend/tools/ClientesMigrationSimulator -- --source backend/.artifacts/clientes.jsonl --output backend/.artifacts/simulacao-clientes-01
```

Com conferência do **PostgreSQL de homologação**:

```powershell
$env:WEBAPOLICE_MIGRATION_TARGET = 'Host=SERVIDOR;Database=BASE_HOMOLOGACAO;Username=USUARIO_LEITURA;Password=SENHA;'
dotnet run --project backend/tools/ClientesMigrationSimulator -- --source backend/.artifacts/clientes.jsonl --output backend/.artifacts/simulacao-clientes-02 --target
```

As conexões vêm somente das variáveis de ambiente; não são gravadas no relatório. Use sempre uma pasta de saída nova. Sem `--target`, todos os registros recebem `DESTINO_NAO_VERIFICADO`; o resultado não confirma cobertura dos catálogos nem ausência de duplicados na base nova. Uma falha de conexão encerra o processo, sem fazer uma simulação offline silenciosa.

Uma prioridade explicitamente autorizada sobre pessoa já existente pode ser representada por `--prefer-source CLIENTE_LEGADO:PESSOA_DESTINO`. Para esta carga, o usuário autorizou `--prefer-source 435698:1`. Isso permite priorizar os dados pessoais do maior Cliente desse grupo somente sobre a pessoa especificada; pessoas excluídas ou outras divergências continuam bloqueadas. A opção do simulador apenas muda a classificação, sem executar atualizações.

O processo mantém registros e plano em memória para agrupar CPFs. A execução completa de cerca de 487 mil registros precisa ser medida no ambiente de execução; ainda não há benchmark com a base real. A extração é identificada por SHA-256 e permanece intacta.

## 3. Ler os resultados

- `relatorio.md`: resumo legível e frequência de pendências.
- `resumo.json`: totais exclusivos por classificação, grupos candidatos, candidatos de registros filhos antes da deduplicação, cobertura de catálogos e identificação da extração.
- `pendencias.csv`: ID do cliente antigo, severidade, código e campo. Um cliente pode ter várias linhas.
- `nao-importados.csv`: uma linha por registro ignorado ou bloqueado, com ID antigo, classificação e motivos; sem dados pessoais de identificação. Bloqueios representam pendências da simulação, não exclusões definitivas.
- `plano.jsonl`: uma linha por cliente antigo, com status proposto, sexo convertido, grupo candidato, `personalDataSourceLegacyId` (registro de maior Cliente que fornece os dados pessoais), correspondências do destino, seleção de endereço e pendências. Não contém CPF, nomes, valores de contatos ou dados bancários.
- `schema-destino.json`: inventário de colunas, limites de tamanho, obrigatoriedade, defaults, constraints e índices do banco consultado. Sem conexão, fica identificado como não verificado.

`PRONTO_PARA_REVISAO` não significa carga aprovada. `COM_ADVERTENCIAS` exige avaliar políticas ainda abertas; `BLOQUEADO` inclui conflitos com o destino, referências sem correspondência, schema incompatível e datas inválidas/futuras.

Registros sem nome ou sem CPF oficial `Cli_CPF_11` são classificados como `IGNORADO`, não importados e excluídos da escolha do maior Cliente do grupo. Não geram candidatos de filhos. A lista de motivos fica em `nao-importados.csv`; um registro pode ter os dois motivos. CPF preenchido mas inválido continua candidato isolado com advertência, pois a exclusão confirmada trata CPF ausente. Relatórios antigos não são alterados.

## Regras implementadas

- CPF oficial exclusivamente `Cli_CPF_11`, preservando zeros iniciais, sem completar zeros ou usar `Cli_CPF` como fallback. Validar dígitos para classificar e propor unificação; documentos inválidos/ausentes ficam isolados por ID.
- Todos com status proposto `ativo`; no destino, procurar esse código sem presumir o ID.
- Sexo antigo `0 → 2` feminino, `1 → 1` masculino, NULL → NULL.
- Óbito informado marca falecido verdadeiro. Falecido sem óbito recebe 01/01/1900; óbito inválido/futuro também recebe o padrão. Nascimento e emissão de RG inválidos/futuros recebem 01/01/1900, incluindo cônjuge. O plano registra os valores corrigidos em `dates`, mantendo os originais no arquivo de origem. O padrão de óbito não gera conflito cronológico com nascimento. Datas ausentes de nascimento/RG não são preenchidas; data de cadastro continua sujeita a revisão.
- Ignorar `Cli_Status`, `Grupo` e `Subgrupo`. Dados específicos `Cli_Grupo_Corsan` continuam considerados.
- Desconsiderar também `Cli_Empresa`, `Cli_Rubrica`, `Cidade_Lotacao`, `Cli_Aposentado_Pensionista`, `Cli_ant` e `Cli_id_cliente_vind`. Não gerar pendências ou importar esses campos. O resumo registra todos em `disregardedFields`; dados específicos Corsan continuam considerados.
- Cônjuge com nome gera candidato a `cadastro.cliente_dependente`, relação `CONJUGE`, origem em `spouseSourceLegacyId`. Nome, CPF, nascimento, RG, órgão e emissão têm destino nessa tabela. A migration `20261014010000_AdicionarRgClienteDependente` adiciona os três campos opcionais de RG; a conferência do destino exige que estejam instalados antes da carga. Dados de cônjuge sem nome continuam como pendência. Deduplicação de dependentes fica para o importador de escrita.
- Os dois conjuntos de endereço geram candidatos separados quando houver dados: residencial sem C primeiro, correspondência C depois. Tipos `RESIDENCIAL` e `CORRESPONDENCIA`. `1` ou NULL seleciona sem C; `0` seleciona C. Endereço selecionado vazio: primeiro disponível. Não criar endereço vazio.
- Preservar principal ativo existente no destino. Quando endereços distintos da origem conflitarem como principal, preservar todos os candidatos e selecionar o primeiro por ID antigo crescente, com o conjunto sem C antes do C; registrar advertência, sem bloqueio por esse motivo. Seleções explícitas prevalecem sobre fallback quando não há conflito entre elas.
- Para cada grupo de CPF válido, os dados pessoais de nome, nascimento, sexo, falecido e óbito vêm do registro com maior `Cliente`, inclusive nulos. A hipótese de maior PK significar registro mais recente foi definida pelo usuário, não comprovada por timestamp. Divergências na origem viram advertências resolvidas por essa prioridade, sem bloquear por si só. Validar os dados pessoais selecionados; não preencher ausências com versões antigas. Nome é comparado sem diferenças de caixa, acentos e espaços repetidos; não há aproximação fonética.
- Comparações com pessoas/clientes já existentes no destino usam os dados pessoais selecionados pelo maior Cliente. Essa regra não autoriza sobrescrever dados divergentes no PostgreSQL; conflitos com o destino continuam pendentes.
- Pessoas excluídas, múltiplas pessoas com mesmo CPF, múltiplos clientes para pessoa e mapas inconsistentes geram bloqueios. Clientes já mapeados ficam sinalizados para não reimportar; nenhum dado existente é sobrescrito.
- Conferir referências de cidade, banco, lotação e órgão SIAPE por `legado_id`, inventariar schema e apontar campos sem destino/regra confirmados.

## Limites antes da carga

O plano é de análise, não de escrita. Registros filhos têm contagem de candidatos antes da deduplicação, incluindo cadastros bloqueados, mas excluindo os ignorados. A migration de documentos do cônjuge precisa ser aplicada e conferida no destino. Datas seguem o padrão explicitamente confirmado. Os valores completos permanecem no arquivo original; dados sem destino geram pendências.

A inspeção de schema verifica campos essenciais e inventaria constraints/índices para revisão. Não executa inserts de teste nem certifica que todas as constraints aceitarão uma futura carga. Deduplicação de contatos/documentos/vínculos/dependentes e atualização incremental ficam para o importador de escrita, depois da revisão desta simulação.

## Verificação local

```powershell
dotnet run --project backend/tools/ClientesMigrationSimulator -- --self-test
```

Os testes usam dados sintéticos e não abrem conexão. Cobrem CPF oficial e dígitos, sexo, status, endereços 1/0/nulo/fallback, duplicidades, datas, falecimento, catálogos e conflitos no destino.

Referência: [mapa de migração](../../../docs/24-migracao-clientes-legado.md).
