"""Recria derivados locais sem alterar o PNG mestre. Requer Pillow e numpy."""
from pathlib import Path
import base64, json, math, shutil, struct
import numpy as np
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parent
for folder in ('referencias', 'png', 'svg', '3d', 'paleta'):
    (ROOT / folder).mkdir(exist_ok=True)
master = ROOT / 'referencias/mascote-comanda-master.png'
if not master.exists():
    raise SystemExit('Coloque o PNG mestre em referencias/mascote-comanda-master.png.')
im = Image.open(master).convert('RGBA')
for size in (32, 48, 64, 96, 128, 256, 512, 1024):
    thumb = im.copy()
    thumb.thumbnail((round(size*.9), round(size*.9)), Image.Resampling.LANCZOS)
    canvas = Image.new('RGBA', (size, size))
    canvas.alpha_composite(thumb, ((size-thumb.width)//2, (size-thumb.height)//2))
    canvas.save(ROOT / f'png/mascote-comanda-{size}.png')
# SVG portátil que preserva exatamente a referência raster, sem alegar vetorização.
encoded = base64.b64encode(master.read_bytes()).decode()
(ROOT / 'svg/mascote-referencia-embutida.svg').write_text(
    f'<svg xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" viewBox="0 0 {im.width} {im.height}"><title>Referência raster embutida; não é vetor editável</title><image width="{im.width}" height="{im.height}" xlink:href="data:image/png;base64,{encoded}"/></svg>', encoding='utf-8')

colors = {'laranja': '#FF5934', 'azul-profundo': '#17252C', 'marfim': '#FFF9F1'}
(ROOT / 'paleta/cores.json').write_text(json.dumps({'origem': '../../comanda-varthex/04-paleta/paleta.json', 'cores_base': colors, 'nota': 'Iluminação do render produz variações; HEX define a base, não todos os pixels.'}, ensure_ascii=False, indent=2), encoding='utf-8')
(ROOT / 'paleta/cores.css').write_text(':root {\n' + '\n'.join(f'  --mascote-{k}: {v};' for k,v in colors.items()) + '\n}\n', encoding='utf-8')
swatches=Image.new('RGB',(1200,300),'white'); d=ImageDraw.Draw(swatches)
for i,(name,col) in enumerate(colors.items()):
    d.rectangle((i*400,0,(i+1)*400,220), fill=col)
    d.text((i*400+20,240),f'{name}  {col}',fill='#17252C',font_size=24)
swatches.save(ROOT/'paleta/paleta.png')

# GLB real, com malhas e materiais: estudo esquemático independente, não reconstrução.
vertices=[]; faces=[]; parts=[]
def add(name, verts, inds, material):
    parts.append((name,np.array(verts,dtype='<f4'),np.array(inds,dtype='<u4').reshape(-1),material))
def oval(name,center,scale,mat):
    v=[]; f=[]; rings=16; steps=24
    for j in range(rings+1):
        a=math.pi*j/rings
        for i in range(steps+1):
            b=2*math.pi*i/steps
            p=(math.sin(a)*math.cos(b),math.cos(a),math.sin(a)*math.sin(b))
            v.append([center[k]+scale[k]*p[k] for k in range(3)])
    for j in range(rings):
        for i in range(steps):
            a=j*(steps+1)+i; b=a+steps+1
            f.extend([(a,a+1,b),(a+1,b+1,b)])
    add(name,v,f,mat)
outline=[(-.65,-.95)]
for i in range(1,13): outline.append((-.65+i*1.3/12,-.95+(.08 if i%2 else 0)))
outline.append((.65,.95))
for i in range(1,13): outline.append((.65-i*1.3/12,.95-(.08 if i%2 else 0)))
n=len(outline); v=[(x,y,z) for z in (-.10,.10) for x,y in outline]; f=[]
# Triangulation par éventail autour du centre pour les faces dentelées.
v.extend([(0,0,-.10),(0,0,.10)])
for i in range(n):
    k=(i+1)%n
    f.extend([(2*n,k,i),(2*n+1,n+i,n+k),(i,k,n+i),(k,n+k,n+i)])
add('Corpo-comanda',v,f,0)
for x in (-.27,.27):
    oval('Olho',(x,.43,.13),(.16,.22,.055),1)
    oval('Brilho-olho',(x-.035,.51,.185),(.05,.065,.018),2)
    oval('Sobrancelha',(x,.73,.15),(.16,.04,.035),1)
    oval('Bochecha',(x*1.5,.18,.14),(.09,.045,.025),2)
oval('Sorriso',(0,.12,.15),(.16,.10,.035),1)
for y in (-.23,-.45,-.67):
    oval('Marcador',(-.35,y,.13),(.065,.065,.025),2)
    oval('Linha-pedido',(.12,y,.13),(.28,.045,.025),2)
for x in (-.35,.35):
    oval('Perna',(x,-1.05,0),(.075,.20,.075),1)
    oval('Sapato',(x,-1.24,.07),(.23,.12,.18),1)
    oval('Sola',(x,-1.31,.07),(.23,.045,.18),2)
for x in (-.78,.78): oval('Braco',(x,-.12,0),(.20,.08,.08),1)
oval('Luva-positivo',(-.94,-.02,.08),(.13,.15,.10),2)
oval('Polegar',(-.98,.16,.08),(.06,.16,.06),2)
oval('Luva-lateral',(.94,-.17,.06),(.13,.15,.10),2)
# Avental simplificado. O estudo 3D não contém decalque da logo.
add('Avental-estudo-sem-decalque',[(-.56,-.82,.18),(.56,-.82,.18),(.51,-.12,.18),(-.51,-.12,.18)],[(0,1,2),(0,2,3)],1)
doc={'asset':{'version':'2.0','generator':'VComanda - estudo esquematico; nao e modelo original do render'},'scene':0,'scenes':[{'nodes':[]}], 'nodes':[], 'meshes':[], 'materials':[], 'buffers':[], 'bufferViews':[], 'accessors':[]}
for name,col in colors.items():
    rgb=[int(col[i:i+2],16)/255 for i in (1,3,5)]
    doc['materials'].append({'name':name,'doubleSided':True,'pbrMetallicRoughness':{'baseColorFactor':rgb+[1], 'metallicFactor':0,'roughnessFactor':.7}})
blob=bytearray()
def accessor(arr,target,typ,component):
    while len(blob)%4: blob.append(0)
    off=len(blob); blob.extend(arr.tobytes()); view=len(doc['bufferViews'])
    doc['bufferViews'].append({'buffer':0,'byteOffset':off,'byteLength':arr.nbytes,'target':target})
    acc={'bufferView':view,'componentType':component,'count':len(arr),'type':typ}
    if typ=='VEC3': acc.update(min=arr.min(axis=0).tolist(),max=arr.max(axis=0).tolist())
    doc['accessors'].append(acc); return len(doc['accessors'])-1
for name,v,ind,mat in parts:
    pos=accessor(v,34962,'VEC3',5126); idx=accessor(ind,34963,'SCALAR',5125)
    mesh=len(doc['meshes']); doc['meshes'].append({'name':name,'primitives':[{'attributes':{'POSITION':pos},'indices':idx,'material':mat}]})
    doc['scenes'][0]['nodes'].append(len(doc['nodes'])); doc['nodes'].append({'name':name,'mesh':mesh})
doc['buffers']=[{'byteLength':len(blob)}]
j=json.dumps(doc,separators=(',',':')).encode(); j+=b' '*((-len(j))%4); blob+=b'\0'*((-len(blob))%4)
glb=struct.pack('<4sII',b'glTF',2,12+8+len(j)+8+len(blob))+struct.pack('<I4s',len(j),b'JSON')+j+struct.pack('<I4s',len(blob),b'BIN\0')+blob
(ROOT/'3d/mascote-estudo-esquematico.glb').write_bytes(glb)
print(f'Gerados 8 PNGs, SVG de referência, paleta e GLB com {len(parts)} malhas.')
