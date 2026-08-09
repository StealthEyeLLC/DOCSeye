using System.Security.Cryptography;
using DOCSeye.Core;
using Microsoft.Data.Sqlite;

namespace DOCSeye.Storage.Sqlite;

public sealed record ValidationReport(bool Writable,string Classification,int FailedLevel,IReadOnlyList<string> Diagnostics,byte[]? ComputedRoot=null);

public sealed class DndStore : IDisposable
{
    public const int ApplicationId=0x444E4431;
    private readonly SqliteConnection c; public string Path{get;}
    private DndStore(string path,SqliteConnection connection){Path=System.IO.Path.GetFullPath(path);c=connection;}
    public static DndStore Open(string path,bool create=false)=>new(path,SqliteBootstrap.Open(path,create));
    public void Dispose()=>c.Dispose();

    public static DndStore Create(string path,SemanticState state,IReadOnlyList<Guid>? parents=null)
    {
        path=System.IO.Path.GetFullPath(path);Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);foreach(string suffix in new[]{"","-journal","-wal","-shm"})try{File.Delete(path+suffix);}catch{}
        var s=Open(path,true);s.CreateSchema();using var tx=s.c.BeginTransaction();
        foreach(var o in state.Objects.Values)s.InsertObject(tx,o);foreach(var b in state.Boundaries.Values)s.InsertBoundary(tx,b);foreach(var e in state.Extensions.Values)s.InsertExtension(tx,e);foreach(var a in state.Assets.Values)s.InsertAsset(tx,a);foreach(var w in state.Retired.Values)s.InsertRetired(tx,w);
        var idx=new MerkleIndex(s.c,tx);foreach(var e in state.RootEntries())idx.Upsert(e);byte[] root=idx.OverallRoot();byte[] independent=state.ComputeRoot();if(!root.SequenceEqual(independent))throw new InvalidOperationException("incremental/full root disagreement on create");
        state.Sequence=Math.Max(0,state.Sequence);s.c.Exec(tx,"INSERT INTO head(id,family_id,branch_id,revision_id,sequence,semantic_root,mode) VALUES(1,$f,$b,$r,$s,$h,$m)",("$f",Ids.RfcBytes(state.FamilyId)),("$b",Ids.RfcBytes(state.BranchId)),("$r",Ids.RfcBytes(state.RevisionId)),("$s",state.Sequence),("$h",root),("$m",SemanticState.ModeToken(state.Mode)));
        byte[]? p1=parents is {Count:>0}?Ids.RfcBytes(parents[0]):null,p2=parents is {Count:>1}?Ids.RfcBytes(parents[1]):null;s.c.Exec(tx,"INSERT INTO revisions(revision_id,branch_id,sequence,semantic_root,parent1,parent2) VALUES($r,$b,$s,$h,$p1,$p2)",("$r",Ids.RfcBytes(state.RevisionId)),("$b",Ids.RfcBytes(state.BranchId)),("$s",state.Sequence),("$h",root),("$p1",p1),("$p2",p2));tx.Commit();return s;
    }
    private void CreateSchema()
    {
        using var cmd=c.CreateCommand();cmd.CommandText=$@"
PRAGMA application_id={ApplicationId}; PRAGMA user_version=100;
CREATE TABLE format_info(key TEXT PRIMARY KEY,value TEXT NOT NULL);
INSERT INTO format_info VALUES('format','DOCSeye DND generation 1'); INSERT INTO format_info VALUES('architecture_freeze','{DndConstants.ArchitectureFreeze}');
CREATE TABLE head(id INTEGER PRIMARY KEY CHECK(id=1),family_id BLOB NOT NULL,branch_id BLOB NOT NULL,revision_id BLOB NOT NULL,sequence INTEGER NOT NULL,semantic_root BLOB NOT NULL,mode TEXT NOT NULL);
CREATE TABLE revisions(row_no INTEGER PRIMARY KEY AUTOINCREMENT,revision_id BLOB NOT NULL,branch_id BLOB NOT NULL,sequence INTEGER NOT NULL,semantic_root BLOB NOT NULL,parent1 BLOB,parent2 BLOB);
CREATE TABLE objects(row_no INTEGER PRIMARY KEY AUTOINCREMENT,id BLOB NOT NULL,type TEXT NOT NULL,parent_id BLOB,order_key BLOB NOT NULL,role TEXT,cbor BLOB NOT NULL,retired INTEGER NOT NULL DEFAULT 0);
CREATE INDEX ix_objects_id ON objects(id); CREATE INDEX ix_objects_parent_order ON objects(parent_id,order_key);
CREATE TABLE boundaries(row_no INTEGER PRIMARY KEY AUTOINCREMENT,id BLOB NOT NULL,owner_id BLOB NOT NULL,scalar_offset INTEGER NOT NULL,affinity TEXT NOT NULL,state TEXT NOT NULL,cbor BLOB NOT NULL);
CREATE INDEX ix_boundaries_id ON boundaries(id); CREATE INDEX ix_boundaries_owner ON boundaries(owner_id);
CREATE TABLE extensions(row_no INTEGER PRIMARY KEY AUTOINCREMENT,id BLOB NOT NULL,namespace_uri TEXT NOT NULL,type_name TEXT NOT NULL,major INTEGER NOT NULL,minor INTEGER NOT NULL,payload_encoding TEXT NOT NULL,payload BLOB NOT NULL,payload_digest BLOB NOT NULL,coverage_kind TEXT NOT NULL,target_id BLOB,property_name TEXT,start_boundary_id BLOB,end_boundary_id BLOB,edit_policy TEXT NOT NULL,required INTEGER NOT NULL,fallback TEXT,cbor BLOB NOT NULL);
CREATE INDEX ix_extensions_id ON extensions(id); CREATE INDEX ix_extensions_target ON extensions(target_id);
CREATE TABLE assets(row_no INTEGER PRIMARY KEY AUTOINCREMENT,digest BLOB NOT NULL,length INTEGER NOT NULL,state TEXT NOT NULL,shell_reference TEXT);
CREATE INDEX ix_assets_digest ON assets(digest);
CREATE TABLE asset_chunks(digest BLOB NOT NULL,chunk_index INTEGER NOT NULL,data BLOB NOT NULL,PRIMARY KEY(digest,chunk_index));
CREATE TABLE retired_witnesses(object_id BLOB NOT NULL,resolution TEXT NOT NULL,successors_cbor BLOB NOT NULL,expires_after_sequence INTEGER NOT NULL);
CREATE TABLE source_capsules(digest BLOB PRIMARY KEY,provider TEXT NOT NULL,source_bytes BLOB NOT NULL);
CREATE TABLE provider_facets(id BLOB PRIMARY KEY,provider TEXT NOT NULL,kind TEXT NOT NULL,target_id BLOB,payload BLOB NOT NULL,digest BLOB NOT NULL,alignment TEXT NOT NULL,required INTEGER NOT NULL);
CREATE TABLE root_entries(domain TEXT NOT NULL,key BLOB NOT NULL,value BLOB NOT NULL,leaf_hash BLOB NOT NULL,PRIMARY KEY(domain,key));
CREATE TABLE merkle_nodes(domain TEXT NOT NULL,depth INTEGER NOT NULL,prefix BLOB NOT NULL,parent_prefix BLOB NOT NULL,last_byte INTEGER NOT NULL,hash BLOB NOT NULL,PRIMARY KEY(domain,depth,prefix));
CREATE INDEX ix_merkle_children ON merkle_nodes(domain,depth,parent_prefix,last_byte);
CREATE TABLE deltas(sequence INTEGER PRIMARY KEY,from_revision_id BLOB NOT NULL,to_revision_id BLOB NOT NULL,delta_cbor BLOB NOT NULL,acknowledged INTEGER NOT NULL DEFAULT 0,expires_after_sequence INTEGER NOT NULL);
CREATE TABLE idempotency(key TEXT PRIMARY KEY,revision_id BLOB NOT NULL,sequence INTEGER NOT NULL,result_cbor BLOB NOT NULL,expires_after_sequence INTEGER NOT NULL);
CREATE TABLE provider_state(key TEXT PRIMARY KEY,value BLOB NOT NULL);
";cmd.ExecuteNonQuery();
    }
    public RevisionHead ReadHead()
    {
        using var cmd=c.CreateCommand();cmd.CommandText="SELECT family_id,branch_id,revision_id,sequence,semantic_root,mode FROM head WHERE id=1";using var r=cmd.ExecuteReader();if(!r.Read())throw new InvalidOperationException("missing head");Guid revision=Ids.GuidFromRfcBytes((byte[])r[2]); string mode=r.GetString(5); return new(Ids.GuidFromRfcBytes((byte[])r[0]),Ids.GuidFromRfcBytes((byte[])r[1]),revision,r.GetInt64(3),(byte[])r[4],ReadParents(revision),mode);
    }
    private IReadOnlyList<Guid> ReadParents(Guid revision){var a=new List<Guid>();using var cmd=c.CreateCommand();cmd.CommandText="SELECT parent1,parent2 FROM revisions WHERE revision_id=$r ORDER BY row_no DESC LIMIT 1";cmd.Parameters.AddWithValue("$r",Ids.RfcBytes(revision));using var r=cmd.ExecuteReader();if(r.Read()){if(r[0] is byte[] p1)a.Add(Ids.GuidFromRfcBytes(p1));if(r[1] is byte[] p2)a.Add(Ids.GuidFromRfcBytes(p2));}return a;}
    private static AuthorityMode ReadMode(string m)=>m switch{"native-authored"=>AuthorityMode.NativeAuthored,"converted/imported"=>AuthorityMode.ConvertedImported,"foreign-managed"=>AuthorityMode.ForeignManaged,_=>throw new InvalidOperationException("invalid mode")};

    public SemanticState LoadState()
    {
        var h=ReadHead();var s=new SemanticState{FamilyId=h.FamilyId,BranchId=h.BranchId,RevisionId=h.RevisionId,Sequence=h.Sequence,Mode=ReadMode(h.Mode)};
        using(var cmd=c.CreateCommand()){cmd.CommandText="SELECT id,type,parent_id,order_key,role,cbor,retired FROM objects ORDER BY row_no";using var r=cmd.ExecuteReader();while(r.Read()){Guid id=Ids.GuidFromRfcBytes((byte[])r[0]);Guid? parent=r[2] is byte[] pb?Ids.GuidFromRfcBytes(pb):null;var map=(Dictionary<string,object?>)CanonicalCbor.Decode((byte[])r[5])!;var data=(Dictionary<string,object?>)map["data"]!;s.Objects[id]=new(id,r.GetString(1),parent,OrderKey.FromBytes((byte[])r[3]),r[4] is DBNull?null:r.GetString(4),data,r.GetInt64(6)!=0);}}
        using(var cmd=c.CreateCommand()){cmd.CommandText="SELECT id,owner_id,scalar_offset,affinity,state FROM boundaries ORDER BY row_no";using var r=cmd.ExecuteReader();while(r.Read()){Guid id=Ids.GuidFromRfcBytes((byte[])r[0]);s.Boundaries[id]=new(id,Ids.GuidFromRfcBytes((byte[])r[1]),checked((int)r.GetInt64(2)),ParseAffinity(r.GetString(3)),Enum.Parse<AnchorState>(r.GetString(4),true));}}
        using(var cmd=c.CreateCommand()){cmd.CommandText="SELECT id,namespace_uri,type_name,major,minor,payload_encoding,payload,payload_digest,coverage_kind,target_id,property_name,start_boundary_id,end_boundary_id,edit_policy,required,fallback FROM extensions ORDER BY row_no";using var r=cmd.ExecuteReader();while(r.Read()){Guid id=Ids.GuidFromRfcBytes((byte[])r[0]);s.Extensions[id]=new(id,r.GetString(1),r.GetString(2),r.GetInt32(3),r.GetInt32(4),r.GetString(5),(byte[])r[6],(byte[])r[7],ParseCoverage(r.GetString(8)),r[9] is byte[] t?Ids.GuidFromRfcBytes(t):null,r[10] is DBNull?null:r.GetString(10),r[11] is byte[] sb?Ids.GuidFromRfcBytes(sb):null,r[12] is byte[] eb?Ids.GuidFromRfcBytes(eb):null,ParsePolicy(r.GetString(13)),r.GetInt64(14)!=0,r[15] is DBNull?null:r.GetString(15));}}
        using(var cmd=c.CreateCommand()){cmd.CommandText="SELECT digest,length,state,shell_reference FROM assets ORDER BY row_no";using var r=cmd.ExecuteReader();while(r.Read()){byte[] d=(byte[])r[0];s.Assets[Convert.ToHexString(d)]=new(d,r.GetInt64(1),r.GetString(2),r[3] is DBNull?null:r.GetString(3));}}
        using(var cmd=c.CreateCommand()){cmd.CommandText="SELECT object_id,resolution,successors_cbor,expires_after_sequence FROM retired_witnesses";using var r=cmd.ExecuteReader();while(r.Read()){Guid id=Ids.GuidFromRfcBytes((byte[])r[0]);var arr=(object?[])CanonicalCbor.Decode((byte[])r[2])!;s.Retired[id]=new(id,Enum.Parse<RetiredResolution>(r.GetString(1),true),arr.Cast<Guid>().ToArray(),r.GetInt64(3));}}
        return s;
    }
    private static EdgeAffinity ParseAffinity(string s)=>s switch{"include_at_edge"=>EdgeAffinity.IncludeAtEdge,"exclude_at_edge"=>EdgeAffinity.ExcludeAtEdge,"before_insertion"=>EdgeAffinity.BeforeInsertion,"after_insertion"=>EdgeAffinity.AfterInsertion,_=>throw new InvalidOperationException()};
    private static ExtensionCoverageKind ParseCoverage(string s)=>s switch{"object"=>ExtensionCoverageKind.Object,"property"=>ExtensionCoverageKind.Property,"subtree"=>ExtensionCoverageKind.Subtree,"text_interval"=>ExtensionCoverageKind.TextInterval,"relation"=>ExtensionCoverageKind.Relation,"topology_region"=>ExtensionCoverageKind.TopologyRegion,"layout_profile"=>ExtensionCoverageKind.LayoutProfile,"document_global"=>ExtensionCoverageKind.DocumentGlobal,_=>throw new InvalidOperationException()};
    private static ExtensionEditPolicy ParsePolicy(string s)=>s switch{"independent"=>ExtensionEditPolicy.Independent,"move_with_target"=>ExtensionEditPolicy.MoveWithTarget,"generic_transform"=>ExtensionEditPolicy.GenericTransform,"invalidate"=>ExtensionEditPolicy.Invalidate,"must_understand_before_edit"=>ExtensionEditPolicy.MustUnderstandBeforeEdit,_=>throw new InvalidOperationException()};

    private void InsertObject(SqliteTransaction tx,SemanticObject o)=>c.Exec(tx,"INSERT INTO objects(id,type,parent_id,order_key,role,cbor,retired) VALUES($i,$t,$p,$o,$r,$c,$x)",("$i",Ids.RfcBytes(o.Id)),("$t",o.Type),("$p",o.ParentId is Guid p?Ids.RfcBytes(p):null),("$o",o.Order.Bytes()),("$r",o.Role),("$c",CanonicalCbor.EncodeObject(o)),("$x",o.Retired?1:0));
    private void InsertBoundary(SqliteTransaction tx,TextBoundary b)=>c.Exec(tx,"INSERT INTO boundaries(id,owner_id,scalar_offset,affinity,state,cbor) VALUES($i,$o,$s,$a,$st,$c)",("$i",Ids.RfcBytes(b.Id)),("$o",Ids.RfcBytes(b.OwnerId)),("$s",b.ScalarOffset),("$a",CanonicalCbor.AffinityToken(b.Affinity)),("$st",b.State.ToString().ToLowerInvariant()),("$c",CanonicalCbor.EncodeBoundary(b)));
    private void InsertExtension(SqliteTransaction tx,ExtensionEnvelope e)=>c.Exec(tx,"INSERT INTO extensions(id,namespace_uri,type_name,major,minor,payload_encoding,payload,payload_digest,coverage_kind,target_id,property_name,start_boundary_id,end_boundary_id,edit_policy,required,fallback,cbor) VALUES($i,$n,$t,$ma,$mi,$pe,$p,$pd,$ck,$ti,$pn,$sb,$eb,$ep,$r,$f,$c)",("$i",Ids.RfcBytes(e.ExtensionId)),("$n",e.NamespaceUri),("$t",e.TypeName),("$ma",e.Major),("$mi",e.Minor),("$pe",e.PayloadEncoding),("$p",e.ExactPayload),("$pd",e.PayloadDigest),("$ck",CanonicalCbor.CoverageToken(e.CoverageKind)),("$ti",e.TargetId is Guid ti?Ids.RfcBytes(ti):null),("$pn",e.PropertyName),("$sb",e.StartBoundaryId is Guid sb?Ids.RfcBytes(sb):null),("$eb",e.EndBoundaryId is Guid eb?Ids.RfcBytes(eb):null),("$ep",CanonicalCbor.PolicyToken(e.EditPolicy)),("$r",e.Required?1:0),("$f",e.Fallback),("$c",CanonicalCbor.EncodeExtension(e)));
    private void InsertAsset(SqliteTransaction tx,AssetCommitment a)=>c.Exec(tx,"INSERT INTO assets(digest,length,state,shell_reference) VALUES($d,$l,$s,$r)",("$d",a.Digest),("$l",a.Length),("$s",a.State),("$r",a.ShellReference));
    private void InsertRetired(SqliteTransaction tx,RetiredWitness w)=>c.Exec(tx,"INSERT INTO retired_witnesses(object_id,resolution,successors_cbor,expires_after_sequence) VALUES($i,$r,$s,$e)",("$i",Ids.RfcBytes(w.ObjectId)),("$r",w.Resolution.ToString().ToLowerInvariant()),("$s",CanonicalCbor.Encode(w.Successors.Cast<object?>().ToArray())),("$e",w.ExpiresAfterSequence));

    public ValidationReport Validate(bool verifyAssets=true)
    {
        var d=new List<string>();try
        {
            object? app=c.Scalar(null,"PRAGMA application_id");object? ver=c.Scalar(null,"PRAGMA user_version");string qc=Convert.ToString(c.Scalar(null,"PRAGMA quick_check"))??"";if(Convert.ToInt64(app)!=ApplicationId||Convert.ToInt64(ver)!=100||qc!="ok")return new(false,"invalid_container",1,["container/application/version/quick_check failed"]);
            foreach(string table in new[]{"objects","boundaries","extensions"})using(var cmd=c.CreateCommand()){cmd.CommandText=$"SELECT cbor FROM {table}";using var r=cmd.ExecuteReader();while(r.Read()){string v=CanonicalCbor.ValidateRecord((byte[])r[0]);if(v!="valid")return new(false,v,2,[$"{table} canonical record {v}"]);}}
            if(HasDuplicates("objects","id")||HasDuplicates("boundaries","id")||HasDuplicates("extensions","id"))return new(false,"duplicate_id",3,["duplicate public/boundary/extension id"]);
            var h=ReadHead();if(!Ids.IsV4(h.FamilyId)||!Ids.IsV4(h.BranchId)||!Ids.IsV4(h.RevisionId))return new(false,"invalid_identity",3,["family/branch/revision is not UUIDv4"]);
            using(var cmd=c.CreateCommand()){cmd.CommandText="SELECT id,parent_id,order_key FROM objects WHERE retired=0";using var r=cmd.ExecuteReader();var ids=new HashSet<Guid>();var rows=new List<(Guid,Guid?,byte[])>();while(r.Read()){Guid id=Ids.GuidFromRfcBytes((byte[])r[0]);if(!Ids.IsV4(id))return new(false,"invalid_identity",3,[$"object {id} is not UUIDv4"]);ids.Add(id);rows.Add((id,r[1] is byte[] p?Ids.GuidFromRfcBytes(p):null,(byte[])r[2]));}foreach(var x in rows)if(x.Item2 is Guid p&&!ids.Contains(p))return new(false,"broken_reference",3,[$"missing parent {p}"]);if(rows.GroupBy(x=>(x.Item2,Convert.ToHexString(x.Item3))).Any(g=>g.Count()>1))return new(false,"invalid_order",4,["tied sibling order key"]);}
            using(var cmd=c.CreateCommand()){cmd.CommandText="SELECT id,owner_id,scalar_offset FROM boundaries";using var r=cmd.ExecuteReader();while(r.Read()){Guid id=Ids.GuidFromRfcBytes((byte[])r[0]),owner=Ids.GuidFromRfcBytes((byte[])r[1]);if(!Ids.IsV4(id))return new(false,"invalid_identity",3,[$"boundary {id} not UUIDv4"]);if(Convert.ToInt64(c.Scalar(null,"SELECT count(*) FROM objects WHERE id=$i AND retired=0",("$i",Ids.RfcBytes(owner))))!=1)return new(false,"broken_reference",3,[$"boundary owner {owner} missing/ambiguous"]);if(r.GetInt64(2)<0)return new(false,"invalid_boundary",4,["negative scalar offset"]);}}
            using(var cmd=c.CreateCommand()){cmd.CommandText="SELECT id,payload,payload_digest FROM extensions";using var r=cmd.ExecuteReader();while(r.Read()){Guid id=Ids.GuidFromRfcBytes((byte[])r[0]);if(!Ids.IsV4(id)||!CryptographicOperations.FixedTimeEquals(SHA256.HashData((byte[])r[1]),(byte[])r[2]))return new(false,"invalid_extension",5,[$"extension {id} invalid"]);}}
            using(var cmd=c.CreateCommand()){cmd.CommandText="SELECT digest,source_bytes FROM source_capsules";using var r=cmd.ExecuteReader();while(r.Read())if(!CryptographicOperations.FixedTimeEquals((byte[])r[0],SHA256.HashData((byte[])r[1])))return new(false,"invalid_capsule",6,["source capsule digest mismatch"]);}
            if(verifyAssets){using var cmd=c.CreateCommand();cmd.CommandText="SELECT digest,length,state FROM assets";using var r=cmd.ExecuteReader();while(r.Read()){byte[] dg=(byte[])r[0];long len=r.GetInt64(1);string st=r.GetString(2);if(st=="embedded"&&HasAssetChunks(dg)){var (actualLen,actual)=HashAsset(dg);if(actualLen!=len||!actual.SequenceEqual(dg))return new(false,"corrupt_asset",6,["embedded asset digest/length mismatch"]);}}}
            var state=LoadState();byte[] root=state.ComputeRoot();if(!root.SequenceEqual(h.SemanticRoot))return new(false,"invalid_root",7,["semantic root mismatch"],root);d.Add("levels 1-7 valid");return new(true,"valid",0,d,root);
        }catch(SqliteException ex){return new(false,"invalid_container",1,[ex.SqliteErrorCode+":"+ex.Message]);}catch(Exception ex){return new(false,"invalid",2,[ex.GetType().Name+":"+ex.Message]);}
    }
    private bool HasDuplicates(string table,string column)=>Convert.ToInt64(c.Scalar(null,$"SELECT count(*) FROM (SELECT {column} FROM {table} GROUP BY {column} HAVING count(*)>1 LIMIT 1)"))>0;
    private bool HasAssetChunks(byte[] digest)=>Convert.ToInt64(c.Scalar(null,"SELECT count(*) FROM asset_chunks WHERE digest=$d",("$d",digest)))>0;
    private (long,byte[]) HashAsset(byte[] digest){using var h=IncrementalHash.CreateHash(HashAlgorithmName.SHA256);long len=0;using var cmd=c.CreateCommand();cmd.CommandText="SELECT data FROM asset_chunks WHERE digest=$d ORDER BY chunk_index";cmd.Parameters.AddWithValue("$d",digest);using var r=cmd.ExecuteReader();while(r.Read()){byte[] b=(byte[])r[0];h.AppendData(b);len+=b.LongLength;}return(len,h.GetHashAndReset());}

    public void VacuumAndRebuildIndexes(){c.Exec(null,"VACUUM");c.Exec(null,"REINDEX");}
    public void RebuildMerkleCache(){using var tx=c.BeginTransaction();MerkleIndex.Rebuild(c,tx);tx.Commit();}
    public void BackupTo(string destination){Directory.CreateDirectory(System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(destination))!);foreach(string s in new[]{"","-journal","-wal","-shm"})try{File.Delete(destination+s);}catch{}using var dst=SqliteBootstrap.Open(destination,true);c.BackupDatabase(dst);}
    public string Digest()=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path))).ToLowerInvariant();
}