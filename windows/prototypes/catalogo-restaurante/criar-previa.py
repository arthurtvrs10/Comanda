from pathlib import Path
from PIL import Image,ImageOps,ImageDraw,ImageFont
import json,html
r=Path('prototypes/catalogo-restaurante')
items=[
('Refeições','Marmita por unidade',2200,'UN','marmita'),('Refeições','Marmita por quilo',5490,'KG','marmita'),('Porções','Batata frita — porção',1800,'UN','batata'),('Doces','Pudim — fatia',800,'UN','pudim'),
('Bebidas','Coca-Cola 2 L',1400,'UN','coca2'),('Bebidas','Coca-Cola 1,5 L',1100,'UN','coca15'),('Bebidas','Fanta Laranja 2 L',1200,'UN','fanta'),('Bebidas','Guaraná Antarctica 2 L',1200,'UN','guarana'),
('Bebidas','Coca-Cola KS 290 ml',500,'UN','ks'),('Bebidas','Coca-Cola lata 350 ml',600,'UN','lata'),('Bebidas','Monster Original 473 ml',1400,'UN','monster'),('Bebidas','Água sem gás 500 ml',400,'UN','agua'),
('Bebidas','Suco de laranja na hora 300 ml',800,'UN','suco'),('Bebidas','Suco de laranja na hora 500 ml',1200,'UN','suco'),('Doces','Balinha de iogurte — unidade',50,'UN','bala'),('Doces','Paçoca rolha — pacote 55 g',300,'UN','pacoca')]
sources=json.loads((r/'fontes.json').read_text(encoding='utf-8'))
rows=[]
for cat,name,price,unit,key in items:
 rows.append(dict(Categoria=cat,Nome=name,PrecoCentavos=price,Unidade=unit,Foto=key+'.png',Fonte=sources[key]['pagina']))
for key in sources:
 with Image.open(r/'fotos'/f'{key}.original') as im:
  im=ImageOps.exif_transpose(im).convert('RGBA');bg=Image.new('RGBA',im.size,'white');bg.alpha_composite(im);bg.convert('RGB').save(r/'fotos'/f'{key}.png')
(r/'catalogo-restaurante.proposta.json').write_text(json.dumps(rows,ensure_ascii=False,indent=2),encoding='utf-8')
fontdir=Path('C:/Windows/Fonts')
def font(n,bold=False):return ImageFont.truetype(str(fontdir/('seguisb.ttf' if bold else 'segoeui.ttf')),n)
def money(p):return ('R$ '+format(p/100,'.2f')).replace('.',',')
W,H=1680,1410;img=Image.new('RGB',(W,H),'#f2f2f2');d=ImageDraw.Draw(img)
d.rectangle((0,0,W,40),fill='#454545');d.text((24,8),'VComanda · Prévia do catálogo de restaurante',font=font(17),fill='white')
d.rectangle((0,40,W,94),fill='#192b32');d.text((28,55),'DEMONSTRAÇÃO',font=font(18,True),fill='white');d.text((270,55),'Lanchonete atual   /   Restaurante proposto',font=font(18),fill='#d3dde2')
d.text((32,114),'Restaurante: refeições, bebidas e doces',font=font(32,True),fill='#182a32')
d.text((32,160),'16 produtos de exemplo · Fotos da internet · Preços ilustrativos para validação',font=font(19),fill='#4e606b')
x=32
for text in ['Todos (16)','Refeições (2)','Bebidas (10)','Porções (1)','Doces (3)']:
 width=int(d.textlength(text,font=font(17)))+30;d.rectangle((x,204,x+width,242),fill='#192b32' if x==32 else '#ffffff',outline='#a9b3bb');d.text((x+15,212),text,font=font(17),fill='white' if x==32 else '#253c48');x+=width+12
margin=32;gap=18;cw=(W-margin*2-gap*3)//4;ch=257
for i,item in enumerate(rows):
 x=margin+(i%4)*(cw+gap);y=264+(i//4)*(ch+18)
 d.rectangle((x,y,x+cw,y+ch),fill='white',outline='#aeb8bf',width=1)
 im=Image.open(r/'fotos'/item['Foto']).convert('RGB');im=ImageOps.contain(im,(cw-36,132),Image.Resampling.LANCZOS)
 img.paste(im,(x+(cw-im.width)//2,y+12+(132-im.height)//2))
 d.text((x+16,y+150),item['Categoria'],font=font(14),fill='#526574')
 name=item['Nome'];words=name.split();lines=['']
 for word in words:
  candidate=(lines[-1]+' '+word).strip()
  if d.textlength(candidate,font=font(18,True))>cw-32:lines.append(word)
  else:lines[-1]=candidate
 for j,line in enumerate(lines):d.text((x+16,y+172+j*23),line,font=font(18,True),fill='#192b32')
 label=money(item['PrecoCentavos'])+(' / kg' if item['Unidade']=='KG' else '')
 d.text((x+16,y+222),label,font=font(23,True),fill='#192b32')
 unit=item['Unidade'];uw=44;d.rectangle((x+cw-uw-14,y+222,x+cw-14,y+246),fill='#e5f0fa' if unit=='KG' else '#edf0f2');d.text((x+cw-uw-6,y+224),unit,font=font(14,True),fill='#1e5680' if unit=='KG' else '#4e606b')
d.line((32,1370,W-32,1370),fill='#aeb8bf',width=1);d.text((32,1380),'Somente prévia. O catálogo atual e os dados do sistema continuam preservados.',font=font(17),fill='#4e606b')
img.save(r/'previa-restaurante.png')
cards=''.join(f'''<article data-cat="{html.escape(a['Categoria'])}"><img src="fotos/{a['Foto']}" alt="{html.escape(a['Nome'])}"><small>{a['Categoria']}</small><h2>{html.escape(a['Nome'])}</h2><p><strong>{money(a['PrecoCentavos'])}{' / kg' if a['Unidade']=='KG' else ''}</strong><span>{a['Unidade']}</span></p><a href="{html.escape(a['Fonte'])}" target="_blank" rel="noreferrer">Fonte da foto</a></article>''' for a in rows)
page='''<!doctype html><html lang="pt-BR"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>VComanda — proposta restaurante</title><style>*{box-sizing:border-box}body{margin:0;background:#f2f2f2;color:#192b32;font:16px 'Segoe UI',sans-serif}header{background:#192b32;color:white;padding:18px 28px}main{max-width:1500px;padding:28px;margin:auto}h1{margin:0 0 8px;font-size:30px}summary,.nota{color:#526574}nav{display:flex;gap:10px;flex-wrap:wrap;margin:24px 0}button{border:1px solid #aeb8bf;background:white;padding:12px 18px;font:inherit;color:inherit;cursor:pointer}button[aria-pressed=true]{background:#192b32;color:white}button:focus-visible,a:focus-visible{outline:3px solid #2781de;outline-offset:3px}.catalogo{display:grid;grid-template-columns:repeat(4,minmax(0,1fr));gap:18px}article{background:white;border:1px solid #aeb8bf;padding:16px}article[hidden]{display:none}article img{display:block;width:100%;height:165px;object-fit:contain;background:white}small{color:#526574}h2{font-size:18px;min-height:48px;margin:10px 0}article p{display:flex;justify-content:space-between;align-items:center}strong{font-size:23px}span{font-size:13px;background:#edf0f2;padding:5px 9px}a{font-size:13px;color:#215b84}footer{margin-top:28px;padding-top:18px;border-top:1px solid #aeb8bf}@media(max-width:1000px){.catalogo{grid-template-columns:repeat(3,minmax(0,1fr))}}@media(max-width:720px){.catalogo{grid-template-columns:repeat(2,minmax(0,1fr))}main{padding:18px}}@media(max-width:430px){.catalogo{grid-template-columns:1fr}}</style><header>VComanda · Demonstração restaurante · Prévia para avaliação</header><main><h1>Refeições, bebidas e doces</h1><p class="nota">16 produtos de exemplo. Fotos da internet e preços ilustrativos; nenhum cadastro foi alterado.</p><nav>'''+''.join(f'<button aria-pressed="{str(c=="Todos").lower()}">{c}</button>' for c in ['Todos','Refeições','Bebidas','Porções','Doces'])+'</nav><section class="catalogo">'+cards+'''</section><footer>Proposta: manter a demonstração de lanchonete e oferecer um catálogo separado para restaurante. UN: venda por unidade. KG: venda por peso.</footer></main><script>document.querySelectorAll('nav button').forEach(b=>b.onclick=()=>{document.querySelectorAll('nav button').forEach(a=>a.setAttribute('aria-pressed',a===b));document.querySelectorAll('article').forEach(a=>a.hidden=b.textContent!=='Todos'&&a.dataset.cat!==b.textContent)})</script></html>'''
(r/'index.html').write_text(page,encoding='utf-8')
print('Prévia criada:',(r/'previa-restaurante.png').resolve());print('Produtos:',len(rows),'Fotos distintas:',len(sources))
