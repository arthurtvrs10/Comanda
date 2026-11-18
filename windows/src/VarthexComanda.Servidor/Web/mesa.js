'use strict';
const campo = id => document.getElementById(id);
const parametros = new URLSearchParams(location.hash.slice(1));
let conexao;
try { conexao = JSON.parse(sessionStorage.getItem('mesa-conexao') || 'null'); } catch {}
if (parametros.has('mesa') && parametros.has('chave')) {
 conexao = { mesa: Number(parametros.get('mesa')), chave: parametros.get('chave') };
 sessionStorage.setItem('mesa-conexao', JSON.stringify(conexao));
 history.replaceState(null, '', '/mesa');
}
const valida = conexao && Number.isInteger(conexao.mesa) && conexao.mesa > 0 && conexao.mesa <= 999 && /^[a-f0-9]{64}$/i.test(conexao.chave);
let ocupada = false, pendente = false, indisponivel = !valida;
const armazenamento = 'mesa-tentativa-' + conexao?.mesa;
function controles() {
 campo('chamar').disabled = ocupada || pendente || indisponivel;
 campo('solicitacao').disabled = ocupada || pendente || indisponivel;
 campo('chamar').textContent = ocupada ? 'Enviando…' : pendente ? 'Garçom chamado' : 'Chamar garçom';
}
async function api(opcoes = {}) {
 let resposta;
 try { resposta = await fetch('/api/mesa-publica', { ...opcoes, cache: 'no-store', signal: AbortSignal.timeout(12000), headers: { Authorization: 'Bearer ' + conexao.chave, 'X-Mesa': String(conexao.mesa), ...opcoes.headers } }); }
 catch { throw Error('Sem conexão. Confira o Wi-Fi e tente novamente.'); }
 if (resposta.status === 401) { indisponivel = true; throw Error('QR Code indisponível. Peça à equipe para conferir o código desta mesa.'); }
 const dados = await resposta.json();
 if (!resposta.ok) throw Error(dados.erro || 'Não foi possível chamar. Tente novamente.');
 return dados;
}
function novaIdentificacao() {
 const bytes = crypto.getRandomValues(new Uint8Array(16));bytes[6] = bytes[6] & 15 | 64;bytes[8] = bytes[8] & 63 | 128;
 const s = Array.from(bytes, b => b.toString(16).padStart(2,'0')).join('');
 return `${s.slice(0,8)}-${s.slice(8,12)}-${s.slice(12,16)}-${s.slice(16,20)}-${s.slice(20)}`;
}
campo('formChamada').onsubmit = async evento => {
 evento.preventDefault(); if (ocupada || pendente || indisponivel) return;
 let pedido;
 try { pedido = JSON.parse(sessionStorage.getItem(armazenamento) || 'null'); } catch {}
 pedido = pedido || { solicitacao: campo('solicitacao').value.trim(), identificacao: novaIdentificacao() };
 sessionStorage.setItem(armazenamento, JSON.stringify(pedido));
 ocupada = true; controles();campo('estado').textContent = 'Enviando sua solicitação…';
 try {
  const dados = await api({ method: 'POST', headers: {'Content-Type':'application/json'}, body: JSON.stringify(pedido) });
  sessionStorage.removeItem(armazenamento);pendente = !!dados.chamada;
  campo('estado').textContent = dados.jaAtendida ? 'A equipe já atendeu esta solicitação. Você pode chamar novamente.' : 'A equipe foi avisada. Aguarde o garçom na sua mesa.';
  campo('solicitacao').value = '';
 } catch(erro) { campo('estado').textContent = erro.message; }
 finally { ocupada = false;controles(); }
};
async function atualizar() {
 if (!valida || ocupada || indisponivel || document.hidden) return;
 ocupada = true;
 try {
  const dados = await api();const antes = pendente;pendente = !!dados.chamada;
  campo('tituloMesa').textContent = 'Mesa ' + dados.mesa;
  campo('estado').textContent = pendente ? 'A equipe foi avisada. Aguarde o garçom na sua mesa.' : antes ? 'Solicitação atendida. Se precisar, chame novamente.' : 'Toque no botão para chamar o garçom.';
 } catch(erro) {campo('estado').textContent = erro.message;}
 finally {ocupada = false;controles();}
}
if (valida) {campo('tituloMesa').textContent = 'Mesa ' + conexao.mesa;atualizar();setInterval(atualizar,4000);}
else campo('estado').textContent = 'Leia o QR Code disponível na sua mesa para chamar o garçom.';
