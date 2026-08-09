using System.Security.Cryptography;
using DOCSeye.Core;
using Microsoft.Data.Sqlite;

namespace DOCSeye.Storage.Sqlite;

public sealed partial class DndStore
{
    public IReadOnlyList<Guid> RevisionIds()
    {
        var ids = new List<Guid>();
        using var cmd = c.CreateCommand(); cmd.CommandText = "SELECT DISTINCT revision_id FROM revisions ORDER BY row_no";
        using var r = cmd.ExecuteReader(); while (r.Read()) ids.Add(Ids.GuidFromRfcBytes((byte[])r[0]));
        return ids;
    }

    public bool ContainsRevision(Guid revisionId) => Convert.ToInt64(c.Scalar(null,
        "SELECT count(*) FROM revisions WHERE revision_id=$r", ("$r", Ids.RfcBytes(revisionId)))) > 0;

    public bool IsAncestor(Guid ancestor, Guid descendant)
    {
        if (ancestor == descendant) return true;
        var seen = new HashSet<Guid>(); var stack = new Stack<Guid>(); stack.Push(descendant);
        while (stack.Count > 0)
        {
            Guid current = stack.Pop(); if (!seen.Add(current)) continue;
            using var cmd = c.CreateCommand(); cmd.CommandText = "SELECT parent1,parent2 FROM revisions WHERE revision_id=$r ORDER BY row_no DESC LIMIT 1";
            cmd.Parameters.AddWithValue("$r", Ids.RfcBytes(current)); using var r = cmd.ExecuteReader(); if (!r.Read()) continue;
            foreach (int col in new[] { 0, 1 }) if (r[col] is byte[] p)
            {
                Guid parent = Ids.GuidFromRfcBytes(p); if (parent == ancestor) return true; stack.Push(parent);
            }
        }
        return false;
    }

    public ReplicaPresentation PresentWith(DndStore other)
    {
        var a = ReadHead(); var b = other.ReadHead();
        if (a.FamilyId != b.FamilyId) return new("separate_families", true, null, [a.RevisionId, b.RevisionId]);
        if (a.BranchId != b.BranchId) return new("separate_branches", true, null, [a.RevisionId, b.RevisionId]);
        if (a.RevisionId == b.RevisionId)
            return a.SemanticRoot.SequenceEqual(b.SemanticRoot) ? new("same_head", true, a.RevisionId, [a.RevisionId]) : new("invalid_root_identity", false, a.RevisionId, [a.RevisionId]);
        if (other.IsAncestor(a.RevisionId, b.RevisionId)) return new("right_descends_from_left", true, a.RevisionId, [b.RevisionId]);
        if (IsAncestor(b.RevisionId, a.RevisionId)) return new("left_descends_from_right", true, b.RevisionId, [a.RevisionId]);
        Guid? common = RevisionIds().Intersect(other.RevisionIds()).LastOrDefault();
        return common is Guid g && g != Guid.Empty
            ? new("divergent_heads", false, g, [a.RevisionId, b.RevisionId])
            : new("unrelated_same_branch", false, null, [a.RevisionId, b.RevisionId]);
    }

    public TransactionResult CommitObjectData(PortableWriteHandle handle, Dictionary<string, object?> newData, string idempotencyKey)
    {
        var head = ReadHead();
        if (handle.FamilyId != head.FamilyId) return new(false, "wrong_family", head, null, ["portable handle family mismatch"]);
        if (handle.BranchId != head.BranchId) return new(false, "cross_branch_handle", head, null, ["portable handle branch mismatch"]);
        if (handle.ExpectedRevisionId != head.RevisionId) return new(false, "stale_revision", head, null, ["portable handle revision mismatch"]);
        return CommitObjectData(handle.ObjectId, newData, handle.ExpectedRevisionId, idempotencyKey).Result;
    }

    public void InitializeEmbeddedAssetBytes(byte[] digest, ReadOnlySpan<byte> bytes, int chunkBytes = 1024 * 1024)
    {
        if (chunkBytes < 64 * 1024 || chunkBytes > 8 * 1024 * 1024) throw new ArgumentOutOfRangeException(nameof(chunkBytes));
        byte[] actual = SHA256.HashData(bytes); if (!CryptographicOperations.FixedTimeEquals(actual, digest)) throw new InvalidOperationException("asset digest mismatch");
        object? length = c.Scalar(null, "SELECT length FROM assets WHERE digest=$d AND state='embedded'", ("$d", digest));
        if (length is null || length is DBNull || Convert.ToInt64(length) != bytes.Length) throw new InvalidOperationException("asset commitment missing or length mismatch");
        if (Convert.ToInt64(c.Scalar(null, "SELECT count(*) FROM asset_chunks WHERE digest=$d", ("$d", digest))) != 0) throw new InvalidOperationException("asset bytes already initialized");
        using var tx = c.BeginTransaction(); int index = 0;
        for (int offset = 0; offset < bytes.Length; offset += chunkBytes)
        {
            int n = Math.Min(chunkBytes, bytes.Length - offset); byte[] chunk = bytes.Slice(offset, n).ToArray();
            c.Exec(tx, "INSERT INTO asset_chunks(digest,chunk_index,data) VALUES($d,$i,$b)", ("$d", digest), ("$i", index++), ("$b", chunk));
        }
        tx.Commit();
    }

    public void ReconcilePrivateMerkleCache()
    {
        var head=ReadHead();var state=LoadState();var expected=state.RootEntries();
        bool rootEntriesMatch=Convert.ToInt64(c.Scalar(null,"SELECT count(*) FROM root_entries"))==expected.Count;
        if(rootEntriesMatch)
        {
            foreach(var e in expected)
            {
                object? value=c.Scalar(null,"SELECT value FROM root_entries WHERE domain=$d AND key=$k",("$d",e.Domain),("$k",e.Key));
                if(value is not byte[] bytes||!bytes.SequenceEqual(e.Value)){rootEntriesMatch=false;break;}
            }
        }
        using var tx=c.BeginTransaction();
        if(!rootEntriesMatch)
        {
            c.Exec(tx,"DELETE FROM root_entries; DELETE FROM merkle_nodes;");var idx=new MerkleIndex(c,tx);foreach(var e in expected)idx.Upsert(e);
            if(!idx.OverallRoot().SequenceEqual(head.SemanticRoot))throw new InvalidOperationException("validated semantic state/cache root disagreement");
        }
        else
        {
            var idx=new MerkleIndex(c,tx);if(!idx.OverallRoot().SequenceEqual(head.SemanticRoot)){MerkleIndex.Rebuild(c,tx);if(!idx.OverallRoot().SequenceEqual(head.SemanticRoot))throw new InvalidOperationException("merkle cache rebuild disagreement");}
        }
        tx.Commit();
    }}