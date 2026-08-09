const {ShellEyeClient,sdk}=require('C:/StealthEyeLLC/SHELLeye-src/program-host/sdk');
(async()=>{
  const src=process.argv[2],dst=process.argv[3];
  const c=new ShellEyeClient('shelleye-dev'); const s=sdk(c);
  try{
    const hello=await s.rpc.hello();
    const retained=await s.file.retain(src);
    const before=await s.file.inspect(retained.id);
    const renamed=await s.file.rename(retained.id,dst);
    const after=await s.file.inspect(retained.id);
    if(before.id!==after.id||renamed.id!==retained.id)throw new Error('physical identity changed across SHELLeye rename');
    console.log(JSON.stringify({hello,retained:{id:retained.id,path:retained.path,identity:retained.identity,revision:retained.revision},renamed:{id:renamed.id,path:renamed.path,identity:renamed.identity,revision:renamed.revision},after:{id:after.id,path:after.path,identity:after.identity,revision:after.revision}}));
  }finally{await c.close();}
})().catch(e=>{console.error(e.stack||e);process.exit(1)});