using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ClientesMigrationSimulator;

if (args.SequenceEqual(["--self-test"])) return SelfTests.Run();
if (args.Length == 0 || args.Contains("--help"))
{
    Console.WriteLine("Simulação somente de leitura.\n" +
        "dotnet run --project backend/tools/ClientesMigrationSimulator -- --source clientes.jsonl --output pasta-nova [--target] [--prefer-source CLIENTE:PESSOA]\n" +
        "--target lê a conexão da variável WEBAPOLICE_MIGRATION_TARGET (PostgreSQL de homologação).\n" +
        "Sem --target, a conferência do destino fica explicitamente pendente.\n" +
        "--self-test executa cenários sintéticos sem conexão.");
    return 0;
}
try
{
    string? source = null, output = null;
    var useTarget = false;
    var authorizedOverrides = new Dictionary<int, long>();
    for (var i = 0; i < args.Length; i++)
    {
        switch (args[i])
        {
            case "--source" when i + 1 < args.Length: source = args[++i]; break;
            case "--output" when i + 1 < args.Length: output = args[++i]; break;
            case "--target": useTarget = true; break;
            case "--prefer-source" when i + 1 < args.Length:
                var pair = args[++i].Split(':');
                if (pair.Length != 2 || !int.TryParse(pair[0], out var sourceId) || !long.TryParse(pair[1], out var personId))
                    throw new ArgumentException("Use --prefer-source CLIENTE_LEGADO:PESSOA_DESTINO somente para prioridade autorizada.");
                authorizedOverrides.Add(sourceId, personId);
                break;
            default: throw new ArgumentException("Opção desconhecida ou sem valor. Consulte --help.");
        }
    }
    if (source is null || output is null) throw new ArgumentException("Informe --source e --output.");
    source = Path.GetFullPath(source);
    output = Path.GetFullPath(output);
    if (!File.Exists(source)) throw new ArgumentException("Arquivo de origem não encontrado.");
    if (Directory.Exists(output) || File.Exists(output))
        throw new ArgumentException("Use uma pasta de saída nova para não sobrescrever relatórios.");

    var rows = new List<SourceRow>();
    var ids = new HashSet<int>();
    // Hash the same bytes read by the analysis (no second read of a mutable file).
    using var stream = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read);
    var hash = Convert.ToHexString(SHA256.HashData(stream));
    stream.Position = 0;
    using var reader = new StreamReader(stream, Encoding.UTF8, true, leaveOpen: true);
    var lineNumber = 0;
    while (reader.ReadLine() is { } line)
    {
        lineNumber++;
        if (string.IsNullOrWhiteSpace(line)) continue;
        try
        {
            var row = SourceRow.Parse(line);
            if (row.Id <= 0 || !ids.Add(row.Id)) throw new FormatException();
            if (SourceRow.RequiredFields.Any(f => !row.Has(f))) throw new FormatException();
            rows.Add(row);
        }
        catch (Exception ex) when (ex is JsonException or FormatException or OverflowException or ArgumentNullException)
        {
            throw new ArgumentException($"Origem inválida na linha {lineNumber}: confira JSON, campos obrigatórios e ID único. Nenhum relatório foi gerado.");
        }
    }
    if (rows.Count == 0) throw new ArgumentException("A origem está vazia.");
    Console.WriteLine($"Origem lida: {rows.Count} cadastros. Conferindo regras.");
    var target = new TargetSnapshot();
    if (useTarget)
    {
        var connection = Environment.GetEnvironmentVariable("WEBAPOLICE_MIGRATION_TARGET");
        if (string.IsNullOrWhiteSpace(connection)) throw new ArgumentException("Defina WEBAPOLICE_MIGRATION_TARGET para usar --target.");
        target = await TargetReader.ReadAsync(connection);
    }
    var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTimeOffset.UtcNow,
        "America/Sao_Paulo").DateTime);
    var plans = Simulator.Analyze(rows, target, today, authorizedOverrides);
    Directory.CreateDirectory(output);
    await Reports.WriteAsync(output, plans, rows, target, hash, today);
    Console.WriteLine($"Simulação concluída: {plans.Count} cadastros. Relatórios em {output}.");
    Console.WriteLine(target.Verified ? "Destino consultado em transação somente de leitura." : "Destino NÃO consultado; relatório incompleto para carga.");
    Console.WriteLine("Nenhum dado foi gravado no banco. Consulte resumo.json e relatorio.md.");
    return 0;
}
catch (ArgumentException ex)
{
    Console.Error.WriteLine(ex.Message);
    return 1;
}
catch (Exception ex)
{
    // Do not print database exceptions/connection strings/PII to console.
    Console.Error.WriteLine($"Simulação interrompida ({ex.GetType().Name}). Confira arquivos, permissões e conexão. Nenhuma operação de escrita em banco está implementada.");
    return 1;
}
