// Executa o JavaScript real com DOM mínimo em memória. Não substitui revisão visual em navegador.
const { test } = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const { webcrypto } = require('node:crypto');
const web = path.resolve(__dirname, '../../../src/VarthexComanda.Servidor/Web');
const html = fs.readFileSync(path.join(web, 'index.html'), 'utf8');
const js = fs.readFileSync(path.join(web, 'app.js'), 'utf8');
class No {
 constructor(tag, todos) { this.tagName = tag; this.todos = todos; todos.push(this); this.children = []; this.dataset = {}; this.attrs = {}; this.value = ''; this.hidden = false; this.disabled = false; this.className = ''; }
 set textContent(v) { this.texto = String(v); this.children = []; }
 get textContent() { return (this.texto || '') + this.children.map(n => n.textContent).join(''); }
 append(...nos) { for (const n of nos) { n.parent = this; this.children.push(n); } }
 replaceChildren(...nos) { this.texto = ''; for (const n of this.children) n.parent = null; this.children = []; this.append(...nos); }
 setAttribute(k, v) { this.attrs[k] = String(v); }
 getAttribute(k) { return this.attrs[k]; }
 focus() { this.todos.focado = this; }
 showModal() { this.open = true; }
 close() { this.open = false; }
 remove() { if (this.parent) this.parent.children = this.parent.children.filter(n => n !== this); this.parent = null; }
 querySelectorAll(seletor) { return this.children.flatMap(n => [n,...n.querySelectorAll('*')]).filter(n=>seletor==='*'||(n.tagName==='input'&&(!seletor.includes(':checked')||n.checked))); }
 get isConnected() { return !!this.id || !!this.parent?.isConnected; }
}
async function montar(opcoes = {}) {
 const nos = [], ids = {}, eventos = {}, chamadas = [], confirmadas = new Map(), revogadas = [];
 for (const [, tag, atributos, id] of html.matchAll(/<([a-z0-9]+)\b([^>]*\bid="([^"]+)"[^>]*)>/gi)) {
  const n = new No(tag, nos); n.id = id; n.hidden = /\bhidden\b/.test(atributos); ids[id] = n;
 }
 let pedido = { id: 9, numero: 102, mesa: 4, cliente: 'João', emFechamento: false, totalCentavos: 750, itens: [{ id: 1, nome: 'Hambúrguer', quantidade: '1', medida: 'UN', observacao: 'Sem cebola', subtotalCentavos: 750 }] };
 const produtos = [{ id: 1, nome: 'Hambúrguer', categoriaId: 1, medida: 'UN', precoCentavos: 750, temFoto: true }, { id: 2, nome: 'Almoço por quilo', categoriaId: 1, medida: 'KG', precoCentavos: 750, temFoto: false }, { id: 3, nome: 'Água', categoriaId: 2, medida: 'UN', precoCentavos: 500, temFoto: false }];
 let perdeu = false, falharLeitura = false;
 const storage = new Map([['comanda-operador', 'Ana']]);
 class URLTeste extends URL { static createObjectURL() { return 'blob:foto-' + chamadas.length; } static revokeObjectURL(url) { revogadas.push(url); } }
 const json = (body, status = 200) => new Response(JSON.stringify(body), { status, headers: { 'Content-Type': 'application/json' } });
 const fetch = async (url, req = {}) => {
  chamadas.push({ url, ...req });
  assert.equal(req.headers.Authorization, 'Bearer chave-de-teste');
  if (url === '/api/catalogo') return json({ categorias: [{ id: 1, nome: 'Refeições' }, { id: 2, nome: 'Bebidas' }], produtos });
  if (url === '/api/comandas' && !req.method) return json({ primeiroNumero: 100, comandas: [pedido] });
  if (url === '/api/comandas/9' && !req.method) {
   if (falharLeitura) throw Error('rede');
   return json(pedido);
  }
  if (url === '/api/produtos/1/foto') return new Response(new Uint8Array([137, 80, 78, 71]), { headers: { 'Content-Type': opcoes.fotoInvalida ? 'text/html' : 'image/png' } });
  if (req.method === 'POST') {
   if(url.endsWith('/chamadas')) {
    if(opcoes.perderChamada && !perdeu){perdeu=true;throw Error('resposta perdida');}
    return json({chamada:{id:'chamada-teste'},jaAtendida:false});
   }
   const chave = req.headers['Idempotency-Key'];
   if (!confirmadas.has(chave)) {
    confirmadas.set(chave, req.body);
    if (url.endsWith('/itens')) {
     const item = JSON.parse(req.body), p = produtos.find(p => p.id === item.produtoId);
     const subtotal = Math.round(item.quantidade * p.precoCentavos);
     pedido.itens.push({ id: pedido.itens.length + 1, nome: p.nome, quantidade: String(item.quantidade), medida: p.medida, observacao: item.observacao, subtotalCentavos: subtotal }); pedido.totalCentavos += subtotal;
    } else pedido = { ...pedido, numero: JSON.parse(req.body).numero, itens: [], totalCentavos: 0 };
   } else assert.equal(confirmadas.get(chave), req.body);
   if (opcoes.perderResposta && !perdeu) { perdeu = true; throw Error('resposta perdida após commit'); }
   if (opcoes.falharAposCommit) falharLeitura = true;
   return json({ id: 9 });
  }
  throw Error('Rota inesperada: ' + url);
 };
 const historico = { state: null, pushState(s) { this.state = s; }, replaceState(s) { this.state = s; } };
 const document = { hidden: false, getElementById: id => { assert.ok(ids[id], 'ID no HTML: ' + id); return ids[id]; }, createTextNode: texto => { const n=new No('#text',nos);n.textContent=texto;return n; }, createElement: tag => new No(tag, nos), createElementNS: (_, tag) => new No(tag, nos), querySelectorAll: seletor => seletor==='#complementos input'?ids.complementos.querySelectorAll('input'):nos.filter(n => seletor === 'button' ? n.tagName === 'button' : n.className === seletor.slice(1)) };
 const contexto = vm.createContext({ document, fetch, history: historico, location: { hash: '#chave=chave-de-teste' }, sessionStorage: { getItem: k => storage.get(k) || null, setItem: (k, v) => storage.set(k, v), removeItem: k => storage.delete(k) }, URL: URLTeste, URLSearchParams, crypto: webcrypto, AbortSignal, Intl, console, window: { addEventListener: (tipo, fn) => eventos[tipo] = fn, scrollTo() {} }, setInterval: fn => eventos.poll = fn });
 vm.runInContext(js, contexto); const esperar = () => new Promise(resolve => setImmediate(resolve)); await esperar();
 return { ids, nos, chamadas, storage, revogadas, produtos, eventos, historico, esperar, pedido: () => pedido,
  async clicar(id) { assert.equal(ids[id].disabled, false, id + ' habilitado'); await ids[id].onclick(); await esperar(); },
  async abrir() { await ids.comandas.children[0].onclick(); await esperar(); },
  async escolher(indice = 0) { ids.produtos.children[indice].onclick(); await esperar(); },
  async enviar() { ids.lancar.onsubmit({ preventDefault() {} }); await esperar(); }
 };
}
test('abrir comanda mostra itens e total; + Produtos abre catálogo separado com fotos autenticadas', async () => {
 const t = await montar(); await t.abrir();
 assert.equal(t.ids.pedido.hidden, false); assert.equal(t.ids.catalogo.hidden, true); assert.equal(t.ids.lista.hidden, true);
 assert.match(t.ids.itens.textContent, /Hambúrguer1 UNSem cebola/); assert.match(t.ids.total.textContent, /7,50/);
 await t.clicar('abrirProdutos'); assert.equal(t.ids.pedido.hidden, true); assert.equal(t.ids.catalogo.hidden, false);
 assert.equal(t.ids.produtos.children.length, 3);
 const foto = t.ids.produtos.children[0].children[0].children.find(n => n.tagName === 'img');
 assert.ok(foto.src.startsWith('blob:')); assert.equal(foto.alt, '');
 assert.ok(t.chamadas.some(c => c.url === '/api/produtos/1/foto'));
 assert.ok(t.chamadas.every(c => !c.url.includes('chave=')));
 await t.clicar('voltarPedido'); assert.equal(t.ids.pedido.hidden, false);
});
test('cartão inteiro abre personalização; lançamento KG volta ao pedido com total persistido', async () => {
 const t = await montar(); await t.abrir(); await t.clicar('abrirProdutos'); await t.escolher(1);
 assert.equal(t.ids.personalizar.open, true); assert.equal(t.ids.quantidade.step, '0.001');
 t.ids.quantidade.value = '0.125'; t.ids.quantidade.oninput(); assert.match(t.ids.totalItem.textContent, /0,94/);
 t.ids.observacao.value = 'Sem sal'; await t.enviar();
 assert.equal(t.ids.personalizar.open, false); assert.equal(t.ids.pedido.hidden, false); assert.equal(t.ids.catalogo.hidden, true);
 assert.equal(t.pedido().itens.length, 2); assert.match(t.ids.itens.textContent, /Sem sal/); assert.match(t.ids.total.textContent, /8,44/);
 const corpo = JSON.parse(t.chamadas.find(c => c.method === 'POST').body); assert.equal(corpo.operador, 'Ana'); assert.equal(corpo.quantidade, .125);
});
test('sincronização mantém catálogo, filtro e rascunho; fechamento bloqueia novos itens em amarelo', async () => {
 const t = await montar(); await t.abrir(); await t.clicar('abrirProdutos');
 t.ids.busca.value = 'Almoço'; t.ids.busca.oninput(); await t.escolher(); t.ids.observacao.value = 'Rascunho';
 await t.eventos.poll(); assert.equal(t.ids.catalogo.hidden, false); assert.equal(t.ids.produtos.children.length, 1); assert.equal(t.ids.observacao.value, 'Rascunho');
 t.pedido().emFechamento = true; await t.eventos.poll();
 assert.equal(t.ids.personalizar.open, false); assert.equal(t.ids.pedido.hidden, false); assert.equal(t.ids.abrirProdutos.disabled, true);
 assert.equal(t.ids.situacao.dataset.fechamento, 'true'); assert.equal(t.ids.avisoFechamento.hidden, false);
});
test('resposta perdida guarda operação e reenvia mesma chave sem duplicar; volta aos itens', async () => {
 const t = await montar({ perderResposta: true }); await t.abrir(); await t.clicar('abrirProdutos'); await t.escolher(); await t.enviar();
 assert.equal(t.ids.pendencia.hidden, false); assert.equal(t.ids.abrirProdutos.disabled, true); assert.equal(t.pedido().itens.length, 2);
 await t.clicar('reenviar'); const posts = t.chamadas.filter(c => c.method === 'POST');
 assert.equal(posts.length, 2); assert.equal(posts[0].headers['Idempotency-Key'], posts[1].headers['Idempotency-Key']);
 assert.equal(t.pedido().itens.length, 2); assert.equal(t.ids.pendencia.hidden, true); assert.equal(t.ids.pedido.hidden, false);
});
test('confirmação seguida de falha de leitura não manda repetir um lançamento confirmado', async () => {
 const t = await montar({ falharAposCommit: true }); await t.abrir(); await t.clicar('abrirProdutos'); await t.escolher(); await t.enviar();
 assert.equal(t.ids.pendencia.hidden, true); assert.equal(t.storage.has('comanda-pendente'), false);
 assert.match(t.ids.aviso.textContent, /Lançamento confirmado.*confira os itens/); assert.equal(t.pedido().itens.length, 2);
});
test('busca e categoria filtram sem perder navegação; foto inválida mantém substituto neutro', async () => {
 const t = await montar({ fotoInvalida: true }); await t.abrir(); await t.clicar('abrirProdutos');
 assert.equal(t.ids.produtos.children[0].children[0].children.length, 1);
 t.ids.categorias.children[2].onclick(); assert.equal(t.ids.produtos.children.length, 1); assert.match(t.ids.produtos.textContent, /Água/);
 t.ids.busca.value = 'inexistente'; t.ids.busca.oninput(); assert.equal(t.ids.semProdutos.hidden, false);
 await t.eventos.popstate({ state: { tela: 'pedido', id: 9 } }); assert.equal(t.ids.pedido.hidden, false);
});
test('nova comanda começa na conferência vazia; sem atendente não abre lançamento', async () => {
 const t = await montar(); t.ids.numero.value = '105'; t.ids.abrir.onsubmit({ preventDefault() {} }); await t.esperar();
 assert.equal(t.ids.pedido.hidden, false); assert.equal(t.ids.semItens.hidden, false); assert.match(t.ids.total.textContent, /0,00/);
 await t.clicar('abrirProdutos'); t.ids.operador.value = ''; await t.escolher();
 assert.notEqual(t.ids.personalizar.open, true); assert.match(t.ids.aviso.textContent, /nome do atendente/); assert.equal(t.nos.focado, t.ids.operador);
});


test('complementos validam mínimo, atualizam total e enviam apenas os identificadores', async () => {
 const t = await montar();
 t.produtos[0].complementos = [{ id: 'grupo', nome: 'Adicionais', minimo: 1, maximo: 1, opcoes: [{ id: 'queijo', nome: 'Queijo', precoCentavos: 250 }, { id: 'bacon', nome: 'Bacon', precoCentavos: 350 }] }];
 await t.abrir(); await t.clicar('abrirProdutos'); await t.clicar('atualizarCatalogo'); await t.escolher();
 await t.enviar(); assert.equal(t.chamadas.filter(c => c.method === 'POST').length, 0);
 assert.match(t.ids.avisoItem.textContent, /Adicionais/);
 const caixas = t.ids.complementos.querySelectorAll('input'); caixas[0].checked = true; caixas[0].onchange();
 assert.match(t.ids.totalItem.textContent, /10,00/);
 caixas[1].checked = true; caixas[1].onchange(); assert.equal(caixas[0].checked, false);
 assert.match(t.ids.totalItem.textContent, /11,00/);
 await t.enviar(); const corpo = JSON.parse(t.chamadas.find(c => c.method === 'POST').body);
 assert.deepEqual(corpo.complementos, ['bacon']); assert.equal(corpo.precoCentavos, undefined);
});


test('complemento esgotado permanece identificado e desabilitado após atualizar os controles', async () => {
 const t=await montar();t.produtos[0].complementos=[{ id:'grupo',nome:'Extras',minimo:1,maximo:1,opcoes:[{ id:'bacon',nome:'Bacon',precoCentavos:200,disponivel:false,quantidadeDisponivel:0 }] }];
 await t.abrir();await t.clicar('abrirProdutos');await t.clicar('atualizarCatalogo');await t.escolher();
 const opcao=t.ids.complementos.querySelectorAll('input')[0];assert.equal(opcao.disabled,true);assert.match(t.ids.complementos.textContent,/Esgotado/);
 await t.eventos.poll();assert.equal(opcao.disabled,true);await t.enviar();assert.equal(t.chamadas.filter(c=>c.method==='POST').length,0);
});

test('regras individuais permitem várias unidades e validam mínimo e máximo do complemento', async () => {
 const t=await montar();t.produtos[0].complementos=[{ id:'grupo',nome:'Extras',minimo:0,maximo:3,opcoes:[{ id:'queijo',complementoId:5,nome:'Queijo',precoCentavos:250,disponivel:true,quantidadeDisponivel:5,minimoQuantidade:2,maximoQuantidade:3 }] }];
 await t.abrir();await t.clicar('abrirProdutos');await t.clicar('atualizarCatalogo');await t.escolher();
 const campo=t.ids.complementos.querySelectorAll('input')[0];assert.equal(campo.type,'number');assert.equal(campo.max,'3');
 campo.value='1';campo.onchange();await t.enviar();assert.match(t.ids.avisoItem.textContent,/Queijo/);assert.equal(t.chamadas.filter(c=>c.method==='POST').length,0);
 campo.value='4';campo.onchange();await t.enviar();assert.equal(t.chamadas.filter(c=>c.method==='POST').length,0);
 campo.value='2';campo.onchange();assert.match(t.ids.totalItem.textContent,/12,50/);await t.enviar();
 const corpo=JSON.parse(t.chamadas.find(c=>c.method==='POST').body);assert.deepEqual(corpo.complementos,['queijo','queijo']);
});

test('atendimento da equipe não oferece chamada de garçom',()=>{
 assert.ok(!html.includes('chamarAtendente'));
 assert.ok(!html.includes('chamadaMesa'));
 assert.ok(!js.includes('/chamadas'));
});
