using DOCSeye.Core;
using Microsoft.Data.Sqlite;

namespace DOCSeye.Storage.Sqlite;

public sealed class RuntimeIndex : IDisposable
{
    private readonly SqliteConnection c;
    public string Path { get; }

    public RuntimeIndex(string path)
    {
        Path = System.IO.Path.GetFullPath(path);
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path)!);
        SqliteBootstrap.Ensure();
        var cs = new SqliteConnectionStringBuilder
        {
            DataSource = Path,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Private,
            Pooling = false
        }.ToString();
        c = new SqliteConnection(cs);
        c.Open();
        c.Exec(null, "PRAGMA journal_mode=WAL; PRAGMA synchronous=NORMAL; PRAGMA trusted_schema=OFF; CREATE TABLE IF NOT EXISTS meta(key TEXT PRIMARY KEY,value TEXT NOT NULL); CREATE VIRTUAL TABLE IF NOT EXISTS text_fts USING fts5(object_id UNINDEXED,text,tokenize='unicode61');");
    }

    public void Dispose() => c.Dispose();

    public int RebuildFrom(DndStore store)
    {
        var head = store.ReadHead();
        using var tx = c.BeginTransaction();
        c.Exec(tx, "DELETE FROM text_fts; DELETE FROM meta;");
        int count = 0;
        foreach (var obj in store.QueryObjects(limit: int.MaxValue))
        {
            if (obj.Data.TryGetValue("text", out var value) && value is string text)
            {
                c.Exec(tx, "INSERT INTO text_fts(object_id,text) VALUES($i,$t)", ("$i", Ids.Lower(obj.Id)), ("$t", text));
                count++;
            }
        }
        c.Exec(tx,
            "INSERT INTO meta(key,value) VALUES('family_id',$f),('branch_id',$b),('revision_id',$r),('semantic_root',$h)",
            ("$f", Ids.Lower(head.FamilyId)), ("$b", Ids.Lower(head.BranchId)),
            ("$r", Ids.Lower(head.RevisionId)), ("$h", Convert.ToHexString(head.SemanticRoot).ToLowerInvariant()));
        tx.Commit();
        return count;
    }

    public IReadOnlyList<Guid> Search(string query, int limit = 100)
    {
        string literalPhrase = "\"" + query.Replace("\"", "\"\"") + "\"";
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT object_id FROM text_fts WHERE text_fts MATCH $q LIMIT $l";
        cmd.Parameters.AddWithValue("$q", literalPhrase);
        cmd.Parameters.AddWithValue("$l", limit);
        var list = new List<Guid>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) list.Add(Guid.Parse(reader.GetString(0)));
        return list;
    }

    public string? RevisionId => Convert.ToString(c.Scalar(null, "SELECT value FROM meta WHERE key='revision_id'"));
}