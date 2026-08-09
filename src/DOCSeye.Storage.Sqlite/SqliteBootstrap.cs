using Microsoft.Data.Sqlite;
using SQLitePCL;

namespace DOCSeye.Storage.Sqlite;

internal static class SqliteBootstrap
{
    private static int initialized;
    public static void Ensure(){if(Interlocked.Exchange(ref initialized,1)==0){raw.SetProvider(new SQLite3Provider_sqlite3());raw.FreezeProvider();}}
    public static SqliteConnection Open(string path,bool create=true)
    {
        Ensure();var cs=new SqliteConnectionStringBuilder{DataSource=Path.GetFullPath(path),Mode=create?SqliteOpenMode.ReadWriteCreate:SqliteOpenMode.ReadWrite,Cache=SqliteCacheMode.Private,Pooling=false}.ToString();var c=new SqliteConnection(cs);c.Open();using var cmd=c.CreateCommand();cmd.CommandText="PRAGMA journal_mode=DELETE; PRAGMA synchronous=FULL; PRAGMA foreign_keys=OFF; PRAGMA trusted_schema=OFF; PRAGMA recursive_triggers=OFF; PRAGMA busy_timeout=5000;";cmd.ExecuteNonQuery();return c;
    }
}

internal static class Sql
{
    public static void Exec(this SqliteConnection c,SqliteTransaction? tx,string sql,params (string,object?)[] args){using var cmd=c.CreateCommand();cmd.Transaction=tx;cmd.CommandText=sql;foreach(var a in args)cmd.Parameters.AddWithValue(a.Item1,a.Item2??DBNull.Value);cmd.ExecuteNonQuery();}
    public static object? Scalar(this SqliteConnection c,SqliteTransaction? tx,string sql,params (string,object?)[] args){using var cmd=c.CreateCommand();cmd.Transaction=tx;cmd.CommandText=sql;foreach(var a in args)cmd.Parameters.AddWithValue(a.Item1,a.Item2??DBNull.Value);return cmd.ExecuteScalar();}
    public static byte[]? Blob(object? v)=>v is DBNull or null?null:(byte[])v;
}