using System.Text;
using System.Text.Json;

namespace ClientesMigrationSimulator;

public static class Reports
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private static readonly JsonSerializerOptions Lines = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    public static async Task WriteAsync(string output, List<Plan> plans, List<SourceRow> rows, TargetSnapshot target,
        string sourceHash, DateOnly today)
    {
        var classifications = plans.GroupBy(p => p.Classification).ToDictionary(g => g.Key, g => g.Count());
        var issues = plans.SelectMany(p => p.Issues).GroupBy(i => new { i.Severity, i.Code, i.Field })
            .Select(g => new { g.Key.Severity, g.Key.Code, g.Key.Field, Count = g.Count() }).OrderByDescending(i => i.Count).ToList();
        var catalogCoverage = new[] { ("Cidade", "cidade"), ("CidadeC", "cidade"), ("Banco", "banco"), ("Lotacao", "lotacao"), ("Cli_Siape_Orgao", "siape_orgao") }
            .Select(pair =>
            {
                var references = rows.Where(r => r.Text(pair.Item1) is not null).ToList();
                return new
                {
                    Field = pair.Item1, ReferencedRows = references.Count,
                    MappedRows = target.Verified ? (int?)references.Count(r => r.Number(pair.Item1) is { } id && target.Catalogs[pair.Item2].ContainsKey(id)) : null,
                    DistinctLegacyCodes = references.Select(r => r.Text(pair.Item1)).Distinct().Count()
                };
            }).ToList();
        var summary = new
        {
            Mode = "SIMULACAO_SEM_ESCRITA", GeneratedAt = DateTimeOffset.UtcNow, AnalysisDate = today, SourceSha256 = sourceHash,
            Total = plans.Count, TargetVerified = target.Verified, TargetCapturedAt = target.CapturedAt, target.ActiveStatusId,
            LoadAuthorized = false, Classifications = classifications, Issues = issues, SchemaIssues = target.SchemaIssues,
            CatalogCoverage = catalogCoverage,
            CandidateGroups = plans.Where(p => p.Classification is not ("BLOQUEADO" or "IGNORADO")).Select(p => p.GroupKey).Distinct().Count(),
            IgnoredWithoutName = plans.Count(p => p.Issues.Any(i => i.Code == "NOME_AUSENTE_IGNORADO")),
            IgnoredWithoutCpf = plans.Count(p => p.Issues.Any(i => i.Code == "CPF_AUSENTE_IGNORADO")),
            DisregardedFields = new[] { "Cli_Status", "Grupo", "Subgrupo", "Cli_Empresa", "Cli_Rubrica", "Cidade_Lotacao", "Cli_Aposentado_Pensionista", "Cli_ant", "Cli_id_cliente_vind" },
            AlreadyMapped = plans.Count(p => p.Action == "JA_MAPEADO_REVISAR"),
            AddressCandidatesBeforeDeduplication = plans.Sum(p => p.Addresses.Count),
            ChildCandidatesBeforeDeduplication = plans.SelectMany(p => p.ChildCandidates).GroupBy(x => x.Key).ToDictionary(g => g.Key, g => g.Sum(x => x.Value)),
            Note = "Pronto significa pronto para revisão da simulação, não autorização de carga. Contagens de registros filhos são anteriores à deduplicação e incluem cadastros com pendências. Os valores completos e a deduplicação de filhos ainda não constituem um plano de escrita."
        };
        await File.WriteAllTextAsync(Path.Combine(output, "resumo.json"), JsonSerializer.Serialize(summary, Options), Encoding.UTF8);
        await using (var writer = new StreamWriter(Path.Combine(output, "plano.jsonl"), false, new UTF8Encoding(false)))
            foreach (var plan in plans) await writer.WriteLineAsync(JsonSerializer.Serialize(plan, Lines));
        await using (var writer = new StreamWriter(Path.Combine(output, "pendencias.csv"), false, new UTF8Encoding(true)))
        {
            await writer.WriteLineAsync("cliente_legado;severidade;codigo;campo");
            foreach (var plan in plans)
                foreach (var issue in plan.Issues)
                    await writer.WriteLineAsync($"{plan.LegacyId};{issue.Severity};{issue.Code};{issue.Field}");
        }
        await using (var writer = new StreamWriter(Path.Combine(output, "nao-importados.csv"), false, new UTF8Encoding(true)))
        {
            await writer.WriteLineAsync("cliente_legado;classificacao;motivos");
            foreach (var plan in plans.Where(p => p.Classification is "IGNORADO" or "BLOQUEADO"))
                await writer.WriteLineAsync($"{plan.LegacyId};{plan.Classification};{string.Join('|', plan.Issues.Where(i => i.Severity == "BLOQUEIO" || i.Code.EndsWith("_IGNORADO")).Select(i => i.Code).Distinct())}");
        }
        // Schema only: does not serialize names, documents, addresses or connection strings.
        await File.WriteAllTextAsync(Path.Combine(output, "schema-destino.json"), JsonSerializer.Serialize(new
        {
            target.Verified, target.CapturedAt, target.Columns, target.Constraints, target.Indexes, target.SchemaIssues
        }, Options), Encoding.UTF8);
        var report = new StringBuilder("# Simulação da migração de clientes\n\n");
        report.AppendLine($"Cadastros analisados: **{plans.Count:N0}**. Destino consultado: **{(target.Verified ? "sim" : "não")}**.");
        report.AppendLine("\nNenhum banco foi alterado. A unificação é uma proposta e precisa ser revisada antes da carga.\n");
        report.AppendLine("| Classificação | Cadastros |\n|---|---:|");
        foreach (var entry in classifications) report.AppendLine($"| {entry.Key} | {entry.Value:N0} |");
        report.AppendLine("\n## Pendências por frequência\n\n| Severidade | Código | Campo | Ocorrências |\n|---|---|---|---:|");
        foreach (var issue in issues) report.AppendLine($"| {issue.Severity} | {issue.Code} | {issue.Field} | {issue.Count:N0} |");
        report.AppendLine("\nUm cadastro pode ter várias pendências; a soma das ocorrências não é o total de clientes.");
        if (target.SchemaIssues.Count > 0)
        {
            report.AppendLine("\n## Pendências do schema\n");
            foreach (var issue in target.SchemaIssues) report.AppendLine("- " + issue);
        }
        report.AppendLine("\n## Limites desta etapa\n\n- Sem destino, não há confirmação de catálogos, registros existentes ou estrutura instalada.");
        report.AppendLine("- O schema é inventariado e campos essenciais são conferidos; constraints adicionais ainda exigem revisão antes de escrever.");
        report.AppendLine("- O plano contém IDs, sexo convertido, status proposto, grupos candidatos e seleção de endereço. Não é um arquivo de carga.");
        report.AppendLine("- Endereços usam RESIDENCIAL (sem C) e CORRESPONDENCIA (C). Documentos do cônjuge vão para cadastro.cliente_dependente; a migration de RG precisa estar aplicada no destino.");
        report.AppendLine("- Retenção integral dos dados de origem depende do arquivo JSONL original, identificado pelo SHA-256 em resumo.json; ele não é modificado nem copiado para os relatórios.");
        await File.WriteAllTextAsync(Path.Combine(output, "relatorio.md"), report.ToString(), Encoding.UTF8);
    }
}
