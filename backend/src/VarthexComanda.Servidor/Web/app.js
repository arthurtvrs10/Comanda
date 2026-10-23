'use strict';
const $=id=>document.getElementById(id), moeda=v=>new Intl.NumberFormat('pt-BR',{style:'currency',currency:'BRL'}).format(v/100);
const chaveNova=new URLSearchParams(location.hash.slice(1)).get('chave');
let chave=sessionStorage.getItem('comanda-chave')||'',catalogo={produtos:[],categorias:[]}, atual=null, produto=null, ocupada=false;
if(chaveNova){if(chaveNova!==chave)sessionStorage.removeItem('comanda-pendente');chave=chaveNova;sessionStorage.setItem('comanda-chave',chave);history.replaceState(null,'','/');}
let pendente=JSON.parse(sessionStorage.getItem('comanda-pendente')||'null');
function aviso(texto,erro=false){$('aviso').textContent=texto;$('aviso').hidden=!texto;$('aviso').dataset.erro=String(erro);}
function elemento(tag,texto,classe){const n=document.createElement(tag);if(texto!==undefined)n.textContent=texto;if(classe)n.className=classe;return n;}
async function api(caminho,opcoes={}){
 let resposta;try{resposta=await fetch(caminho,{...opcoes,signal:AbortSignal.timeout(12000),cache:'no-store',headers:{Authorization:'Bearer '+chave,...opcoes.headers}});}catch{throw Error('Sem resposta do computador. Confira o Wi-Fi e se o servidor continua ligado.');}
 if(resposta.status===401)throw Error('Conexão encerrada. Leia novamente o QR Code no computador.');
 let dados;try{dados=await resposta.json();}catch{throw Error('Não foi possível ler a resposta. Confira o servidor no computador.');}
 if(!resposta.ok){const e=Error(dados.erro||'Não foi possível concluir.');e.definitivo=[400,404,409].includes(resposta.status);throw e;}return dados;
}
function salvarPendente(valor){pendente=valor;if(valor)sessionStorage.setItem('comanda-pendente',JSON.stringify(valor));else sessionStorage.removeItem('comanda-pendente');$('pendencia').hidden=!valor;}
async function enviar(caminho,dados){
 if(ocupada)return;
 if(pendente&&(pendente.caminho!==caminho||pendente.corpo!==JSON.stringify(dados)))throw Error('Confirme primeiro o lançamento pendente.');
 salvarPendente(pendente||{caminho,corpo:JSON.stringify(dados),id:crypto.randomUUID?crypto.randomUUID():gerarId()});
 ocupada=true;let confirmado=false;document.querySelectorAll('button').forEach(b=>b.disabled=true);
 try{const r=await api(pendente.caminho,{method:'POST',headers:{'Content-Type':'application/json','Idempotency-Key':pendente.id},body:pendente.corpo});confirmado=true;salvarPendente(null);$('personalizar').close();await selecionar(r.id);aviso('Lançamento confirmado no computador.');}
 catch(e){if(e.definitivo)salvarPendente(null);$('avisoItem').textContent=e.message;aviso(confirmado?'Lançamento confirmado no computador. Não foi possível atualizar a tela; confira os itens antes de lançar novamente.':e.message+(pendente?' Use “Confirmar lançamento pendente” para reenviar a mesma operação.':''),!confirmado);}
 finally{ocupada=false;document.querySelectorAll('button').forEach(b=>b.disabled=false);if(atual?.emFechamento)listarProdutos();}
}
// randomUUID requer contexto seguro em alguns navegadores; a rede local HTTP usa getRandomValues.
function gerarId(){const b=crypto.getRandomValues(new Uint8Array(16));b[6]=b[6]&15|64;b[8]=b[8]&63|128;const s=Array.from(b,x=>x.toString(16).padStart(2,'0')).join('');return `${s.slice(0,8)}-${s.slice(8,12)}-${s.slice(12,16)}-${s.slice(16,20)}-${s.slice(20)}`;}
async function listarComandas(){
 const r=await api('/api/comandas');$('numero').min=r.primeiroNumero;$('numero').placeholder='A partir de '+r.primeiroNumero;$('comandas').replaceChildren();
 for(const c of r.comandas){const b=elemento('button');b.append(elemento('small',c.mesa?'Mesa '+c.mesa:'Balcão'),elemento('strong',String(c.numero).padStart(2,'0')),elemento('span',moeda(c.totalCentavos)),elemento('small',c.emFechamento?'Em fechamento':c.cliente||'Em aberto'));b.onclick=()=>selecionar(c.id).catch(e=>aviso(e.message,true));$('comandas').append(b);}
 $('vazio').textContent=r.comandas.length?'':'Nenhuma comanda aberta. Informe o número acima para começar.';
}
async function selecionar(id){atual=await api('/api/comandas/'+id);$('lista').hidden=true;$('pedido').hidden=false;$('tituloPedido').textContent='Comanda '+String(atual.numero).padStart(2,'0');$('total').textContent=moeda(atual.totalCentavos);$('situacao').textContent=atual.emFechamento?'Em fechamento · novos itens estão indisponíveis.':(atual.mesa?'Mesa '+atual.mesa+' · ':'')+(atual.cliente||'Em aberto');$('itens').replaceChildren();for(const i of atual.itens){$('itens').append(elemento('li',`${i.quantidade} ${i.medida} · ${i.nome} · ${moeda(i.subtotalCentavos)}${i.observacao?' — '+i.observacao:''}`));}listarProdutos();}
function listarProdutos(){
 const busca=$('busca').value.toLocaleLowerCase('pt-BR'),categoria=$('categoria').value;$('produtos').replaceChildren();
 const lista=catalogo.produtos.filter(p=>(!categoria||String(p.categoriaId)===categoria)&&(!busca||p.nome.toLocaleLowerCase('pt-BR').includes(busca)||String(p.id)===busca));
 for(const p of lista){const b=elemento('button',undefined,'produto'),nome=elemento('span');nome.append(elemento('strong',p.nome),elemento('small','Cód. '+p.id+' · '+p.medida));b.append(nome,elemento('b',moeda(p.precoCentavos)+' +'));b.disabled=ocupada||!!atual?.emFechamento;b.onclick=()=>{if(pendente){aviso('Confirme primeiro o lançamento pendente.',true);return;}produto=p;$('nomeProduto').textContent=p.nome;$('precoProduto').textContent=moeda(p.precoCentavos)+' / '+p.medida;$('quantidade').value='1';$('quantidade').step=p.medida==='UN'?'1':'0.001';$('quantidade').min=p.medida==='UN'?'1':'0.001';$('observacao').value='';$('avisoItem').textContent='';$('personalizar').showModal();$('quantidade').focus();};$('produtos').append(b);}
 if(!lista.length)$('produtos').append(elemento('p','Nenhum produto encontrado.'));
}
$('abrir').onsubmit=e=>{e.preventDefault();enviar('/api/comandas',{numero:Number($('numero').value)}).catch(e=>aviso(e.message,true));};
$('lancar').onsubmit=e=>{e.preventDefault();const nome=$('operador').value.trim();if(!nome){$('avisoItem').textContent='Informe o nome do atendente no início da tela.';return;}enviar('/api/comandas/'+atual.id+'/itens',{produtoId:produto.id,quantidade:Number($('quantidade').value),observacao:$('observacao').value,operador:nome}).catch(e=>aviso(e.message,true));};
$('reenviar').onclick=()=>{if(pendente)enviar(pendente.caminho,JSON.parse(pendente.corpo)).catch(e=>aviso(e.message,true));};
$('cancelar').onclick=()=>{if(!ocupada)$('personalizar').close();};$('personalizar').oncancel=e=>{if(ocupada)e.preventDefault();};
$('voltar').onclick=()=>{atual=null;$('pedido').hidden=true;$('lista').hidden=false;listarComandas().catch(e=>aviso(e.message,true));};
$('atualizar').onclick=()=>listarComandas().then(()=>aviso('Comandas atualizadas.')).catch(e=>aviso(e.message,true));
$('busca').oninput=listarProdutos;$('categoria').onchange=listarProdutos;
$('operador').value=sessionStorage.getItem('comanda-operador')||'';$('operador').oninput=()=>sessionStorage.setItem('comanda-operador',$('operador').value);
async function iniciar(){
 $('parear').hidden=!!chave;if(!chave){$('conexao').textContent='Sem conexão';return;}
 try{catalogo=await api('/api/catalogo');for(const c of catalogo.categorias){const o=elemento('option',c.nome);o.value=c.id;$('categoria').append(o);}$('atendimento').hidden=false;$('conexao').textContent='Conectado à rede local';salvarPendente(pendente);await listarComandas();}
 catch(e){$('parear').hidden=false;$('conexao').textContent='Sem conexão';aviso(e.message,true);}
}
iniciar();setInterval(async()=>{if(document.hidden||ocupada||!chave||$('atendimento').hidden)return;try{if(atual&&!$('personalizar').open)await selecionar(atual.id);else if(!atual)await listarComandas();$('conexao').textContent='Conectado à rede local';}catch{$('conexao').textContent='Reconectando…';}},4000);
