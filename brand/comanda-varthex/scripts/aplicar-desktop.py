"""Sincroniza a marca aprovada com os recursos nativos do aplicativo WPF."""
from pathlib import Path
import shutil
import xml.etree.ElementTree as ET
from PIL import Image

brand=Path(__file__).resolve().parents[1]
repo=brand.parents[1]
assets=repo/'backend/src/VarthexComanda.Desktop/Assets'
ns={'s':'http://www.w3.org/2000/svg'}
parts=['<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">']
for key,variant in [('ComandaLogoClaro','colorida'),('ComandaLogoEscuro','negativa')]:
    source=brand/f'exportacoes/logo-horizontal/{variant}/svg/comanda-logo-horizontal-{variant}.svg'
    svg=ET.parse(source).getroot(); x,y,w,h=map(float,svg.attrib['viewBox'].split())
    parts.append(f'<DrawingImage x:Key="{key}"><DrawingImage.Drawing><DrawingGroup><GeometryDrawing Brush="Transparent"><GeometryDrawing.Geometry><RectangleGeometry Rect="0,0,{w},{h}"/></GeometryDrawing.Geometry></GeometryDrawing><DrawingGroup><DrawingGroup.Transform><TranslateTransform X="{-x}" Y="{-y}"/></DrawingGroup.Transform>')
    for p in svg.findall('.//s:path',ns):
        if p.attrib['d']:
            parts.append(f'<GeometryDrawing Brush="{p.attrib["fill"]}" Geometry="F0 {p.attrib["d"]}"/>')
    parts.append('</DrawingGroup></DrawingGroup></DrawingImage.Drawing></DrawingImage>')
parts.append('</ResourceDictionary>')
(assets/'MarcaComanda.xaml').write_text('\n'.join(parts),encoding='utf-8')
for dest,variant in [('LogoRodape.png','negativa'),('LogoBranca.png','branca'),('LogoAjudaClara.png','preta'),('LogoAjudaEscura.png','branca')]:
    shutil.copy2(brand/f'exportacoes/logo-horizontal/{variant}/png/comanda-logo-horizontal-{variant}-1024px.png',assets/dest)
symbol=Image.open(brand/'exportacoes/simbolo/colorida/png/comanda-simbolo-colorida-1024px.png').convert('RGBA')
icon=Image.new('RGBA',(1200,1200),'#FFFFFF')
symbol.thumbnail((880,880),Image.Resampling.LANCZOS)
icon.alpha_composite(symbol,((1200-symbol.width)//2,(1200-symbol.height)//2))
icon.save(assets/'VarthexComanda.ico',format='ICO',sizes=[(16,16),(24,24),(32,32),(48,48),(64,64),(128,128),(256,256)])
print('Recursos WPF, PNGs e ícone sincronizados.')
