Add-Type -AssemblyName System.Drawing
$dir = Split-Path $PSScriptRoot
$root = [IO.Path]::GetFullPath("$dir/../../..")
$img = [Drawing.Image]::FromFile("$dir/fundos/abertura-original.png")
$bg = [Drawing.Bitmap]::new(1080,1080)
$g = [Drawing.Graphics]::FromImage($bg)
$g.InterpolationMode = 'HighQualityBicubic'
# Mesmo recorte central UniformToFill da abertura; adaptado ao quadrado.
$g.DrawImage($img,[Drawing.Rectangle]::new(0,0,1080,1080),[Drawing.Rectangle]::new(0,700,1200,1200),[Drawing.GraphicsUnit]::Pixel)
$g.FillRectangle([Drawing.SolidBrush]::new([Drawing.Color]::FromArgb(71,0,0,0)),0,0,1080,1080)
$g.Dispose()
$bg.Save("$dir/fundos/fundo-mestre.png")
$white = [Drawing.Brushes]::White
$pen = [Drawing.Pen]::new([Drawing.Color]::White,34)
$pen.StartCap='Round'; $pen.EndCap='Round'; $pen.LineJoin='Round'
$files = @()
foreach($name in @('duvidas','treinamento','suporte','comanda')) {
 $b = [Drawing.Bitmap]::new($bg); $g=[Drawing.Graphics]::FromImage($b); $g.SmoothingMode='AntiAlias'
 switch($name) {
 'duvidas' {
 $g.FillEllipse($white,335,345,410,330)
 $g.FillPolygon($white,[Drawing.Point[]]@([Drawing.Point]::new(390,610),[Drawing.Point]::new(375,735),[Drawing.Point]::new(500,650)))
 $dark=[Drawing.SolidBrush]::new([Drawing.ColorTranslator]::FromHtml('#711f18'))
 $font=[Drawing.Font]::new('Arial',150,[Drawing.FontStyle]::Bold,[Drawing.GraphicsUnit]::Pixel)
 $g.DrawString('?', $font,$dark, [single]479,[single]413)
 }
 'treinamento' {
 $g.DrawRectangle($pen,335,345,410,290)
 $g.FillPolygon($white,[Drawing.Point[]]@([Drawing.Point]::new(500,415),[Drawing.Point]::new(500,565),[Drawing.Point]::new(620,490)))
 $g.DrawLine($pen,540,640,540,720); $g.DrawLine($pen,445,725,635,725)
 }
 'suporte' {
 $g.DrawArc($pen,365,350,350,350,180,180)
 $g.FillRectangle($white,345,505,85,150); $g.FillEllipse($white,345,470,85,85)
 $g.FillRectangle($white,650,505,85,150); $g.FillEllipse($white,650,470,85,85)
 $g.DrawArc($pen,555,565,155,145,0,90); $g.DrawLine($pen,550,710,630,710)
 $g.FillEllipse($white,515,685,75,50)
 }
 'comanda' {
 $logo=[Drawing.Bitmap]::FromFile("$root/brand/comanda-varthex/exportacoes/simbolo/branca/png/comanda-simbolo-branca-1024px.png")
 # Remover somente margens transparentes do arquivo oficial.
 $minX=$logo.Width; $minY=$logo.Height; $maxX=0; $maxY=0
 for($y=0;$y -lt $logo.Height;$y++){for($x=0;$x -lt $logo.Width;$x++){if($logo.GetPixel($x,$y).A -gt 20){$minX=[Math]::Min($minX,$x);$minY=[Math]::Min($minY,$y);$maxX=[Math]::Max($maxX,$x);$maxY=[Math]::Max($maxY,$y)}}}
 $w=$maxX-$minX+1; $h=$maxY-$minY+1; $dh=450*$h/$w
 $g.DrawImage($logo,[Drawing.RectangleF]::new(315,(1080-$dh)/2,450,$dh),[Drawing.RectangleF]::new($minX,$minY,$w,$h),[Drawing.GraphicsUnit]::Pixel)
 $logo.Dispose()
 }
 }
 $relative = if ($name -eq 'comanda') { "icone/$name-1080.png" } else { "previas/capas-sugeridas/$name-1080.png" }
 $g.Dispose(); $b.Save("$dir/$relative"); $files+=,$relative; $b.Dispose()
}
$sheet=[Drawing.Bitmap]::new(1440,780);$g=[Drawing.Graphics]::FromImage($sheet);$g.Clear([Drawing.ColorTranslator]::FromHtml('#101a20'))
$font=[Drawing.Font]::new('Segoe UI',22,[Drawing.FontStyle]::Regular,[Drawing.GraphicsUnit]::Pixel)
$title=[Drawing.Font]::new('Segoe UI',30,[Drawing.FontStyle]::Bold,[Drawing.GraphicsUnit]::Pixel)
$g.DrawString('COMANDA / Prévia dos destaques',$title,$white,35,25)
$labels=@('Dúvidas','Treinamento','Suporte','Comanda')
for($i=0;$i -lt 4;$i++){
 $im=[Drawing.Image]::FromFile("$dir/$($files[$i])");$x=35+$i*355
 $g.DrawImage($im,$x,105,305,305);$g.DrawString($labels[$i],$font,$white,$x,425)
 foreach($size in @(48,64,96)){
 $cx=$x+(@(48,64,96).IndexOf($size))*105
 $path=[Drawing.Drawing2D.GraphicsPath]::new();$path.AddEllipse($cx,505,$size,$size)
 $state=$g.Save();$g.SetClip($path);$g.DrawImage($im,$cx,505,$size,$size);$g.Restore($state)
 $g.DrawString("$size px",$font,$white,$cx,620)
 }
 $im.Dispose()
}
$g.DrawString('Categorias sugeridas • Simulação de recorte circular • Ícones sem texto nas capas',$font,$white,35,720)
$g.Dispose();$sheet.Save("$dir/previas/previa.png");$sheet.Dispose();$bg.Dispose();$img.Dispose()
