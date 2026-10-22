const fs = require('fs');
const path = require('path');
const sharp = require('C:/Users/arthu/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp');
const root = path.resolve(__dirname, '..');
const manifest = JSON.parse(fs.readFileSync(path.join(root, 'manifesto.json'), 'utf8'));
(async () => {
  const rows=[]; manifest.png=[];
  for (const rel of Object.keys(manifest.svg)) {
    const abs=path.join(root,rel), isSymbol=rel.includes('/simbolo/');
    const sizes=isSymbol?[32,64,128,256,512,1024]:[256,512,1024,2048,4096];
    const dir=path.join(path.dirname(path.dirname(abs)),'png');fs.mkdirSync(dir,{recursive:true});
    for(const width of sizes){
      const dest=path.join(dir,path.basename(abs,'.svg')+`-${width}px.png`);
      await sharp(abs,{density:300}).resize({width}).png().toFile(dest);
      manifest.png.push(path.relative(root,dest).replaceAll('\\','/'));
    }
    const dark=rel.includes('/branca/')||rel.includes('/negativa/');
    rows.push(`<article><div class="preview ${dark?'dark':''}"><img src="${rel}" alt="${path.basename(abs,'.svg')}" /></div><h2>${rel.split('/')[1]} · ${rel.split('/')[2]}</h2><a href="${rel}">SVG vetorial</a><p>PNG: ${sizes.map(s=>`<a href="${path.relative(root,dir).replaceAll('\\','/')}/${path.basename(abs,'.svg')}-${s}px.png">${s}px</a>`).join(' · ')}</p></article>`);
  }
  fs.writeFileSync(path.join(root,'manifesto.json'),JSON.stringify(manifest,null,2));
  fs.writeFileSync(path.join(root,'catalogo.html'),`<!doctype html><html lang="pt-BR"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>COMANDA · Arquivos da marca</title><style>*{box-sizing:border-box}body{margin:0;padding:48px;background:#fff9f1;color:#17252c;font:16px system-ui}header{max-width:1000px;margin-bottom:40px}h1{font-size:42px;margin:0 0 16px}p{line-height:1.6}main{display:grid;grid-template-columns:repeat(auto-fit,minmax(340px,1fr));gap:24px}article{border:1px solid #d7d4ce;padding:20px;background:white;border-radius:12px}.preview{height:180px;display:flex;align-items:center;justify-content:center;background:#fff9f1;padding:24px}.preview.dark{background:#17252c}.preview img{max-width:100%;max-height:140px}h2{font-size:17px}a{color:#994019}footer{margin-top:40px;font-size:14px}</style><header><h1>comanda <small>por Varthex</small></h1><p>Catálogo dos arquivos da marca. SVGs em curvas, com fundo transparente. PNGs com transparência; medidas indicam a largura em pixels. As versões brancas e negativas aparecem sobre fundo escuro apenas para visualização.</p><p><a href="05-guia/guia-de-uso.md">Guia de uso</a> · <a href="01-apresentacao/identidade-conceito-01.png">Conceito original</a> · <a href="04-paleta/paleta.json">Paleta</a></p></header><main>${rows.join('')}</main><footer>Vetorização derivada da imagem aprovada em 22/10/2026. Lettering em curvas; não depende de instalação de fontes. Versões para impressão devem ser convertidas no perfil de cor da gráfica.</footer></html>`);
  // Contact sheet rendered from the vector sources, not raster upscales.
  const tiles=[];let i=0;
  for(const rel of Object.keys(manifest.svg)){
    const dark=/\/(branca|negativa)\//.test(rel);
    const tile=await sharp({create:{width:600,height:210,channels:4,background:dark?'#17252C':'#FFF9F1'}})
      .composite([{input:await sharp(path.join(root,rel)).resize({width:520,height:150,fit:'inside'}).png().toBuffer(),gravity:'centre'}]).png().toBuffer();
    tiles.push({input:tile,left:(i%2)*600,top:Math.floor(i/2)*210}); i++;
  }
  await sharp({create:{width:1200,height:Math.ceil(i/2)*210,channels:4,background:'#ffffff'}}).composite(tiles).png().toFile(path.join(root,'01-apresentacao/variacoes.png'));
  console.log(`${Object.keys(manifest.svg).length} SVGs e ${manifest.png.length} PNGs; catálogo e contato visual criados.`);
})().catch(e=>{console.error(e);process.exit(1)});
