from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
r=Path('prototypes/licencas-mobile');fonts=Path('C:/Windows/Fonts')
INK='#17232c';MUTED='#52616d';BG='#f7f8fa';LINE='#d6dce1';ACCENT='#b53620';SUCCESS='#24674a'
def F(size,bold=False):return ImageFont.truetype(str(fonts/('seguisb.ttf' if bold else 'segoeui.ttf')),size)
def text(d,xy,t,size=18,bold=False,color=INK):d.text(xy,t,font=F(size,bold),fill=color)
def wrap(d,t,w,size=18):
 lines=['']
 for word in t.split():
  z=(lines[-1]+' '+word).strip()
  if d.textlength(z,font=F(size))>w:lines.append(word)
  else:lines[-1]=z
 return lines

def paragraph(d,x,y,t,w=356,size=18,color=MUTED):
 for line in wrap(d,t,w,size):text(d,(x,y),line,size,color=color);y+=size+8
 return y

def button(d,x,y,w,label,primary=False,disabled=False,icon=False):
 fill='#e5e8ec' if disabled else ACCENT if primary else 'white';border=fill if primary or disabled else '#b9c3cc'
 d.rounded_rectangle((x,y,x+w,y+54),radius=11,fill=fill,outline=border,width=1)
 color='#697984' if disabled else 'white' if primary else INK
 if icon:
  for ox,oy in [(14,16),(30,16),(14,32)]:d.rectangle((x+ox,y+oy,x+ox+9,y+oy+9),outline=color,width=2)
  d.line((x+32,y+34,x+40,y+42),fill=color,width=2)
  tx=x+51
 else:tx=x+(w-d.textlength(label,font=F(17,True)))/2
 text(d,(tx,y+15),label,17,True,color)

def field(d,x,y,label,value,w=356,multi=False,required=True):
 text(d,(x,y),label,17,True)
 if required:text(d,(x+d.textlength(label,font=F(17,True))+5,y),'*',17,True,ACCENT)
 h=76 if multi else 54;d.rounded_rectangle((x,y+30,x+w,y+30+h),radius=9,fill='white',outline='#b9c3cc')
 for j,line in enumerate(value.split('\n')):text(d,(x+14,y+45+j*24),line,18,color=INK)
 return y+30+h+24

def check(d,x,y,label,checked=False,w=320):
 d.rounded_rectangle((x,y+3,x+22,y+25),radius=4,fill=ACCENT if checked else 'white',outline=ACCENT if checked else '#a5b2bd',width=1)
 if checked:d.line((x+5,y+13,x+10,y+18,x+18,y+8),fill='white',width=2)
 return paragraph(d,x+35,y,label,w,17,INK)

def phone(title,subtitle=None,step=None):
 im=Image.new('RGB',(430,920),'white');d=ImageDraw.Draw(im)
 text(d,(24,10),'9:41',15,True);text(d,(342,10),'5G  100%',13)
 d.line((0,40,430,40),fill=LINE)
 text(d,(24,61),'Varthex Licenças',20,True);text(d,(353,64),'Chave',15,True,ACCENT)
 d.line((24,105,406,105),fill=LINE)
 d.ellipse((25,126,34,135),fill=SUCCESS);text(d,(44,120),'Chave protegida neste aparelho',15,color=SUCCESS)
 text(d,(24,165),title,29,True)
 if subtitle:paragraph(d,24,207,subtitle,378,17)
 if step is not None:
  labels=['Cliente','Validade','Revisão']
  for i,l in enumerate(labels):
   x=24+i*132;col=ACCENT if i==step else SUCCESS if i<step else '#9ba7b1'
   d.ellipse((x,256,x+23,279),fill=col if i<=step else '#e6eaee')
   if i<step:d.line((x+5,267,x+10,272,x+18,262),fill='white',width=2)
   else:text(d,(x+7,258),str(i+1),13,True,'white' if i==step else MUTED)
   text(d,(x+31,257),l,15,i==step,col)
  d.line((24,302,406,302),fill=LINE)
 return im,d

def footer(d,label,back=False,disabled=False,step=None):
 d.rectangle((0,805,430,920),fill='white');d.line((0,805,430,805),fill=LINE)
 if step is not None:text(d,(24,821),f'Etapa {step+1} de 3',14,color=MUTED)
 if back:
  button(d,24,850,91,'Voltar');button(d,127,850,279,label,True,disabled)
 else:button(d,24,850,382,label,True,disabled)

def validity(d,y,compact=False):
 text(d,(24,y),'Qual o período da licença?',19,True);y+=40
 labels=['1 mês','2 meses','3 meses','Vitalícia','Dias','Data']
 for i,l in enumerate(labels):
  x=24+(i%3)*130;yy=y+(i//3)*56;sel=i==0
  d.rounded_rectangle((x,yy,x+122,yy+46),radius=9,fill='#fff0eb' if sel else 'white',outline=ACCENT if sel else LINE,width=2 if sel else 1)
  text(d,(x+(122-d.textlength(l,font=F(17,True)))/2,yy+12),l,17,True,ACCENT if sel else INK)
 return y+122

p1,d=phone('Nova licença','Identifique quem vai usar o VComanda.',0)
y=field(d,24,327,'Cliente ou estabelecimento','Restaurante Exemplo')
y=field(d,24,y,'Computador do cliente','4D8A7C…9F21\nIdentificado pelo QR Code',multi=True)
button(d,24,y,382,'Escanear QR Code',icon=True)
text(d,(139,y+78),'Colar código',17,True,ACCENT)
paragraph(d,24,y+128,'Leia o QR exibido na tela de ativação do VComanda.',378,17)
footer(d,'Continuar',step=0)
p2,d=phone('Validade','Escolha por quanto tempo liberar o acesso.',1)
y=validity(d,332)
y=check(d,24,y+16,'Renovação antecipada: somar ao vencimento atual')
d.line((24,y+28,406,y+28),fill=LINE)
text(d,(24,y+53),'Último dia de uso',16,color=MUTED);text(d,(24,y+82),'29 de novembro de 2026',23,True)
paragraph(d,24,y+127,'Vence em 30/11/2026, às 00:00. Horário de Brasília.',378,17)
footer(d,'Revisar licença',True,step=1)
p3,d=phone('Confira antes de emitir','Revise os dados e autorize a emissão.',2)
y=330
for label,value in [('Cliente','Restaurante Exemplo'),('Computador','4D8A7C…9F21'),('Validade','1 mês'),('Último dia de uso','29 de novembro de 2026')]:
 text(d,(24,y),label,15,color=MUTED);text(d,(24,y+27),value,21,True);d.line((24,y+64,406,y+64),fill=LINE);y+=84
check(d,24,685,'Pagamento confirmado ou emissão autorizada',True)
paragraph(d,24,750,'Confirme com o bloqueio do celular.',378,15)
footer(d,'Confirmar e gerar',True,step=2)
board=Image.new('RGB',(1550,1170),'#edf0f3');bd=ImageDraw.Draw(board)
text(bd,(48,28),'Opção 1 · Etapas curtas',35,True)
text(bd,(48,82),'Uma decisão por tela, com avanço e retorno sem perder o preenchimento.',21,color=MUTED)
for i,(p,l) in enumerate([(p1,'1. Cliente e computador'),(p2,'2. Validade'),(p3,'3. Revisão e emissão')]):
 x=48+i*502;board.paste(p,(x,158));bd.rounded_rectangle((x-1,157,x+430,1079),radius=1,outline='#b7c2cb',width=1);text(bd,(x,120),l,20,True)
text(bd,(48,1110),'Exemplo ilustrativo com dados fictícios. A autenticação do aparelho continua obrigatória.',18,color=MUTED)
board.save(r/'fluxo-etapas.png')

s1,d=phone('Nova licença','Preencha as seções e revise para emitir.')
y=field(d,24,265,'Cliente ou estabelecimento','Restaurante Exemplo')
y=field(d,24,y,'Computador do cliente','4D8A7C…9F21')
button(d,24,y,245,'Escanear QR Code',icon=True);button(d,281,y,125,'Colar')
y=validity(d,y+90)
check(d,24,y+15,'Renovação antecipada')
footer(d,'Revisar e gerar',disabled=True)
s2,d=phone('Nova licença','Mesma página, após rolar até a revisão.')
text(d,(24,266),'Resumo da licença',22,True);y=310
for l,v in [('Cliente','Restaurante Exemplo'),('Computador','4D8A7C…9F21'),('Período','1 mês'),('Uso até','29/11/2026')]:
 text(d,(24,y),l,15,color=MUTED);text(d,(24,y+25),v,20,True);d.line((24,y+62,406,y+62),fill=LINE);y+=79
check(d,24,656,'Pagamento confirmado ou emissão autorizada',True)
paragraph(d,24,724,'Ao continuar, confira a confirmação e desbloqueie o celular para emitir.',378,16)
footer(d,'Revisar e gerar')
board=Image.new('RGB',(1180,1170),'#edf0f3');bd=ImageDraw.Draw(board)
text(bd,(48,28),'Opção 2 · Uma tela organizada',35,True)
text(bd,(48,82),'Cliente, computador e validade no mesmo formulário, com resumo ao rolar.',20,color=MUTED)
for x,p,l in [(48,s1,'Início do formulário'),(674,s2,'Após rolar a mesma página')]:
 board.paste(p,(x,158));bd.rectangle((x-1,157,x+430,1079),outline='#b7c2cb');text(bd,(x,120),l,20,True)
bd.line((527,585,623,585),fill='#718492',width=3);bd.polygon([(623,585),(610,576),(610,594)],fill='#718492');text(bd,(536,606),'Rolar',17,color=MUTED)
text(bd,(48,1110),'Exemplo ilustrativo com dados fictícios. Os botões de importar/remover chave ficam em “Chave”.',17,color=MUTED)
board.save(r/'fluxo-tela-unica.png')
print('Exemplos criados:',(r/'fluxo-etapas.png').resolve(),(r/'fluxo-tela-unica.png').resolve())
