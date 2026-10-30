[CmdletBinding()]
param([switch]$Aplicar)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$repo=(Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).Path
$atuais=[IO.Path]::GetFullPath((Join-Path $repo 'artifacts/release-atual'))
$raizes=@((Join-Path $repo 'artifacts'),(Join-Path $repo 'backend/artifacts'))
$relatorio=Join-Path $repo 'artifacts/limpeza-publicacao'
New-Item -ItemType Directory -Force $relatorio | Out-Null
function Dentro([string]$Caminho,[string]$Raiz){return $Caminho.StartsWith($Raiz+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)}
function Seguro([string]$Caminho){
    $absoluto=(Resolve-Path -LiteralPath $Caminho).Path
    if(-not(@($raizes|Where-Object {Dentro $absoluto $_}).Count) -or $absoluto -eq $atuais -or (Dentro $absoluto $atuais)){throw "Destino fora da limpeza autorizada: $absoluto"}
    $cursor=Get-Item -LiteralPath $absoluto
    while($cursor.FullName -ne $repo){if($cursor.Attributes -band [IO.FileAttributes]::ReparsePoint){throw "Link de diretório recusado: $absoluto"};$cursor=$cursor.Parent;if($null -eq $cursor){throw 'Destino fora do projeto.'}}
    if(Get-ChildItem -LiteralPath $absoluto -Recurse -Attributes ReparsePoint -ErrorAction SilentlyContinue){throw "Pasta contém links; preservada: $absoluto"}
    return $absoluto
}
$fontes=@('backend/artifacts/release-1.4.6-fonte','backend/artifacts/release-1.5.0-fonte','backend/artifacts/chamadas-publicacao-fonte','artifacts/release148/fonte','artifacts/release149/fonte','artifacts/release-1.4.3/source','artifacts/release-1.4.4/source')|ForEach-Object{Join-Path $repo $_}|Where-Object {Test-Path -LiteralPath $_}
$builds=foreach($raiz in $raizes){Get-ChildItem -LiteralPath $raiz -Recurse -File -Filter VarthexComanda.exe -ErrorAction SilentlyContinue | Where-Object { -not(Dentro $_.FullName $atuais) } | ForEach-Object {$_.DirectoryName}}
$candidatos=@($fontes)+@($builds)|Sort-Object -Unique|Sort-Object Length
$planejados=[Collections.Generic.List[object]]::new();$selecionados=[Collections.Generic.List[string]]::new()
$processos=Get-Process -ErrorAction SilentlyContinue | ForEach-Object {try{if($_.Path){$_.Path}}catch{}}
foreach($pasta in $candidatos){
    if(@($selecionados|Where-Object {Dentro $pasta $_}).Count){continue}
    try{$alvo=Seguro $pasta}catch{$planejados.Add([pscustomobject]@{Pasta=$pasta;Acao='Preservada';Motivo=$_.Exception.Message});continue}
    $arquivos=@(Get-ChildItem -LiteralPath $alvo -Recurse -File -Force)
    $protegido=$arquivos|Where-Object {$_.Extension -in '.db','.sqlite','.sqlite3','.credencial','.p12','.keystore','.dat' -or $_.Name -match '^\.env($|\.(local|production|development)$)|^\.dev\.vars$|privada|private.*\.pem|password|google-oauth-client|google-drive-token|colaboradores\.json|recuperacao-email\.json' -or ($_.Extension -notin '.cs','.xaml','.csproj','.md','.ps1','.py','.baml' -and $_.FullName -match '[\\/](segredos|backups?|dados|data)[\\/]')}|Select-Object -First 1
    if($protegido -or @($processos|Where-Object {Dentro $_ $alvo}).Count){$planejados.Add([pscustomobject]@{Pasta=$alvo;Acao='Preservada';Motivo='Contém dados, credenciais ou processo em uso'});continue}
    $tipo=if($fontes -contains $alvo){'Fonte antiga arquivada'}else{'Build descartável'}
    $planejados.Add([pscustomobject]@{Pasta=$alvo;Acao=$tipo;Motivo='Cópia antiga fora da fonte única'});$selecionados.Add($alvo)
}
$manifesto=Join-Path $relatorio 'pastas.json';$planejados|ConvertTo-Json -Depth 4|Set-Content -LiteralPath $manifesto -Encoding UTF8
if(-not $Aplicar){Write-Host "Plano: $($selecionados.Count) pastas antigas removíveis. Inventário: $manifesto. Use -Aplicar após conferir.";return}
$removidas=0
Add-Type -AssemblyName System.IO.Compression.FileSystem
foreach($pasta in $selecionados){
    $alvo=Seguro $pasta
    if($fontes -contains $alvo){
        # Keep a non-executable source archive, excluding generated binaries and build caches.
        $nome=([IO.Path]::GetRelativePath($repo,$alvo)).Replace('\','_').Replace('/','_')
        $zip=Join-Path $relatorio ($nome+'-fontes.zip')
        if(Test-Path -LiteralPath $zip){throw "Arquivo de preservação já existe: $zip"}
        $archive=[IO.Compression.ZipFile]::Open($zip,[IO.Compression.ZipArchiveMode]::Create)
        try{foreach($arq in Get-ChildItem -LiteralPath $alvo -Recurse -File -Force|Where-Object {[IO.Path]::GetRelativePath($alvo,$_.FullName) -notmatch '(^|[\\/])(bin|obj|node_modules|\.git|artifacts)[\\/]' } ){$rel=[IO.Path]::GetRelativePath($alvo,$arq.FullName).Replace('\','/');[void][IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive,$arq.FullName,$rel,[IO.Compression.CompressionLevel]::Optimal)}}finally{$archive.Dispose()}
        $conferir=[IO.Compression.ZipFile]::OpenRead($zip);try{if($conferir.Entries.Count -eq 0){throw 'Arquivo de preservação vazio.'}}finally{$conferir.Dispose()}
    }
    Remove-Item -LiteralPath $alvo -Recurse -Force
    $removidas++
}
Write-Host "Limpeza concluída: $removidas pastas antigas removidas. Dados/credenciais preservados; inventário em $manifesto."
