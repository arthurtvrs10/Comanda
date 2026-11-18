import {Store} from './store.mjs';
import {sha,code,mac,verifyCode,mask,signGrant} from './crypto.mjs';
class Problem extends Error {constructor(status,message){super(message);this.status=status;}}
const fail=(status,message)=>{throw new Problem(status,message);};
const json=(body,status=200)=>new Response(JSON.stringify(body),{status,headers:{'Content-Type':'application/json','Cache-Control':'no-store','X-Content-Type-Options':'nosniff'}});
const idPattern=/^[a-f0-9]{32}$/;
const secretPattern=/^[a-f0-9]{64}$/;
async function body(request) {
  if(!request.headers.get('content-type')?.startsWith('application/json'))fail(415,'Envie dados JSON.');
  const reader=request.body?.getReader();if(!reader)fail(400,'Solicitação inválida.');
  let length=0;const parts=[];
  while(true){const {done,value}=await reader.read();if(done)break;length+=value.length;if(length>8192){await reader.cancel();fail(413,'Solicitação muito grande.');}parts.push(value);}
  const bytes=new Uint8Array(length);let offset=0;for(const p of parts){bytes.set(p,offset);offset+=p.length;}
  try{return JSON.parse(new TextDecoder().decode(bytes));}catch{fail(400,'Solicitação inválida.');}
}
export async function sendResend(env,to,value,id,fetcher=fetch) {
  const result=await fetcher('https://api.resend.com/emails',{
    method:'POST',headers:{Authorization:`Bearer ${env.RESEND_API_KEY}`,'Content-Type':'application/json','Idempotency-Key':id},
    body:JSON.stringify({from:env.EMAIL_FROM,to:[to],subject:'Seu código de acesso ao VComanda',
      text:`Seu código do VComanda é ${value}. Ele expira em 10 minutos. Não compartilhe este código. Se você não solicitou, ignore este e-mail.`}),
    signal:AbortSignal.timeout(15000)
  });
  if(!result.ok)throw new Error('email');
}
export function createHandler({clock=()=>Math.floor(Date.now()/1000),send=sendResend}={}) {
  return async function(request,env) {
    try {
      const path=new URL(request.url).pathname;
      if(request.method==='GET'&&path==='/health')return json({service:'vcomanda-recuperacao',version:1});
      if(request.method!=='POST'||!['/v1/enroll/request','/v1/enroll/confirm','/v1/reset/request','/v1/reset/confirm'].includes(path))return json({error:'Rota não encontrada.'},404);
      if(!env.RESEND_API_KEY||!env.EMAIL_FROM||env.OTP_PEPPER?.length<32||!env.OTP_PEPPER||!env.SIGNING_PRIVATE_KEY)fail(503,'Recuperação por e-mail indisponível. Use a senha de recuperação offline.');
      const now=clock(),store=new Store(env.DB);
      const ip=await sha(request.headers.get('CF-Connecting-IP')??'local');
      if(!await store.limit(`requests:${ip}`,60,60,now))fail(429,'Muitas tentativas. Aguarde um minuto.');
      const b=await body(request);
      if(!b||typeof b!=='object'||!idPattern.test(b.installationId??''))fail(400,'Instalação inválida.');
      const auth=request.headers.get('Authorization');
      if(!/^Bearer [a-f0-9]{64}$/.test(auth??''))fail(401,'Instalação não autorizada.');
      const secret=auth.slice(7);
      if(!secretPattern.test(secret??''))fail(401,'Instalação não autorizada.');
      const secretHash=await sha(secret);
      let installation=await store.installation(b.installationId);
      if(!installation&&path==='/v1/enroll/request') {
        if(!await store.limit(`register:${ip}`,5,3600,now))fail(429,'Limite de cadastros atingido. Tente mais tarde.');
        installation=await store.register(b.installationId,secretHash,now);
      }
      if(!installation||installation.secret_hash!==secretHash)fail(401,'Instalação não autorizada.');
      const purpose=path.includes('/enroll/')?'enroll':'reset';
      if(path.endsWith('/request')) {
        if(!idPattern.test(b.nonce??'')||!secretPattern.test(b.credentialHash??''))fail(400,'Solicitação inválida.');
        const email=purpose==='enroll'?String(b.email??'').trim().toLowerCase():installation.email;
        if(!email||email.length>254||!/^[^\s@<>]+@[^\s@<>]+\.[^\s@<>]+$/.test(email))fail(400,'Cadastre e verifique um e-mail nas configurações de Segurança.');
        // Cooldown is a rolling lease, independent of a fixed bucket boundary.
        if(!await store.stmt(`INSERT INTO rate_limits(key,count,expires_at) VALUES(?,1,?)
          ON CONFLICT(key) DO UPDATE SET expires_at=excluded.expires_at WHERE expires_at<=? RETURNING key`,
          `cooldown:${installation.id}`,now+60,now).first())fail(429,'Aguarde 60 segundos antes de reenviar.');
        for(const [key,max,seconds] of [[`send:${installation.id}`,5,3600],[`send-ip:${ip}`,10,3600],['send-global',90,86400]])
          if(!await store.limit(key,max,seconds,now))fail(429,'Limite de envios atingido. Tente mais tarde ou use a recuperação offline.');
        const month=new Date(now*1000);
        if(!await store.reserve(`send-month:${month.toISOString().slice(0,7)}`,2700,Date.UTC(month.getUTCFullYear(),month.getUTCMonth()+1,1)/1000))
          fail(429,'Limite mensal de envios atingido. Use a recuperação offline.');
        const id=crypto.randomUUID().replaceAll('-',''),value=code();
        await store.challenge({id,installation_id:installation.id,purpose,email,email_version:installation.email_version,
          mac:await mac(env.OTP_PEPPER,id,value),nonce:b.nonce,credential_hash:b.credentialHash,expires_at:now+600});
        try {await send(env,email,value,id);}catch{await store.consume(id);fail(503,'Não foi possível enviar o e-mail. Aguarde um minuto e tente novamente.');}
        return json({challengeId:id,maskedEmail:mask(email),expiresIn:600,retryAfter:60});
      }
      if(!idPattern.test(b.challengeId??'')||!/^\d{6}$/.test(b.code??''))fail(400,'Informe os seis números do código.');
      const challenge=await store.attempt(b.challengeId,installation.id,purpose,now);
      if(!challenge||!await verifyCode(env.OTP_PEPPER,challenge.id,b.code,challenge.mac))fail(400,'Código inválido, expirado ou com limite de tentativas atingido.');
      if(challenge.email_version!==installation.email_version)fail(400,'O e-mail foi alterado. Solicite outro código.');
      if(purpose==='enroll') {
        if(!await store.confirmEmail(challenge,now))fail(400,'Código já utilizado ou e-mail alterado.');
        return json({maskedEmail:mask(challenge.email),verified:true});
      }
      const grant=await signGrant(env.SIGNING_PRIVATE_KEY,{aud:'vcomanda-gestor-reset',installationId:installation.id,
        challengeId:challenge.id,nonce:challenge.nonce,credentialHash:challenge.credential_hash,iat:now,exp:now+600,jti:crypto.randomUUID()});
      if(!await store.consume(challenge.id,now))fail(400,'Código já utilizado ou e-mail alterado.');
      return json({grant});
    } catch(error) {
      return json({error:error instanceof Problem?error.message:'Serviço indisponível. Tente novamente mais tarde.'},error instanceof Problem?error.status:503);
    }
  };
}
export default {fetch:createHandler(),async scheduled(_event,env){await new Store(env.DB).cleanup(Math.floor(Date.now()/1000));}};
