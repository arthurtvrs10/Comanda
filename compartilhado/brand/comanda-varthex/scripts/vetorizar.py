"""Vetoriza os contornos da referência; não incorpora bitmap no SVG."""
from pathlib import Path
from collections import defaultdict
import json
import numpy as np
from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
im = np.array(Image.open(ROOT / '02-logos/comanda-horizontal-transparente.png').convert('RGBA'))
alpha = im[:,:,3] > 160
orange = alpha & (im[:,:,0] > 160) & (im[:,:,1] < 160)
ink = alpha & ~orange

def simplify(points, eps=0.85):
    if len(points)<3: return points
    a,b=np.array(points[0]),np.array(points[-1]); v=b-a
    pts=np.array(points); delta=pts-a
    if np.dot(v,v)==0: distances=np.linalg.norm(delta,axis=1)
    else:
        t=np.clip(delta@v/np.dot(v,v),0,1)
        distances=np.linalg.norm(delta-t[:,None]*v,axis=1)
    i=int(np.argmax(distances))
    if distances[i]<=eps:return [points[0],points[-1]]
    return simplify(points[:i+1],eps)[:-1]+simplify(points[i:],eps)

def trace(mask):
    # Oriented pixel boundaries preserve holes and letter counters.
    edges=defaultdict(list)
    h,w=mask.shape
    for y,x in zip(*np.where(mask)):
        x,y=int(x),int(y)
        if y==0 or not mask[y-1,x]:edges[(x,y)].append((x+1,y))
        if x==w-1 or not mask[y,x+1]:edges[(x+1,y)].append((x+1,y+1))
        if y==h-1 or not mask[y+1,x]:edges[(x+1,y+1)].append((x,y+1))
        if x==0 or not mask[y,x-1]:edges[(x,y+1)].append((x,y))
    loops=[]
    while edges:
        start=next(iter(edges)); p=start; loop=[p]
        while p in edges:
            q=edges[p].pop()
            if not edges[p]:del edges[p]
            p=q;loop.append(p)
            if p==start:break
        area=abs(sum(a[0]*b[1]-b[0]*a[1] for a,b in zip(loop,loop[1:])))/2
        if area<18:continue
        # Split closed loop to keep RDP stable.
        mid=len(loop)//2
        loop=simplify(loop[:mid+1])[:-1]+simplify(loop[mid:])
        loops.append(loop)
    return loops

def path(loops):
    return ' '.join('M'+' L'.join(f'{x},{y}' for x,y in loop)+' Z' for loop in loops)

variants={'colorida':('#FF5934','#17252C'),'preta':('#000000','#000000'),'branca':('#FFFFFF','#FFFFFF'),'negativa':('#FF5934','#FFFFFF')}
assets={}
for name, mask_o,mask_i in [
    ('logo-horizontal',orange,ink),
    ('logo-sem-assinatura',orange,ink & (np.indices(ink.shape)[0]<470)),
    ('logotipo',np.zeros_like(orange),ink & (np.indices(ink.shape)[0]<470)),
    ('simbolo',orange,np.zeros_like(ink))]:
    # Full-logo raster is the single reference for every composition.
    union=mask_o|mask_i; ys,xs=np.where(union)
    x0,y0,x1,y1=int(xs.min()),int(ys.min()),int(xs.max()+1),int(ys.max()+1)
    pad=20; box=(x0-pad,y0-pad,x1-x0+pad*2,y1-y0+pad*2)
    po,pi=path(trace(mask_o)),path(trace(mask_i))
    for variant,(co,ci) in variants.items():
        folder=ROOT/'exportacoes'/name/variant
        (folder/'svg').mkdir(parents=True,exist_ok=True)
        svg=f'<svg xmlns="http://www.w3.org/2000/svg" viewBox="{box[0]} {box[1]} {box[2]} {box[3]}" width="{box[2]}" height="{box[3]}" role="img" aria-label="Comanda por Varthex"><title>Comanda — {name} — {variant}</title><g fill-rule="evenodd"><path fill="{co}" d="{po}"/><path fill="{ci}" d="{pi}"/></g></svg>'
        out=folder/'svg'/f'comanda-{name}-{variant}.svg'
        out.write_text(svg,encoding='utf-8')
        assets[str(out.relative_to(ROOT)).replace('\\','/')]={'width':box[2],'height':box[3]}
(ROOT/'fontes/vetores').mkdir(parents=True,exist_ok=True)
for name in ['logo-horizontal','logo-sem-assinatura','logotipo','simbolo']:
    src=ROOT/'exportacoes'/name/'colorida/svg'/f'comanda-{name}-colorida.svg'
    (ROOT/'fontes/vetores'/f'{name}.svg').write_text(src.read_text(encoding='utf-8'),encoding='utf-8')
(ROOT/'manifesto.json').write_text(json.dumps({'referencia':'02-logos/comanda-horizontal-transparente.png','metodo':'Contornos vetoriais simplificados; cores sólidas; sem imagens incorporadas','svg':assets},ensure_ascii=False,indent=2),encoding='utf-8')
print(f'{len(assets)} SVGs exportados.')
