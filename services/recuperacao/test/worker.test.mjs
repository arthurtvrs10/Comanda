import {test} from 'node:test';
import assert from 'node:assert/strict';
import {DatabaseSync} from 'node:sqlite';
import {readFileSync} from 'node:fs';
import {createHandler,sendResend} from '../src/worker.mjs';
import {Store} from '../src/store.mjs';
import {code,mask} from '../src/crypto.mjs';
import {bundleDashboard} from '../build-dashboard.mjs';

// Executes the real SQL, including conditional UPDATE/RETURNING and batch rollback.
class D1 {
  constructor(){this.sql=new DatabaseSync(':memory:');this.sql.exec('PRAGMA foreign_keys=ON');this.sql.exec(readFileSync(new URL('../migrations/0001_recuperacao.sql',import.meta.url),'utf8'));}
  prepare(sql){return {bind:(...args)=>({first:async()=>this.sql.prepare(sql).get(...args)??null,run:async()=>({meta:{changes:Number(this.sql.prepare(sql).run(...args).changes)}})})};}
  async batch(statements){this.sql.exec('BEGIN');try{const result=[];for(const s of statements)result.push(await s.run());this.sql.exec('COMMIT');return result;}catch(e){this.sql.exec('ROLLBACK');throw e;}}
}
const keys=await crypto.subtle.generateKey({name:'RSA-PSS',modulusLength:2048,publicExponent:new Uint8Array([1,0,1]),hash:'SHA-256'},true,['sign','verify']);
const pem='-----BEGIN PRIVATE KEY-----\n'+Buffer.from(await crypto.subtle.exportKey('pkcs8',keys.privateKey)).toString('base64')+'\n-----END PRIVATE KEY-----';
function fixture(){
  let now=1800000000;const sent=[];
  const env={DB:new D1(),RESEND_API_KEY:'test-only',EMAIL_FROM:'VComanda <test@example.com>',OTP_PEPPER:'a'.repeat(64),SIGNING_PRIVATE_KEY:pem};
  const handle=createHandler({clock:()=>now,send:async(_e,email,value,id)=>{sent.push({email,value,id});}});
  const id='a'.repeat(32),secret='b'.repeat(64),nonce='c'.repeat(32),credentialHash='d'.repeat(64);
  const call=async(path,b={},token=secret)=>{
    const response=await handle(new Request('https://example.com/v1/'+path,{method:'POST',headers:{Authorization:`Bearer ${token}`,'Content-Type':'application/json','CF-Connecting-IP':'127.0.0.1'},body:JSON.stringify({installationId:id,nonce,credentialHash,...b})}),env);
    return {status:response.status,body:await response.json()};
  };
  const enroll=async()=>{const r=await call('enroll/request',{email:'gestor@example.com'});assert.equal(r.status,200);assert.equal((await call('enroll/confirm',{challengeId:r.body.challengeId,code:sent.at(-1).value})).status,200);now+=61;};
  return {env,call,enroll,sent,id,nonce,credentialHash,tick:s=>now+=s,now:()=>now,handle};
}
test('cadastro verificado, reset assinado, destinatário fixo e uso único',async()=>{
  const f=fixture();await f.enroll();
  const r=await f.call('reset/request',{email:'attacker@example.com'});assert.equal(r.status,200);
  assert.equal(f.sent.at(-1).email,'gestor@example.com');assert.equal(r.body.code,undefined);
  const payload={challengeId:r.body.challengeId,code:f.sent.at(-1).value};
  const valid=await f.call('reset/confirm',payload);assert.equal(valid.status,200);
  const [v,p,sig]=valid.body.grant.split('.');assert.equal(v,'v1');
  assert.equal(await crypto.subtle.verify({name:'RSA-PSS',saltLength:32},keys.publicKey,Buffer.from(sig,'base64url'),Buffer.from(`v1.${p}`)),true);
  const claims=JSON.parse(Buffer.from(p,'base64url'));assert.equal(claims.installationId,f.id);assert.equal(claims.nonce,f.nonce);assert.equal(claims.credentialHash,f.credentialHash);assert.equal(claims.exp-claims.iat,600);
  assert.equal((await f.call('reset/confirm',payload)).status,400);
});
test('não permite substituir segredo de instalação existente',async()=>{
  const f=fixture();await f.enroll();assert.equal((await f.call('enroll/request',{email:'attacker@example.com'},'e'.repeat(64))).status,401);
  assert.equal((await new Store(f.env.DB).installation(f.id)).email,'gestor@example.com');
});
test('código errado bloqueia após cinco tentativas; código não fica em texto no banco',async()=>{
  const f=fixture();await f.enroll();const r=await f.call('reset/request');const sent=f.sent.at(-1);
  const c=f.env.DB.sql.prepare('SELECT * FROM challenges WHERE id=?').get(r.body.challengeId);
  assert.notEqual(c.mac,sent.value);assert.equal(c.mac.length,64);
  for(let i=0;i<5;i++)assert.equal((await f.call('reset/confirm',{challengeId:c.id,code:sent.value==='000000'?'111111':'000000'})).status,400);
  assert.equal((await f.call('reset/confirm',{challengeId:c.id,code:sent.value})).status,400);
});
test('quinta tentativa ainda permite código correto',async()=>{
  const f=fixture();await f.enroll();const r=await f.call('reset/request');const good=f.sent.at(-1).value;
  for(let i=0;i<4;i++)await f.call('reset/confirm',{challengeId:r.body.challengeId,code:good==='000000'?'111111':'000000'});
  assert.equal((await f.call('reset/confirm',{challengeId:r.body.challengeId,code:good})).status,200);
});
test('expiração, cooldown móvel e reenvio invalidam código anterior',async()=>{
  const f=fixture();await f.enroll();const r=await f.call('reset/request'),old=f.sent.at(-1).value;
  assert.equal((await f.call('reset/request')).status,429);f.tick(61);
  assert.equal((await f.call('reset/request')).status,200);
  assert.equal((await f.call('reset/confirm',{challengeId:r.body.challengeId,code:old})).status,400);
  const latest=f.sent.at(-1);f.tick(601);
  assert.equal((await f.call('reset/confirm',{challengeId:latest.id,code:latest.value})).status,400);
});
test('cadastro de e-mail não verificado não permite reset',async()=>{
  const f=fixture();await f.call('enroll/request',{email:'gestor@example.com'});f.tick(61);
  assert.equal((await f.call('reset/request')).status,400);
});
test('troca de e-mail invalida desafios de recuperação anteriores',async()=>{
  const f=fixture();await f.enroll();const r=await f.call('reset/request');const old=f.sent.at(-1).value;f.tick(61);
  const e=await f.call('enroll/request',{email:'novo@example.com'});
  assert.equal((await f.call('enroll/confirm',{challengeId:e.body.challengeId,code:f.sent.at(-1).value})).status,200);
  assert.equal((await f.call('reset/confirm',{challengeId:r.body.challengeId,code:old})).status,400);
});
test('apenas uma confirmação concorrente recebe autorização',async()=>{
  const f=fixture();await f.enroll();const r=await f.call('reset/request');const b={challengeId:r.body.challengeId,code:f.sent.at(-1).value};
  const result=await Promise.all([f.call('reset/confirm',b),f.call('reset/confirm',b)]);assert.deepEqual(result.map(x=>x.status).sort(),[200,400]);
});
test('orçamento diário impede envio acima da margem gratuita',async()=>{
  const f=fixture();await f.enroll();const store=new Store(f.env.DB);
  const key=`send-global:${Math.floor(f.now()/86400)}`;
  f.env.DB.sql.prepare('UPDATE rate_limits SET count=90 WHERE key=?').run(key);
  assert.equal((await f.call('reset/request')).status,429);assert.equal(f.sent.length,1);
});
test('limite mensal usa calendário UTC e limpeza remove expirados',async()=>{
  const f=fixture();await f.enroll();const month=new Date(f.now()*1000).toISOString().slice(0,7);
  f.env.DB.sql.prepare('UPDATE rate_limits SET count=2700 WHERE key=?').run(`send-month:${month}`);
  assert.equal((await f.call('reset/request')).status,429);
  f.tick(40*86400);await new Store(f.env.DB).cleanup(f.now());
  assert.equal(f.env.DB.sql.prepare('SELECT count(*) AS n FROM challenges').get().n,0);
});
test('falha de provedor invalida código, não vaza erro ou segredo',async()=>{
  const f=fixture();await f.enroll();const h=createHandler({clock:f.now,send:async()=>{throw new Error('secret-key-test');}});
  const res=await h(new Request('https://example.com/v1/reset/request',{method:'POST',headers:{'Content-Type':'application/json',Authorization:'Bearer '+'b'.repeat(64)},body:JSON.stringify({installationId:f.id,nonce:f.nonce,credentialHash:f.credentialHash})}),f.env);
  assert.equal(res.status,503);assert.equal((await res.text()).includes('secret-key-test'),false);
  assert.equal(f.env.DB.sql.prepare("SELECT count(*) n FROM challenges WHERE purpose='reset' AND used=0").get().n,0);
});
test('Resend usa destinatário, idempotência e conteúdo correto sem enviar senha',async()=>{
  const f=fixture();let captured;
  await sendResend(f.env,'gestor@example.com','123456','challenge',async(url,options)=>{captured={url,...options};return new Response('{}',{status:200});});
  const b=JSON.parse(captured.body);assert.equal(captured.url,'https://api.resend.com/emails');assert.equal(captured.headers['Idempotency-Key'],'challenge');assert.deepEqual(b.to,['gestor@example.com']);assert.match(b.text,/123456/);
});
test('rota não disponível, serviço não configurado, dados inválidos e corpo grande',async()=>{
  const f=fixture();assert.equal((await f.call('unknown')).status,404);
  assert.equal((await f.call('enroll/request',{installationId:'bad'})).status,400);
  assert.equal((await f.call('enroll/request',{email:'bad'})).status,400);
  assert.equal((await f.call('enroll/request',{email:'x'.repeat(9000)})).status,413);
  delete f.env.OTP_PEPPER;assert.equal((await f.call('reset/request')).status,503);
});
test('código tem seis números e destinatário é mascarado',()=>{
  for(let i=0;i<100;i++)assert.match(code(),/^\d{6}$/);assert.equal(mask('arthur@example.com'),'ar***@example.com');
});
test('bundle do dashboard mantém saúde e bloqueio dos envios não configurados',async()=>{
  const bundled=await import('data:text/javascript;base64,'+Buffer.from(bundleDashboard()).toString('base64'));
  const healthy=await bundled.default.fetch(new Request('https://example.com/health'),{});
  assert.equal(healthy.status,200);assert.equal((await healthy.json()).service,'vcomanda-recuperacao');
  const disabled=await bundled.default.fetch(new Request('https://example.com/v1/reset/request',{method:'POST'}),{});
  assert.equal(disabled.status,503);
});
test('migração inicial pode ser reaplicada após criação pelo painel',()=>{
  const db=new D1();db.sql.exec(readFileSync(new URL('../migrations/0001_recuperacao.sql',import.meta.url),'utf8'));
  assert.equal(db.sql.prepare("SELECT count(*) n FROM sqlite_master WHERE type='table' AND name IN ('installations','challenges','rate_limits')").get().n,3);
});
