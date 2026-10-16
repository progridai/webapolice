param(
    [Parameter(Mandatory = $true)][string]$OutputPath,
    [ValidateSet('Clientes', 'Cidades', 'Bancos', 'Lotacoes', 'Orgao_Siape')]
    [string]$Table = 'Clientes'
)

# Connection is supplied through the environment, never written to output.
# Example (Windows authentication):
# $env:WEBAPOLICE_LEGACY_SQLSERVER = 'Server=SERVIDOR;Database=BASE_ANTIGA;Integrated Security=True;'
# .\backend\scripts\exportar-clientes-simulacao.ps1 -OutputPath .\backend\.artifacts\clientes.jsonl
# Uses a SERIALIZABLE read-only query (no DML). Can hold locks: schedule outside peak usage.
# SQL Server 2016+ required for FOR JSON. All columns, including Cli_CPF_11, are preserved.
$ErrorActionPreference = 'Stop'
$taskConnectionString = $env:WEBAPOLICE_LEGACY_SQLSERVER
if ([string]::IsNullOrWhiteSpace($taskConnectionString)) {
    throw 'Defina WEBAPOLICE_LEGACY_SQLSERVER com a conexão da base antiga.'
}
# Resolve relative paths against PowerShell's current location, not the .NET
# process working directory (Set-Location does not necessarily update it).
$taskOutput = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutputPath)
if ([IO.File]::Exists($taskOutput) -or [IO.Directory]::Exists($taskOutput)) {
    throw 'Use um caminho novo para não sobrescrever uma extração.'
}
$taskParent = [IO.Path]::GetDirectoryName($taskOutput)
if (-not [IO.Directory]::Exists($taskParent)) {
    throw 'Crie a pasta de destino antes de exportar.'
}
$taskPartial = $taskOutput + '.partial'
if ([IO.File]::Exists($taskPartial)) {
    throw 'Já existe um arquivo parcial nesse caminho. Escolha outro caminho.'
}
$taskConnection = New-Object System.Data.SqlClient.SqlConnection($taskConnectionString)
$taskTransaction = $null
$taskReader = $null
$taskWriter = $null
try {
    $taskConnection.Open()
    $taskTransaction = $taskConnection.BeginTransaction([Data.IsolationLevel]::Serializable)
    $taskCommand = $taskConnection.CreateCommand()
    $taskCommand.Transaction = $taskTransaction
    $taskCommand.CommandTimeout = 1800
    $taskOrderColumns = @{ Clientes = 'Cliente'; Cidades = 'Cidade'; Bancos = 'Banco'; Lotacoes = 'Lotacao'; Orgao_Siape = 'Orgao' }
    $taskOrderColumn = $taskOrderColumns[$Table]
    # Table is restricted by ValidateSet and the order column comes from constants.
    $taskCommand.CommandText = @"
SELECT (SELECT c.* FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER)
FROM dbo.[$Table] AS c
ORDER BY c.[$taskOrderColumn];
"@
    $taskReader = $taskCommand.ExecuteReader([Data.CommandBehavior]::SequentialAccess)
    $taskFileStream = [IO.File]::Open($taskPartial, [IO.FileMode]::CreateNew, [IO.FileAccess]::Write, [IO.FileShare]::None)
    $taskWriter = New-Object IO.StreamWriter($taskFileStream, (New-Object Text.UTF8Encoding($false)))
    $taskCount = 0L
    while ($taskReader.Read()) {
        $taskWriter.WriteLine($taskReader.GetString(0))
        $taskCount++
    }
    $taskReader.Dispose()
    $taskReader = $null
    $taskWriter.Dispose()
    $taskWriter = $null
    $taskTransaction.Rollback()
    $taskTransaction.Dispose()
    $taskTransaction = $null
    [IO.File]::Move($taskPartial, $taskOutput)
    Write-Host "Exportação de $Table concluída: $taskCount registros. Nenhum dado foi alterado no SQL Server."
}
catch {
    # Keep a partial file for diagnosis; never treat it as a complete extraction.
    throw 'Exportação interrompida. Confira conexão, permissões e caminho. Um arquivo .partial, se existir, não é uma extração concluída.'
}
finally {
    if ($null -ne $taskReader) { $taskReader.Dispose() }
    if ($null -ne $taskWriter) { $taskWriter.Dispose() }
    if ($null -ne $taskTransaction) { $taskTransaction.Dispose() }
    $taskConnection.Dispose()
}
