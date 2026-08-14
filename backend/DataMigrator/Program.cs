using System;
using System.Collections.Generic;
using Npgsql;
using NpgsqlTypes;

class Program
{
    static void Main()
    {
        string connOldStr = "Host=painel.bravida.com.br;Port=5432;Database=webapolice_antigo;Username=bravito;Password=Bravida@2023!";
        string connNewStr = "Host=painel.bravida.com.br;Port=5432;Database=webapolice_teste;Username=bravito;Password=Bravida@2023!";

        using var connOld = new NpgsqlConnection(connOldStr);
        using var connNew = new NpgsqlConnection(connNewStr);
        connOld.Open();
        connNew.Open();
        
        using (var cmd = new NpgsqlCommand("SET session_replication_role = replica;", connNew))
        {
            cmd.ExecuteNonQuery();
        }

        string[] tablesToFix = new[] { 
            "core.pessoa_contato_institucional", 
            "core.banco" 
        };

        foreach(var fullTable in tablesToFix)
        {
            var parts = fullTable.Split('.');
            string schema = parts[0];
            string table = parts[1];

            var cols = new List<string>();
            var colTypes = new Dictionary<string, string>();
            using (var cmd = new NpgsqlCommand($"SELECT column_name, data_type FROM information_schema.columns WHERE table_schema = '{schema}' AND table_name = '{table}'", connOld))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    string colName = reader.GetString(0);
                    cols.Add(colName);
                    colTypes[colName] = reader.GetString(1);
                }
            }

            string colList = string.Join(", ", cols.ConvertAll(c => $"\"{c}\""));
            string paramNames = string.Join(", ", cols.ConvertAll(c => $"@{c}"));
            string insertCmd = $"INSERT INTO {schema}.\"{table}\" ({colList}) OVERRIDING SYSTEM VALUE VALUES ({paramNames})";

            try 
            {
                using var cmdSelect = new NpgsqlCommand($"SELECT {colList} FROM {schema}.\"{table}\"", connOld);
                using var readerSelect = cmdSelect.ExecuteReader();
                
                long rowCount = 0;
                while (readerSelect.Read())
                {
                    using var cmdInsert = new NpgsqlCommand(insertCmd, connNew);
                    for (int i = 0; i < cols.Count; i++)
                    {
                        var colName = cols[i];
                        var p = new NpgsqlParameter($"@{colName}", readerSelect.IsDBNull(i) ? DBNull.Value : readerSelect.GetValue(i));
                        if (colTypes[colName] == "jsonb")
                        {
                            p.NpgsqlDbType = NpgsqlDbType.Jsonb;
                        }
                        cmdInsert.Parameters.Add(p);
                    }
                    cmdInsert.ExecuteNonQuery();
                    rowCount++;
                }
                Console.WriteLine($"  Copied {rowCount} rows for {fullTable}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Error copying {fullTable}: {ex.Message}");
            }
        }
        
        using (var cmd = new NpgsqlCommand("SET session_replication_role = origin;", connNew))
        {
            cmd.ExecuteNonQuery();
        }
    }
}
