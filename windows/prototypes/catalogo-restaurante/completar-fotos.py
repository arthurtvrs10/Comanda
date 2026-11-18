from pathlib import Path
import urllib.request,json
r=Path('prototypes/catalogo-restaurante');s=json.loads((r/'fontes.json').read_text(encoding='utf-8'))
updates={'fanta':('https://static.extramercado.com.br/img/uploads/1/955/33321955.png?im=Resize%2Cwidth%3D200','https://www.extramercado.com.br/produto/12767/refrigerante-laranja-fanta-garrafa-2l'),'ks':('https://cdn.awsli.com.br/800x800/2650/2650877/produto/2367111205e53f28a69.jpg','https://www.loja.fernandaobebidas.com.br/produto/coca-cola-media-290ml-ret-ks')}
for key,(url,page) in updates.items():
 req=urllib.request.Request(url,headers={'User-Agent':'Mozilla/5.0'})
 with urllib.request.urlopen(req,timeout=25) as f:(r/'fotos'/f'{key}.original').write_bytes(f.read())
 s[key]={'imagem':url,'pagina':page};print(key,'ok')
(r/'fontes.json').write_text(json.dumps(s,ensure_ascii=False,indent=2),encoding='utf-8')
