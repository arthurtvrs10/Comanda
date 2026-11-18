const enc=new TextEncoder();
const json=(b,status=200)=>new Response(JSON.stringify(b),{status,headers:{'Content-Type':'application/json','Cache-Control':'no-store','X-Content-Type-Options':'nosniff'}});
class Problem extends Error{constructor(status,message){super(message);this.status=status;}}
const fail=(status,message)=>{throw new Problem(status,message)};
export const hash=async text=>Array.from(new Uint8Array(await crypto.subtle.digest('SHA-256',enc.encode(text))),b=>b.toString(16).padStart(2,'0')).join('');
function bytes(s){if(typeof s!=='string'||!s.length||!/^[A-Za-z0-9+/]*={0,2}$/.test(s))throw new Error('base64');return Uint8Array.from(atob(s),c=>c.charCodeAt(0));}
const b64=b=>btoa(String.fromCharCode(...new Uint8Array(b)));
export function code(){let n;do{n=crypto.getRandomValues(new Uint32Array(1))[0];}while(n>=4200000000);return String(n%100000000).padStart(8,'0');}
async function cipherKey(secret){if(!/^[a-f0-9]{64}$/.test(secret??''))fail(503,'Transferência indisponível. Copie a licença manualmente.');return crypto.subtle.importKey('raw',Uint8Array.from(secret.match(/../g),h=>parseInt(h,16)),{name:'AES-GCM'},false,['encrypt','decrypt']);}
export async function validate(token,key,now){
  try{
    if(typeof token!=='string'||token.length>12000)throw new Error();
    const parts=token.split('.');if(parts.length!==2)throw new Error();
    const payload=bytes(parts[0]);if(!await crypto.subtle.verify({name:'RSA-PSS',saltLength:32},key,bytes(parts[1]),payload))throw new Error();
    const b=JSON.parse(new TextDecoder('utf-8',{fatal:true}).decode(payload));
    const begin=Date.parse(b.Inicio),end=b.Vencimento===null?null:Date.parse(b.Vencimento);
    if(b.Versao!==1||!b.Id||typeof b.Cliente!=='string'||!b.Cliente.trim()||b.Cliente.length>200||!/^[A-F0-9]{64}$/.test(b.Computador)||!Number.isFinite(begin)||begin>now*1000||(end!==null&&(!Number.isFinite(end)||end<=begin||end<=now*1000)))throw new Error();
    return b;
  }catch{fail(400,'Licença inválida ou vencida. Gere novamente no emissor.');}
}
async function read(request){
  if(!request.headers.get('content-type')?.startsWith('application/json'))fail(415,'Envie JSON.');
  const reader=request.body?.getReader();if(!reader)fail(400,'Solicitação inválida.');let size=0;const chunks=[];
  while(true){const {done,value}=await reader.read();if(done)break;size+=value.length;if(size>16000){await reader.cancel();fail(413,'Solicitação muito grande.');}chunks.push(value);}
  const all=new Uint8Array(size);let offset=0;for(const chunk of chunks){all.set(chunk,offset);offset+=chunk.length;}
  try{const result=JSON.parse(new TextDecoder().decode(all));if(!result||typeof result!=='object'||Array.isArray(result))throw new Error();return result;}catch{fail(400,'Solicitação inválida.');}
}
async function limit(db,key,max,seconds,now){const bucket=Math.floor(now/seconds);return !!await db.prepare('INSERT INTO limits(key,count,expires) VALUES(?,1,?) ON CONFLICT(key) DO UPDATE SET count=count+1 WHERE count<? RETURNING count').bind(key+':'+bucket,(bucket+1)*seconds,max).first();}
export function createHandler(pem,{clock=()=>Math.floor(Date.now()/1000)}={}){
  let publicKey;
  return async(request,env)=>{
    try{
      const path=new URL(request.url).pathname;
      if(request.method==='GET'&&path==='/health')return json({service:'vcomanda-licencas-transferencia',version:1});
      if(request.method!=='POST'||!['/v1/send','/v1/receive'].includes(path))return json({error:'Rota não encontrada.'},404);
      const key=await cipherKey(env.TRANSFER_KEY),now=clock();
      const ip=await hash(request.headers.get('CF-Connecting-IP')??'local');
      if(!await limit(env.DB,'ip:'+ip,30,600,now)||!await limit(env.DB,'all',2000,86400,now))fail(429,'Muitas tentativas. Aguarde e tente novamente.');
      const b=await read(request);
      if(path==='/v1/send'){
        publicKey??=crypto.subtle.importKey('spki',bytes(pem.replace(/-----[^-]+-----|\s/g,'')),{name:'RSA-PSS',hash:'SHA-256'},false,['verify']);
        const claims=await validate(b.license,await publicKey,now),machine=await hash(claims.Computador);
        if(!await limit(env.DB,'send:'+machine,5,600,now)||!await limit(env.DB,'send-all',100,86400,now))fail(429,'Limite de envios atingido. Copie a licença ou aguarde.');
        const short=code(),digest=await hash(short+':'+claims.Computador),iv=crypto.getRandomValues(new Uint8Array(12));
        const encrypted=await crypto.subtle.encrypt({name:'AES-GCM',iv,additionalData:enc.encode(digest)},key,enc.encode(b.license));
        // One active transfer per machine; another send invalidates the previous code.
        await env.DB.prepare('INSERT INTO transfers(hash,machine,encrypted,expires) VALUES(?,?,?,?) ON CONFLICT(machine) DO UPDATE SET hash=excluded.hash,encrypted=excluded.encrypted,expires=excluded.expires').bind(digest,machine,b64(iv)+'.'+b64(encrypted),now+600).run();
        return json({code:short,expiresAt:now+600});
      }
      if(!/^[0-9]{8}$/.test(b.code??'')||!/^[A-F0-9]{64}$/.test(b.machine??''))fail(400,'Informe os 8 dígitos do celular.');
      const machine=await hash(b.machine);
      if(!await limit(env.DB,'receive:'+machine,5,600,now))fail(429,'Limite de tentativas. Aguarde 10 minutos.');
      const digest=await hash(b.code+':'+b.machine);
      // Conditional DELETE/RETURNING is atomic: concurrent consumers cannot receive twice.
      const row=await env.DB.prepare('DELETE FROM transfers WHERE hash=? AND machine=? AND expires>? RETURNING encrypted').bind(digest,machine,now).first();
      if(!row)fail(400,'Código inválido, expirado ou já utilizado. Confira ou gere outro no celular.');
      const [iv,data]=row.encrypted.split('.');
      const license=new TextDecoder().decode(await crypto.subtle.decrypt({name:'AES-GCM',iv:bytes(iv),additionalData:enc.encode(digest)},key,bytes(data)));
      return json({license});
    }catch(e){return json({error:e instanceof Problem?e.message:'Transferência indisponível. Tente novamente ou copie a licença.'},e instanceof Problem?e.status:503);}
  };
}
