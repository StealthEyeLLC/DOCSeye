using DOCSeye.Core;
using Microsoft.Data.Sqlite;

namespace DOCSeye.Storage.Sqlite;

internal sealed class MerkleIndex(SqliteConnection connection,SqliteTransaction transaction)
{
    private readonly SqliteConnection c=connection; private readonly SqliteTransaction tx=transaction;
    public void Upsert(RootEntry e)
    {
        byte[] leaf=SemanticRoot.LeafHash(e.Domain,e.Key,e.Value);c.Exec(tx,"INSERT INTO root_entries(domain,key,value,leaf_hash) VALUES($d,$k,$v,$h) ON CONFLICT(domain,key) DO UPDATE SET value=excluded.value,leaf_hash=excluded.leaf_hash",("$d",e.Domain),("$k",e.Key),("$v",e.Value),("$h",leaf));
        UpsertNode(e.Domain,32,e.Key,e.Key[..31],e.Key[31],leaf);RecomputePath(e.Domain,e.Key);
    }
    public void Delete(string domain,byte[] key)
    {
        c.Exec(tx,"DELETE FROM root_entries WHERE domain=$d AND key=$k",("$d",domain),("$k",key));c.Exec(tx,"DELETE FROM merkle_nodes WHERE domain=$d AND depth=32 AND prefix=$p",("$d",domain),("$p",key));RecomputePath(domain,key);
    }
    private void RecomputePath(string domain,byte[] key)
    {
        for(int depth=31;depth>=0;depth--)
        {
            byte[] prefix=key[..depth];var children=new List<(byte child,byte[] hash)>();using(var cmd=c.CreateCommand()){cmd.Transaction=tx;cmd.CommandText="SELECT last_byte,hash FROM merkle_nodes WHERE domain=$d AND depth=$x AND parent_prefix=$p ORDER BY last_byte";cmd.Parameters.AddWithValue("$d",domain);cmd.Parameters.AddWithValue("$x",depth+1);cmd.Parameters.AddWithValue("$p",prefix);using var r=cmd.ExecuteReader();while(r.Read())children.Add(((byte)r.GetInt32(0),(byte[])r[1]));}
            if(children.Count==0&&depth>0){c.Exec(tx,"DELETE FROM merkle_nodes WHERE domain=$d AND depth=$x AND prefix=$p",("$d",domain),("$x",depth),("$p",prefix));continue;}
            byte[] hash=SemanticRoot.InternalHash(domain,depth,children);byte[] parent=depth==0?[]:prefix[..^1];int last=depth==0?-1:prefix[^1];UpsertNode(domain,depth,prefix,parent,last,hash);
        }
    }
    private void UpsertNode(string domain,int depth,byte[] prefix,byte[] parent,int last,byte[] hash)=>c.Exec(tx,"INSERT INTO merkle_nodes(domain,depth,prefix,parent_prefix,last_byte,hash) VALUES($d,$x,$p,$pp,$b,$h) ON CONFLICT(domain,depth,prefix) DO UPDATE SET parent_prefix=excluded.parent_prefix,last_byte=excluded.last_byte,hash=excluded.hash",("$d",domain),("$x",depth),("$p",prefix),("$pp",parent),("$b",last),("$h",hash));
    public byte[] OverallRoot()
    {
        var roots=new Dictionary<string,byte[]>(StringComparer.Ordinal);foreach(string d in DndConstants.RootDomains){var v=c.Scalar(tx,"SELECT hash FROM merkle_nodes WHERE domain=$d AND depth=0 AND length(prefix)=0",("$d",d));roots[d]=v is byte[] b?b:SemanticRoot.EmptyHash(d,0);}return SemanticRoot.CombineDomainRoots(roots);
    }
    public static void Rebuild(SqliteConnection c,SqliteTransaction tx)
    {
        c.Exec(tx,"DELETE FROM merkle_nodes");var rows=new List<RootEntry>();using(var cmd=c.CreateCommand()){cmd.Transaction=tx;cmd.CommandText="SELECT domain,key,value FROM root_entries ORDER BY domain,key";using var r=cmd.ExecuteReader();while(r.Read())rows.Add(new(r.GetString(0),(byte[])r[1],(byte[])r[2]));}var idx=new MerkleIndex(c,tx);foreach(var e in rows)idx.Upsert(e);
    }
}