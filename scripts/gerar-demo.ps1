<# Gera a edição com catálogo fictício (RF03, RNF09, CT21). #>
[CmdletBinding()]
param([string]$Compilador, [string]$Saida, [string]$PublicacaoExistente)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$raiz = Split-Path -Parent $PSScriptRoot
$versao = (Select-Xml -Path "$raiz/backend/src/VarthexComanda.Desktop/VarthexComanda.Desktop.csproj" -XPath '//Version').Node.InnerText.Trim()
$saida = if ($Saida) { $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($Saida) } else { Join-Path $raiz "artifacts/demo-$versao" }
$publicacao = Join-Path $saida 'publish/win-x64'
if (-not $Compilador) { $Compilador = Join-Path $raiz 'artifacts/tools/InnoSetup/ISCC.exe' }
if (-not (Test-Path -LiteralPath $Compilador)) { throw 'Informe o caminho do Inno Setup em -Compilador.' }
if ($PublicacaoExistente) {
    $publicacao = (Resolve-Path -LiteralPath $PublicacaoExistente).Path
    if (-not (Test-Path -LiteralPath (Join-Path $publicacao 'VarthexComanda.exe'))) { throw 'Publicação existente sem executável.' }
} else {
    & (Join-Path $PSScriptRoot 'publicar.ps1') -Saida $saida
    if ($LASTEXITCODE -ne 0) { throw 'Publicação falhou.' }
}
# O instalador demo adiciona apenas o catálogo e o marcador da edição. Nunca inclui dados locais.
$indevidos = Get-ChildItem -LiteralPath $publicacao -File -Recurse | Where-Object {
    $_.Extension -in '.db','.sqlite','.sqlite3','.credencial' -or $_.Name -match 'google-oauth-client|google-drive-token|estado\.dat|colaboradores\.json|edicao-demonstracao\.txt'
}
if ($indevidos) { throw 'Publicação contém dados locais ou marcador de edição. Gere uma publicação limpa.' }
[void](New-Item -ItemType Directory -Force -Path $saida)
& $Compilador "/DAppVersion=$versao" "/DPublishPath=$publicacao" "/DOutputPath=$saida" (Join-Path $PSScriptRoot 'instalador-demo.iss')
if ($LASTEXITCODE -ne 0) { throw 'Compilação do instalador demo falhou.' }
$instalador = Join-Path $saida "VarthexComanda-Demo-$versao-Setup-win-x64.exe"
$hash = (Get-FileHash -LiteralPath $instalador -Algorithm SHA256).Hash
"$hash  $([IO.Path]::GetFileName($instalador))" | Set-Content -LiteralPath "$instalador.sha256" -Encoding ascii
Write-Host "Instalador de demonstração: $instalador"
