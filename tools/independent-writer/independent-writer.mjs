import { DatabaseSync } from 'node:sqlite';
import { createHash, randomBytes } from 'node:crypto';

const DOMAINS=['objects','boundaries','extensions','assets','relations','layout','commit'];
const sha=(...parts)=>{const h=createHash('sha256'); for(const p of parts)h.update(p); return h.digest();};
const ascii=s=>Buffer.from(s,'ascii');
const part=b=>{b=Buffer.from(b);const n=Buffer.alloc(4);n.writeInt32BE(b.length);return Buffer.concat([n,b]);};
const keyFor=(kind,id)=>sha(ascii('DOCSeye:DND1:key\0'),ascii(kind),Buffer.from([0]),Buffer.from(id));
const leafHash=(domain,key,value)=>sha(ascii('DOCSeye:DND1:leaf\0'),ascii(domain),Buffer.from([0]),key,sha(value));
const emptyHash=(domain,depth)=>sha(ascii('DOCSeye:DND1:empty\0'),ascii(domain),Buffer.from([0,depth]));
const internalHash=(domain,depth,children)=>{
  if(children.length===0)return emptyHash(domain,depth);
  children=[...children].sort((a,b)=>a[0]-b[0]);
  return sha(ascii('DOCSeye:DND1:node\0'),part(ascii(domain)),Buffer.from([depth]),...children.flatMap(([b,h])=>[Buffer.from([b]),h]));
};
function domainRoot(domain,entries){
  function walk(rows,depth){
    if(depth===32){if(rows.length!==1)throw new Error('duplicate root key');return leafHash(domain,rows[0].key,rows[0].value);}
    if(rows.length===0)return emptyHash(domain,depth);
    const groups=new Map();for(const r of rows){const b=r.key[depth];if(!groups.has(b))groups.set(b,[]);groups.get(b).push(r);}
    return internalHash(domain,depth,[...groups].map(([b,g])=>[b,walk(g,depth+1)]));
  }
  return walk(entries,0);
}
function overallRoot(entries){
  const roots=new Map();for(const d of DOMAINS)roots.set(d,domainRoot(d,entries.filter(e=>e.domain===d)));
  return sha(ascii('DOCSeye:DND1:semantic-root\0'),...DOMAINS.flatMap(d=>[part(ascii(d)),part(roots.get(d))]));
}
function head(major,n){
  n=BigInt(n);let ai;
  if(n<24n)return Buffer.from([(major<<5)|Number(n)]);
  if(n<=0xffn)return Buffer.from([(major<<5)|24,Number(n)]);
  if(n<=0xffffn){const b=Buffer.alloc(3);b[0]=(major<<5)|25;b.writeUInt16BE(Number(n),1);return b;}
  if(n<=0xffffffffn){const b=Buffer.alloc(5);b[0]=(major<<5)|26;b.writeUInt32BE(Number(n),1);return b;}
  const b=Buffer.alloc(9);b[0]=(major<<5)|27;b.writeBigUInt64BE(n,1);return b;
}
const taggedUuid=b=>({__uuid:Buffer.from(b)});
function cbor(v){
  if(v===null||v===undefined)return Buffer.from([0xf6]);
  if(typeof v==='boolean')return Buffer.from([v?0xf5:0xf4]);
  if(typeof v==='number'){if(!Number.isSafeInteger(v))throw new Error('non-integer canonical number');return v>=0?head(0,v):head(1,-1-v);}
  if(typeof v==='bigint')return v>=0n?head(0,v):head(1,-1n-v);
  if(typeof v==='string'){const b=Buffer.from(v,'utf8');return Buffer.concat([head(3,b.length),b]);}
  if(Buffer.isBuffer(v)||v instanceof Uint8Array){const b=Buffer.from(v);return Buffer.concat([head(2,b.length),b]);}
  if(v&&v.__uuid){const b=Buffer.from(v.__uuid);if(b.length!==16)throw new Error('uuid length');return Buffer.concat([Buffer.from([0xd8,37]),head(2,16),b]);}
  if(Array.isArray(v)){const vals=v.map(cbor);return Buffer.concat([head(4,vals.length),...vals]);}
  if(typeof v==='object'){
    const pairs=Object.entries(v).map(([k,val])=>{if(!/^[\x00-\x7f]*$/.test(k))throw new Error('non-ascii key');const ek=cbor(k);return [ek,cbor(val)];});
    pairs.sort((a,b)=>Buffer.compare(a[0],b[0]));return Buffer.concat([head(5,pairs.length),...pairs.flat()]);
  }
  throw new Error('unsupported CBOR type');
}
function v4bytes(){const b=randomBytes(16);b[6]=(b[6]&0x0f)|0x40;b[8]=(b[8]&0x3f)|0x80;return b;}
function affinityToken(s){return s;}
function boundaryCbor(row,newOffset){return cbor({id:taggedUuid(row.id),owner_id:taggedUuid(row.owner_id),scalar_offset:newOffset,affinity:affinityToken(row.affinity),state:row.state});}

function moveBoundary(dbPath,idHex,delta){
  const db=new DatabaseSync(dbPath,{readOnly:false});
  db.exec('PRAGMA journal_mode=DELETE; PRAGMA synchronous=FULL; PRAGMA trusted_schema=OFF; BEGIN IMMEDIATE');
  try{
    const id=Buffer.from(idHex.replaceAll('-',''),'hex');
    const row=db.prepare('SELECT id,owner_id,scalar_offset,affinity,state FROM boundaries WHERE id=?').get(id);
    if(!row)throw new Error('boundary not found');
    const newOffset=Number(row.scalar_offset)+Number(delta);if(newOffset<0)throw new Error('invalid offset');
    const value=boundaryCbor({id:Buffer.from(row.id),owner_id:Buffer.from(row.owner_id),affinity:row.affinity,state:row.state},newOffset);
    db.prepare('UPDATE boundaries SET scalar_offset=?,cbor=? WHERE id=?').run(newOffset,value,id);
    const key=keyFor('boundary',id);
    db.prepare('INSERT INTO root_entries(domain,key,value,leaf_hash) VALUES(?,?,?,?) ON CONFLICT(domain,key) DO UPDATE SET value=excluded.value,leaf_hash=excluded.leaf_hash').run('boundaries',key,value,leafHash('boundaries',key,value));
    const rows=db.prepare('SELECT domain,key,value FROM root_entries ORDER BY domain,key').all().map(r=>({domain:r.domain,key:Buffer.from(r.key),value:Buffer.from(r.value)}));
    const root=overallRoot(rows);
    const old=db.prepare('SELECT branch_id,revision_id,sequence FROM head WHERE id=1').get();
    const revision=v4bytes(), seq=Number(old.sequence)+1;
    db.prepare('INSERT INTO revisions(revision_id,branch_id,sequence,semantic_root,parent1,parent2) VALUES(?,?,?,?,?,NULL)').run(revision,old.branch_id,seq,root,old.revision_id);
    db.prepare('UPDATE head SET revision_id=?,sequence=?,semantic_root=? WHERE id=1').run(revision,seq,root);
    const deltaBytes=cbor({kind:'external_boundary_move',boundary_id:taggedUuid(id),new_offset:newOffset});
    db.prepare('INSERT OR REPLACE INTO deltas(sequence,from_revision_id,to_revision_id,delta_cbor,acknowledged,expires_after_sequence) VALUES(?,?,?,?,0,?)').run(seq,old.revision_id,revision,deltaBytes,seq+64);
    db.exec('COMMIT');
    console.log(JSON.stringify({classification:'committed',sequence:seq,revision_id:revision.toString('hex'),semantic_root:root.toString('hex'),boundary_id:idHex,new_offset:newOffset}));
  }catch(e){try{db.exec('ROLLBACK')}catch{};throw e;}finally{db.close();}
}

const [cmd,...args]=process.argv.slice(2);
if(cmd==='move-boundary')moveBoundary(args[0],args[1],Number(args[2]));
else if(cmd==='cbor-vector'){const value={a:1,z:'e\u0301',uuid:taggedUuid(Buffer.from('00112233445546778899aabbccddeeff','hex')),bytes:Buffer.from('00ff10','hex')};console.log(cbor(value).toString('hex'));}
else throw new Error(`unknown command ${cmd}`);