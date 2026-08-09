import net from 'node:net';
import fs from 'node:fs';
import path from 'node:path';
import {spawnSync} from 'node:child_process';
import {randomUUID} from 'node:crypto';

const [pipeName,dndPath,manifestPath,workDir,evidencePath,typstExe,veraPdfBat,javaHome,figureSvg,writerPath]=process.argv.slice(2);
if(!writerPath) throw new Error('program-host arguments incomplete');
const invocationId=randomUUID();
const transcript=[];
let callCount=0;
let seq=1;
let buffer='';
const lines=[];
let lineWaiter=null;
function pushLine(line){if(lineWaiter){const r=lineWaiter;lineWaiter=null;r(line);}else lines.push(line);}
function nextLine(){if(lines.length)return Promise.resolve(lines.shift());return new Promise(r=>lineWaiter=r);}
async function connect(){for(let i=0;i<120;i++){try{return await new Promise((resolve,reject)=>{const s=net.createConnection('\\\\.\\pipe\\'+pipeName,()=>resolve(s));s.once('error',reject);});}catch{await new Promise(r=>setTimeout(r,50));}}throw new Error('kernel pipe unavailable');}
const socket=await connect();
socket.setEncoding('utf8');
socket.on('data',chunk=>{buffer+=chunk;for(;;){const i=buffer.indexOf('\n');if(i<0)break;const line=buffer.slice(0,i).replace(/\r$/,'');buffer=buffer.slice(i+1);if(line)pushLine(line);}});
async function call(id,extra={}){const params={callId:id,...extra};const request={jsonrpc:'2.0',id:seq++,method:id,params};const raw=JSON.stringify(request);socket.write(raw+'\n');const response=JSON.parse(await nextLine());if(response.error)throw new Error(`${id}: ${response.error.code}: ${response.error.message}`);callCount++;transcript.push({ordinal:callCount,id,requestBytes:Buffer.byteLength(raw),result:response.result});return response.result;}

let q18;
for(let i=1;i<=18;i++){
  const id=`D.Q-${String(i).padStart(2,'0')}`;
  const extra=i===1?{dndPath,manifestPath,workDir,evidencePath,typstExe,veraPdfBat,javaHome,figureSvg,nodePid:process.pid,invocationId}:{};
  const result=await call(id,extra);if(i===18)q18=result;
}
await call('D.T-01');for(let i=1;i<=16;i++)await call(`D.M-${String(i).padStart(2,'0')}`);await call('D.T-02');
await call('D.G-01');await call('D.G-02');await call('D.V-01');await call('D.V-02');await call('D.V-03');
const external=spawnSync(process.execPath,[writerPath,'move-object',dndPath,q18.externalAction.targetId,String(q18.externalAction.index)],{encoding:'utf8',windowsHide:true});
if(external.status!==0)throw new Error(`external writer failed: ${external.stderr}`);const externalResult=JSON.parse(external.stdout.trim());if(externalResult.classification!=='committed')throw new Error('external writer did not commit');
for(const id of ['D.R-01','D.R-02','D.R-03','D.R-04'])await call(id);
await call('D.T-03');for(let i=17;i<=32;i++)await call(`D.M-${String(i).padStart(2,'0')}`);await call('D.T-04');await call('D.G-03');await call('D.G-04');await call('D.V-04');
await call('D.T-05');for(let i=33;i<=48;i++)await call(`D.M-${String(i).padStart(2,'0')}`);await call('D.T-06');await call('D.G-05');await call('D.G-06');await call('D.V-05');await call('D.V-06');
for(const id of ['D.L-01','D.L-02','D.L-03','D.L-04','D.O-01','D.O-02','D.O-03','D.O-04'])await call(id);
if(callCount!==96)throw new Error(`call cardinality ${callCount} != 96`);
socket.end();
fs.mkdirSync(workDir,{recursive:true});const transcriptPath=path.join(workDir,'node-transcript.json');fs.writeFileSync(transcriptPath,JSON.stringify({invocationId,nodePid:process.pid,callCount,externalWriter:{command:'move-object',result:externalResult},calls:transcript},null,2));
console.log(JSON.stringify({classification:'PASS',invocationId,nodePid:process.pid,callCount,externalWriter:externalResult,evidencePath,transcriptPath}));
