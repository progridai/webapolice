/*
 Diagnóstico para migração de dbo.Clientes (SQL Server).
 Execute no SSMS, conectado à base antiga. Não altera dados nem estrutura.
 Retorna vários conjuntos de resultados; salve-os com os títulos correspondentes.
 Resultados detalhados contêm dados pessoais: mantenha o arquivo em acesso restrito.
 Execute preferencialmente fora do horário de maior uso: há leituras completas.
 Não usa NOLOCK. Se houver alterações durante a execução, os resultados podem variar.
 A análise de CPF abaixo verifica formato e repetição, NÃO os dígitos verificadores.
*/
SET NOCOUNT ON;

IF OBJECT_ID(N'dbo.Clientes', N'U') IS NULL
BEGIN
    RAISERROR('Tabela dbo.Clientes não encontrada. Selecione a base antiga.', 16, 1);
    RETURN;
END;

-- 1. Volume geral.
SELECT N'01 - Volume' AS diagnostico, COUNT_BIG(*) AS total_clientes
FROM dbo.Clientes;

-- 2. Preenchimento de TODAS as colunas, inclusive campos não usados.
-- Vazio = NULL ou texto sem conteúdo após remover espaços, TAB, CR e LF.
-- Zero e datas sentinela contam como preenchidos; são analisados separadamente.
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql +
    CASE WHEN @sql = N'' THEN N'' ELSE N' UNION ALL ' END +
    N'SELECT N''' + REPLACE(c.name, '''', '''''') + N''' AS campo, ' +
    CASE WHEN c.is_computed = 1 THEN N'1' ELSE N'0' END + N' AS calculado, ' +
    N'COUNT_BIG(*) AS total, ' +
    N'COUNT_BIG(CASE WHEN ' + QUOTENAME(c.name) + N' IS NULL THEN 1 END) AS nulos, ' +
    N'COUNT_BIG(CASE WHEN ' + QUOTENAME(c.name) + N' IS NOT NULL AND ' +
    N'NULLIF(LTRIM(RTRIM(REPLACE(REPLACE(REPLACE(CONVERT(nvarchar(max), ' + QUOTENAME(c.name) +
    N'), CHAR(9), N''''), CHAR(13), N''''), CHAR(10), N''''))), N'''') IS NULL THEN 1 END) AS vazios, ' +
    N'COUNT_BIG(CASE WHEN NULLIF(LTRIM(RTRIM(REPLACE(REPLACE(REPLACE(CONVERT(nvarchar(max), ' + QUOTENAME(c.name) +
    N'), CHAR(9), N''''), CHAR(13), N''''), CHAR(10), N''''))), N'''') IS NOT NULL THEN 1 END) AS preenchidos ' +
    N'FROM dbo.Clientes'
FROM sys.columns c
WHERE c.object_id = OBJECT_ID(N'dbo.Clientes')
ORDER BY c.column_id;

SET @sql = N'SELECT N''02 - Preenchimento'' AS diagnostico, *, '
    + N'CAST(100.0 * preenchidos / NULLIF(total, 0) AS decimal(6,2)) AS percentual_preenchido '
    + N'FROM (' + @sql + N') d ORDER BY preenchidos DESC, campo;';
EXEC sys.sp_executesql @sql;

-- 3. Distribuição dos códigos. NULL, vazio e zero permanecem distinguíveis.
SET @sql = N'';
SELECT @sql = @sql +
    CASE WHEN @sql = N'' THEN N'' ELSE N' UNION ALL ' END +
    N'SELECT N''' + c.name + N''' AS campo, CONVERT(nvarchar(100), ' + QUOTENAME(c.name) +
    N') AS valor, COUNT_BIG(*) AS quantidade FROM dbo.Clientes GROUP BY CONVERT(nvarchar(100), ' +
    QUOTENAME(c.name) + N')'
FROM sys.columns c
WHERE c.object_id = OBJECT_ID(N'dbo.Clientes')
  AND c.name IN (N'Cli_Sexo', N'Cli_Status', N'Cli_Falecido', N'Cli_Endereco_Padrao',
    N'Cli_Aposentado_Pensionista', N'Cli_Situacao_Endereco', N'Cli_Situacao_EnderecoC',
    N'Cli_Func_Corsan', N'Grupo', N'Subgrupo', N'Lotacao', N'Banco', N'Cli_Siape_Orgao')
ORDER BY c.column_id;
SET @sql = N'SELECT N''03 - Códigos'' AS diagnostico, * FROM (' + @sql + N') d ORDER BY campo, quantidade DESC;';
EXEC sys.sp_executesql @sql;

-- 4. Formato do CPF oficial: Cli_CPF_11, conforme regra da migração.
;WITH cpf AS (
    SELECT Cliente,
        NULLIF(LTRIM(RTRIM(Cli_CPF_11)), '') AS original,
        LTRIM(RTRIM(Cli_CPF_11)) AS limpo
    FROM dbo.Clientes
), classificado AS (
    SELECT CASE
        WHEN original IS NULL THEN N'Ausente'
        WHEN limpo = '' THEN N'Somente separadores'
        WHEN limpo COLLATE Latin1_General_BIN2 LIKE '%[^0-9]%' THEN N'Contém caracteres não numéricos'
        WHEN LEN(limpo) <> 11 THEN N'Quantidade de dígitos diferente de 11'
        WHEN limpo = REPLICATE(LEFT(limpo, 1), 11) THEN N'Onze dígitos iguais'
        ELSE N'Formato de 11 dígitos; validar dígitos verificadores'
        END AS situacao
    FROM cpf
)
SELECT N'04 - Formato CPF' AS diagnostico, situacao, COUNT_BIG(*) AS quantidade
FROM classificado GROUP BY situacao ORDER BY quantidade DESC;

-- 5. Grupos de CPF repetido com formato candidato; não comprova mesma pessoa.
;WITH cpf AS (
    SELECT Cliente, Cli_Nome, Cli_Data_Nascimento,
        LTRIM(RTRIM(Cli_CPF_11)) AS limpo
    FROM dbo.Clientes
)
SELECT N'05 - CPF repetido' AS diagnostico, limpo AS cpf_normalizado,
    COUNT_BIG(*) AS quantidade, MIN(Cliente) AS primeiro_id, MAX(Cliente) AS ultimo_id,
    COUNT(DISTINCT NULLIF(UPPER(LTRIM(RTRIM(Cli_Nome))), '')) AS nomes_distintos,
    COUNT(DISTINCT CONVERT(date, Cli_Data_Nascimento)) AS nascimentos_distintos
FROM cpf
WHERE LEN(limpo) = 11
  AND limpo COLLATE Latin1_General_BIN2 NOT LIKE '%[^0-9]%'
  AND limpo <> REPLICATE(LEFT(limpo, 1), 11)
GROUP BY limpo HAVING COUNT_BIG(*) > 1
ORDER BY quantidade DESC, limpo;

-- 6. Inconsistências. Um cliente pode aparecer em mais de uma categoria.
DECLARE @hoje date = CONVERT(date, GETDATE());
SELECT N'06 - Inconsistências' AS diagnostico, a.problema, COUNT_BIG(*) AS quantidade
FROM dbo.Clientes c
CROSS APPLY (VALUES
    (N'Nome ausente', CASE WHEN NULLIF(LTRIM(RTRIM(c.Cli_Nome)), '') IS NULL THEN 1 ELSE 0 END),
    (N'Óbito informado sem falecido marcado', CASE WHEN c.Cli_Data_Obito IS NOT NULL AND (c.Cli_Falecido IS NULL OR c.Cli_Falecido = 0) THEN 1 ELSE 0 END),
    (N'Falecido sem data de óbito (avaliar regra)', CASE WHEN c.Cli_Falecido = 1 AND c.Cli_Data_Obito IS NULL THEN 1 ELSE 0 END),
    (N'Nascimento futuro', CASE WHEN CONVERT(date, c.Cli_Data_Nascimento) > @hoje THEN 1 ELSE 0 END),
    (N'Óbito futuro', CASE WHEN CONVERT(date, c.Cli_Data_Obito) > @hoje THEN 1 ELSE 0 END),
    (N'Óbito anterior ao nascimento', CASE WHEN CONVERT(date, c.Cli_Data_Obito) < CONVERT(date, c.Cli_Data_Nascimento) THEN 1 ELSE 0 END),
    (N'Cadastro futuro', CASE WHEN c.Cli_Data_Cadastro > @hoje THEN 1 ELSE 0 END),
    (N'Dados de cônjuge sem nome', CASE WHEN NULLIF(LTRIM(RTRIM(c.Cli_Nome_Conjuge)), '') IS NULL AND
        (NULLIF(LTRIM(RTRIM(c.Cli_CPF_Conjuge)), '') IS NOT NULL OR c.Cli_Data_Nascimento_Conjuge IS NOT NULL OR NULLIF(LTRIM(RTRIM(c.Cli_RG_Conjuge)), '') IS NOT NULL) THEN 1 ELSE 0 END),
    (N'Conta ou agência sem banco', CASE WHEN (c.Banco IS NULL OR c.Banco = 0) AND
        (NULLIF(LTRIM(RTRIM(c.Cli_Agencia)), '') IS NOT NULL OR NULLIF(LTRIM(RTRIM(c.Cli_Conta_Corrente)), '') IS NOT NULL) THEN 1 ELSE 0 END)
) a(problema, encontrado)
WHERE a.encontrado = 1
GROUP BY a.problema ORDER BY quantidade DESC;

-- 7. Datas extremas: ajuda a encontrar sentinelas como 1900-01-01.
SELECT N'07 - Datas extremas' AS diagnostico, a.campo,
    MIN(a.valor) AS menor_data, MAX(a.valor) AS maior_data,
    COUNT_BIG(a.valor) AS preenchidos,
    COUNT_BIG(CASE WHEN a.valor < CONVERT(date, '19010101', 112) THEN 1 END) AS anteriores_a_1901
FROM dbo.Clientes c
CROSS APPLY (VALUES
    (N'Nascimento', CONVERT(date, c.Cli_Data_Nascimento)),
    (N'Óbito', CONVERT(date, c.Cli_Data_Obito)),
    (N'Cadastro', c.Cli_Data_Cadastro),
    (N'Emissão RG', CONVERT(date, c.Cli_Data_Emissao_RG)),
    (N'Nascimento cônjuge', CONVERT(date, c.Cli_Data_Nascimento_Conjuge)),
    (N'Emissão RG cônjuge', CONVERT(date, c.Cli_Data_Emissao_RG_Conjuge))
) a(campo, valor)
GROUP BY a.campo ORDER BY a.campo;

-- 8. Referências órfãs das FKs declaradas no DDL fornecido.
-- CidadeC, Cidade_Lotacao e Grupo não têm FK no DDL: confirmar seus catálogos.
SELECT N'08 - Referências órfãs' AS diagnostico, N'Cidade' AS campo, COUNT_BIG(*) AS quantidade
FROM dbo.Clientes c LEFT JOIN dbo.Cidades r ON r.Cidade = c.Cidade WHERE c.Cidade IS NOT NULL AND r.Cidade IS NULL
UNION ALL
SELECT N'08 - Referências órfãs', N'Banco', COUNT_BIG(*)
FROM dbo.Clientes c LEFT JOIN dbo.Bancos r ON r.Banco = c.Banco WHERE c.Banco IS NOT NULL AND r.Banco IS NULL
UNION ALL
SELECT N'08 - Referências órfãs', N'Lotacao', COUNT_BIG(*)
FROM dbo.Clientes c LEFT JOIN dbo.Lotacoes r ON r.Lotacao = c.Lotacao WHERE c.Lotacao IS NOT NULL AND r.Lotacao IS NULL
UNION ALL
SELECT N'08 - Referências órfãs', N'Cli_Siape_Orgao', COUNT_BIG(*)
FROM dbo.Clientes c LEFT JOIN dbo.Orgao_Siape r ON r.Orgao = c.Cli_Siape_Orgao WHERE c.Cli_Siape_Orgao IS NOT NULL AND r.Orgao IS NULL
UNION ALL
SELECT N'08 - Referências órfãs', N'Subgrupo', COUNT_BIG(*)
FROM dbo.Clientes c LEFT JOIN dbo.Subgrupos r ON r.Subgrupo = c.Subgrupo WHERE c.Subgrupo IS NOT NULL AND r.Subgrupo IS NULL;

-- 9. Amostra de cadastros sem nome, identificada pelo ID antigo.
SELECT TOP (100) N'09 - Amostra sem nome' AS diagnostico, Cliente,
    Cli_Nome, Cli_Status, Cli_Data_Cadastro
FROM dbo.Clientes
WHERE NULLIF(LTRIM(RTRIM(Cli_Nome)), '') IS NULL
ORDER BY Cliente;
