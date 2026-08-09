using System.Security.Cryptography;
using DOCSeye.Core;
using Microsoft.Data.Sqlite;

namespace DOCSeye.Storage.Sqlite;

public sealed record ValidationReport(bool Writable,string Classification,int FailedLevel,IReadOnlyList<string> Diagnostics,byte[]? ComputedRoot=null);

public sealed partial class DndStore : IDisposable
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
        foreach(var o in state.Objects.Values)s.InsertObject(tx,o);foreach(var b in state.Boundaries.Values)s.InsertBoundary(tx,b);foreach(var r in state.Ranges.Values)s.InsertRange(tx,r);foreach(var e in state.Extensions.Values)s.InsertExtension(tx,e);foreach(var a in state.Assets.Values)s.InsertAsset(tx,a);foreach(var f in state.ProviderFacets.Values)s.InsertProviderFacet(tx,f);foreach(var capsule in state.SourceCapsules.Values)s.InsertSourceCapsule(tx,capsule);foreach(var w in state.Retired.Values)s.InsertRetired(tx,w);foreach(var o in state.OriginRefs.Values)s.InsertOriginRef(tx,o);
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
CREATE TABLE retained_ranges(row_no INTEGER PRIMARY KEY AUTOINCREMENT,id BLOB NOT NULL,start_boundary_id BLOB NOT NULL,end_boundary_id BLOB NOT NULL,allow_multi_interval INTEGER NOT NULL,kind TEXT NOT NULL,state TEXT NOT NULL,cbor BLOB NOT NULL);
CREATE INDEX ix_ranges_id ON retained_ranges(id); CREATE INDEX ix_ranges_boundaries ON retained_ranges(start_boundary_id,end_boundary_id);
CREATE TABLE extensions(row_no INTEGER PRIMARY KEY AUTOINCREMENT,id BLOB NOT NULL,namespace_uri TEXT NOT NULL,type_name TEXT NOT NULL,major INTEGER NOT NULL,minor INTEGER NOT NULL,payload_encoding TEXT NOT NULL,payload BLOB NOT NULL,payload_digest BLOB NOT NULL,coverage_kind TEXT NOT NULL,target_id BLOB,property_name TEXT,start_boundary_id BLOB,end_boundary_id BLOB,edit_policy TEXT NOT NULL,required INTEGER NOT NULL,fallback TEXT,cbor BLOB NOT NULL);
CREATE INDEX ix_extensions_id ON extensions(id); CREATE INDEX ix_extensions_target ON extensions(target_id);
CREATE TABLE assets(row_no INTEGER PRIMARY KEY AUTOINCREMENT,digest BLOB NOT NULL UNIQUE,length INTEGER NOT NULL,state TEXT NOT NULL,shell_reference TEXT);
CREATE INDEX ix_assets_digest ON assets(digest);
CREATE TABLE asset_chunks(digest BLOB NOT NULL,chunk_index INTEGER NOT NULL,data BLOB NOT NULL,PRIMARY KEY(digest,chunk_index));
CREATE TABLE retired_witnesses(object_id BLOB NOT NULL,resolution TEXT NOT NULL,successors_cbor BLOB NOT NULL,expires_after_sequence INTEGER NOT NULL);
CREATE TABLE origin_refs(row_no INTEGER PRIMARY KEY AUTOINCREMENT,current_id BLOB NOT NULL,kind TEXT NOT NULL,source_family_id BLOB NOT NULL,source_branch_id BLOB NOT NULL,source_revision_id BLOB NOT NULL,source_id BLOB NOT NULL,cbor BLOB NOT NULL);
CREATE INDEX ix_origin_refs_current ON origin_refs(current_id);
CREATE TABLE source_capsules(digest BLOB PRIMARY KEY,provider TEXT NOT NULL,source_bytes BLOB NOT NULL,alignment TEXT NOT NULL);
CREATE TABLE provider_facets(id BLOB PRIMARY KEY,provider TEXT NOT NULL,kind TEXT NOT NULL,target_id BLOB,coverage_kind TEXT NOT NULL,edit_policy TEXT NOT NULL,payload BLOB NOT NULL,digest BLOB NOT NULL,alignment TEXT NOT NULL,required INTEGER NOT NULL);
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
        using(var cmd=c.CreateCommand()){cmd.CommandText="SELECT id,start_boundary_id,end_boundary_id,allow_multi_interval,kind,state FROM retained_ranges ORDER BY row_no";using var r=cmd.ExecuteReader();while(r.Read()){Guid id=Ids.GuidFromRfcBytes((byte[])r[0]);s.Ranges[id]=new(id,Ids.GuidFromRfcBytes((byte[])r[1]),Ids.GuidFromRfcBytes((byte[])r[2]),r.GetInt64(3)!=0,r.GetString(4),r.GetString(5));}}
        using(var cmd=c.CreateCommand()){cmd.CommandText="SELECT id,namespace_uri,type_name,major,minor,payload_encoding,payload,payload_digest,coverage_kind,target_id,property_name,start_boundary_id,end_boundary_id,edit_policy,required,fallback FROM extensions ORDER BY row_no";using var r=cmd.ExecuteReader();while(r.Read()){Guid id=Ids.GuidFromRfcBytes((byte[])r[0]);s.Extensions[id]=new(id,r.GetString(1),r.GetString(2),r.GetInt32(3),r.GetInt32(4),r.GetString(5),(byte[])r[6],(byte[])r[7],ParseCoverage(r.GetString(8)),r[9] is byte[] t?Ids.GuidFromRfcBytes(t):null,r[10] is DBNull?null:r.GetString(10),r[11] is byte[] sb?Ids.GuidFromRfcBytes(sb):null,r[12] is byte[] eb?Ids.GuidFromRfcBytes(eb):null,ParsePolicy(r.GetString(13)),r.GetInt64(14)!=0,r[15] is DBNull?null:r.GetString(15));}}
        using(var cmd=c.CreateCommand()){cmd.CommandText="SELECT digest,length,state,shell_reference FROM assets ORDER BY row_no";using var r=cmd.ExecuteReader();while(r.Read()){byte[] d=(byte[])r[0];s.Assets[Convert.ToHexString(d)]=new(d,r.GetInt64(1),r.GetString(2),r[3] is DBNull?null:r.GetString(3));}}
        using(var cmd=c.CreateCommand()){cmd.CommandText="SELECT id,provider,kind,target_id,coverage_kind,edit_policy,payload,digest,alignment,required FROM provider_facets ORDER BY id";using var r=cmd.ExecuteReader();while(r.Read()){Guid id=Ids.GuidFromRfcBytes((byte[])r[0]);s.ProviderFacets[id]=new(id,r.GetString(1),r.GetString(2),r[3] is byte[] t?Ids.GuidFromRfcBytes(t):null,ParseCoverage(r.GetString(4)),ParsePolicy(r.GetString(5)),(byte[])r[6],(byte[])r[7],r.GetString(8),r.GetInt64(9)!=0);}}
        using(var cmd=c.CreateCommand()){cmd.CommandText="SELECT provider,source_bytes,digest,alignment FROM source_capsules ORDER BY digest";using var r=cmd.ExecuteReader();while(r.Read()){byte[] digest=(byte[])r[2];s.SourceCapsules[Convert.ToHexString(digest)]=new(r.GetString(0),(byte[])r[1],digest,r.GetString(3));}}
        using(var cmd=c.CreateCommand()){cmd.CommandText="SELECT object_id,resolution,successors_cbor,expires_after_sequence FROM retired_witnesses";using var r=cmd.ExecuteReader();while(r.Read()){Guid id=Ids.GuidFromRfcBytes((byte[])r[0]);var arr=(object?[])CanonicalCbor.Decode((byte[])r[2])!;s.Retired[id]=new(id,ParseRetired(r.GetString(1)),arr.Cast<Guid>().ToArray(),r.GetInt64(3));}}
        using(var cmd=c.CreateCommand()){cmd.CommandText="SELECT current_id,kind,source_family_id,source_branch_id,source_revision_id,source_id FROM origin_refs ORDER BY row_no";using var r=cmd.ExecuteReader();while(r.Read()){Guid id=Ids.GuidFromRfcBytes((byte[])r[0]);s.OriginRefs[id]=new(id,r.GetString(1),Ids.GuidFromRfcBytes((byte[])r[2]),Ids.GuidFromRfcBytes((byte[])r[3]),Ids.GuidFromRfcBytes((byte[])r[4]),Ids.GuidFromRfcBytes((byte[])r[5]));}}
        return s;
    }
    private static EdgeAffinity ParseAffinity(string s)=>s switch{"include_at_edge"=>EdgeAffinity.IncludeAtEdge,"exclude_at_edge"=>EdgeAffinity.ExcludeAtEdge,"before_insertion"=>EdgeAffinity.BeforeInsertion,"after_insertion"=>EdgeAffinity.AfterInsertion,_=>throw new InvalidOperationException()};
    private static ExtensionCoverageKind ParseCoverage(string s)=>s switch{"object"=>ExtensionCoverageKind.Object,"property"=>ExtensionCoverageKind.Property,"subtree"=>ExtensionCoverageKind.Subtree,"text_interval"=>ExtensionCoverageKind.TextInterval,"relation"=>ExtensionCoverageKind.Relation,"topology_region"=>ExtensionCoverageKind.TopologyRegion,"layout_profile"=>ExtensionCoverageKind.LayoutProfile,"document_global"=>ExtensionCoverageKind.DocumentGlobal,_=>throw new InvalidOperationException()};
    private static ExtensionEditPolicy ParsePolicy(string s)=>s switch{"independent"=>ExtensionEditPolicy.Independent,"move_with_target"=>ExtensionEditPolicy.MoveWithTarget,"generic_transform"=>ExtensionEditPolicy.GenericTransform,"invalidate"=>ExtensionEditPolicy.Invalidate,"must_understand_before_edit"=>ExtensionEditPolicy.MustUnderstandBeforeEdit,_=>throw new InvalidOperationException()};
    private static RetiredResolution ParseRetired(string s)=>s switch{"destroyed"=>RetiredResolution.Destroyed,"split"=>RetiredResolution.Split,"merged"=>RetiredResolution.Merged,"unknown_retired"=>RetiredResolution.UnknownRetired,_=>throw new InvalidOperationException()};

    private void InsertObject(SqliteTransaction tx,SemanticObject o)=>c.Exec(tx,"INSERT INTO objects(id,type,parent_id,order_key,role,cbor,retired) VALUES($i,$t,$p,$o,$r,$c,$x)",("$i",Ids.RfcBytes(o.Id)),("$t",o.Type),("$p",o.ParentId is Guid p?Ids.RfcBytes(p):null),("$o",o.Order.Bytes()),("$r",o.Role),("$c",CanonicalCbor.EncodeObject(o)),("$x",o.Retired?1:0));
    private void InsertBoundary(SqliteTransaction tx,TextBoundary b)=>c.Exec(tx,"INSERT INTO boundaries(id,owner_id,scalar_offset,affinity,state,cbor) VALUES($i,$o,$s,$a,$st,$c)",("$i",Ids.RfcBytes(b.Id)),("$o",Ids.RfcBytes(b.OwnerId)),("$s",b.ScalarOffset),("$a",CanonicalCbor.AffinityToken(b.Affinity)),("$st",b.State.ToString().ToLowerInvariant()),("$c",CanonicalCbor.EncodeBoundary(b)));
    private void InsertRange(SqliteTransaction tx,RetainedRange r)=>c.Exec(tx,"INSERT INTO retained_ranges(id,start_boundary_id,end_boundary_id,allow_multi_interval,kind,state,cbor) VALUES($i,$s,$e,$m,$k,$st,$c)",("$i",Ids.RfcBytes(r.Id)),("$s",Ids.RfcBytes(r.StartBoundaryId)),("$e",Ids.RfcBytes(r.EndBoundaryId)),("$m",r.AllowMultiInterval?1:0),("$k",r.Kind),("$st",r.State),("$c",CanonicalCbor.EncodeRange(r)));
    private void InsertExtension(SqliteTransaction tx,ExtensionEnvelope e)=>c.Exec(tx,"INSERT INTO extensions(id,namespace_uri,type_name,major,minor,payload_encoding,payload,payload_digest,coverage_kind,target_id,property_name,start_boundary_id,end_boundary_id,edit_policy,required,fallback,cbor) VALUES($i,$n,$t,$ma,$mi,$pe,$p,$pd,$ck,$ti,$pn,$sb,$eb,$ep,$r,$f,$c)",("$i",Ids.RfcBytes(e.ExtensionId)),("$n",e.NamespaceUri),("$t",e.TypeName),("$ma",e.Major),("$mi",e.Minor),("$pe",e.PayloadEncoding),("$p",e.ExactPayload),("$pd",e.PayloadDigest),("$ck",CanonicalCbor.CoverageToken(e.CoverageKind)),("$ti",e.TargetId is Guid ti?Ids.RfcBytes(ti):null),("$pn",e.PropertyName),("$sb",e.StartBoundaryId is Guid sb?Ids.RfcBytes(sb):null),("$eb",e.EndBoundaryId is Guid eb?Ids.RfcBytes(eb):null),("$ep",CanonicalCbor.PolicyToken(e.EditPolicy)),("$r",e.Required?1:0),("$f",e.Fallback),("$c",CanonicalCbor.EncodeExtension(e)));
    private void InsertAsset(SqliteTransaction tx,AssetCommitment a)=>c.Exec(tx,"INSERT INTO assets(digest,length,state,shell_reference) VALUES($d,$l,$s,$r)",("$d",a.Digest),("$l",a.Length),("$s",a.State),("$r",a.ShellReference));
    private void InsertProviderFacet(SqliteTransaction tx,ProviderFacet f)=>c.Exec(tx,"INSERT INTO provider_facets(id,provider,kind,target_id,coverage_kind,edit_policy,payload,digest,alignment,required) VALUES($i,$p,$k,$t,$c,$e,$b,$d,$a,$r)",("$i",Ids.RfcBytes(f.Id)),("$p",f.Provider),("$k",f.Kind),("$t",f.TargetId is Guid t?Ids.RfcBytes(t):null),("$c",CanonicalCbor.CoverageToken(f.CoverageKind)),("$e",CanonicalCbor.PolicyToken(f.EditPolicy)),("$b",f.ExactPayload),("$d",f.Digest),("$a",f.Alignment),("$r",f.Required?1:0));
    private void InsertSourceCapsule(SqliteTransaction tx,SourceCapsuleEvidence capsule)=>c.Exec(tx,"INSERT INTO source_capsules(digest,provider,source_bytes,alignment) VALUES($d,$p,$b,$a)",("$d",capsule.Digest),("$p",capsule.Provider),("$b",capsule.ExactBytes),("$a",capsule.Alignment));
    private void InsertRetired(SqliteTransaction tx,RetiredWitness w)=>c.Exec(tx,"INSERT INTO retired_witnesses(object_id,resolution,successors_cbor,expires_after_sequence) VALUES($i,$r,$s,$e)",("$i",Ids.RfcBytes(w.ObjectId)),("$r",CanonicalCbor.RetiredToken(w.Resolution)),("$s",CanonicalCbor.Encode(w.Successors.Cast<object?>().ToArray())),("$e",w.ExpiresAfterSequence));
    private void InsertOriginRef(SqliteTransaction tx,OriginRef o)=>c.Exec(tx,"INSERT INTO origin_refs(current_id,kind,source_family_id,source_branch_id,source_revision_id,source_id,cbor) VALUES($i,$k,$f,$b,$r,$s,$c)",("$i",Ids.RfcBytes(o.CurrentId)),("$k",o.Kind),("$f",Ids.RfcBytes(o.SourceFamilyId)),("$b",Ids.RfcBytes(o.SourceBranchId)),("$r",Ids.RfcBytes(o.SourceRevisionId)),("$s",Ids.RfcBytes(o.SourceId)),("$c",CanonicalCbor.EncodeOriginRef(o)));

    public ValidationReport Validate(bool verifyAssets=true)
    {
        var d=new List<string>();try
        {
            object? app=c.Scalar(null,"PRAGMA application_id");object? ver=c.Scalar(null,"PRAGMA user_version");string qc=Convert.ToString(c.Scalar(null,"PRAGMA quick_check"))??"";if(Convert.ToInt64(app)!=ApplicationId||Convert.ToInt64(ver)!=100||qc!="ok")return new(false,"invalid_container",1,["container/application/version/quick_check failed"]);
            foreach(string table in new[]{"objects","boundaries","retained_ranges","extensions","origin_refs"})using(var cmd=c.CreateCommand()){cmd.CommandText=$"SELECT cbor FROM {table}";using var r=cmd.ExecuteReader();while(r.Read()){string v=CanonicalCbor.ValidateRecord((byte[])r[0]);if(v!="valid")return new(false,v,2,[$"{table} canonical record {v}"]);}}
            if(HasDuplicates("objects","id")||HasDuplicates("boundaries","id")||HasDuplicates("retained_ranges","id")||HasDuplicates("extensions","id")||HasDuplicates("origin_refs","current_id")||HasPublicIdCollision())return new(false,"duplicate_id",3,["duplicate public identity"]);
            var h=ReadHead();if(!Ids.IsV4(h.FamilyId)||!Ids.IsV4(h.BranchId)||!Ids.IsV4(h.RevisionId))return new(false,"invalid_identity",3,["family/branch/revision is not UUIDv4"]);
            using(var cmd=c.CreateCommand()){cmd.CommandText="SELECT id,parent_id,order_key FROM objects WHERE retired=0";using var r=cmd.ExecuteReader();var ids=new HashSet<Guid>();var rows=new List<(Guid,Guid?,byte[])>();while(r.Read()){Guid id=Ids.GuidFromRfcBytes((byte[])r[0]);if(!Ids.IsV4(id))return new(false,"invalid_identity",3,[$"object {id} is not UUIDv4"]);ids.Add(id);rows.Add((id,r[1] is byte[] p?Ids.GuidFromRfcBytes(p):null,(byte[])r[2]));}foreach(var x in rows)if(x.Item2 is Guid p&&!ids.Contains(p))return new(false,"broken_reference",3,[$"missing parent {p}"]);if(rows.GroupBy(x=>(x.Item2,Convert.ToHexString(x.Item3))).Any(g=>g.Count()>1))return new(false,"invalid_order",4,["tied sibling order key"]);}
            using(var cmd=c.CreateCommand()){cmd.CommandText="SELECT id,owner_id,scalar_offset FROM boundaries";using var r=cmd.ExecuteReader();while(r.Read()){Guid id=Ids.GuidFromRfcBytes((byte[])r[0]),owner=Ids.GuidFromRfcBytes((byte[])r[1]);if(!Ids.IsV4(id))return new(false,"invalid_identity",3,[$"boundary {id} not UUIDv4"]);if(Convert.ToInt64(c.Scalar(null,"SELECT count(*) FROM objects WHERE id=$i AND retired=0",("$i",Ids.RfcBytes(owner))))!=1)return new(false,"broken_reference",3,[$"boundary owner {owner} missing/ambiguous"]);if(r.GetInt64(2)<0)return new(false,"invalid_boundary",4,["negative scalar offset"]);}}
            using(var cmd=c.CreateCommand()){cmd.CommandText="SELECT id,start_boundary_id,end_boundary_id FROM retained_ranges";using var r=cmd.ExecuteReader();while(r.Read()){Guid id=Ids.GuidFromRfcBytes((byte[])r[0]);if(!Ids.IsV4(id))return new(false,"invalid_identity",3,[$"range {id} not UUIDv4"]);foreach(int col in new[]{1,2}){byte[] b=(byte[])r[col];if(Convert.ToInt64(c.Scalar(null,"SELECT count(*) FROM boundaries WHERE id=$i",("$i",b)))!=1)return new(false,"broken_reference",3,[$"range {id} boundary missing"]);}}}
            using(var cmd=c.CreateCommand()){cmd.CommandText="SELECT id,payload,payload_digest FROM extensions";using var r=cmd.ExecuteReader();while(r.Read()){Guid id=Ids.GuidFromRfcBytes((byte[])r[0]);if(!Ids.IsV4(id)||!CryptographicOperations.FixedTimeEquals(SHA256.HashData((byte[])r[1]),(byte[])r[2]))return new(false,"invalid_extension",5,[$"extension {id} invalid"]);}}
            using(var cmd=c.CreateCommand()){cmd.CommandText="SELECT id,target_id,payload,digest FROM provider_facets";using var r=cmd.ExecuteReader();while(r.Read()){Guid id=Ids.GuidFromRfcBytes((byte[])r[0]);if(!Ids.IsV4(id)||!CryptographicOperations.FixedTimeEquals(SHA256.HashData((byte[])r[2]),(byte[])r[3]))return new(false,"invalid_provider_facet",5,[$"provider facet {id} invalid"]);if(r[1] is byte[] t&&Convert.ToInt64(c.Scalar(null,"SELECT count(*) FROM objects WHERE id=$i AND retired=0",("$i",t)))!=1)return new(false,"broken_reference",5,[$"provider facet {id} target missing"]);}}
            using(var cmd=c.CreateCommand()){cmd.CommandText="SELECT digest,source_bytes FROM source_capsules";using var r=cmd.ExecuteReader();while(r.Read())if(!CryptographicOperations.FixedTimeEquals((byte[])r[0],SHA256.HashData((byte[])r[1])))return new(false,"invalid_capsule",6,["source capsule digest mismatch"]);}
            if(verifyAssets){using var cmd=c.CreateCommand();cmd.CommandText="SELECT digest,length,state FROM assets";using var r=cmd.ExecuteReader();while(r.Read()){byte[] dg=(byte[])r[0];long len=r.GetInt64(1);string st=r.GetString(2);if(st=="embedded"){if(!HasAssetChunks(dg))return new(false,"corrupt_asset",6,["embedded asset bytes missing"]);var (actualLen,actual)=HashAsset(dg);if(actualLen!=len||!actual.SequenceEqual(dg))return new(false,"corrupt_asset",6,["embedded asset digest/length mismatch"]);}}}
            var state=LoadState();byte[] root=state.ComputeRoot();if(!root.SequenceEqual(h.SemanticRoot))return new(false,"invalid_root",7,["semantic root mismatch"],root);d.Add("levels 1-7 valid");return new(true,"valid",0,d,root);
        }catch(SqliteException ex){return new(false,"invalid_container",1,[ex.SqliteErrorCode+":"+ex.Message]);}catch(Exception ex){return new(false,"invalid",2,[ex.GetType().Name+":"+ex.Message]);}
    }
    private bool HasDuplicates(string table,string column)=>Convert.ToInt64(c.Scalar(null,$"SELECT count(*) FROM (SELECT {column} FROM {table} GROUP BY {column} HAVING count(*)>1 LIMIT 1)"))>0;
    private bool HasPublicIdCollision()=>Convert.ToInt64(c.Scalar(null,@"SELECT count(*) FROM (SELECT id FROM objects UNION ALL SELECT id FROM boundaries UNION ALL SELECT id FROM retained_ranges UNION ALL SELECT id FROM extensions UNION ALL SELECT id FROM provider_facets) q GROUP BY id HAVING count(*)>1 LIMIT 1"))>0;
    private bool HasAssetChunks(byte[] digest)=>Convert.ToInt64(c.Scalar(null,"SELECT count(*) FROM asset_chunks WHERE digest=$d",("$d",digest)))>0;
    private (long,byte[]) HashAsset(byte[] digest){using var h=IncrementalHash.CreateHash(HashAlgorithmName.SHA256);long len=0;using var cmd=c.CreateCommand();cmd.CommandText="SELECT data FROM asset_chunks WHERE digest=$d ORDER BY chunk_index";cmd.Parameters.AddWithValue("$d",digest);using var r=cmd.ExecuteReader();while(r.Read()){byte[] b=(byte[])r[0];h.AppendData(b);len+=b.LongLength;}return(len,h.GetHashAndReset());}

    public SemanticObject? ReadObject(Guid id)
    {
        using var cmd=c.CreateCommand();cmd.CommandText="SELECT id,type,parent_id,order_key,role,cbor,retired FROM objects WHERE id=$i ORDER BY row_no DESC LIMIT 1";cmd.Parameters.AddWithValue("$i",Ids.RfcBytes(id));using var r=cmd.ExecuteReader();if(!r.Read())return null;
        var map=(Dictionary<string,object?>)CanonicalCbor.Decode((byte[])r[5])!;var data=(Dictionary<string,object?>)map["data"]!;
        return new(id,r.GetString(1),r[2] is byte[] p?Ids.GuidFromRfcBytes(p):null,OrderKey.FromBytes((byte[])r[3]),r[4] is DBNull?null:r.GetString(4),data,r.GetInt64(6)!=0);
    }

    public IReadOnlyList<SemanticObject> QueryObjects(string? type=null,string? role=null,int limit=10000)
    {
        using var cmd=c.CreateCommand();var where=new List<string>{"retired=0"};if(type is not null){where.Add("type=$t");cmd.Parameters.AddWithValue("$t",type);}if(role is not null){where.Add("role=$r");cmd.Parameters.AddWithValue("$r",role);}cmd.Parameters.AddWithValue("$l",limit);cmd.CommandText=$"SELECT id,type,parent_id,order_key,role,cbor,retired FROM objects WHERE {string.Join(" AND ",where)} ORDER BY parent_id,order_key LIMIT $l";
        var list=new List<SemanticObject>();using var r=cmd.ExecuteReader();while(r.Read()){Guid id=Ids.GuidFromRfcBytes((byte[])r[0]);var map=(Dictionary<string,object?>)CanonicalCbor.Decode((byte[])r[5])!;var data=(Dictionary<string,object?>)map["data"]!;list.Add(new(id,r.GetString(1),r[2] is byte[] p?Ids.GuidFromRfcBytes(p):null,OrderKey.FromBytes((byte[])r[3]),r[4] is DBNull?null:r.GetString(4),data,false));}return list;
    }

    public (TransactionResult Result,LocalMutationMetrics Metrics) CommitObjectData(Guid objectId,Dictionary<string,object?> newData,Guid expectedRevisionId,string idempotencyKey)
    {
        var before=ReadHead();if(before.RevisionId!=expectedRevisionId)return(new(false,"stale_revision",before,null,["expected revision mismatch"]),new(0,0,0,0,0));
        using var tx=c.BeginTransaction();
        try
        {
            using var q=c.CreateCommand();q.Transaction=tx;q.CommandText="SELECT type,parent_id,order_key,role,cbor,retired FROM objects WHERE id=$i ORDER BY row_no DESC LIMIT 1";q.Parameters.AddWithValue("$i",Ids.RfcBytes(objectId));using var r=q.ExecuteReader();if(!r.Read()){tx.Rollback();return(new(false,"not_found",before,null,["object not found"]),new(1,0,0,0,0));}
            if(r.GetInt64(5)!=0){tx.Rollback();return(new(false,"retired_object",before,null,["object retired"]),new(1,0,0,0,0));}
            string type=r.GetString(0);Guid? parent=r[1] is byte[] pb?Ids.GuidFromRfcBytes(pb):null;var order=OrderKey.FromBytes((byte[])r[2]);string? role=r[3] is DBNull?null:r.GetString(3);long bytesRead=((byte[])r[4]).LongLength;r.Close();
            var obj=new SemanticObject(objectId,type,parent,order,role,newData,false);byte[] encoded=CanonicalCbor.EncodeObject(obj);c.Exec(tx,"UPDATE objects SET cbor=$c WHERE id=$i",("$c",encoded),("$i",Ids.RfcBytes(objectId)));
            var idx=new MerkleIndex(c,tx);idx.Upsert(RootEntry.ForObject(obj));byte[] root=idx.OverallRoot();Guid revision=Ids.NewV4();long seq=before.Sequence+1;
            c.Exec(tx,"INSERT INTO revisions(revision_id,branch_id,sequence,semantic_root,parent1,parent2) VALUES($r,$b,$s,$h,$p,NULL)",("$r",Ids.RfcBytes(revision)),("$b",Ids.RfcBytes(before.BranchId)),("$s",seq),("$h",root),("$p",Ids.RfcBytes(before.RevisionId)));c.Exec(tx,"UPDATE head SET revision_id=$r,sequence=$s,semantic_root=$h WHERE id=1",("$r",Ids.RfcBytes(revision)),("$s",seq),("$h",root));
            byte[] deltaBytes=CanonicalCbor.Encode(new Dictionary<string,object?>{{"kind","object_data"},{"object_id",objectId},{"data",newData}});c.Exec(tx,"INSERT INTO deltas(sequence,from_revision_id,to_revision_id,delta_cbor,acknowledged,expires_after_sequence) VALUES($s,$f,$t,$d,0,$e)",("$s",seq),("$f",Ids.RfcBytes(before.RevisionId)),("$t",Ids.RfcBytes(revision)),("$d",deltaBytes),("$e",seq+64));byte[] resultBytes=CanonicalCbor.Encode(new Dictionary<string,object?>{{"classification","committed"},{"revision_id",revision},{"semantic_root",root}});c.Exec(tx,"INSERT OR REPLACE INTO idempotency(key,revision_id,sequence,result_cbor,expires_after_sequence) VALUES($k,$r,$s,$b,$e)",("$k",idempotencyKey),("$r",Ids.RfcBytes(revision)),("$s",seq),("$b",resultBytes),("$e",seq+64));tx.Commit();
            var head=new RevisionHead(before.FamilyId,before.BranchId,revision,seq,root,[before.RevisionId],before.Mode);var delta=new SemanticDelta(before.BranchId,before.RevisionId,revision,[objectId],[],[],["object:"+Ids.Lower(objectId)],deltaBytes);return(new(true,"committed",head,delta,[]),new(1,1,SemanticRoot.BoundedPathNodesPerEntry,bytesRead,encoded.LongLength));
        }
        catch(Exception ex){try{tx.Rollback();}catch{}return(new(false,"transaction_failed",before,null,[ex.GetType().Name+":"+ex.Message]),new(0,0,0,0,0));}
    }

    public (TransactionResult Result,LocalMutationMetrics Metrics,byte[] Digest) CommitEmbeddedAsset(Stream source,Guid expectedRevisionId,string idempotencyKey,Guid? figureObjectId=null,int chunkBytes=1024*1024,Action<int>? chunkProgress=null)
    {
        if(chunkBytes<64*1024||chunkBytes>8*1024*1024)throw new ArgumentOutOfRangeException(nameof(chunkBytes));
        string? spool=null;Stream input=source;long originalPosition=0;
        try
        {
            if(!source.CanSeek)
            {
                spool=System.IO.Path.Combine(System.IO.Path.GetTempPath(),"docseye-asset-"+Guid.NewGuid().ToString("N")+".bin");using(var fs=File.Create(spool))source.CopyTo(fs);input=File.OpenRead(spool);
            }
            originalPosition=input.Position;using var hash=IncrementalHash.CreateHash(HashAlgorithmName.SHA256);byte[] buffer=new byte[chunkBytes];long length=0;int read;
            while((read=input.Read(buffer,0,buffer.Length))>0){hash.AppendData(buffer,0,read);length+=read;}byte[] digest=hash.GetHashAndReset();input.Position=originalPosition;
            var before=ReadHead();if(before.RevisionId!=expectedRevisionId)return(new(false,"stale_revision",before,null,["expected revision mismatch"]),new(0,0,0,0,0),digest);
            using var tx=c.BeginTransaction();
            try
            {
                c.Exec(tx,"INSERT OR IGNORE INTO assets(digest,length,state,shell_reference) VALUES($d,$l,'embedded',NULL)",("$d",digest),("$l",length));
                int chunkIndex=0;long written=0;while((read=input.Read(buffer,0,buffer.Length))>0){byte[] chunk=read==buffer.Length?buffer[..read]:buffer[..read];c.Exec(tx,"INSERT OR IGNORE INTO asset_chunks(digest,chunk_index,data) VALUES($d,$i,$b)",("$d",digest),("$i",chunkIndex),("$b",chunk));written+=read;chunkProgress?.Invoke(chunkIndex);chunkIndex++;}
                if(written!=length)throw new IOException("asset stream length changed between hash and commit");
                var idxMerkle=new MerkleIndex(c,tx);idxMerkle.Upsert(RootEntry.ForAsset(new AssetCommitment(digest,length,"embedded")));int logicalWrites=1;long bytesWritten=length;var changedObjects=new List<Guid>();
                if(figureObjectId is Guid figure)
                {
                    using var q=c.CreateCommand();q.Transaction=tx;q.CommandText="SELECT type,parent_id,order_key,role,cbor,retired FROM objects WHERE id=$i ORDER BY row_no DESC LIMIT 1";q.Parameters.AddWithValue("$i",Ids.RfcBytes(figure));using var r=q.ExecuteReader();if(!r.Read()||r.GetInt64(5)!=0)throw new InvalidOperationException("figure object not found");string type=r.GetString(0);Guid? parent=r[1] is byte[] pb?Ids.GuidFromRfcBytes(pb):null;var order=OrderKey.FromBytes((byte[])r[2]);string? role=r[3] is DBNull?null:r.GetString(3);var map=(Dictionary<string,object?>)CanonicalCbor.Decode((byte[])r[4])!;var data=(Dictionary<string,object?>)map["data"]!;r.Close();var nextData=new Dictionary<string,object?>(data,StringComparer.Ordinal){["asset_digest"]=digest};var obj=new SemanticObject(figure,type,parent,order,role,nextData,false);byte[] encoded=CanonicalCbor.EncodeObject(obj);c.Exec(tx,"UPDATE objects SET cbor=$c WHERE id=$i",("$c",encoded),("$i",Ids.RfcBytes(figure)));idxMerkle.Upsert(RootEntry.ForObject(obj));logicalWrites++;bytesWritten+=encoded.LongLength;changedObjects.Add(figure);
                }
                byte[] root=idxMerkle.OverallRoot();Guid revision=Ids.NewV4();long seq=before.Sequence+1;c.Exec(tx,"INSERT INTO revisions(revision_id,branch_id,sequence,semantic_root,parent1,parent2) VALUES($r,$b,$s,$h,$p,NULL)",("$r",Ids.RfcBytes(revision)),("$b",Ids.RfcBytes(before.BranchId)),("$s",seq),("$h",root),("$p",Ids.RfcBytes(before.RevisionId)));c.Exec(tx,"UPDATE head SET revision_id=$r,sequence=$s,semantic_root=$h WHERE id=1",("$r",Ids.RfcBytes(revision)),("$s",seq),("$h",root));byte[] deltaBytes=CanonicalCbor.Encode(new Dictionary<string,object?>{{"kind","embedded_asset"},{"digest",digest},{"length",length},{"figure_id",figureObjectId}});c.Exec(tx,"INSERT INTO deltas(sequence,from_revision_id,to_revision_id,delta_cbor,acknowledged,expires_after_sequence) VALUES($s,$f,$t,$d,0,$e)",("$s",seq),("$f",Ids.RfcBytes(before.RevisionId)),("$t",Ids.RfcBytes(revision)),("$d",deltaBytes),("$e",seq+64));byte[] resultBytes=CanonicalCbor.Encode(new Dictionary<string,object?>{{"classification","committed"},{"revision_id",revision},{"semantic_root",root},{"asset_digest",digest}});c.Exec(tx,"INSERT OR REPLACE INTO idempotency(key,revision_id,sequence,result_cbor,expires_after_sequence) VALUES($k,$r,$s,$b,$e)",("$k",idempotencyKey),("$r",Ids.RfcBytes(revision)),("$s",seq),("$b",resultBytes),("$e",seq+64));tx.Commit();var head=new RevisionHead(before.FamilyId,before.BranchId,revision,seq,root,[before.RevisionId],before.Mode);var delta=new SemanticDelta(before.BranchId,before.RevisionId,revision,changedObjects,[],[],["asset"],deltaBytes);return(new(true,"committed",head,delta,[]),new(1,logicalWrites,SemanticRoot.BoundedPathNodesPerEntry*(figureObjectId is null?1:2),length,bytesWritten),digest);
            }
            catch(Exception ex){try{tx.Rollback();}catch{}return(new(false,"transaction_failed",before,null,[ex.GetType().Name+":"+ex.Message]),new(1,0,0,length,0),digest);}
        }
        finally{if(input!=source)input.Dispose();if(source.CanSeek)try{source.Position=originalPosition;}catch{}if(spool is not null)try{File.Delete(spool);}catch{}}
    }
    public DeltaReadResult ReadDeltas(long cursorSequence)
    {
        var head=ReadHead();long? min=null;object? m=c.Scalar(null,"SELECT min(sequence) FROM deltas");if(m is not null&&m is not DBNull)min=Convert.ToInt64(m);if((min is long mn&&cursorSequence<mn-1)||(min is null&&cursorSequence<head.Sequence))return new("cursor_expired",cursorSequence,head.Sequence,[],true);
        var list=new List<byte[]>();using var cmd=c.CreateCommand();cmd.CommandText="SELECT delta_cbor FROM deltas WHERE sequence>$s ORDER BY sequence";cmd.Parameters.AddWithValue("$s",cursorSequence);using var r=cmd.ExecuteReader();while(r.Read())list.Add((byte[])r[0]);return new("ok",cursorSequence,head.Sequence,list,false);
    }

    public void AcknowledgeDeltasThrough(long sequence)=>c.Exec(null,"UPDATE deltas SET acknowledged=1 WHERE sequence<=$s",("$s",sequence));
    public void PruneExpiredWitnesses(long keepFromSequence)
    {
        c.Exec(null,"DELETE FROM deltas WHERE sequence<$s",("$s",keepFromSequence));c.Exec(null,"DELETE FROM idempotency WHERE expires_after_sequence<$s",("$s",keepFromSequence));
    }
    public TransactionResult CommitBoundaryOffset(Guid boundaryId,int newOffset,Guid expectedRevisionId,string idempotencyKey)
    {
        if(newOffset<0)return new(false,"invalid_boundary",null,null,["negative scalar offset"]);
        var before=ReadHead();
        if(before.RevisionId!=expectedRevisionId)return new(false,"stale_revision",before,null,["expected revision mismatch"]);
        using var tx=c.BeginTransaction();
        try
        {
            using var q=c.CreateCommand();q.Transaction=tx;q.CommandText="SELECT id,owner_id,scalar_offset,affinity,state FROM boundaries WHERE id=$i";q.Parameters.AddWithValue("$i",Ids.RfcBytes(boundaryId));
            using var r=q.ExecuteReader();if(!r.Read()){tx.Rollback();return new(false,"not_found",before,null,["boundary not found"]);}
            var b=new TextBoundary(boundaryId,Ids.GuidFromRfcBytes((byte[])r[1]),newOffset,ParseAffinity(r.GetString(3)),Enum.Parse<AnchorState>(r.GetString(4),true));r.Close();
            byte[] encoded=CanonicalCbor.EncodeBoundary(b);
            c.Exec(tx,"UPDATE boundaries SET scalar_offset=$o,cbor=$c WHERE id=$i",("$o",newOffset),("$c",encoded),("$i",Ids.RfcBytes(boundaryId)));
            var idx=new MerkleIndex(c,tx);idx.Upsert(RootEntry.ForBoundary(b));byte[] root=idx.OverallRoot();
            Guid revision=Ids.NewV4();long seq=before.Sequence+1;
            c.Exec(tx,"INSERT INTO revisions(revision_id,branch_id,sequence,semantic_root,parent1,parent2) VALUES($r,$b,$s,$h,$p,NULL)",("$r",Ids.RfcBytes(revision)),("$b",Ids.RfcBytes(before.BranchId)),("$s",seq),("$h",root),("$p",Ids.RfcBytes(before.RevisionId)));
            c.Exec(tx,"UPDATE head SET revision_id=$r,sequence=$s,semantic_root=$h WHERE id=1",("$r",Ids.RfcBytes(revision)),("$s",seq),("$h",root));
            byte[] deltaBytes=CanonicalCbor.Encode(new Dictionary<string,object?>{{"kind","boundary_offset"},{"boundary_id",boundaryId},{"new_offset",newOffset}});
            c.Exec(tx,"INSERT INTO deltas(sequence,from_revision_id,to_revision_id,delta_cbor,acknowledged,expires_after_sequence) VALUES($s,$f,$t,$d,0,$e)",("$s",seq),("$f",Ids.RfcBytes(before.RevisionId)),("$t",Ids.RfcBytes(revision)),("$d",deltaBytes),("$e",seq+64));
            byte[] resultBytes=CanonicalCbor.Encode(new Dictionary<string,object?>{{"classification","committed"},{"revision_id",revision},{"semantic_root",root}});
            c.Exec(tx,"INSERT OR REPLACE INTO idempotency(key,revision_id,sequence,result_cbor,expires_after_sequence) VALUES($k,$r,$s,$b,$e)",("$k",idempotencyKey),("$r",Ids.RfcBytes(revision)),("$s",seq),("$b",resultBytes),("$e",seq+64));
            tx.Commit();
            var head=new RevisionHead(before.FamilyId,before.BranchId,revision,seq,root,[before.RevisionId],before.Mode);
            var delta=new SemanticDelta(before.BranchId,before.RevisionId,revision,[],[boundaryId],[],["text-boundary-owner"],deltaBytes);
            return new(true,"committed",head,delta,[$"merkle_path_nodes<={SemanticRoot.BoundedPathNodesPerEntry}"]);
        }
        catch(Exception ex){try{tx.Rollback();}catch{}return new(false,"transaction_failed",before,null,[ex.GetType().Name+":"+ex.Message]);}
    }

    public int MerkleNodeCount()
    {
        using var cmd=c.CreateCommand();cmd.CommandText="SELECT count(*) FROM merkle_nodes";return Convert.ToInt32(cmd.ExecuteScalar());
    }
    public void VacuumAndRebuildIndexes(){c.Exec(null,"VACUUM");c.Exec(null,"REINDEX");}
    public void RebuildMerkleCache(){using var tx=c.BeginTransaction();MerkleIndex.Rebuild(c,tx);tx.Commit();}
    public void BackupTo(string destination){Directory.CreateDirectory(System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(destination))!);foreach(string s in new[]{"","-journal","-wal","-shm"})try{File.Delete(destination+s);}catch{}using var dst=SqliteBootstrap.Open(destination,true);c.BackupDatabase(dst);}
    public string Digest()=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path))).ToLowerInvariant();
}