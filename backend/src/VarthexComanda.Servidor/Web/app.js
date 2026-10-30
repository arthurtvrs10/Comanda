'use strict';
const $ = id => document.getElementById(id);
const moeda = valor => new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(valor / 100);
const numeroComanda = numero => String(numero).padStart(2, '0');
const chaveNova = new URLSearchParams(location.hash.slice(1)).get('chave');
let chave = sessionStorage.getItem('comanda-chave') || '';
let catalogo = { produtos: [], categorias: [] }, atual = null, produto = null;
let ocupada = false, navegando = false, tela = 'lista', categoria = '', versaoFotos = 0;
let fotos = new Map(), observadorFotos = null;
if (chaveNova) {
 if (chaveNova !== chave) sessionStorage.removeItem('comanda-pendente');
 chave = chaveNova; sessionStorage.setItem('comanda-chave', chave); history.replaceState(null, '', '/');
}
function lerPendente() {
 try {
  const p = JSON.parse(sessionStorage.getItem('comanda-pendente') || 'null');
  return p && typeof p.caminho === 'string' && typeof p.corpo === 'string' && typeof p.id === 'string' ? p : null;
 } catch { return null; }
}
let pendente = lerPendente();
function aviso(texto, erro = false) {
 $('aviso').textContent = texto; $('aviso').hidden = !texto; $('aviso').dataset.erro = String(erro);
}
function elemento(tag, texto, classe) {
 const n = document.createElement(tag);
 if (texto !== undefined) n.textContent = texto;
 if (classe) n.className = classe;
 return n;
}
function iconeFoto() {
 const svg = document.createElementNS('http://www.w3.org/2000/svg', 'svg');
 svg.setAttribute('viewBox', '0 0 32 24'); svg.setAttribute('aria-hidden', 'true');
 const path = document.createElementNS('http://www.w3.org/2000/svg', 'path');
 path.setAttribute('d', 'M12 3a9 9 0 1 0 0 18 9 9 0 0 0 0-18 M12 6a6 6 0 1 0 0 12 6 6 0 0 0 0-12 M26 3v6m3-6v6m-6-6v6m0 0q3 4 6 0m-3 3v9');
 svg.append(path); return svg;
}
async function solicitar(caminho, opcoes = {}) {
 let resposta;
 try {
  resposta = await fetch(caminho, { ...opcoes, signal: AbortSignal.timeout(12000), cache: 'no-store', headers: { Authorization: 'Bearer ' + chave, ...opcoes.headers } });
 } catch { throw Error('Sem resposta do computador. Confira o Wi-Fi e se o servidor continua ligado.'); }
 if (resposta.status === 401) { const e = Error('Conexão encerrada. Leia novamente o QR Code no computador.'); e.status = 401; throw e; }
 return resposta;
}
async function api(caminho, opcoes = {}) {
 const resposta = await solicitar(caminho, opcoes);
 let dados;
 try { dados = await resposta.json(); } catch { throw Error('Não foi possível ler a resposta. Confira o servidor no computador.'); }
 if (!resposta.ok) {
  const e = Error(dados.erro || 'Não foi possível concluir.');
  e.status = resposta.status; e.definitivo = [400, 404, 409].includes(resposta.status); throw e;
 }
 return dados;
}
function controles() {
 document.querySelectorAll('button').forEach(b => b.disabled = ocupada || navegando);
 const indisponivel = ocupada || navegando || !!atual?.emFechamento || !!pendente;
 $('abrirProdutos').disabled = indisponivel;
 document.querySelectorAll('.produto').forEach(b => b.disabled = indisponivel);
 $('adicionar').disabled = indisponivel;
 $('numero').disabled = ocupada || navegando || !!pendente;
 $('operador').disabled = ocupada;
 $('quantidade').disabled = ocupada; $('observacao').disabled = ocupada;
}
function navegar(destino, registrar = true, foco = true) {
 tela = destino;
 for (const nome of ['lista', 'pedido', 'catalogo']) $(nome).hidden = nome !== destino;
 observadorFotos?.disconnect();
 if (destino === 'catalogo') listarProdutos();
 if (registrar) history.pushState({ tela: destino, id: atual?.id }, '');
 if (foco) {
  $(destino === 'lista' ? 'tituloLista' : destino === 'pedido' ? 'tituloPedido' : 'tituloCatalogo').focus({ preventScroll: true });
  window.scrollTo({ top: 0, behavior: 'instant' });
 }
 controles();
}
function salvarPendente(valor) {
 pendente = valor;
 if (valor) sessionStorage.setItem('comanda-pendente', JSON.stringify(valor));
 else sessionStorage.removeItem('comanda-pendente');
 $('pendencia').hidden = !valor; controles();
}
// randomUUID depende de contexto seguro; getRandomValues também funciona na rede local HTTP.
function gerarId() {
 if (crypto.randomUUID) return crypto.randomUUID();
 const b = crypto.getRandomValues(new Uint8Array(16)); b[6] = b[6] & 15 | 64; b[8] = b[8] & 63 | 128;
 const s = Array.from(b, x => x.toString(16).padStart(2, '0')).join('');
 return `${s.slice(0, 8)}-${s.slice(8, 12)}-${s.slice(12, 16)}-${s.slice(16, 20)}-${s.slice(20)}`;
}
async function enviar(caminho, dados) {
 if (ocupada || navegando) return;
 if (pendente && (pendente.caminho !== caminho || pendente.corpo !== JSON.stringify(dados))) throw Error('Confirme primeiro o lançamento pendente.');
 salvarPendente(pendente || { caminho, corpo: JSON.stringify(dados), id: gerarId() });
 ocupada = true; controles(); $('adicionar').textContent = 'Adicionando…';
 let confirmado = false;
 try {
  const r = await api(pendente.caminho, { method: 'POST', headers: { 'Content-Type': 'application/json', 'Idempotency-Key': pendente.id }, body: pendente.corpo });
  confirmado = true; salvarPendente(null); $('personalizar').close();
  await selecionar(r.id);
  aviso('Lançamento confirmado no computador.');
 } catch (e) {
  if (e.definitivo) salvarPendente(null);
  $('avisoItem').textContent = e.message;
  aviso(confirmado ? 'Lançamento confirmado no computador. Não foi possível atualizar a tela; confira os itens antes de lançar novamente.' : e.message + (pendente ? ' Use “Confirmar lançamento pendente” para reenviar a mesma operação.' : ''), !confirmado);
 } finally { ocupada = false; $('adicionar').textContent = 'Adicionar item'; controles(); }
}
async function listarComandas() {
 const r = await api('/api/comandas');
 $('numero').min = r.primeiroNumero; $('numero').placeholder = 'A partir de ' + r.primeiroNumero;
 $('comandas').replaceChildren();
 for (const c of r.comandas) {
  const b = elemento('button', undefined, 'comanda');
  b.append(elemento('small', c.mesa ? 'Mesa ' + c.mesa : 'Balcão'), elemento('strong', numeroComanda(c.numero)), elemento('span', moeda(c.totalCentavos)), elemento('small', c.emFechamento ? 'Em fechamento' : c.cliente || 'Em aberto', c.emFechamento ? 'fechamento' : ''));
  b.onclick = () => abrirPedido(c.id); $('comandas').append(b);
 }
 $('vazio').textContent = r.comandas.length ? '' : 'Nenhuma comanda aberta. Informe o número acima para começar.';
 $('vazio').hidden = !!r.comandas.length; controles();
}
function renderizarPedido() {
 $('tituloPedido').textContent = 'Comanda ' + numeroComanda(atual.numero);
 $('contextoPedido').textContent = [atual.mesa ? 'Mesa ' + atual.mesa : 'Balcão', atual.cliente].filter(Boolean).join(' · ');
 $('total').textContent = moeda(atual.totalCentavos);
 $('situacao').textContent = atual.emFechamento ? 'Em fechamento' : 'Em aberto';
 $('situacao').dataset.fechamento = String(atual.emFechamento);
 $('avisoFechamento').hidden = !atual.emFechamento;
 $('voltarPedidoTexto').textContent = 'Comanda ' + numeroComanda(atual.numero);
 $('itens').replaceChildren();
 for (const i of atual.itens) {
  const item = elemento('li', undefined, 'item'), dados = elemento('div');
  dados.append(elemento('h3', i.nome), elemento('p', i.quantidade + ' ' + i.medida));
  if (i.observacao) dados.append(elemento('p', i.observacao, 'observacao'));
  item.append(dados, elemento('strong', moeda(i.subtotalCentavos))); $('itens').append(item);
 }
 $('semItens').hidden = !!atual.itens.length;
 $('quantidadeItens').textContent = atual.itens.length + (atual.itens.length === 1 ? ' lançamento' : ' lançamentos');
 controles();
}
async function selecionar(id, registrar = true) {
 const dados = await api('/api/comandas/' + id);
 atual = dados; renderizarPedido(); navegar('pedido', registrar);
}
async function abrirPedido(id) {
 if (ocupada || navegando) return;
 navegando = true; controles(); $('conexao').textContent = 'Abrindo comanda…';
 try { await selecionar(id); aviso(''); $('conexao').textContent = 'Conectado à rede local'; }
 catch (e) { aviso(e.message, true); }
 finally { navegando = false; controles(); }
}
function limparFotos() {
 versaoFotos++; observadorFotos?.disconnect();
 for (const promessa of fotos.values()) promessa.then(url => { if (url) URL.revokeObjectURL(url); });
 fotos = new Map();
}
async function carregarFoto(p, quadro, alternativa) {
 const versao = versaoFotos;
 if (!fotos.has(p.id)) fotos.set(p.id, (async () => {
  try {
   const r = await solicitar('/api/produtos/' + p.id + '/foto');
   if (!r.ok || !['image/jpeg', 'image/png', 'image/bmp'].includes(r.headers.get('Content-Type')?.split(';')[0])) return null;
   return URL.createObjectURL(await r.blob());
  } catch { return null; }
 })());
 const url = await fotos.get(p.id);
 if (!url || versao !== versaoFotos || !quadro.isConnected) return;
 const img = elemento('img'); img.alt = ''; img.decoding = 'async';
 img.onload = () => alternativa.hidden = true;
 img.onerror = () => { img.remove(); alternativa.hidden = false; };
 img.src = url; quadro.append(img);
}
function listarCategorias() {
 $('categorias').replaceChildren();
 for (const c of [{ id: '', nome: 'Todas' }, ...catalogo.categorias]) {
  const b = elemento('button', c.nome); b.setAttribute('aria-pressed', String(String(c.id) === categoria));
  b.onclick = () => {
   categoria = String(c.id); listarCategorias(); listarProdutos();
   Array.from($('categorias').children).find(botao => botao.getAttribute('aria-pressed') === 'true')?.focus();
  };
  $('categorias').append(b);
 }
}
function listarProdutos() {
 observadorFotos?.disconnect();
 observadorFotos = typeof IntersectionObserver === 'function' ? new IntersectionObserver(entradas => {
  for (const entrada of entradas) if (entrada.isIntersecting) {
   const q = entrada.target; observadorFotos.unobserve(q); carregarFoto(q.produto, q, q.alternativa);
  }
 }, { rootMargin: '120px' }) : null;
 const busca = $('busca').value.trim().toLocaleLowerCase('pt-BR');
 const lista = catalogo.produtos.filter(p => (!categoria || String(p.categoriaId) === categoria) && (!busca || p.nome.toLocaleLowerCase('pt-BR').includes(busca) || String(p.id) === busca));
 $('produtos').replaceChildren();
 for (const p of lista) {
  const b = elemento('button', undefined, 'produto'), quadro = elemento('span', undefined, 'foto-produto');
  const alternativa = elemento('span', undefined, 'sem-foto'); alternativa.setAttribute('aria-hidden', 'true');
  alternativa.append(iconeFoto(), elemento('span', 'Sem foto')); quadro.append(alternativa);
  const dados = elemento('span', undefined, 'dados-produto'), preco = elemento('span', moeda(p.precoCentavos), 'preco-produto');
  preco.append(elemento('small', ' / ' + p.medida)); dados.append(elemento('small', 'Cód. ' + p.id), elemento('strong', p.nome), preco);
  b.append(quadro, dados); b.setAttribute('aria-label', p.nome + ', ' + moeda(p.precoCentavos) + ' por ' + p.medida + '. Personalizar e adicionar');
  b.onclick = () => personalizar(p); $('produtos').append(b);
  if (p.temFoto) {
   quadro.produto = p; quadro.alternativa = alternativa;
   if (observadorFotos) observadorFotos.observe(quadro); else carregarFoto(p, quadro, alternativa);
  }
 }
 $('encontrados').textContent = lista.length + (lista.length === 1 ? ' produto encontrado' : ' produtos encontrados');
 $('semProdutos').hidden = !!lista.length; controles();
}
async function atualizarCatalogo() {
 const dados = await api('/api/catalogo');
 catalogo = dados; limparFotos();
 if (categoria && !dados.categorias.some(c => String(c.id) === categoria)) categoria = '';
 listarCategorias(); if (tela === 'catalogo') listarProdutos();
}
function personalizar(p) {
 if (ocupada || pendente || atual?.emFechamento) return;
 if (!$('operador').value.trim()) { aviso('Informe o nome do atendente antes de adicionar produtos.', true); $('operador').focus(); return; }
 produto = p; $('nomeProduto').textContent = p.nome; $('precoProduto').textContent = moeda(p.precoCentavos) + ' / ' + p.medida;
 $('medidaProduto').textContent = '(' + p.medida + ')'; $('quantidade').value = '1';
 $('quantidade').step = p.medida === 'UN' ? '1' : '0.001'; $('quantidade').min = p.medida === 'UN' ? '1' : '0.001';
 $('observacao').value = ''; $('avisoItem').textContent = ''; atualizarTotalItem();
 $('personalizar').showModal(); $('quantidade').focus();
}
function atualizarTotalItem() {
 const q = Number($('quantidade').value);
 $('totalItem').textContent = Number.isFinite(q) && q > 0 ? moeda(Math.floor((produto.precoCentavos * Math.round(q * 1000) + 500) / 1000)) : '—';
}
function ajustarQuantidade(direcao) {
 const campo = $('quantidade'), passo = produto.medida === 'UN' ? 1 : .1;
 const q = Math.max(Number(campo.min), Math.round(((Number(campo.value) || 0) + direcao * passo) * 1000) / 1000);
 campo.value = String(q); atualizarTotalItem();
}
$('abrir').onsubmit = e => { e.preventDefault(); enviar('/api/comandas', { numero: Number($('numero').value) }).catch(e => aviso(e.message, true)); };
$('lancar').onsubmit = e => {
 e.preventDefault();
 const nome = $('operador').value.trim();
 if (!nome) { $('avisoItem').textContent = 'Informe o nome do atendente no início da tela.'; return; }
 enviar('/api/comandas/' + atual.id + '/itens', { produtoId: produto.id, quantidade: Number($('quantidade').value), observacao: $('observacao').value, operador: nome }).catch(e => aviso(e.message, true));
};
$('reenviar').onclick = () => { if (pendente) enviar(pendente.caminho, JSON.parse(pendente.corpo)).catch(e => aviso(e.message, true)); };
function fecharPersonalizacao() { if (!ocupada) $('personalizar').close(); }
$('cancelar').onclick = fecharPersonalizacao; $('cancelarTopo').onclick = fecharPersonalizacao;
$('personalizar').oncancel = e => { if (ocupada) e.preventDefault(); };
$('aumentar').onclick = () => ajustarQuantidade(1); $('diminuir').onclick = () => ajustarQuantidade(-1);
$('quantidade').oninput = atualizarTotalItem;
$('voltar').onclick = () => { atual = null; navegar('lista'); listarComandas().catch(e => aviso(e.message, true)); };
$('abrirProdutos').onclick = () => { if (atual && !atual.emFechamento && !pendente) { listarCategorias(); navegar('catalogo'); } };
$('voltarPedido').onclick = () => navegar('pedido');
$('atualizar').onclick = () => listarComandas().then(() => aviso('Comandas atualizadas.')).catch(e => aviso(e.message, true));
$('atualizarCatalogo').onclick = async () => {
 if (navegando || ocupada) return;
 navegando = true; controles(); $('encontrados').textContent = 'Atualizando produtos…';
 try { await atualizarCatalogo(); aviso('Produtos atualizados.'); } catch (e) { aviso(e.message, true); }
 finally { navegando = false; controles(); }
};
$('busca').oninput = listarProdutos;
$('operador').value = sessionStorage.getItem('comanda-operador') || '';
$('operador').oninput = () => sessionStorage.setItem('comanda-operador', $('operador').value);
window.addEventListener('popstate', async e => {
 if (ocupada || navegando) { history.pushState({ tela, id: atual?.id }, ''); return; }
 const destino = e.state?.tela || 'lista';
 if ($('personalizar').open) $('personalizar').close();
 if (destino === 'lista' || !e.state?.id) { atual = null; navegar('lista', false); listarComandas().catch(e => aviso(e.message, true)); return; }
 navegando = true; controles();
 try { await selecionar(e.state.id, false); if (destino === 'catalogo' && !atual.emFechamento) navegar('catalogo', false); }
 catch (erro) { atual = null; navegar('lista', false); aviso(erro.message, true); }
 finally { navegando = false; controles(); }
});
window.addEventListener('pagehide', limparFotos);
window.addEventListener('pageshow', e => { if (e.persisted && tela === 'catalogo') listarProdutos(); });
async function iniciar() {
 $('parear').hidden = !!chave;
 if (!chave) { $('conexao').textContent = 'Sem conexão'; return; }
 try {
  await atualizarCatalogo(); $('atendimento').hidden = false; $('conexao').textContent = 'Conectado à rede local';
  salvarPendente(pendente); await listarComandas(); history.replaceState({ tela: 'lista' }, '');
 } catch (e) { $('parear').hidden = false; $('conexao').textContent = 'Sem conexão'; aviso(e.message, true); }
}
async function sincronizar() {
 if (document.hidden || ocupada || navegando || !chave || $('atendimento').hidden) return;
 try {
  if (atual) {
   const id = atual.id, dados = await api('/api/comandas/' + id);
   if (atual?.id !== id || ocupada || navegando) return;
   atual = dados; renderizarPedido();
   if (atual.emFechamento) {
    if ($('personalizar').open) $('personalizar').close();
    if (tela === 'catalogo') { navegar('pedido'); aviso('A comanda entrou em fechamento. Continue no computador.'); }
   }
  } else await listarComandas();
  $('conexao').textContent = 'Conectado à rede local';
 } catch (e) {
  if (e.status === 404 && atual) {
   atual = null; $('personalizar').close(); navegar('lista'); aviso(e.message, true);
   await listarComandas().catch(() => {});
  } else $('conexao').textContent = 'Reconectando…';
 }
}
iniciar(); setInterval(sincronizar, 4000);
