using Microsoft.Data.Sqlite;

namespace DOCSeye.Storage.Sqlite;

public static class SqliteMechanismProbe
{
    public static void Initialize(string path)
    {
        foreach (string suffix in new[] { "", "-journal", "-wal", "-shm" }) try { File.Delete(path + suffix); } catch { }
        using var c = SqliteBootstrap.Open(path, true);
        c.Exec(null, "CREATE TABLE probe(id INTEGER PRIMARY KEY CHECK(id=1), value TEXT NOT NULL); INSERT INTO probe VALUES(1,'old'); CREATE TABLE witness(key TEXT PRIMARY KEY,value TEXT NOT NULL);");
    }

    public static string Read(string path)
    {
        using var c = SqliteBootstrap.Open(path, false);
        return Convert.ToString(c.Scalar(null, "SELECT value FROM probe WHERE id=1")) ?? "";
    }

    public static string? ReadWitness(string path, string key)
    {
        using var c = SqliteBootstrap.Open(path, false);
        return Convert.ToString(c.Scalar(null, "SELECT value FROM witness WHERE key=$k", ("$k", key)));
    }

    public static void Worker(string path, string value, string readyFile, string mode)
    {
        using var c = SqliteBootstrap.Open(path, false);
        using var tx = c.BeginTransaction();
        c.Exec(tx, "UPDATE probe SET value=$v WHERE id=1", ("$v", value));
        if (mode == "postcommit-loss")
            c.Exec(tx, "INSERT OR REPLACE INTO witness(key,value) VALUES('commit-key',$v)", ("$v", value));
        File.WriteAllText(readyFile, "ready");
        if (mode == "hold")
        {
            Thread.Sleep(Timeout.Infinite);
            return;
        }
        if (mode == "failfast-before-commit") Environment.FailFast("intentional precommit simulated power loss");
        tx.Commit();
        if (mode == "postcommit-loss") Environment.FailFast("intentional response loss after commit");
    }

    public static (bool FullRaised, string ValueAfter) DiskFull(string path)
    {
        using var c = SqliteBootstrap.Open(path, false);
        long pageCount = Convert.ToInt64(c.Scalar(null, "PRAGMA page_count"));
        c.Exec(null, $"PRAGMA max_page_count={pageCount + 1}");
        bool full = false;
        try
        {
            using var tx = c.BeginTransaction();
            c.Exec(tx, "CREATE TABLE IF NOT EXISTS fill(x BLOB)");
            c.Exec(tx, "INSERT INTO fill(x) VALUES(zeroblob(8388608))");
            c.Exec(tx, "UPDATE probe SET value='diskfull-new' WHERE id=1");
            tx.Commit();
        }
        catch (SqliteException e) when (e.SqliteErrorCode == 13)
        {
            full = true;
        }
        return (full, Read(path));
    }
}