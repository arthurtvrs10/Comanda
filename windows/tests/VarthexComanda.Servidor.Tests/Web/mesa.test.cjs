const {test}=require('node:test');const assert=require('node:assert/strict');const fs=require('node:fs');const path=require('node:path');const vm=require('node:vm');const {webcrypto}=require('node:crypto');
const pasta=path.resolve(__dirname,'../../../src/VarthexComanda.Servidor/Web');
async function montar(opcoes={}){
 const ids=Object.fromEntries(['formChamada','solicitacao','chamar','estado','tituloMesa'].map(id=>[id,{value:'',textContent:'',dataset:{},disabled:false}]));
 const armazenamento=new Map();const requisicoes=[];let ativa=null,perdeu=false,poll;
 const fetch=async(url,r)=>{requisicoes.push({url,...r});assert.equal(url,'/api/mesa-publica');assert.equal(r.headers['X-Mesa'],'12');assert.equal(r.headers.Authorization,'Bearer '+'A'.repeat(64));
 if(opcoes.invalida)return new Response('{}',{status:401});
 if(r.method==='POST'){ativa={id:'chamada'};if(opcoes.perderResposta&&!perdeu){perdeu=true;throw Error('sem rede');}return Response.json({chamada:ativa,jaAtendida:false});}
 return Response.json({mesa:12,chamada:ativa});};
 const contexto={URLSearchParams,location:{hash:opcoes.semQr?'':'#mesa=12&chave='+'A'.repeat(64)},sessionStorage:{getItem:k=>armazenamento.get(k)||null,setItem:(k,v)=>armazenamento.set(k,v),removeItem:k=>armazenamento.delete(k)},history:{replaceState(){}},document:{hidden:false,getElementById:id=>ids[id]},AbortSignal,fetch,crypto:webcrypto,setInterval:f=>{poll=f}};
 vm.runInNewContext(fs.readFileSync(path.join(pasta,'mesa.js'),'utf8'),contexto);await new Promise(r=>setImmediate(r));
 return {ids,requisicoes,armazenamento,poll:()=>poll?.(),atender:()=>{ativa=null},enviar:()=>ids.formChamada.onsubmit({preventDefault(){}})};
}
test('página restrita da mesa chama sem comandas e bloqueia enquanto aguarda atendimento',async()=>{const t=await montar();assert.equal(t.ids.tituloMesa.textContent,'Mesa 12');assert.equal(t.ids.chamar.disabled,false);t.ids.solicitacao.value='Guardanapos';await t.enviar();assert.match(t.ids.estado.textContent,/equipe foi avisada/);assert.equal(t.ids.chamar.disabled,true);await t.enviar();assert.equal(t.requisicoes.filter(r=>r.method==='POST').length,1);t.atender();await t.poll();assert.equal(t.ids.chamar.disabled,false);assert.match(t.ids.estado.textContent,/atendida/);assert.ok(t.requisicoes.every(r=>!r.url.includes('comandas')));});
test('resposta perdida preserva identificação e só a remove após confirmação',async()=>{const t=await montar({perderResposta:true});t.ids.solicitacao.value='Ajuda';await t.enviar();assert.equal(t.ids.chamar.disabled,false);await t.enviar();const req=t.requisicoes.filter(r=>r.method==='POST');assert.equal(req[0].body,req[1].body);assert.equal(t.armazenamento.has('mesa-tentativa-12'),false);});
test('QR inválido não habilita envio e ausência de QR não busca dados',async()=>{const a=await montar({invalida:true});assert.equal(a.ids.chamar.disabled,true);assert.match(a.ids.estado.textContent,/QR Code indisponível/);const b=await montar({semQr:true});assert.equal(b.requisicoes.length,0);await b.enviar();assert.equal(b.requisicoes.length,0);});
test('HTML público não carrega interface da equipe',()=>{const html=fs.readFileSync(path.join(pasta,'mesa.html'),'utf8');assert.ok(html.includes('/mesa.js'));assert.ok(!html.includes('/app.js'));assert.ok(!html.includes('comandas'));assert.ok(html.includes('aria-live="polite"'));});
