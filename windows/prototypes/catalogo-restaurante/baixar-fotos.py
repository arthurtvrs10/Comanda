import json,concurrent.futures,urllib.request
from pathlib import Path
root=Path('prototypes/catalogo-restaurante');assets={
'marmita':('https://rnstatic.sfo3.digitaloceanspaces.com/Upload/Noticias/Materia/18-05-2021-10-10-33.jpg','https://rondonoticias.com.br/noticia/geral/56760/programa-prato-facil-inicia-atendimento-com-refeicoes-a-r-2-reais'),
'coca2':('https://lojasmel1.vtexassets.com/arquivos/ids/156250/Refrigerante_Pet_2_Litros_-_Coca-Cola_0.jpg?v=638031921412770000','https://www.lojasmel.com/refrigerante-pet-2-litros-55209-cocacola/p'),
'coca15':('https://cdn.shopify.com/s/files/1/0587/8487/4575/files/01f76d74-0529-47b4-86c3-65465cc67ee8.png?v=1757030906','https://marche.com.br/products/coca-cola-sabor-original-pet-1-5-litros'),
'fanta':('https://andinacocacola.vtexassets.com/arquivos/ids/159003/Fanta-Laranja-2L-Pet--110306_COCA-.jpg?v=639094449078300000','https://andina.nasuacasa.coca-cola.com.br/fanta-laranja-pet-2l-110306-coca/p'),
'guarana':('https://www.paodeacucar.com/img/uploads/1/88/33036088.jpg','https://www.paodeacucar.com/produto/44656/refrigerante-guarana-antarctica-garrafa-2l'),
'ks':('https://assets.instabuy.app.br/ib.item.image.large/l-22208a3db2344bd88519b2d7531969e1.jpeg','https://mercadopina.com.br/p/Coca-Cola-Ks-Ultra-290Ml'),
'lata':('https://bretas.vtexassets.com/arquivos/ids/201501-800-auto?aspect=true&height=auto&v=638376352002870000&width=800','https://www.bretas.com.br/refrigerante-coca-cola-lata-350ml/p'),
'monster':('https://boa.vtexassets.com/arquivos/ids/600950/Bebida-Energetica-Monster-Energy-Lata-473ml.jpg?v=639077385134230000','https://www.boasupermercados.com.br/bebida-energetica-monster-energy-lata-473ml/p'),
'agua':('https://img.superhoje.com/img.produtos/7894900530001/img_1200_1.png','https://superkan.com.br/agua-mineral-natural-sem-gas-crystal-500ml/7894900530001/d'),
'suco':('https://www.agrolink.com.br/upload/imagens-resizes/ff907f81d0c441ddaee850a9d76f1e61_1200x800.jpg','https://www.agrolink.com.br/noticias/quem-tem-diabetes-pode-tomar-suco-de-laranja-_474312.html'),
'bala':('https://images.tcdn.com.br/img/img_prod/1098943/bala_mastigavel_de_yogurte_morango_dori_100g_1und_30613_3_7dc0fe43189269203d718bf594d002e9.jpg','https://www.embalagemfacil.com.br/bala-mastigavel-de-yogurte-morango-dori-100g-1und'),
'pacoca':('https://rafinhaalimentos.com.br/media/catalog/product/cache/f7e57eaec4cbc1f73df85f438e834d41/3/0/3073_3.jpg','https://rafinhaalimentos.com.br/rafinha-doce-atacado-pacoca-rolha-140x55gr-2128.html'),
'pudim':('https://receitatodahora.com.br/wp-content/uploads/2018/08/pudim-de-leite-condensado-com-furinhos-1200x746.jpg','https://receitatodahora.com.br/pudim-de-leite-condensado-com-furinhos/'),
'batata':('https://dynamic-media-cdn.tripadvisor.com/media/photo-o/1a/96/32/ad/photo3jpg.jpg?h=800&s=1&w=1200','https://www.tripadvisor.com.br/Restaurants-g1231719-Sobral_State_of_Ceara.html')}
(root/'fontes.json').write_text(json.dumps({k:{'imagem':v[0],'pagina':v[1]} for k,v in assets.items()},ensure_ascii=False,indent=2),encoding='utf-8')
def baixar(pair):
 key,(url,_) = pair;p=root/'fotos'/f'{key}.original'
 try:
  req=urllib.request.Request(url,headers={'User-Agent':'Mozilla/5.0'})
  with urllib.request.urlopen(req,timeout=25) as r:p.write_bytes(r.read(15_000_000))
  from PIL import Image
  with Image.open(p) as im:im.verify()
  return key,'ok'
 except Exception as e:return key,str(e)[:140]
with concurrent.futures.ThreadPoolExecutor(max_workers=6) as ex:
 for key,status in ex.map(baixar,assets.items()):print(key,status,flush=True)
